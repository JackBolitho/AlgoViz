using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;

public enum Outcome
{
    An_TOO_LARGE,
    BASE_CASE,
    INCLUDE,
    EXCLUDE
}

public struct TreeElement
{
    public int value;
    public Outcome outcome;
    public int n, b;
    public int nParent, bParent;

    public TreeElement(int value, Outcome outcome, int n, int b, int nParent, int bParent)
    {
        this.value = value;
        this.outcome = outcome;
        this.n = n;
        this.b = b;
        this.nParent = nParent;
        this.bParent = bParent;
    }
}

public class TreeBuilder : MonoBehaviour
{
    [SerializeField] private GameObject vertexPrefab;
    [SerializeField] private Vector2 spacing;
    [SerializeField] private GameObject backdropPrefab;
    private Vector3 startPosition;
    private Vertex treeHead;

    //parameters for algorithm
    private List<int> A;
    private int B;
    private TreeElement[,] F;

    //visuals
    [SerializeField] private Vector2 backdropPadding;
    private Vector3 backdropGoalPosition;
    private Vector3 backdropGoalScale;
    private RectTransform backdropRectTransform;


    public void CreateTree(List<int> A, int B, Vector3 startPosition)
    {
        transform.position = startPosition;
        startPosition = Vector3.zero;

        this.A = A;
        this.B = B;
        F = SubsetSum();

        CreateBackdrop();
        CreateTreeHead(F[A.Count, B]);
    }

    private GameObject backdrop;
    private void CreateBackdrop()
    {
        float nodeWidth = 2f; //magic numbers
        float nodeHeight = 1.5f;

        backdrop = Instantiate(backdropPrefab, transform);
        backdropRectTransform = backdrop.GetComponent<RectTransform>();
        backdropRectTransform.sizeDelta = new Vector2(nodeWidth, nodeHeight);
        backdrop.transform.localPosition = new Vector2(0f, 0f);
        backdrop.transform.SetAsFirstSibling();
    }

    private void SetBackdrop()
    {
        float nodeWidth = 2f;
        float nodeHeight = 1.5f;
        float treeWidth = maxX - minX + nodeWidth;
        float treeHeight = maxDepth * spacing.y + nodeHeight;

        backdropGoalScale = new Vector2(treeWidth + backdropPadding.x * 2f, treeHeight + backdropPadding.y * 2f);
        backdropGoalPosition = new Vector2((maxX + minX) / 2f, (nodeHeight / 2f) - (treeHeight / 2f));
    }

    private void Update()
    {
        BackdropMoveToGoalPosition();
    }

    private void BackdropMoveToGoalPosition()
    {
        // Quadratic easing factor
        float speed = 5f; // Adjust this value to control the speed of the movement
        float distance = Vector3.Distance(backdrop.transform.localPosition, backdropGoalPosition);

        // If the vertex is not already at the goal position
        if (distance > 0.01f)
        {
            // Smoothly move the vertex towards the goal position
            backdrop.transform.localPosition = Vector3.Lerp(backdrop.transform.localPosition, backdropGoalPosition, Time.deltaTime * speed);
        }
        else
        {
            // Snap to the goal position if close enough
            backdrop.transform.localPosition = backdropGoalPosition;
        }

        float scaleDist = Vector3.Distance(backdropRectTransform.sizeDelta, backdropGoalScale);

        // If the vertex is not already at the goal position
        if (scaleDist > 0.01f)
        {
            // Smoothly move the vertex towards the goal position
            backdropRectTransform.sizeDelta = Vector3.Lerp(backdropRectTransform.sizeDelta, backdropGoalScale, Time.deltaTime * speed);
        }
        else
        {
            // Snap to the goal position if close enough
            backdropRectTransform.sizeDelta = backdropGoalScale;
        }
    }

    public TreeElement GetTreeElement(int n, int b)
    {
        return F[n, b];
    }

    public List<int> GetA()
    {
        return A;
    }

    //input: A is a list of integers, B is the target value
    //output: true if there exists a subset of A that sums to B, false if otherwise
    private TreeElement[,] SubsetSum()
    {
        TreeElement[,] F = new TreeElement[A.Count + 1, B + 1];

        //base cases
        for (int b = 0; b < B + 1; b++)
        {
            F[0, b] = new TreeElement(0, Outcome.BASE_CASE, 0, b, -1, -1);
        }
        for (int m = 0; m < A.Count + 1; m++)
        {
            F[m, 0] = new TreeElement(1, Outcome.BASE_CASE, m, 0, -1, -1);
        }

        //recurrence
        for (int m = 1; m < A.Count + 1; m++)
        {
            for (int b = 1; b < B + 1; b++)
            {
                if (A[m - 1] > b)
                {
                    F[m, b] = new TreeElement(F[m - 1, b].value, Outcome.An_TOO_LARGE, m, b, m - 1, b);
                }
                else
                {
                    if (F[m - 1, b - A[m - 1]].value == 1)
                    {
                        F[m, b] = new TreeElement(1, Outcome.INCLUDE, m, b, m - 1, b - A[m - 1]);
                    }
                    else
                    {
                        F[m, b] = new TreeElement(F[m - 1, b].value, Outcome.EXCLUDE, m, b, m - 1, b);
                    }
                }
            }
        }

        return F;
    }

    private void CreateTreeHead(TreeElement element)
    {
        //create parent
        Vertex parent = InstantiateVertex(element, null);
        parent.gameObject.GetComponent<Button>().interactable = false;
        parent.ExpandVertex();
        treeHead = parent;

        //create children of vertex
        if (element.outcome != Outcome.BASE_CASE)
        {
            CreateVertexChildren(parent);
        }
    }

    public void CreateVertexChildren(Vertex parent)
    {
        TreeElement element = parent.element;

        if (parent.element.outcome == Outcome.An_TOO_LARGE)
        {
            //exclude case
            Vertex c1 = InstantiateVertex(F[element.n - 1, element.b], parent);
            c1.CloseVertex(false);
            parent.SetChildren(new List<Vertex>() { c1 });
        }
        else
        {
            //include case
            Vertex c1 = InstantiateVertex(F[element.n - 1, element.b - A[element.n - 1]], parent);
            c1.CloseVertex(true);

            //exclude case
            Vertex c2 = InstantiateVertex(F[element.n - 1, element.b], parent);
            c2.CloseVertex(false);

            parent.SetChildren(new List<Vertex>() { c1, c2 });
        }

        //graphics
        ReformTree(treeHead);
    }

    //instantiate a vertex and set its values
    private Vertex InstantiateVertex(TreeElement element, Vertex parent)
    {
        Vertex v = Instantiate(vertexPrefab).GetComponent<Vertex>();
        v.transform.SetParent(transform);
        if (parent == null)
        {
            v.transform.localPosition = startPosition;
        }
        else
        {
            v.transform.localPosition = parent.transform.localPosition;
        }
        v.InitializeVertex(this, element, parent);

        return v;
    }

    public void DeleteVisualization()
    {
        Destroy(transform.parent.gameObject);
    }

    //------ Reingold Tilford algorithm ------

    private Dictionary<Vertex, float> prelim = new();
    private Dictionary<Vertex, float> mod = new();
    private float nextX = 0;
    private float minX, maxX = 0f;
    private float maxDepth = 0f;

    public void ReformTree(Vertex root)
    {
        if (root == null) return;

        prelim.Clear();
        mod.Clear();
        nextX = 0;

        FirstPass(root, 0);

        // Calculate offset to keep root in place
        float desiredRootX = root.transform.localPosition.x - startPosition.x;
        float computedRootX = prelim[root];
        float offsetX = desiredRootX - computedRootX;

        SecondPass(root, offsetX);

        SetBackdrop();
    }

    private void FirstPass(Vertex v, int depth)
    {
        v.SetTreeDepth(depth);

        if(depth > maxDepth)
        {
            maxDepth = depth;
        }

        if (v.children == null || v.children.Count == 0)
        {
            prelim[v] = nextX;
            nextX += spacing.x;
        }
        else
        {
            foreach (var child in v.children)
            {
                FirstPass(child, depth + 1);
            }

            float left = prelim[v.children[0]];
            float right = prelim[v.children[v.children.Count - 1]];
            prelim[v] = (left + right) / 2f;
        }
    }

    private void SecondPass(Vertex v, float accMod)
    {
        float x = prelim[v] + accMod;
        float y = startPosition.y - v.GetTreeDepth() * spacing.y;

        float worldX = startPosition.x + x;
        v.SetGoalPosition(new Vector3(worldX, y, 0f));

        //set extremes
        if (worldX > maxX)
        {
            maxX = worldX;
        }
        if(worldX < minX)
        {
            minX = worldX;
        }

        if (v.children != null)
        {
            foreach (var child in v.children)
            {
                SecondPass(child, accMod + mod.GetValueOrDefault(v, 0));
            }
        }
    }
    
    //------
}

    

