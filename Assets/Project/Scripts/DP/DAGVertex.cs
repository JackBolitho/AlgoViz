using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DAGVertex : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI vertexText;
    [SerializeField] private Vector3 goalPosition;
    private string text;
    [HideInInspector] public List<DAGVertex> children = new List<DAGVertex>();
    private List<Arrow> arrowsToChildren = new List<Arrow>();
    [SerializeField] private Color arrowColor;
    [SerializeField] private GameObject lineArrowPrefab;
    private DAGBuilder dAGBuilder;
 
    public void SetDAGVertex(string text, DAGVertex includeChild, DAGVertex excludeChild)
    {
        this.text = text;
        vertexText.text = text;

        children.Add(includeChild);
        children.Add(excludeChild);

        /*

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
        }*/
    }

    /*

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

        DrawLine();
    }

    private void DrawLine()
    {
        for (int i = 0; i < children.Count; i++)
        {
            if(children[i] != null)
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
    }*/
}