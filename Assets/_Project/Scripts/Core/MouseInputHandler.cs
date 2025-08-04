using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles mouse input for 3D object selection, camera control, and laboratory interactions.
/// This component manages all mouse-based interactions in the virtual chemistry lab.
/// </summary>
public class MouseInputHandler : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1.0f;
    [SerializeField] private float scrollSensitivity = 2.0f;
    [SerializeField] private float dragThreshold = 0.1f;
    [SerializeField] private LayerMask selectableLayerMask = -1;
    [SerializeField] private LayerMask uiLayerMask = 5; // UI layer
    
    [Header("Selection")]
    [SerializeField] private float maxSelectionDistance = 100f;
    [SerializeField] private bool enableObjectHighlighting = true;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightIntensity = 0.3f;
    
    [Header("Camera Control")]
    [SerializeField] private bool enableCameraControl = true;
    [SerializeField] private float cameraMoveSpeed = 5f;
    [SerializeField] private float cameraRotateSpeed = 100f;
    [SerializeField] private float cameraZoomSpeed = 10f;
    [SerializeField] private Vector2 cameraZoomLimits = new Vector2(2f, 20f);
    
    [Header("Interaction")]
    [SerializeField] private bool enableDragAndDrop = true;
    [SerializeField] private float dragStartThreshold = 0.1f;
    [SerializeField] private bool enableRightClickMenu = true;
    [SerializeField] private float doubleClickTime = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showSelectionRay = false;
    [SerializeField] private bool showMousePosition = false;
    
    private static MouseInputHandler instance;
    public static MouseInputHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MouseInputHandler>();
                if (instance == null)
                {
                    GameObject go = new GameObject("MouseInputHandler");
                    instance = go.AddComponent<MouseInputHandler>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<GameObject> OnObjectSelected;
    public event Action<GameObject> OnObjectDeselected;
    public event Action<GameObject> OnObjectClicked;
    public event Action<GameObject> OnObjectDoubleClicked;
    public event Action<GameObject> OnObjectRightClicked;
    public event Action<Vector3> OnMousePositionChanged;
    public event Action<Vector2> OnMouseDrag;
    public event Action<float> OnMouseScroll;
    public event Action<Vector3> OnCameraMove;
    public event Action<Vector2> OnCameraRotate;
    public event Action<float> OnCameraZoom;
    
    // Private variables
    private Camera mainCamera;
    private GameObject selectedObject;
    private GameObject highlightedObject;
    private GameObject draggedObject;
    private Vector3 lastMousePosition;
    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private bool isCameraMoving = false;
    private bool isCameraRotating = false;
    private float lastClickTime = 0f;
    private Material originalMaterial;
    private Material highlightMaterial;
    private RaycastHit[] raycastHits = new RaycastHit[10];
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMouseHandler();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupCamera();
        CreateHighlightMaterial();
    }
    
    private void Update()
    {
        if (mainCamera == null)
        {
            SetupCamera();
            return;
        }
        
        HandleMouseInput();
        UpdateMousePosition();
        HandleObjectHighlighting();
    }
    
    /// <summary>
    /// Initializes the mouse input handler.
    /// </summary>
    private void InitializeMouseHandler()
    {
        lastMousePosition = Input.mousePosition;
        
        if (enableDebugLogging)
        {
            Debug.Log("MouseInputHandler initialized successfully");
        }
    }
    
    /// <summary>
    /// Sets up the main camera reference.
    /// </summary>
    private void SetupCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("No camera found for MouseInputHandler");
        }
    }
    
    /// <summary>
    /// Creates the highlight material for object selection.
    /// </summary>
    private void CreateHighlightMaterial()
    {
        if (enableObjectHighlighting)
        {
            highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = highlightColor;
            highlightMaterial.SetFloat("_Metallic", 0f);
            highlightMaterial.SetFloat("_Glossiness", 0.5f);
        }
    }
    
    /// <summary>
    /// Handles all mouse input processing.
    /// </summary>
    private void HandleMouseInput()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;
        
        // Handle mouse clicks
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            HandleLeftClick();
        }
        else if (Input.GetMouseButtonDown(1)) // Right click
        {
            HandleRightClick();
        }
        
        // Handle mouse drag
        if (Input.GetMouseButton(0))
        {
            HandleLeftDrag(mouseDelta);
        }
        else if (Input.GetMouseButton(1))
        {
            HandleRightDrag(mouseDelta);
        }
        
        // Handle mouse release
        if (Input.GetMouseButtonUp(0))
        {
            HandleLeftRelease();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            HandleRightRelease();
        }
        
        // Handle mouse scroll
        if (Input.mouseScrollDelta.y != 0)
        {
            HandleMouseScroll(Input.mouseScrollDelta.y);
        }
        
        lastMousePosition = currentMousePosition;
    }
    
    /// <summary>
    /// Handles left mouse button click.
    /// </summary>
    private void HandleLeftClick()
    {
        GameObject clickedObject = GetObjectUnderMouse();
        
        if (clickedObject != null)
        {
            // Check for double click
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick < doubleClickTime)
            {
                OnObjectDoubleClicked?.Invoke(clickedObject);
                if (enableDebugLogging)
                {
                    Debug.Log($"Double clicked object: {clickedObject.name}");
                }
            }
            else
            {
                SelectObject(clickedObject);
                OnObjectClicked?.Invoke(clickedObject);
                if (enableDebugLogging)
                {
                    Debug.Log($"Clicked object: {clickedObject.name}");
                }
            }
            
            lastClickTime = Time.time;
            
            // Start drag if object is draggable
            if (enableDragAndDrop && IsDraggable(clickedObject))
            {
                StartDrag(clickedObject);
            }
        }
        else
        {
            DeselectObject();
        }
    }
    
    /// <summary>
    /// Handles right mouse button click.
    /// </summary>
    private void HandleRightClick()
    {
        GameObject clickedObject = GetObjectUnderMouse();
        
        if (clickedObject != null && enableRightClickMenu)
        {
            OnObjectRightClicked?.Invoke(clickedObject);
            if (enableDebugLogging)
            {
                Debug.Log($"Right clicked object: {clickedObject.name}");
            }
        }
        
        // Start camera rotation
        if (enableCameraControl)
        {
            isCameraRotating = true;
        }
    }
    
    /// <summary>
    /// Handles left mouse button drag.
    /// </summary>
    private void HandleLeftDrag(Vector3 mouseDelta)
    {
        if (isDragging && draggedObject != null)
        {
            // Handle object dragging
            Vector3 worldDelta = mainCamera.ScreenToWorldPoint(mouseDelta) - mainCamera.ScreenToWorldPoint(Vector3.zero);
            draggedObject.transform.position += worldDelta * mouseSensitivity;
            
            OnMouseDrag?.Invoke(mouseDelta);
        }
        else if (enableCameraControl && Input.GetKey(KeyCode.LeftControl))
        {
            // Handle camera movement with Ctrl+Left drag
            isCameraMoving = true;
            Vector3 moveDirection = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * cameraMoveSpeed * Time.deltaTime;
            mainCamera.transform.Translate(moveDirection, Space.Self);
            OnCameraMove?.Invoke(moveDirection);
        }
    }
    
    /// <summary>
    /// Handles right mouse button drag.
    /// </summary>
    private void HandleRightDrag(Vector3 mouseDelta)
    {
        if (enableCameraControl && isCameraRotating)
        {
            // Handle camera rotation
            Vector2 rotation = new Vector2(-mouseDelta.y, mouseDelta.x) * cameraRotateSpeed * Time.deltaTime;
            mainCamera.transform.Rotate(rotation.x, rotation.y, 0, Space.Self);
            
            // Clamp vertical rotation
            Vector3 eulerAngles = mainCamera.transform.eulerAngles;
            if (eulerAngles.x > 180f)
            {
                eulerAngles.x -= 360f;
            }
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, -80f, 80f);
            mainCamera.transform.eulerAngles = eulerAngles;
            
            OnCameraRotate?.Invoke(rotation);
        }
    }
    
    /// <summary>
    /// Handles left mouse button release.
    /// </summary>
    private void HandleLeftRelease()
    {
        if (isDragging)
        {
            EndDrag();
        }
        
        if (isCameraMoving)
        {
            isCameraMoving = false;
        }
    }
    
    /// <summary>
    /// Handles right mouse button release.
    /// </summary>
    private void HandleRightRelease()
    {
        if (isCameraRotating)
        {
            isCameraRotating = false;
        }
    }
    
    /// <summary>
    /// Handles mouse scroll wheel.
    /// </summary>
    private void HandleMouseScroll(float scrollDelta)
    {
        if (enableCameraControl)
        {
            float zoomAmount = scrollDelta * scrollSensitivity * cameraZoomSpeed * Time.deltaTime;
            Vector3 zoomDirection = mainCamera.transform.forward * zoomAmount;
            mainCamera.transform.position += zoomDirection;
            
            // Clamp zoom distance
            float distance = Vector3.Distance(mainCamera.transform.position, Vector3.zero);
            if (distance < cameraZoomLimits.x)
            {
                mainCamera.transform.position = Vector3.zero + mainCamera.transform.forward * cameraZoomLimits.x;
            }
            else if (distance > cameraZoomLimits.y)
            {
                mainCamera.transform.position = Vector3.zero + mainCamera.transform.forward * cameraZoomLimits.y;
            }
            
            OnCameraZoom?.Invoke(zoomAmount);
        }
        
        OnMouseScroll?.Invoke(scrollDelta);
    }
    
    /// <summary>
    /// Gets the object under the mouse cursor.
    /// </summary>
    private GameObject GetObjectUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // First check UI elements
        int uiHits = Physics.RaycastNonAlloc(ray, raycastHits, maxSelectionDistance, uiLayerMask);
        if (uiHits > 0)
        {
            return raycastHits[0].collider.gameObject;
        }
        
        // Then check selectable objects
        int objectHits = Physics.RaycastNonAlloc(ray, raycastHits, maxSelectionDistance, selectableLayerMask);
        if (objectHits > 0)
        {
            if (showSelectionRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * maxSelectionDistance, Color.red);
            }
            return raycastHits[0].collider.gameObject;
        }
        
        return null;
    }
    
    /// <summary>
    /// Selects an object.
    /// </summary>
    private void SelectObject(GameObject obj)
    {
        if (selectedObject != obj)
        {
            DeselectObject();
            selectedObject = obj;
            OnObjectSelected?.Invoke(obj);
        }
    }
    
    /// <summary>
    /// Deselects the currently selected object.
    /// </summary>
    private void DeselectObject()
    {
        if (selectedObject != null)
        {
            GameObject previousObject = selectedObject;
            selectedObject = null;
            OnObjectDeselected?.Invoke(previousObject);
        }
    }
    
    /// <summary>
    /// Starts dragging an object.
    /// </summary>
    private void StartDrag(GameObject obj)
    {
        if (enableDragAndDrop && IsDraggable(obj))
        {
            draggedObject = obj;
            isDragging = true;
            dragStartPosition = obj.transform.position;
            
            if (enableDebugLogging)
            {
                Debug.Log($"Started dragging: {obj.name}");
            }
        }
    }
    
    /// <summary>
    /// Ends the current drag operation.
    /// </summary>
    private void EndDrag()
    {
        if (isDragging && draggedObject != null)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Ended dragging: {draggedObject.name}");
            }
            
            draggedObject = null;
            isDragging = false;
        }
    }
    
    /// <summary>
    /// Checks if an object is draggable.
    /// </summary>
    private bool IsDraggable(GameObject obj)
    {
        // Check if object has a Draggable component or specific tag
        return obj.CompareTag("Draggable") || obj.GetComponent<Draggable>() != null;
    }
    
    /// <summary>
    /// Updates mouse position tracking.
    /// </summary>
    private void UpdateMousePosition()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        OnMousePositionChanged?.Invoke(worldPosition);
        
        if (showMousePosition)
        {
            Debug.Log($"Mouse World Position: {worldPosition}");
        }
    }
    
    /// <summary>
    /// Handles object highlighting on hover.
    /// </summary>
    private void HandleObjectHighlighting()
    {
        if (!enableObjectHighlighting) return;
        
        GameObject hoveredObject = GetObjectUnderMouse();
        
        if (hoveredObject != highlightedObject)
        {
            // Remove highlight from previous object
            if (highlightedObject != null && highlightedObject != selectedObject)
            {
                RemoveHighlight(highlightedObject);
            }
            
            // Add highlight to new object
            if (hoveredObject != null && hoveredObject != selectedObject && IsHighlightable(hoveredObject))
            {
                AddHighlight(hoveredObject);
            }
            
            highlightedObject = hoveredObject;
        }
    }
    
    /// <summary>
    /// Checks if an object can be highlighted.
    /// </summary>
    private bool IsHighlightable(GameObject obj)
    {
        return obj.CompareTag("Highlightable") || obj.GetComponent<Renderer>() != null;
    }
    
    /// <summary>
    /// Adds highlight to an object.
    /// </summary>
    private void AddHighlight(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            originalMaterial = renderer.material;
            renderer.material = highlightMaterial;
        }
    }
    
    /// <summary>
    /// Removes highlight from an object.
    /// </summary>
    private void RemoveHighlight(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
        }
    }
    
    // Public getters and setters
    public GameObject GetSelectedObject() => selectedObject;
    public GameObject GetHighlightedObject() => highlightedObject;
    public bool IsDragging() => isDragging;
    public bool IsCameraMoving() => isCameraMoving;
    public bool IsCameraRotating() => isCameraRotating;
    
    /// <summary>
    /// Sets mouse sensitivity.
    /// </summary>
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
    }
    
    /// <summary>
    /// Sets camera control enabled state.
    /// </summary>
    public void SetCameraControlEnabled(bool enabled)
    {
        enableCameraControl = enabled;
    }
    
    /// <summary>
    /// Sets drag and drop enabled state.
    /// </summary>
    public void SetDragAndDropEnabled(bool enabled)
    {
        enableDragAndDrop = enabled;
    }
    
    /// <summary>
    /// Logs the current mouse input status.
    /// </summary>
    public void LogMouseStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Mouse Input Handler Status ===");
        Debug.Log($"Selected Object: {(selectedObject ? selectedObject.name : "None")}");
        Debug.Log($"Highlighted Object: {(highlightedObject ? highlightedObject.name : "None")}");
        Debug.Log($"Dragged Object: {(draggedObject ? draggedObject.name : "None")}");
        Debug.Log($"Is Dragging: {isDragging}");
        Debug.Log($"Is Camera Moving: {isCameraMoving}");
        Debug.Log($"Is Camera Rotating: {isCameraRotating}");
        Debug.Log($"Mouse Sensitivity: {mouseSensitivity}");
        Debug.Log($"Camera Control: {(enableCameraControl ? "Enabled" : "Disabled")}");
        Debug.Log($"Drag and Drop: {(enableDragAndDrop ? "Enabled" : "Disabled")}");
        Debug.Log("===================================");
    }
}

/// <summary>
/// Component to mark objects as draggable.
/// </summary>
public class Draggable : MonoBehaviour
{
    [SerializeField] private bool isDraggable = true;
    [SerializeField] private Vector3 dragConstraints = Vector3.one;
    [SerializeField] private float dragSpeed = 1f;
    
    public bool IsDraggable => isDraggable;
    public Vector3 DragConstraints => dragConstraints;
    public float DragSpeed => dragSpeed;
} 