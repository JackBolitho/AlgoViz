using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;

public class DAGVertex : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI vertexText;
    [SerializeField] private Vector3 goalPosition;
    private string text;
    [HideInInspector] public List<DAGVertex> children;
    private List<LabelledArrow> arrowsToChildren;
    [SerializeField] private Color includeColor;
    [SerializeField] private Color excludeColor;
    [SerializeField] private GameObject lineArrowPrefab;
    private DAGBuilder dAGBuilder;
    public Element element {private get; set;}

 
    public void SetDAGVertex(DAGVertex includeChild, DAGVertex excludeChild, DAGBuilder dAGBuilder, Element element)
    {
        this.dAGBuilder = dAGBuilder;
        this.element = element;

        arrowsToChildren = new List<LabelledArrow>();
        children = new List<DAGVertex>
        {
            includeChild,
            excludeChild
        };

        for(int i = 0; i < children.Count; i++)
        {
            if(children[i] != null)
            {
                GameObject arrowObj = Instantiate(lineArrowPrefab);
                arrowObj.transform.SetParent(dAGBuilder.transform);
                LabelledArrow arrow = arrowObj.GetComponent<LabelledArrow>();
                arrowsToChildren.Add(arrow);

                //make label
                string actionStr;
                if(i == 0)
                {
                    actionStr = "Picking ";
                }
                else
                {
                    actionStr = "Not picking ";
                }   
                string lastElement = dAGBuilder.dPMatrixBuilder.GetElementAtIndex(element.nval);
                arrow.CreateLabel(dAGBuilder.transform, actionStr + lastElement);

            }
            else
            {
                arrowsToChildren.Add(null);
            }
        }

        //compute relevant strings
        string target = element.bval.ToString();
        string subset = dAGBuilder.dPMatrixBuilder.GetSubsetText(element.nval);

        //determine text here
        if(element.val == 1)
        {
            //base case
            if(includeChild == null && excludeChild == null)
            {
                text = "Base Case: \nThe empty set is a valid subset that sums to 0 by definition.";
            }
            else
            {
                text = "You can get " + target + " from\n" + subset + " by:";
            }
        }
        else
        {
            //base case
            if(includeChild == null && excludeChild == null)
            {
                text = "Base Case: \nThere is no subset of the empty set that sums to " + target + ".";
            }
            else
            {
                text = "You cannot get " + target + " from\n" + subset;
            }
        }
        vertexText.text = text;
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
                arrowsToChildren[i].DrawLabel(parentPos, childPos);
            }
        }
    }
}