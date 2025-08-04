using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple input manager to handle basic user input for the virtual chemistry lab.
/// This is a minimal implementation for basic interaction.
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool enableMouseInput = true;
    [SerializeField] private bool enableKeyboardInput = true;
    [SerializeField] private bool enableTouchInput = true;
    [SerializeField] private float mouseSensitivity = 1f;
    
    [Header("Camera Control")]
    [SerializeField] private bool enableCameraControl = true;
    [SerializeField] private float cameraMoveSpeed = 5f;
    [SerializeField] private float cameraZoomSpeed = 2f;
    
    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 10f;
    [SerializeField] private LayerMask interactableLayers = -1;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private Camera mainCamera;
    private GameObject hoveredObject;
    private GameObject selectedObject;
    private bool isDragging = false;
    
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("InputManager");
                    instance = go.AddComponent<InputManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInputManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupCamera();
    }
    
    private void Update()
    {
        if (enableMouseInput)
        {
            HandleMouseInput();
        }
        
        if (enableKeyboardInput)
        {
            HandleKeyboardInput();
        }
        
        if (enableCameraControl)
        {
            HandleCameraControl();
        }
    }
    
    /// <summary>
    /// Initializes the input manager with basic settings.
    /// </summary>
    private void InitializeInputManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("InputManager initialized successfully");
        }
        
        hoveredObject = null;
        selectedObject = null;
        isDragging = false;
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
            Debug.LogError("No camera found in scene");
        }
        else if (enableDebugLogging)
        {
            Debug.Log("Camera reference set up successfully");
        }
    }
    
    /// <summary>
    /// Handles mouse input for interaction and selection.
    /// </summary>
    private void HandleMouseInput()
    {
        Vector3 mousePosition = Input.mousePosition;
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick(mousePosition);
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            HandleMouseRelease(mousePosition);
        }
        
        HandleMouseHover(mousePosition);
        
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollWheel) > 0.01f)
        {
            HandleMouseScroll(scrollWheel);
        }
    }
    
    /// <summary>
    /// Handles keyboard input for shortcuts and commands.
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpaceKey();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            HandleRKey();
        }
    }
    
    /// <summary>
    /// Handles camera movement and rotation.
    /// </summary>
    private void HandleCameraControl()
    {
        if (mainCamera == null) return;
        
        Vector3 cameraMovement = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W))
            cameraMovement += mainCamera.transform.forward;
        if (Input.GetKey(KeyCode.S))
            cameraMovement -= mainCamera.transform.forward;
        if (Input.GetKey(KeyCode.A))
            cameraMovement -= mainCamera.transform.right;
        if (Input.GetKey(KeyCode.D))
            cameraMovement += mainCamera.transform.right;
        
        if (cameraMovement.magnitude > 0)
        {
            mainCamera.transform.position += cameraMovement * cameraMoveSpeed * Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Handles mouse click events.
    /// </summary>
    private void HandleMouseClick(Vector3 mousePosition)
    {
        GameObject clickedObject = GetObjectAtMousePosition(mousePosition);
        
        if (clickedObject != null)
        {
            selectedObject = clickedObject;
            isDragging = true;
            
            if (enableDebugLogging)
            {
                Debug.Log($"Clicked on: {clickedObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Handles mouse release events.
    /// </summary>
    private void HandleMouseRelease(Vector3 mousePosition)
    {
        if (selectedObject != null)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Released: {selectedObject.name}");
            }
        }
        
        selectedObject = null;
        isDragging = false;
    }
    
    /// <summary>
    /// Handles mouse hover events.
    /// </summary>
    private void HandleMouseHover(Vector3 mousePosition)
    {
        GameObject newHoveredObject = GetObjectAtMousePosition(mousePosition);
        
        if (newHoveredObject != hoveredObject)
        {
            hoveredObject = newHoveredObject;
            
            if (hoveredObject != null && enableDebugLogging)
            {
                Debug.Log($"Hovering: {hoveredObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Handles mouse scroll wheel events.
    /// </summary>
    private void HandleMouseScroll(float scrollValue)
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position += mainCamera.transform.forward * scrollValue * cameraZoomSpeed;
        }
    }
    
    /// <summary>
    /// Handles escape key press.
    /// </summary>
    private void HandleEscapeKey()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }
    
    /// <summary>
    /// Handles space key press.
    /// </summary>
    private void HandleSpaceKey()
    {
        if (ExperimentManager.Instance != null)
        {
            ExperimentManager.Instance.ValidateCurrentStep();
        }
    }
    
    /// <summary>
    /// Handles R key press.
    /// </summary>
    private void HandleRKey()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.ReloadCurrentScene();
        }
    }
    
    /// <summary>
    /// Gets the object at the specified mouse position using raycasting.
    /// </summary>
    private GameObject GetObjectAtMousePosition(Vector3 mousePosition)
    {
        if (mainCamera == null) return null;
        
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayers))
        {
            return hit.collider.gameObject;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the currently hovered object.
    /// </summary>
    public GameObject GetHoveredObject()
    {
        return hoveredObject;
    }
    
    /// <summary>
    /// Gets the currently selected object.
    /// </summary>
    public GameObject GetSelectedObject()
    {
        return selectedObject;
    }
    
    /// <summary>
    /// Checks if the user is currently dragging.
    /// </summary>
    public bool IsDragging()
    {
        return isDragging;
    }
    
    /// <summary>
    /// Logs the current input status.
    /// </summary>
    public void LogInputStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Input Manager Status ===");
        Debug.Log($"Mouse Input: {(enableMouseInput ? "Enabled" : "Disabled")}");
        Debug.Log($"Keyboard Input: {(enableKeyboardInput ? "Enabled" : "Disabled")}");
        Debug.Log($"Camera Control: {(enableCameraControl ? "Enabled" : "Disabled")}");
        Debug.Log($"Hovered Object: {(hoveredObject != null ? hoveredObject.name : "None")}");
        Debug.Log($"Selected Object: {(selectedObject != null ? selectedObject.name : "None")}");
        Debug.Log("===========================");
    }
} 