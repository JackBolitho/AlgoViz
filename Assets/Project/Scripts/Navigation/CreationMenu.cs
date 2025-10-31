using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class CreationMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField arrayInput;
    [SerializeField] private TMP_InputField goalInput;
    [SerializeField] private TMP_Dropdown visualizationTypeInput;

    //visualization components
    private Navigator navigator;
    private Animator animator;
    [SerializeField] private GameObject dragableBackdrop;
    [SerializeField] private GameObject dPMatrixBuilderPrefab;
    [SerializeField] private GameObject treeBuilderPrefab;

    //input restrictions
    [SerializeField] private int maxB;
    [SerializeField] private int maxALength;

    private GameObject visualizationParent;

    // Start is called before the first frame update
    void Awake()
    {
        navigator = GameObject.Find("Navigator").GetComponent<Navigator>();
        visualizationParent = GameObject.Find("Visualizations");
        animator = GetComponent<Animator>();
        StartCoroutine(QueueAnimation());
    }

    private IEnumerator QueueAnimation()
    {
        yield return new WaitForEndOfFrame();
        animator.SetTrigger("OnCreation");
    }

    private List<int> ParseArray(string arrayText)
    {
        List<int> array = new List<int>();

        string currVal = "";
        for (int i = 0; i < arrayText.Length; i++)
        {
            if (Char.IsDigit(arrayText[i]))
            {
                currVal += arrayText[i];
            }
            else if (currVal.Length > 0)
            {
                int newVal = int.Parse(currVal);
                array.Add(newVal);
                currVal = "";
            }
        }

        if (currVal.Length > 0)
        {
            int newVal = int.Parse(currVal);
            array.Add(newVal);
        }

        return array;
    }

    public void Visualize()
    {
        string arrayText = arrayInput.text;
        string goalText = goalInput.text;

        //try to get array values
        List<int> arrayValues = ParseArray(arrayText);
        if (arrayValues.Count == 0 || arrayValues.Count > maxALength)
        {
            return;
        }

        //validate that input is nonnegative
        for (int i = 0; i < arrayValues.Count; i++)
        {
            if (arrayValues[i] < 0)
            {
                return;
            }
        }

        //try to get goal value
        int goalValue;
        try
        {
            goalValue = int.Parse(goalText);
        }
        catch
        {
            return;
        }

        //validate input is nonnegative
        if (goalValue < 0 || goalValue > maxB)
        {
            return;
        }

        //get dropdown input
        int selectedIndex = visualizationTypeInput.value;
        string selectedText = visualizationTypeInput.options[selectedIndex].text;

        switch (selectedText)
        {
            case "DP Matrix":
                GameObject dPObj = Instantiate(dPMatrixBuilderPrefab, visualizationParent.transform);
                DPMatrixBuilder dPMatrixBuilder = dPObj.GetComponentInChildren<DPMatrixBuilder>();
                dPMatrixBuilder.CreateMatrix(arrayValues, goalValue, gameObject.transform.position);
                navigator.DrawPanelFirst(dPMatrixBuilder.transform.GetChild(0).gameObject);
                break;
            case "Decision Tree":
                GameObject treeObj = Instantiate(treeBuilderPrefab, visualizationParent.transform);
                TreeBuilder treeBuilder = treeObj.GetComponentInChildren<TreeBuilder>();
                treeBuilder.CreateTree(arrayValues, goalValue, gameObject.transform.position);
                navigator.DrawPanelFirst(treeBuilder.transform.GetChild(0).gameObject);
                break;
            default:
                break;
        }

        RemoveCurrentCreationMenu();
    }   
    
    //activated by the exit button in the creation menu, or when the creation menu makes an algorithm visualization
    public void RemoveCurrentCreationMenu()
    {
        navigator.NullifyCurrentCreationMenu();
        Destroy(this.gameObject);
    }
}
