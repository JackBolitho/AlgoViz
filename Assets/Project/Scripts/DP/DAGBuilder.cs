using System.Collections.Generic;
using UnityEngine;


public class DAGBuilder : MonoBehaviour
{
    [SerializeField] private GameObject dAGVertexPrefab;   
    private Dictionary<Element, DAGVertex> elementVertexPairs = new Dictionary<Element, DAGVertex>();
    private HashSet<Element> clickableSet = new HashSet<Element>();

    public void AddToDAGVisualization(Element e)
    {
        //to the DAG visualization
        if(clickableSet.Contains(e) || elementVertexPairs.Count == 0)
        {
            CreateDAGVertex(e);
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
            InstantiateVertexGraphic(e.includeChild, false);
        }
        if(e.excludeChild != null)
        {
            InstantiateVertexGraphic(e.excludeChild, false);
        }

        InstantiateVertexGraphic(e, true);
    }

    //creates the gameobject for the vertex, and deals with keeping track of the vertex
    private void InstantiateVertexGraphic(Element e, bool parent)
    {
        //only include this vertex in the clickable set if it is not already in the DAG visualization
        if(!elementVertexPairs.ContainsKey(e))
        {
            GameObject vertexObj = Instantiate(dAGVertexPrefab);
            DAGVertex vertex = vertexObj.GetComponent<DAGVertex>();
            vertexObj.transform.SetParent(transform);

            //set values of parent vertex, assuming that the children are deactivated and already in the dictionary
            if(parent)
            {
                DAGVertex includeChild = null;
                if (e.includeChild != null && elementVertexPairs.ContainsKey(e.includeChild))
                {
                    includeChild = elementVertexPairs[e.includeChild];
                }
                DAGVertex excludeChild = null;
                if (e.excludeChild != null &&elementVertexPairs.ContainsKey(e.excludeChild))
                {
                    excludeChild = elementVertexPairs[e.excludeChild];
                }

                vertex.SetDAGVertex("I am a vertex", includeChild, excludeChild);
            }
            else
            {
                //hide it and pair it
                vertexObj.SetActive(false);
                elementVertexPairs.Add(e, vertex);
                clickableSet.Add(e);
            }
        }
        else
        {
            //set values of vertex
            if(parent)
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

                vertex.SetDAGVertex("I am a vertex", includeChild, excludeChild);
                vertex.gameObject.SetActive(true);
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
}
