using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIArrowGenerator))]
[RequireComponent(typeof(RectTransform))]
public class Arrow : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 endPoint;

    UIArrowGenerator arrow;
    RectTransform rectTransform;
    Canvas canvas;

    void Start()
    {
        GetComponents();
    }

    private void GetComponents()
    {
        arrow = GetComponent<UIArrowGenerator>();
        rectTransform = GetComponent<RectTransform>();
        canvas = transform.GetComponentInParent<Canvas>();
    }

    public void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        if(arrow == null || rectTransform == null || canvas == null){
            GetComponents();
        }
        
        // Convert to canvas local space if needed
        Vector2 startLocal;
        Vector2 endLocal;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            startLocal = start;
            endLocal = end;
        }
        else
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, start, canvas.worldCamera, out startLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, end, canvas.worldCamera, out endLocal);
        }

        Vector2 direction = endLocal - startLocal;
        float distance = direction.magnitude;

        if (distance < 0.001f)
            return;

        // Position arrow so stem bottom is at start
        rectTransform.anchoredPosition = startLocal;

        // Rotate arrow to face end
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

        // Update arrow geometry
        arrow.stemLength = Mathf.Max(0, distance - arrow.tipLength);

        // 🔹 APPLY COLOR
        Graphic graphic = arrow.GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.color = color;
        }

        arrow.SetVerticesDirty();
    }

    public void SetSortOrder(int sortOrder)
    {
        
    }
}