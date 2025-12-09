using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableBackdropDPMatrix : MonoBehaviour
{
    private DPMatrixBuilder builder;
    public void SetBuilder(DPMatrixBuilder builder)
    {
        this.builder = builder;
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
