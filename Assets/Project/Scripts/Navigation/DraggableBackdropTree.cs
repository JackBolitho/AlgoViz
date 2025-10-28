using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableBackdropTree : MonoBehaviour
{
    private TreeBuilder builder;

    private void Start()
    {
        builder = GetComponentInParent<TreeBuilder>();
    }

    public void DeleteVisualization()
    {
        builder.DeleteVisualization();
    }
}
