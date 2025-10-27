using UnityEngine;
using UnityEngine.EventSystems;

public class Element : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private DPMatrixBuilder dPMatrixBuilder;
    private GameObject arrow;
    private Element parent;
    private Element excludeParent;
    private Element includeParent;

    //element values
    private int nval;
    private int bval;

    public void SetDPMatrixBuilder(DPMatrixBuilder dPMatrixBuilder)
    {
        this.dPMatrixBuilder = dPMatrixBuilder;
    }

    public void SetElementValues(int nval, int bval)
    {
        this.nval = nval;
        this.bval = bval;
    }

    public void InitializeArrows(GameObject parent, GameObject arrow)
    {
        if (parent != null)
        {
            this.parent = parent.GetComponent<Element>();
            this.arrow = arrow;
        }
    }

    public void InitializeSubproblems(GameObject excludeParent, GameObject includeParent)
    {
        if (excludeParent != null)
        {
            this.excludeParent = excludeParent.GetComponent<Element>();
        }
        if (includeParent != null)
        {
            this.includeParent = includeParent.GetComponent<Element>();
        }
    }

    public void ShowArrows()
    {
        if (arrow != null)
        {
            arrow.SetActive(true);

            if (parent != null)
            {
                parent.ShowArrows();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dPMatrixBuilder.HideAllArrows();
        dPMatrixBuilder.ShowElementPopup(nval, bval);
        if (parent != null)
        {
            ShowArrows();
            parent.ShowArrows();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Called when the mouse pointer exits the object
        dPMatrixBuilder.HideAllArrows();
    }

    public void ShowSubproblemArrows()
    {
        dPMatrixBuilder.ShowSubproblemArrows(this, includeParent, excludeParent);
    }

    public bool IsBaseCase()
    {
        return includeParent == null && excludeParent == null;
    }
}