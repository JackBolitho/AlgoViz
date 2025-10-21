using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private GameObject tip;
    [SerializeField] private LineRenderer lineRenderer;

    public void DrawLine(Vector3 start, Vector3 end)
    {
        Vector3 delta = end - start;
        Vector3 n_delta = delta / delta.magnitude;
        Vector3 lineEnd = end - n_delta * (tip.transform.localScale.y / 2);

        tip.transform.localPosition = lineEnd;
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg - 90f;
        tip.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, lineEnd);
    }

}
