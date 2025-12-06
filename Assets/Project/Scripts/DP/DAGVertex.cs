using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;

public class DAGVertex : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI vertexText;
    [SerializeField] private Vector3 goalPosition;
    private string text;
    [HideInInspector] public List<DAGVertex> children = new List<DAGVertex>();
    private List<Arrow> arrowsToChildren;
    [SerializeField] private Color includeColor;
    [SerializeField] private Color excludeColor;
    [SerializeField] private GameObject lineArrowPrefab;
    private DAGBuilder dAGBuilder;
    public Element element {private get; set;}

 
    public void SetDAGVertex(string text, DAGVertex includeChild, DAGVertex excludeChild, DAGBuilder dAGBuilder, Element element)
    {
        this.dAGBuilder = dAGBuilder;
        this.text = text;
        this.element = element;
        vertexText.text = text;

        arrowsToChildren = new List<Arrow>();
        children.Add(includeChild);
        children.Add(excludeChild);

        foreach (DAGVertex child in children)
        {
            if(child != null)
            {
                GameObject arrow = Instantiate(lineArrowPrefab);
                arrow.transform.SetParent(dAGBuilder.transform);
                arrowsToChildren.Add(arrow.GetComponent<Arrow>());
            }
            else
            {
                arrowsToChildren.Add(null);
            }
        }
    }

    public void SetGoalPosition(Vector3 pos)
    {
        goalPosition = pos;
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

        if(transform.GetChild(0).gameObject.activeSelf)
        {
            DrawLine();
        }
    }

    private void DrawLine()
    {
        for (int i = 0; i < children.Count; i++)
        {
            if(children[i] != null)
            {
                // Get the Backdrop child GameObject of this vertex
                RectTransform backdropTransform = transform.GetChild(0).Find("Backdrop") as RectTransform;
                float parentYOffset = 0f;
                if (backdropTransform != null)
                {
                    parentYOffset = backdropTransform.rect.height / 2f;
                }
                Vector3 parentPos = new Vector3(transform.localPosition.x, transform.localPosition.y - parentYOffset, transform.position.z);

                // Get the Backdrop child GameObject of the child vertex
                float childYOffset = 0f;
                if(children[i].transform.GetChild(0).gameObject.activeSelf)
                {
                    RectTransform childBackdropTransform = children[i].transform.GetChild(0).Find("Backdrop") as RectTransform;
                    childYOffset = 0f;
                    if (childBackdropTransform != null)
                    {
                        childYOffset = childBackdropTransform.rect.height / 2f;
                    }
                }
                else
                {
                    childYOffset = 0.75f;
                }
                Vector3 childPos = new Vector3(children[i].transform.localPosition.x, children[i].transform.localPosition.y + childYOffset, children[i].transform.localPosition.z);

                arrowsToChildren[i].SetSortOrder(transform.parent.parent.GetComponent<Canvas>().sortingOrder + 1);

                if(children[i].element != null && children[i].element.val == 1)
                {
                    arrowsToChildren[i].DrawLine(parentPos, childPos, includeColor);
                }
                else
                {
                    arrowsToChildren[i].DrawLine(parentPos, childPos, excludeColor);
                }
            }
        }
    }
}