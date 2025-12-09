using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;


public class DAGBuilder : MonoBehaviour
{
    [SerializeField] private GameObject dAGVertexPrefab;   
    private Dictionary<Element, DAGVertex> elementVertexPairs = new Dictionary<Element, DAGVertex>();
    private HashSet<Element> clickableSet = new HashSet<Element>();
    private Element head;

    //visualization parameters
    [SerializeField] private Vector2 spacing;

    //visuals for backdrop
    [SerializeField] private GameObject backdropPrefab;
    [SerializeField] private Vector2 backdropPadding;
    private Vector3 backdropGoalPosition;
    private Vector3 backdropGoalScale;
    private RectTransform backdropRectTransform;
    private DPMatrixBuilder dPMatrixBuilder;

    public void SetBuilder(DPMatrixBuilder dPMatrixBuilder)
    {
        this.dPMatrixBuilder = dPMatrixBuilder;
    }

    public void AddToDAGVisualization(Element e)
    {
        if(head == null)
        {
            head = e;
        }

        if(backdrop == null)
        {
            CreateBackdrop();
        }

        //to the DAG visualization
        if(clickableSet.Contains(e) || elementVertexPairs.Count == 0)
        {
            CreateDAGVertex(e);
            Reform(head);
        }
        //clear previous investigations
        else
        {
            Debug.Log("Remove Investigations");
        }
    }

    //creates the vertex for the DAG and creates all ghosts
    private void CreateDAGVertex(Element e)
    {
        clickableSet.Remove(e);

        if(e.includeChild != null)
        {
            InstantiateVertexGraphic(e.includeChild, e);
        }
        if(e.excludeChild != null)
        {
            InstantiateVertexGraphic(e.excludeChild, e);
        }

        InstantiateVertexGraphic(e, null);
    }

    //creates the gameobject for the vertex, and deals with keeping track of the vertex
    //when parent is null, 
    private void InstantiateVertexGraphic(Element e, Element parent)
    {
        //only include this vertex in the clickable set if it is not already in the DAG visualization
        if(!elementVertexPairs.ContainsKey(e))
        {
            GameObject vertexObj = Instantiate(dAGVertexPrefab);
            DAGVertex vertex = vertexObj.GetComponent<DAGVertex>();
            vertexObj.transform.SetParent(transform);

            //set values of parent vertex, assuming that the children are deactivated and already in the dictionary
            if(parent == null)
            {
                DAGVertex includeChild = null;
                if (e.includeChild != null && elementVertexPairs.ContainsKey(e.includeChild))
                {
                    includeChild = elementVertexPairs[e.includeChild];
                }
                DAGVertex excludeChild = null;
                if (e.excludeChild != null && elementVertexPairs.ContainsKey(e.excludeChild))
                {
                    excludeChild = elementVertexPairs[e.excludeChild];
                }

                vertex.SetDAGVertex("I am a vertex", includeChild, excludeChild, this, e);
            }
            else
            {
                //set the position to the position of the parent vertex
                if(elementVertexPairs.ContainsKey(parent)){
                    vertex.transform.position = elementVertexPairs[parent].gameObject.transform.position;
                }

                //hide it and pair it
                vertexObj.transform.GetChild(0).gameObject.SetActive(false);
                clickableSet.Add(e);
                vertex.SetDAGVertex("I am a vertex", null, null, this, e);
            }

            elementVertexPairs.Add(e, vertex);
        }
        else
        {
            //set values of vertex
            if(parent == null)
            {   
                DAGVertex vertex = elementVertexPairs[e];

                DAGVertex includeChild = null;
                if (e.includeChild != null && elementVertexPairs.ContainsKey(e.includeChild))
                {
                    includeChild = elementVertexPairs[e.includeChild];
                }
                DAGVertex excludeChild = null;
                if (e.excludeChild != null && elementVertexPairs.ContainsKey(e.excludeChild))
                {
                    excludeChild = elementVertexPairs[e.excludeChild];
                }

                vertex.SetDAGVertex("I am a vertex", includeChild, excludeChild, this, e);
                vertex.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    private bool ElementIsInDAGDict(Element e)
    {
        return elementVertexPairs.ContainsKey(e);
    }

    //BFS starting at startVertex, and compare each value to goalVertex
    private bool ElementIsInDAG(Element startVertex, Element goalVertex)
    {
        Queue<Element> elementQueue = new Queue<Element>();
        elementQueue.Enqueue(startVertex);
        while(elementQueue.Count > 0)
        {
            Element nextElement = elementQueue.Dequeue();
            if(nextElement == goalVertex)
            {
                return true;
            }

            if(nextElement.includeChild != null)
            {
                elementQueue.Enqueue(nextElement.includeChild);
            }
            if(nextElement.excludeChild != null)
            {
                elementQueue.Enqueue(nextElement.excludeChild);
            }
        }
        return false;
    }
    
    private void BFS(Element startVertex)
    {
        Queue<Element> elementQueue = new Queue<Element>();
        elementQueue.Enqueue(startVertex);
        while(elementQueue.Count > 0)
        {
            Element nextElement = elementQueue.Dequeue();

            if(nextElement.includeChild != null)
            {
                elementQueue.Enqueue(nextElement.includeChild);
            }
            if(nextElement.excludeChild != null)
            {
                elementQueue.Enqueue(nextElement.excludeChild);
            }
        }
    }

    public void Reform(Element e)
    {
        Dictionary<int, List<Element>> antichain = FormAntichainParition(e);
        int widestDepth = 0;
        foreach(int depth in antichain.Keys)
        {
            List<Element> currChain = antichain[depth];
            float startX = -(currChain.Count - 1)/2f * spacing.x;
            for(int i = 0; i < currChain.Count; i++)
            {
                Element currElement = currChain[i];
                DAGVertex vertex = elementVertexPairs[currElement];
                vertex.SetGoalPosition(new Vector3(startX + i * spacing.x, -depth * spacing.y, 0f));
            }

            //calculate the widest depth
            if(widestDepth < currChain.Count)
            {
                widestDepth = currChain.Count;     
            }
        }
        SetBackdrop(widestDepth);
    }

    private Dictionary<int, List<Element>> FormAntichainParition(Element e)
    {
        //start recursive traversal
        Dictionary<int, List<Element>> antichain = new Dictionary<int, List<Element>>();
        HashSet<Element> visited = new HashSet<Element>();
        InOrderTraversal(e, ref antichain, ref visited, 0);
    
        return antichain;
    }

    private void InOrderTraversal(Element e, ref Dictionary<int, List<Element>> antichain, ref HashSet<Element> visited, int depth)
    {
        //recurse on the left
        if(e.includeChild != null && !visited.Contains(e.includeChild) && elementVertexPairs.ContainsKey(e.includeChild))
        {
            InOrderTraversal(e.includeChild, ref antichain, ref visited, depth+1);
        }

        //add this element to the antichain
        if (!antichain.ContainsKey(depth))
        {
            antichain.Add(depth, new List<Element>());
        }
        List<Element> currChain = antichain[depth];
        visited.Add(e);
        currChain.Add(e);

        //track maximum depth
        if(depth > maxDepth){
            maxDepth = depth;
        }

        //recurse on the right
        if(e.excludeChild != null && !visited.Contains(e.excludeChild) && elementVertexPairs.ContainsKey(e.excludeChild))
        {
            InOrderTraversal(e.excludeChild, ref antichain, ref visited, depth+1);
        }
    }

    private GameObject backdrop;
    private float maxDepth = 0f;

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

    private void CreateBackdrop()
    {
        float nodeWidth = 2f; //magic numbers
        float nodeHeight = 1.5f;

        backdrop = Instantiate(backdropPrefab, transform);
        backdrop.GetComponent<DraggableBackdropDPMatrix>().SetBuilder(dPMatrixBuilder);
        backdropRectTransform = backdrop.GetComponent<RectTransform>();
        backdropRectTransform.sizeDelta = new Vector2(nodeWidth, nodeHeight);
        backdrop.transform.localPosition = new Vector2(0f, 0f);
        backdrop.transform.SetAsFirstSibling();
    }

    private void SetBackdrop(int widestDepth)
    {
        float nodeWidth = 2f;
        float nodeHeight = 1.5f;
        float treeWidth = spacing.x * (widestDepth-1) + nodeWidth;
        float treeHeight = maxDepth * spacing.y + nodeHeight;

        backdropGoalScale = new Vector2(treeWidth + backdropPadding.x * 2f, treeHeight + backdropPadding.y * 2f);
        backdropGoalPosition = new Vector2(0, (nodeHeight / 2f) - (treeHeight / 2f));
    }
}
