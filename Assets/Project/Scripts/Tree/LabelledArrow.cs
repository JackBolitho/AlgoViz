using TMPro;
using UnityEngine;

public class LabelledArrow : Arrow
{
    [SerializeField] private GameObject labelPrefab;
    private GameObject label;

    public void CreateLabel(Transform parent, string text)
    {
        label = Instantiate(labelPrefab, parent);
        label.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public void DrawLabel(Vector3 start, Vector3 end)
    {
        if(label == null)
        {
            return;
        }

        Vector3 midpoint = (start + end)/2f;
        label.transform.localPosition = midpoint;
    }
}
