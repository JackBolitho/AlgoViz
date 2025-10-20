using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DPMatrixBuilder : MonoBehaviour
{
    //prefabs
    [SerializeField] private GameObject popupValues;
    [SerializeField] private GameObject elementPrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject labelPrefab;

    //popup graphic
    private TextMeshProUGUI arrayPopupText;
    private TextMeshProUGUI goalPopupText;

    //DP matrix metadata
    [SerializeField] private float distanceFromPopupToViz;
    [SerializeField] private Vector3 elementSpacing;
    [SerializeField] private float elementWriteWait;

    private List<GameObject> arrows = new List<GameObject>();
    private GameObject canvas;
    private Vector3 startPosition;
    private GameObject[,] Fobjs;

    //deals with sub problem arrows
    private List<GameObject> subproblemArrows = new List<GameObject>();

    //parameters given by the algorithm
    private List<int> A;
    private int B;
    private int[,] F;

    void Awake()
    {
        canvas = GameObject.Find("WorldCanvas");
    }

    public void CreateMatrix(List<int> A, int B, Vector3 popupPosition)
    {
        this.startPosition = popupPosition - new Vector3(A.Count / 2f * elementSpacing.x, distanceFromPopupToViz);

        //create popup graphic
        GameObject popup = Instantiate(popupValues, canvas.transform);
        popup.transform.position = popupPosition;
        arrayPopupText = popup.transform.Find("ArrayText").GetComponent<TextMeshProUGUI>();
        goalPopupText = popup.transform.Find("GoalText").GetComponent<TextMeshProUGUI>();

        this.A = A;
        this.B = B;
        DrawLabels();
        StartCoroutine(CreateMatrixImpl());
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
        GameObject element = Instantiate(elementPrefab);
        element.GetComponentInChildren<TextMeshProUGUI>().text = val.ToString();
        element.transform.SetParent(canvas.transform);
        element.transform.localPosition = pos + startPosition;

        Element elem = element.GetComponent<Element>();
        elem.SetDPMatrixBuilder(this);
        elem.SetElementValues(n, b);

        if (arrowDist is not null)
        {
            if (val == 1)
            {
                GameObject arrow = DrawArrow(pos + startPosition, new Vector2(arrowDist.Value.x + startPosition.x, arrowDist.Value.y + startPosition.y));
                elem.InitializeArrows(parent, arrow);
                arrows.Add(arrow);
                arrow.SetActive(false);
            }

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
        GameObject label = Instantiate(labelPrefab);
        label.transform.SetParent(canvas.transform);
        label.transform.position = pos + new Vector2(startPosition.x, startPosition.y);
        label.GetComponentInChildren<TextMeshProUGUI>().text = text;
        return label;
    }

    //arrow base at p1, arrow head at p2
    private GameObject DrawArrow(Vector2 p1, Vector2 p2)
    {
        GameObject arrow = Instantiate(arrowPrefab);
        arrow.GetComponent<Arrow>().DrawLine(p1 /*+ new Vector2(startPosition.x, startPosition.y)*/, p2 /*+ new Vector2(startPosition.x, startPosition.y)*/);
        return arrow;
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
        List<int> A = SubsetSumRetrieval(n, B);

        //construct text
        string strA = "{";
        if (A.Count > 0)
        {
            for (int i = 0; i < A.Count - 1; i++)
            {
                strA += A[i].ToString() + ", ";
            }
            strA += A[A.Count - 1].ToString() + "}";
        }
        else
        {
            strA += "}";
        }
        string strB = B.ToString();

        arrayPopupText.text = strA;
        goalPopupText.text = strB;
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
            GameObject arrow = DrawArrow(element.transform.position, includeParent.transform.position);
            subproblemArrows.Add(arrow);
        }
        
        if(excludeParent != null)
        {
            GameObject arrow = DrawArrow(element.transform.position, excludeParent.transform.position);
            subproblemArrows.Add(arrow);
        }
    }
}
