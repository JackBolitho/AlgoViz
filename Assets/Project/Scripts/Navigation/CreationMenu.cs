using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreationMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField arrayInput;
    [SerializeField] private TMP_InputField goalInput;
    [SerializeField] private TMP_Dropdown visualizationTypeInput;

    //visualization components
    private Navigator navigator;
    [SerializeField] private GameObject dragableBackdrop;
    [SerializeField] private GameObject dPMatrixBuilderPrefab;
    [SerializeField] private GameObject treeBuilderPrefab;
    private GameObject worldCanvas;

    // Start is called before the first frame update
    void Awake()
    {
        navigator = GameObject.Find("Navigator").GetComponent<Navigator>();
        worldCanvas = GameObject.Find("WorldCanvas");
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
        if (arrayValues.Count == 0)
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
        if (goalValue < 0)
        {
            return;
        }

        //get dropdown input
        int selectedIndex = visualizationTypeInput.value;
        string selectedText = visualizationTypeInput.options[selectedIndex].text;

        switch (selectedText)
        {
            case "DP Matrix":
                GameObject dPObj = Instantiate(dPMatrixBuilderPrefab, worldCanvas.transform);
                DPMatrixBuilder dPMatrixBuilder = dPObj.GetComponent<DPMatrixBuilder>();
                dPMatrixBuilder.CreateMatrix(arrayValues, goalValue, gameObject.transform.position);
                break;
            case "Decision Tree":
                GameObject treeObj = Instantiate(treeBuilderPrefab, worldCanvas.transform);
                TreeBuilder treeBuilder = treeObj.GetComponent<TreeBuilder>();
                treeBuilder.CreateTree(arrayValues, goalValue, gameObject.transform.position);
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
