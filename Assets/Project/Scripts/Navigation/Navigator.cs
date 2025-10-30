using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Navigator : MonoBehaviour
{
    //camera zoom attributes
    [SerializeField] private float zoomFactor = 0.004f;
    [SerializeField] private float zoomLerpSpeed = 20f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;

    private Camera mainCamera;
    private PlayerControls controls;
    private GameObject canvas;

    //takes care of camera zooming
    private InputAction zoomCamera;
    private float targetZoom;
    private Vector2 cameraOrigin;
    private bool draggingScreen = false;
    private Vector2 draggingObjectOffset;
    private GameObject draggingObject = null;

    //handles creation menu, which shows possible 
    [SerializeField] private GameObject creationMenuPrefab;
    private GameObject currentCreationMenu = null;

    //deals with UI
    private EventSystem eventSystem;
    private PointerEventData pointerEventData;
    private GameObject visualizationParent;

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Awake()
    {
        controls = new PlayerControls();
        controls.Editor.LeftClick.started += OnLeftClick;
        controls.Editor.RightClick.started += OnRightClick;
        controls.Editor.Restart.started += Restart;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        targetZoom = mainCamera.orthographicSize;
        zoomCamera = controls.Editor.ZoomCamera;
        canvas = GameObject.Find("WorldCanvas");

        visualizationParent = GameObject.Find("Visualizations");
        eventSystem = EventSystem.current;
    }

    //left click to start drag
    private void OnLeftClick(InputAction.CallbackContext context)
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        // Gather all raycast results from all canvases
        List<RaycastResult> results = new List<RaycastResult>();

        // Loop through all active GraphicRaycastersvar 
        var raycasters = FindObjectsOfType<GraphicRaycaster>()
    .OrderByDescending(rc => rc.GetComponentInParent<Canvas>().sortingOrder)
    .ToList();

        foreach (GraphicRaycaster rc in raycasters)
        {
            List<RaycastResult> tempResults = new List<RaycastResult>();
            rc.Raycast(pointerEventData, tempResults);
            results.AddRange(tempResults);
        }

        // Sort results by distance (important!)
        results.Sort((r1, r2) => r1.distance.CompareTo(r2.distance));

        // Get world position relative to mouse
        Vector2 mousePos = context.ReadValue<Vector2>();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        if (results.Count > 0)
        {
            GameObject hitObj = results[0].gameObject;

            if (hitObj.name.Contains("Draggable"))
            {
                draggingObject = hitObj.transform.parent.gameObject;
                draggingObjectOffset = new Vector2(
                    draggingObject.transform.position.x - worldPos.x,
                    draggingObject.transform.position.y - worldPos.y
                );

                DrawPanelFirst(draggingObject);
            }
        }
        else
        {
            StartDragScreen(worldPos);
        }
    }
    
    public void DrawPanelFirst(GameObject panel)
    {
        //get canvas of visualization anchor
        panel.transform.parent.SetAsLastSibling();

        //iterate through every canvas
        foreach (Canvas canvas in visualizationParent.GetComponentsInChildren<Canvas>())
        {
            canvas.sortingOrder = canvas.transform.GetSiblingIndex();

            //set all the arrows to the correct sort order
            foreach (Arrow arrow in canvas.GetComponentsInChildren<Arrow>())
            {
                arrow.SetSortOrder(canvas.transform.GetSiblingIndex() + 1);
            }
        }
    }

    //right click to open creation menu
    private void OnRightClick(InputAction.CallbackContext context)
    {
        //get world position relative to mouse
        Vector2 mousePos = context.ReadValue<Vector2>();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        //create creation menu
        NullifyCurrentCreationMenu();
        currentCreationMenu = Instantiate(creationMenuPrefab, visualizationParent.transform);
        currentCreationMenu.transform.position = worldPos;
        DrawPanelFirst(currentCreationMenu.transform.GetChild(0).gameObject);
    }

    private void StartDragScreen(Vector2 worldPos)
    {
        cameraOrigin = worldPos;
        draggingScreen = true;
    }

    private void EndDragScreen()
    {
        draggingScreen = false;
        draggingObject = null;
    }

    private void Update()
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
        else if(draggingObject != null)
        {
            if (controls.Editor.LeftClick.WasReleasedThisFrame())
            {
                EndDragScreen();
            }
            else
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(controls.Editor.LeftClick.ReadValue<Vector2>());
                draggingObject.transform.position = worldPos + draggingObjectOffset;
            }
        }

        //takes care of scrolling
        float scrollData = zoomCamera.ReadValue<float>();
        targetZoom -= scrollData * zoomFactor;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * zoomLerpSpeed);
    }

    private void Restart(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //activated by the exit button in the creation menu, or when the creation menu makes an algorithm visualization
    public void NullifyCurrentCreationMenu()
    {
        if (currentCreationMenu != null)
        {
            Destroy(currentCreationMenu);
        }
        currentCreationMenu = null;
    }
}
