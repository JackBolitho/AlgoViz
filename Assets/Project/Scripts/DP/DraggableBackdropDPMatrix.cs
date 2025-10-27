using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableBackdropDPMatrix : MonoBehaviour
{
    private DPMatrixBuilder builder;

    private void Start()
    {
        builder = GetComponentInParent<DPMatrixBuilder>();
    }

    public void DeleteVisualization()
    {
        builder.DeleteVisualization();
    }

    public void DeleteSubproblemArrows()
    {
        builder.DeleteSubproblemArrows();
    }
}
