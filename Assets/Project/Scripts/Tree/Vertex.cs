using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Vertex : MonoBehaviour
{
    private string strA;
    private string strB;
    private string strNthValue;
    private TreeBuilder treeBuilder;
    public TreeElement element { get; private set; }
    private int treeDepth;
    public Vertex parent { get; private set; }
    public List<Vertex> children = new List<Vertex>();
    private List<Arrow> arrowsToChildren = new List<Arrow>();
    [SerializeField] private Vector3 goalPosition;
    public List<int> currSubset;

    //text references
    [SerializeField] private TextMeshProUGUI arrayText;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private TextMeshProUGUI coverText;
    [SerializeField] private GameObject lineArrowPrefab;

    //visuals
    [SerializeField] private Color arrowColor;
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;

    //must be called when vertex is being instantiated
    public void InitializeVertex(TreeBuilder treeBuilder, TreeElement element, Vertex parent)
    {
        this.treeBuilder = treeBuilder;
        
        List<int> A = treeBuilder.GetA();
        this.element = element;
        if (parent == null)
        {
            this.treeDepth = 0;
            currSubset = new List<int>();
        }
        else
        {
            this.treeDepth = parent.treeDepth + 1;
            currSubset = new List<int>(parent.currSubset);
        }
        this.parent = parent;

        //construct text
        strA = "{";
        if (A.Count > 0 && element.n > 0)
        {
            for (int i = 0; i < element.n - 1; i++)
            {
                strA += A[i].ToString() + ", ";
            }
            strA += A[element.n - 1].ToString() + "}";
        }
        else
        {
            strA += "}";
        }
        strB = element.b.ToString();


        //get last value text
        if (A.Count > element.n)
        {
            strNthValue = A[element.n].ToString();
        }
        else
        {
            strNthValue = "";
        }
    }

    public void SetChildren(List<Vertex> children)
    {
        this.children = children;
        foreach (Vertex child in children)
        {
            GameObject arrow = Instantiate(lineArrowPrefab);
            arrow.transform.SetParent(treeBuilder.transform);
            arrowsToChildren.Add(arrow.GetComponent<Arrow>());
        }
    }

    //set a vertex to hide its information and query
    //Include value? when true, and Don't include value? when false
    public void CloseVertex(bool include)
    {
        arrayText.gameObject.SetActive(false);
        goalText.gameObject.SetActive(false);
        coverText.gameObject.SetActive(true);

        if (include)
        {
            int nthValueInt;
            if (int.TryParse(strNthValue, out nthValueInt))
            {
                currSubset.Add(nthValueInt);
            }
            coverText.text = "Include " + strNthValue + " in subset?";
        }
        else
        {
            coverText.text = "Don't include " + strNthValue + " in subset?";
        }
    }

    //show arrayText and goalText of vertex
    public void ExpandVertex()
    {
        if (element.outcome == Outcome.BASE_CASE)
        {
            arrayText.gameObject.SetActive(false);
            goalText.gameObject.SetActive(false);
            coverText.gameObject.SetActive(true);

            //show text when no subset
            if (element.value == 0)
            {
                coverText.text = GetSubsetStr() + " is invalid.";
                SetVertexColor(invalidColor); //light red
            }
            //show text when subset
            else
            {
                coverText.text = GetSubsetStr() + " is valid!";
                SetVertexColor(validColor); //light green
            }
        }
        else
        {
            arrayText.gameObject.SetActive(true);
            goalText.gameObject.SetActive(true);
            coverText.gameObject.SetActive(false);

            //show text
            arrayText.text = strA;
            goalText.text = strB;
        }
    }

    public void SetTreeDepth(int depth)
    {
        treeDepth = depth;
    }

    public int GetTreeDepth()
    {
        return treeDepth;
    }

    public string GetStrNthValue()
    {
        return strNthValue;
    }

    public TreeElement GetTreeElement()
    {
        return element;
    }

    public void SetGoalPosition(Vector3 pos)
    {
        goalPosition = pos;
    }

    private void SetVertexColor(Color color)
    {
        var button = GetComponent<Button>();
        var colors = button.colors;
        colors.disabledColor = color;
        button.colors = colors;
    }

    private string GetSubsetStr()
    {
        string str = "{";
        if (currSubset.Count > 0)
        {
            foreach (int val in currSubset)
            {
                str += val.ToString() + ", ";
            }
            str = str.Substring(0, str.Length - 2);
        }
        str += "}";
        return str;
    }

    public void OnVertexClick()
    {
        GetComponent<Button>().interactable = false;
        EventSystem.current.SetSelectedGameObject(null);
        ExpandVertex();
        if (element.outcome != Outcome.BASE_CASE)
        {
            treeBuilder.CreateVertexChildren(this);
        }
    }

    private void Update()
    {
        MoveToGoalPosition();
    }

    public void MoveToGoalPosition()
    {
        // Quadratic easing factor
        float speed = 5f; // Adjust this value to control the speed of the movement
        float distance = Vector3.Distance(transform.localPosition, goalPosition);

        // If the vertex is not already at the goal position
        if (distance > 0.01f)
        {
            // Smoothly move the vertex towards the goal position
            transform.localPosition = Vector3.Lerp(transform.localPosition, goalPosition, Time.deltaTime * speed);
        }
        else
        {
            // Snap to the goal position if close enough
            transform.localPosition = goalPosition;
        }

        DrawLine();
    }

    private void DrawLine()
    {
        for (int i = 0; i < children.Count; i++)
        {
            // Get the Backdrop child GameObject of this vertex
            RectTransform backdropTransform = transform.Find("Backdrop") as RectTransform;
            float parentYOffset = 0f;
            if (backdropTransform != null)
            {
                parentYOffset = backdropTransform.rect.height / 2f;
            }
            Vector3 parentPos = new Vector3(transform.localPosition.x, transform.localPosition.y - parentYOffset, transform.position.z);

            // Get the Backdrop child GameObject of the child vertex
            RectTransform childBackdropTransform = children[i].transform.Find("Backdrop") as RectTransform;
            float childYOffset = 0f;
            if (childBackdropTransform != null)
            {
                childYOffset = childBackdropTransform.rect.height / 2f;
            }
            Vector3 childPos = new Vector3(children[i].transform.localPosition.x, children[i].transform.localPosition.y + childYOffset, children[i].transform.localPosition.z);

            arrowsToChildren[i].SetSortOrder(transform.parent.parent.GetComponent<Canvas>().sortingOrder + 1);
            arrowsToChildren[i].DrawLine(parentPos, childPos, arrowColor);
        }
    }
}
