using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Element : MonoBehaviour, IPointerEnterHandler
{
    private DPMatrixBuilder dPMatrixBuilder;
    private GameObject arrow;
    private Element parent;
    public Element excludeChild {private set; get;}
    public Element includeChild {private set; get;}

    //element values
    public int nval { private set; get; }
    public int bval { private set; get; }
    public int val { private set; get; }

    public void SetDPMatrixBuilder(DPMatrixBuilder dPMatrixBuilder)
    {
        this.dPMatrixBuilder = dPMatrixBuilder;
    }

    public void SetElementValues(int nval, int bval, int val)
    {
        this.nval = nval;
        this.bval = bval;
        this.val = val;
    }

    public void InitializeArrows(GameObject parent, GameObject arrow)
    {
        if (parent != null)
        {
            this.parent = parent.GetComponent<Element>();
            this.arrow = arrow;
        }
    }

    public void InitializeSubproblems(GameObject excludeChild, GameObject includeChild)
    {
        if (excludeChild != null)
        {
            this.excludeChild = excludeChild.GetComponent<Element>();
        }
        if (includeChild != null)
        {
            this.includeChild = includeChild.GetComponent<Element>();
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
        dPMatrixBuilder.ShowElementPopup(nval, bval);
        dPMatrixBuilder.HighlightLabels(nval, bval);
    }

    public void OnElementClick()
    {
        EventSystem.current.SetSelectedGameObject(null);
        dPMatrixBuilder.ShowSubproblemArrows(this, includeChild, excludeChild);
        dPMatrixBuilder.OnElementClick(this);
    }

    public bool IsBaseCase()
    {
        return includeChild == null && excludeChild == null;
    }
}