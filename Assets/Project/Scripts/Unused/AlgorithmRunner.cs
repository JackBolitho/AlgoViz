using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class AlgorithmRunner : MonoBehaviour
{
    [SerializeField] private List<int> A = new List<int>();
    [SerializeField] private int B;
    private TreeBuilder treeBuilder;
    private DPMatrixBuilder dPMatrixBuilder;
    private Camera mainCamera;
    private PlayerControls controls;

    //takes care of camera zooming
    private InputAction zoomCamera;
    private float targetZoom;
    private float zoomFactor = 0.004f;
    private float zoomLerpSpeed = 20f;
    private float minZoom = 3f;
    private float maxZoom = 15f;
    
    void OnEnable(){
        controls.Enable();
    }

    void OnDisable(){
        controls.Disable();
    }

    void Awake()
    {
        controls = new PlayerControls();
        controls.Editor.LeftClick.started += OnLeftClick;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        targetZoom = mainCamera.orthographicSize;
        zoomCamera = controls.Editor.ZoomCamera;
    }

    // Start is called before the first frame update
    void Start()
    {
        dPMatrixBuilder = GetComponent<DPMatrixBuilder>();
        dPMatrixBuilder.CreateMatrix(A, B, Vector3.zero);

        treeBuilder = GetComponent<TreeBuilder>();
        treeBuilder.CreateTree(A, B, Vector3.zero);
    }

    //input: A is a list of integers, B is the target value
    //output: true if there exists a subset of A that sums to B, false if otherwise
    private int[,] SubsetSum(List<int> A, int B)
    {
        int[,] F = new int[A.Count + 1, B + 1];

        //base cases
        for (int b = 0; b < B + 1; b++)
        {
            F[0, b] = 0;
        }
        for (int m = 0; m < A.Count + 1; m++)
        {
            F[m, 0] = 1;
        }

        //recurrence
        for (int m = 1; m < A.Count + 1; m++)
        {
            for (int b = 1; b < B + 1; b++)
            {
                if (A[m - 1] > b)
                {
                    F[m, b] = F[m - 1, b];
                }
                else
                {
                    F[m, b] = Mathf.Max(F[m - 1, b], F[m - 1, b - A[m - 1]]);
                }
            }
        }

        return F;
    }

    //input: the DP matrix F from subset sum, the nth value (1 indexed)
    //output: the subset that sums to b
    private List<int> SubsetSumRetrieval(int[,] F, List<int> A, int n, int B)
    {
        //check if given subset sums to B
        if (F[n, B] == 0)
        {
            return null;
        }

        //retrieval algorithm
        int m = n;
        int b = B;
        List<int> S = new List<int>();
        while (m > 0)
        {
            if (F[m - 1, b] == 1)
            {
                m -= 1;
            }
            else
            {
                S.Add(A[m - 1]);
                b -= A[m - 1];
                m -= 1;
            }
        }
        return S;
    }


    private void OnLeftClick(InputAction.CallbackContext context)
    {
        //get world position relative to mouse
        Vector2 mousePos = context.ReadValue<Vector2>();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        StartDragScreen(worldPos);
    }

    private Vector2 cameraOrigin;
    private bool draggingScreen = false;
    private void StartDragScreen(Vector2 worldPos)
    {
        cameraOrigin = worldPos;
        draggingScreen = true;
    }

    private void EndDragScreen()
    {
        draggingScreen = false;
    }

    void Update()
    {
        if (draggingScreen)
        {
            if (controls.Editor.LeftClick.WasReleasedThisFrame())
            {
                EndDragScreen();
            }
            else
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(controls.Editor.LeftClick.ReadValue<Vector2>());
                Vector2 difference = new Vector2(worldPos.x - mainCamera.transform.position.x, worldPos.y - mainCamera.transform.position.y);
                mainCamera.transform.position = new Vector3(cameraOrigin.x - difference.x, cameraOrigin.y - difference.y, mainCamera.transform.position.z);
            }
        }

        //takes care of scrolling
        float scrollData = zoomCamera.ReadValue<float>();
        targetZoom -= scrollData * zoomFactor;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * zoomLerpSpeed);
    }
}
