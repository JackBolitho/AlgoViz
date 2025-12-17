using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIArrowGenerator : Graphic
{
    [Header("Arrow Dimensions")]
    public float stemLength = 100f;
    public float stemWidth = 20f;
    public float tipLength = 40f;
    public float tipWidth = 40f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float stemHalfWidth = stemWidth * 0.5f;
        float tipHalfWidth = tipWidth * 0.5f;

        // Origin at left-center of RectTransform
        Vector2 origin = Vector2.zero;

        // -------------------
        // STEM VERTICES
        // -------------------
        Vector2 v0 = origin + Vector2.down * stemHalfWidth;
        Vector2 v1 = origin + Vector2.up * stemHalfWidth;
        Vector2 v2 = v0 + Vector2.right * stemLength;
        Vector2 v3 = v1 + Vector2.right * stemLength;

        int i0 = AddVertex(vh, v0);
        int i1 = AddVertex(vh, v1);
        int i2 = AddVertex(vh, v2);
        int i3 = AddVertex(vh, v3);

        vh.AddTriangle(i0, i1, i3);
        vh.AddTriangle(i0, i3, i2);

        // -------------------
        // TIP VERTICES
        // -------------------
        Vector2 tipOrigin = Vector2.right * stemLength;

        Vector2 v4 = tipOrigin + Vector2.up * tipHalfWidth;
        Vector2 v5 = tipOrigin + Vector2.down * tipHalfWidth;
        Vector2 v6 = tipOrigin + Vector2.right * tipLength;

        int i4 = AddVertex(vh, v4);
        int i5 = AddVertex(vh, v5);
        int i6 = AddVertex(vh, v6);

        vh.AddTriangle(i4, i6, i5);
    }

    int AddVertex(VertexHelper vh, Vector2 position)
    {
        vh.AddVert(
            position,
            color,
            Vector2.zero
        );
        return vh.currentVertCount - 1;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetVerticesDirty();
    }
#endif
}

