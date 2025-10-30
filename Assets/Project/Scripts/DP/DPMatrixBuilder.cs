using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

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

    //deals with sub problem arrows
    private List<GameObject> subproblemArrows = new List<GameObject>();

    //parameters given by the algorithm
    private List<int> A;
    private int B;
    private int[,] F;

    //visuals
    [SerializeField] private Vector2 backdropPadding;

    public void CreateMatrix(List<int> A, int B, Vector3 popupPosition)
    {
        transform.position = popupPosition;
        this.startPosition = -new Vector3(A.Count / 2f * elementSpacing.x, distanceFromPopupToViz);

        //create popup graphic
        GameObject popup = Instantiate(popupValues, transform);
        popup.transform.localPosition = Vector3.zero;
        popupText = popup.transform.Find("PopupText").GetComponent<TextMeshProUGUI>();

        this.A = A;
        this.B = B;
        AddBackdrop();
        DrawLabels();
        StartCoroutine(CreateMatrixImpl());
    }

    public void AddBackdrop()
    {
        float elementYScale = elementPrefab.transform.localScale.y;
        float popupYScale = 2f;

        float elementGap = elementSpacing.y - elementYScale;
        float elementsHeight = (elementYScale + elementGap) * (B + 1) - elementGap;
        float elementsWidth = (elementYScale + elementGap) * (A.Count + 1) - elementGap;

        float popupGap = distanceFromPopupToViz - (popupYScale / 2f + elementYScale / 2f);
        float popupHeader = popupYScale + popupGap;
        float totalHeight = elementsHeight + popupHeader;

        Vector3 position = new Vector3(0.0f, (popupYScale / 2f) - (totalHeight / 2f), 0f);
        Vector3 scale = new Vector3(elementsWidth + backdropPadding.x, totalHeight + backdropPadding.y * 2f, 0f);
        
        GameObject backdrop = Instantiate(backdropPrefab, transform);
        backdrop.GetComponent<RectTransform>().sizeDelta = new Vector2(scale.x, scale.y);
        backdrop.transform.localPosition = position;
        backdrop.transform.SetAsFirstSibling();
    }

    private IEnumerator DrawLabels()
    {
        //base cases
        for (int m = 0; m < A.Count + 1; m++)
        {
            if (m == 0)
            {
                CreateLabel("ø", new Vector2(elementSpacing.x * m, elementSpacing.y));
            }
            else
            {
                CreateLabel(A[m - 1].ToString(), new Vector2(elementSpacing.x * m, elementSpacing.y));
            }
            yield return new WaitForSeconds(elementWriteWait);
        }
        for (int b = 0; b < B + 1; b++)
        {
            CreateLabel(b.ToString(), new Vector2(-elementSpacing.x, -elementSpacing.y * b));
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
        element.transform.localPosition = pos + startPosition;

        Element elem = element.GetComponent<Element>();
        elem.SetDPMatrixBuilder(this);
        elem.SetElementValues(n, b);

        if (arrowDist is not null)
        {
            /*
            if (val == 1)
            {
                GameObject arrow = DrawArrow(pos + startPosition, new Vector2(arrowDist.Value.x + startPosition.x, arrowDist.Value.y + startPosition.y));
                elem.InitializeArrows(parent, arrow);
                arrows.Add(arrow);
                arrow.SetActive(false);
            }*/

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
    private GameObject DrawArrow(Vector2 p1, Vector2 p2)
    {
        GameObject arrowObj = Instantiate(arrowPrefab, transform);
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        arrow.DrawLine(p1, p2);
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
                List<int> subset = SubsetSumRetrieval(n, B);

                //construct text
                string subsetStr = "{";
                if (subset.Count > 0)
                {
                    for (int i = 0; i < subset.Count - 1; i++)
                    {
                        subsetStr += subset[i].ToString() + ", ";
                    }
                    subsetStr += subset[subset.Count - 1].ToString() + "}";
                }
                else
                {
                    subsetStr += "}";
                }
                string strB = B.ToString();
                msg = "A subset of \n" + arrayStr + "\nthat sums to " + strB + " is \n" + subsetStr + ".";
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

    public void ShowSubproblemArrows(Element element, Element includeParent, Element excludeParent)
    {
        if (includeParent != null)
        {
            GameObject arrow = DrawArrow(element.transform.localPosition, includeParent.transform.localPosition);
            subproblemArrows.Add(arrow);
        }

        if (excludeParent != null)
        {
            GameObject arrow = DrawArrow(element.transform.localPosition, excludeParent.transform.localPosition);
            subproblemArrows.Add(arrow);
        }
    }

    public void DeleteVisualization()
    {
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
    }
}
