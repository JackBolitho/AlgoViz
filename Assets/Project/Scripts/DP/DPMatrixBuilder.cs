using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.UI;

public class DPMatrixBuilder : MonoBehaviour
{
    //prefabs
    [SerializeField] private GameObject popupValues;
    [SerializeField] private GameObject elementPrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject labelPrefab;
    [SerializeField] private GameObject backdropPrefab;

    //popup graphic
    private TextMeshProUGUI popupText;

    //DP matrix metadata
    [SerializeField] private float distanceFromPopupToViz;
    [SerializeField] private Vector3 elementSpacing;
    [SerializeField] private float elementWriteWait;

    private List<GameObject> arrows = new List<GameObject>();
    private Vector3 startPosition;
    private GameObject[,] Fobjs;
    private GameObject visualizationParent;

    //deals with sub problem arrows
    private List<GameObject> subproblemArrows = new List<GameObject>();
    private List<GameObject> nLabels = new List<GameObject>();
    private List<GameObject> bLabels = new List<GameObject>();

    //parameters given by the algorithm
    private List<int> A;
    private int B;
    private int[,] F;

    //visuals
    [SerializeField] private Vector2 backdropPadding;
    [SerializeField] private Color validHighlightColor;
    [SerializeField] private Color validArrowColor;
    [SerializeField] private Color invalidHighlightColor;
    [SerializeField] private Color invalidArrowColor;

    //label visuals
    [SerializeField] private Color labelHighlight;
    [SerializeField] private Color labelNoHiglight;

    //deals with tree construction
    private bool usesDAG;
    [SerializeField] private GameObject dAGBuilderPrefab;
    private DAGBuilder dAGBuilder = null;
    private Navigator navigator;

    public void CreateMatrix(List<int> A, int B, Vector3 popupPosition, bool usesDAG, GameObject visualizationParent)
    {
        transform.position = popupPosition;
        this.startPosition = -new Vector3(A.Count / 2f * elementSpacing.x, distanceFromPopupToViz);
        this.visualizationParent = visualizationParent;
        navigator = GameObject.Find("Navigator").GetComponent<Navigator>();

        //create popup graphic
        GameObject popup = Instantiate(popupValues, transform);
        popup.transform.localPosition = Vector3.zero;
        popupText = popup.transform.Find("PopupText").GetComponent<TextMeshProUGUI>();

        this.A = A;
        this.B = B;
        this.usesDAG = usesDAG;
        AddBackdrop();
        DrawLabels();
        StartCoroutine(CreateMatrixImpl());
    }

    private float GetHeight()
    {
        float elementYScale = elementPrefab.transform.localScale.y;
        float popupYScale = 2f;

        float elementGap = elementSpacing.y - elementYScale;
        float elementsHeight = (elementYScale + elementGap) * (B + 1) - elementGap;

        float popupGap = distanceFromPopupToViz - (popupYScale / 2f + elementYScale / 2f);
        float popupHeader = popupYScale + popupGap;
        float totalHeight = elementsHeight + popupHeader;
        return totalHeight;
    }

    private float GetWidth()
    {
        float elementYScale = elementPrefab.transform.localScale.y;
        float elementGap = elementSpacing.y - elementYScale;
        float elementsWidth = (elementYScale + elementGap) * (A.Count + 1) - elementGap;
        return elementsWidth;
    }

    public void AddBackdrop()
    {
        float popupYScale = 2f;
        float totalWidth = GetWidth();
        float totalHeight = GetHeight();

        Vector3 position = new Vector3(0.0f, (popupYScale / 2f) - (totalHeight / 2f), 0f);
        Vector3 scale = new Vector3(totalWidth + backdropPadding.x, totalHeight + backdropPadding.y, 0f);
        
        GameObject backdrop = Instantiate(backdropPrefab, transform);
        backdrop.GetComponent<DraggableBackdropDPMatrix>().SetBuilder(this);
        backdrop.GetComponent<RectTransform>().sizeDelta = new Vector2(scale.x, scale.y);
        backdrop.transform.localPosition = position;
        backdrop.transform.SetAsFirstSibling();
    }

    private IEnumerator DrawLabels()
    {
        //base cases
        for (int m = 0; m < A.Count + 1; m++)
        {
            GameObject label;
            if (m == 0)
            {
                label = CreateLabel("ø", new Vector2(elementSpacing.x * m, elementSpacing.y));
            }
            else
            {
                label = CreateLabel(A[m - 1].ToString(), new Vector2(elementSpacing.x * m, elementSpacing.y));
            }
            nLabels.Add(label);
            yield return new WaitForSeconds(elementWriteWait);
        }
        for (int b = 0; b < B + 1; b++)
        {
            GameObject label = CreateLabel(b.ToString(), new Vector2(-elementSpacing.x, -elementSpacing.y * b));
            bLabels.Add(label);
            yield return new WaitForSeconds(elementWriteWait/2f);
        }
    }  

    private IEnumerator CreateMatrixImpl()
    {
        yield return DrawLabels();

        F = new int[A.Count + 1, B + 1];
        Fobjs = new GameObject[A.Count + 1, B + 1];

        //base cases
        for (int m = 0; m < A.Count + 1; m++)
        {
            F[m, 0] = 1;
            Fobjs[m, 0] = CreateElement(1, m, 0, new Vector3(elementSpacing.x * m, 0f), null, null);
            yield return new WaitForSeconds(elementWriteWait);
        }
        for (int b = 1; b < B + 1; b++)
        {
            F[0, b] = 0;
            Fobjs[0, b] = CreateElement(0, 0, b, new Vector3(0f, -elementSpacing.y * b), null, null);
            yield return new WaitForSeconds(elementWriteWait);
        }

        //recurrence
        for (int m = 1; m < A.Count + 1; m++)
        {
            for (int b = 1; b < B + 1; b++)
            {
                if (A[m - 1] > b)
                {
                    F[m, b] = F[m - 1, b];
                    Fobjs[m, b] = CreateElement(F[m, b], m, b, new Vector3(elementSpacing.x * m, -elementSpacing.y * b), new Vector3(elementSpacing.x * (m - 1), -elementSpacing.y * b), Fobjs[m - 1, b]);
                    yield return new WaitForSeconds(elementWriteWait);
                }
                else
                {
                    F[m, b] = Mathf.Max(F[m - 1, b], F[m - 1, b - A[m - 1]]);

                    if (F[m - 1, b - A[m - 1]] > F[m - 1, b])
                    {
                        Fobjs[m, b] = CreateElement(F[m, b], m, b, new Vector3(elementSpacing.x * m, -elementSpacing.y * b), new Vector3(elementSpacing.x * (m - 1), -elementSpacing.y * (b - A[m - 1])), Fobjs[m - 1, b - A[m - 1]]);
                    }
                    else
                    {
                        Fobjs[m, b] = CreateElement(F[m, b], m, b, new Vector3(elementSpacing.x * m, -elementSpacing.y * b), new Vector3(elementSpacing.x * (m - 1), -elementSpacing.y * b), Fobjs[m - 1, b]);
                    }
                    yield return new WaitForSeconds(elementWriteWait);
                }
            }
        }
    }

    private GameObject CreateElement(int val, int n, int b, Vector3 pos, Vector3? arrowDist, GameObject parent)
    {
        GameObject element = Instantiate(elementPrefab, transform);
        element.GetComponentInChildren<TextMeshProUGUI>().text = val.ToString();
        Button button = element.GetComponent<Button>();

        //set color on element
        ColorBlock cb = button.colors;
        cb.highlightedColor = val == 1 ? validHighlightColor : invalidHighlightColor;
        button.colors = cb;

        element.transform.localPosition = pos + startPosition;

        Element elem = element.GetComponent<Element>();
        elem.SetDPMatrixBuilder(this);
        elem.SetElementValues(n, b, val);

        if (arrowDist is not null)
        {
            //set which elements are the subproblem solutions
            if (A[n - 1] > b)
            {
                elem.InitializeSubproblems(Fobjs[n - 1, b], null);
            }
            else
            {
                elem.InitializeSubproblems(Fobjs[n - 1, b], Fobjs[n - 1, b - A[n - 1]]);
            }
        }

        return element;
    }

    private GameObject CreateLabel(string text, Vector2 pos)
    {
        GameObject label = Instantiate(labelPrefab, transform);
        label.transform.localPosition = pos + new Vector2(startPosition.x, startPosition.y);
        label.GetComponentInChildren<TextMeshProUGUI>().text = text;
        return label;
    }

    //arrow base at p1, arrow head at p2
    private GameObject DrawArrow(Vector2 p1, Vector2 p2, Color color)
    {
        GameObject arrowObj = Instantiate(arrowPrefab, transform);
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        arrow.DrawLine(p1, p2, color);
        arrow.SetSortOrder(transform.parent.GetComponent<Canvas>().sortingOrder + 1);
        return arrowObj;
    }

    public void HideAllArrows()
    {
        foreach (GameObject arrow in arrows)
        {
            arrow.SetActive(false);
        }
    }

    public void ShowElementPopup(int n, int B)
    {
        string msg;
        if (Fobjs[n, B].GetComponent<Element>().IsBaseCase())
        {
            if (F[n, B] == 0)
            {
                msg = "Base Case: \nThere is no subset of the empty set that sums to " + B.ToString() + ".";
            }
            else
            {
                msg = "Base Case: \nThe empty set is a valid subset that sums to 0 by definition.";
            }
        }
        else
        {
            //get array string of length B
            string arrayStr = "{";
            for (int i = 0; i < n - 1; i++)
            {
                arrayStr += A[i].ToString() + ", ";
            }
            if (n >= 1)
            {
                arrayStr += A[n - 1].ToString();
            }
            arrayStr += "}";
        
            if (F[n, B] == 0)
            {
                msg = "No subset of \n" + arrayStr + "\nsums up to " + B.ToString() + ".\nClick to see why.";
            }
            else
            {
                string strB = B.ToString();
                msg = "There is a way to get " + strB + " from\n" + arrayStr + ".\nClick to see how!";
            }
        }
        popupText.text = msg;
    }

    //input: the DP matrix F from subset sum, the nth value (1 indexed)
    //output: the subset that sums to b
    private List<int> SubsetSumRetrieval(int n, int B)
    {
        //check if given subset sums to B
        if (F[n, B] == 0)
        {
            return new List<int>();
        }

        //retrieval algorithm
        int m = n;
        int b = B;
        List<int> S = new List<int>();
        while (m > 0)
        {
            if (F[m - 1, b] == 1)
            {
                m -= 1;
            }
            else
            {
                S.Add(A[m - 1]);
                b -= A[m - 1];
                m -= 1;
            }
        }

        return S;
    }

    //show the include/exclude arrows as applicable
    public void ShowSubproblemArrows(Element element, Element includeChild, Element excludeChild)
    {
        if (includeChild != null)
        {
            Color color = includeChild.val == 1 ? validArrowColor : invalidArrowColor;
            GameObject arrow = DrawArrow(element.transform.localPosition, includeChild.transform.localPosition, color);
            subproblemArrows.Add(arrow);
        }

        if (excludeChild != null)
        {
            Color color = excludeChild.val == 1 ? validArrowColor : invalidArrowColor;
            GameObject arrow = DrawArrow(element.transform.localPosition, excludeChild.transform.localPosition, color);
            subproblemArrows.Add(arrow);
        }
    }


    public void DeleteVisualization()
    {
        if(usesDAG && dAGBuilder != null)
        {
            Destroy(dAGBuilder.transform.parent.gameObject);
        }

        DeleteSubproblemArrows();
        Destroy(transform.parent.gameObject);
    }

    public void DeleteSubproblemArrows()
    {
        foreach (GameObject obj in subproblemArrows)
        {
            Destroy(obj);
        }
        subproblemArrows = new List<GameObject>();

        //remove dag visualization
        if(usesDAG && dAGBuilder != null)
        {
            Destroy(dAGBuilder.transform.parent.gameObject);
            dAGBuilder = null;
        }
    }

    public void HighlightLabels(int n, int b)
    {
        //clear all labels
        for (int i = 0; i < nLabels.Count; i++)
        {
            SetLabelColor(nLabels[i], i <= n ? labelHighlight : labelNoHiglight);
        }

        for (int i = 0; i < bLabels.Count; i++)
        {
            SetLabelColor(bLabels[i], labelNoHiglight);
        }
        SetLabelColor(bLabels[b], labelHighlight);
    }

    private void SetLabelColor(GameObject label, Color color)
    {
        label.GetComponentInChildren<TextMeshProUGUI>().color = color;
    }   
    
    public void OnElementClick(Element e)
    {
        if (usesDAG)
        {
            //the first time the DAG is constructed 
            if(dAGBuilder == null)
            {
                GameObject obj = Instantiate(dAGBuilderPrefab, visualizationParent.transform);
                dAGBuilder = obj.GetComponentInChildren<DAGBuilder>();

                Vector3 visualizationPos = new Vector3(transform.position.x + GetWidth() + backdropPadding.x, transform.position.y, 1f);
                dAGBuilder.InitializeVisualization(this, visualizationPos);
                navigator.DrawPanelFirst(dAGBuilder.gameObject);//.transform.GetChild(0).gameObject);
            }
            dAGBuilder.AddToDAGVisualization(e);  
        }
    }
}
