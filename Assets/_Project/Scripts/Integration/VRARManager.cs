using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages VR/AR functionality for immersive virtual reality and augmented reality experiences.
/// Handles VR/AR setup, hand tracking, haptic feedback, and immersive interactions.
/// </summary>
public class VRARManager : MonoBehaviour
{
    [Header("VR/AR Settings")]
    [SerializeField] private bool enableVR = true;
    [SerializeField] private bool enableAR = true;
    [SerializeField] private bool enableHandTracking = true;
    [SerializeField] private bool enableHapticFeedback = true;
    [SerializeField] private bool enableSpatialAudio = true;
    
    [Header("VR Configuration")]
    [SerializeField] private bool enableVRControllers = true;
    [SerializeField] private bool enableVRHands = true;
    [SerializeField] private float vrInteractionDistance = 2.0f;
    [SerializeField] private bool enableVRTeleportation = true;
    [SerializeField] private bool enableVRGrab = true;
    
    [Header("AR Configuration")]
    [SerializeField] private bool enableARPlaneDetection = true;
    [SerializeField] private bool enableARObjectPlacement = true;
    [SerializeField] private bool enableARImageTracking = true;
    [SerializeField] private bool enableARFaceTracking = false;
    [SerializeField] private float arInteractionDistance = 1.0f;
    
    [Header("Hand Tracking")]
    [SerializeField] private bool enableFingerTracking = true;
    [SerializeField] private bool enableGestureRecognition = true;
    [SerializeField] private bool enableHandPhysics = true;
    [SerializeField] private float handTrackingConfidence = 0.8f;
    [SerializeField] private int maxHands = 2;
    
    [Header("Haptic Feedback")]
    [SerializeField] private bool enableControllerHaptics = true;
    [SerializeField] private bool enableHandHaptics = true;
    [SerializeField] private float hapticIntensity = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;
    [SerializeField] private HapticFeedbackType defaultHapticType = HapticFeedbackType.Medium;
    
    [Header("Spatial Audio")]
    [SerializeField] private bool enable3DAudio = true;
    [SerializeField] private bool enableAudioOcclusion = true;
    [SerializeField] private bool enableAudioReverb = true;
    [SerializeField] private float audioMaxDistance = 10.0f;
    [SerializeField] private float audioRolloff = 1.0f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showHandTracking = false;
    [SerializeField] private bool showInteractionRays = false;
    [SerializeField] private bool logVREvents = false;
    
    private static VRARManager instance;
    public static VRARManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<VRARManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("VRARManager");
                    instance = go.AddComponent<VRARManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<Vector3> OnHandPositionChanged;
    public event Action<Quaternion> OnHandRotationChanged;
    public event Action<string> OnGestureDetected;
    public event Action<GameObject> OnObjectGrabbed;
    public event Action<GameObject> OnObjectReleased;
    public event Action<Vector3> OnTeleportRequested;
    public event Action<Vector3> OnARObjectPlaced;
    public event Action<bool> OnVRModeChanged;
    public event Action<bool> OnARModeChanged;
    
    [System.Serializable]
    public class HandData
    {
        public int handId;
        public bool isTracked;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3[] fingerPositions;
        public float[] fingerBends;
        public float confidence;
        public HandGesture currentGesture;
        public bool isGrabbing;
        public GameObject grabbedObject;
        
        public HandData(int id)
        {
            handId = id;
            isTracked = false;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            fingerPositions = new Vector3[21]; // 21 hand landmarks
            fingerBends = new float[5]; // 5 fingers
            confidence = 0f;
            currentGesture = HandGesture.None;
            isGrabbing = false;
            grabbedObject = null;
        }
    }
    
    [System.Serializable]
    public class VRControllerData
    {
        public int controllerId;
        public bool isConnected;
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 thumbstick;
        public float triggerValue;
        public bool triggerPressed;
        public bool gripPressed;
        public bool menuPressed;
        public Vector2 trackpad;
        
        public VRControllerData(int id)
        {
            controllerId = id;
            isConnected = false;
            position = Vector3.zero;
            rotation = Quaternion.identity;
            thumbstick = Vector2.zero;
            triggerValue = 0f;
            triggerPressed = false;
            gripPressed = false;
            menuPressed = false;
            trackpad = Vector2.zero;
        }
    }
    
    [System.Serializable]
    public class ARPlaneData
    {
        public string planeId;
        public Vector3 center;
        public Vector3 size;
        public Quaternion rotation;
        public ARPlaneType planeType;
        public bool isTracked;
        
        public ARPlaneData(string id)
        {
            planeId = id;
            center = Vector3.zero;
            size = Vector3.zero;
            rotation = Quaternion.identity;
            planeType = ARPlaneType.Horizontal;
            isTracked = false;
        }
    }
    
    public enum HandGesture
    {
        None,
        Point,
        Grab,
        Pinch,
        ThumbsUp,
        ThumbsDown,
        Open,
        Closed,
        Custom
    }
    
    public enum ARPlaneType
    {
        Horizontal,
        Vertical,
        Diagonal
    }
    
    public enum HapticFeedbackType
    {
        Light,
        Medium,
        Heavy,
        Continuous
    }
    
    private Dictionary<int, HandData> trackedHands;
    private Dictionary<int, VRControllerData> vrControllers;
    private Dictionary<string, ARPlaneData> arPlanes;
    private bool isVRMode;
    private bool isARMode;
    private Camera vrCamera;
    private Camera arCamera;
    private GameObject vrRig;
    private GameObject arSession;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVRARManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableVR || enableAR)
        {
            DetectVRARSupport();
            SetupVRAR();
        }
    }
    
    private void Update()
    {
        if (isVRMode)
        {
            UpdateVRInput();
        }
        
        if (isARMode)
        {
            UpdateARInput();
        }
        
        if (enableHandTracking)
        {
            UpdateHandTracking();
        }
    }
    
    /// <summary>
    /// Initializes the VR/AR manager with basic settings.
    /// </summary>
    private void InitializeVRARManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("VRARManager initialized successfully");
        }
        
        // Initialize collections
        trackedHands = new Dictionary<int, HandData>();
        vrControllers = new Dictionary<int, VRControllerData>();
        arPlanes = new Dictionary<string, ARPlaneData>();
        
        // Initialize hand tracking
        for (int i = 0; i < maxHands; i++)
        {
            trackedHands[i] = new HandData(i);
        }
        
        // Initialize VR controllers
        vrControllers[0] = new VRControllerData(0); // Left controller
        vrControllers[1] = new VRControllerData(1); // Right controller
    }
    
    /// <summary>
    /// Detects VR/AR support on the current device.
    /// </summary>
    private void DetectVRARSupport()
    {
        // Check VR support
        bool vrSupported = UnityEngine.XR.XRSettings.isDeviceActive;
        
        // Check AR support (this would integrate with AR Foundation)
        bool arSupported = false; // Placeholder for AR detection
        
        if (enableDebugLogging)
        {
            Debug.Log($"VR Supported: {vrSupported}");
            Debug.Log($"AR Supported: {arSupported}");
        }
    }
    
    /// <summary>
    /// Sets up VR/AR functionality.
    /// </summary>
    private void SetupVRAR()
    {
        if (enableVR)
        {
            SetupVR();
        }
        
        if (enableAR)
        {
            SetupAR();
        }
    }
    
    /// <summary>
    /// Sets up VR functionality.
    /// </summary>
    private void SetupVR()
    {
        // Create VR rig
        vrRig = new GameObject("VR Rig");
        vrRig.transform.SetParent(transform);
        
        // Add VR camera
        GameObject vrCameraObj = new GameObject("VR Camera");
        vrCameraObj.transform.SetParent(vrRig.transform);
        vrCamera = vrCameraObj.AddComponent<Camera>();
        vrCamera.nearClipPlane = 0.01f;
        vrCamera.farClipPlane = 1000f;
        
        // Setup VR controllers
        if (enableVRControllers)
        {
            SetupVRControllers();
        }
        
        isVRMode = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("VR setup completed");
        }
        
        OnVRModeChanged?.Invoke(true);
    }
    
    /// <summary>
    /// Sets up VR controllers.
    /// </summary>
    private void SetupVRControllers()
    {
        // Create controller objects
        for (int i = 0; i < 2; i++)
        {
            GameObject controllerObj = new GameObject($"VR Controller {i}");
            controllerObj.transform.SetParent(vrRig.transform);
            
            // Add controller components
            VRController controller = controllerObj.AddComponent<VRController>();
            controller.Initialize(i);
        }
    }
    
    /// <summary>
    /// Sets up AR functionality.
    /// </summary>
    private void SetupAR()
    {
        // Create AR session
        arSession = new GameObject("AR Session");
        arSession.transform.SetParent(transform);
        
        // Add AR camera
        GameObject arCameraObj = new GameObject("AR Camera");
        arCameraObj.transform.SetParent(arSession.transform);
        arCamera = arCameraObj.AddComponent<Camera>();
        arCamera.clearFlags = CameraClearFlags.SolidColor;
        arCamera.backgroundColor = Color.clear;
        
        // Setup AR plane detection
        if (enableARPlaneDetection)
        {
            SetupARPlaneDetection();
        }
        
        isARMode = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("AR setup completed");
        }
        
        OnARModeChanged?.Invoke(true);
    }
    
    /// <summary>
    /// Sets up AR plane detection.
    /// </summary>
    private void SetupARPlaneDetection()
    {
        // This would integrate with AR Foundation plane detection
        // For now, we'll simulate plane detection
        
        if (enableDebugLogging)
        {
            Debug.Log("AR plane detection setup completed");
        }
    }
    
    /// <summary>
    /// Updates VR input handling.
    /// </summary>
    private void UpdateVRInput()
    {
        // Update controller input
        for (int i = 0; i < 2; i++)
        {
            if (vrControllers.ContainsKey(i))
            {
                UpdateVRController(i);
            }
        }
        
        // Handle VR interactions
        HandleVRInteractions();
    }
    
    /// <summary>
    /// Updates a specific VR controller.
    /// </summary>
    private void UpdateVRController(int controllerId)
    {
        VRControllerData controller = vrControllers[controllerId];
        
        // Simulate controller input (this would integrate with XR input)
        controller.isConnected = true;
        
        // Update position and rotation
        controller.position = Vector3.zero; // This would get actual controller position
        controller.rotation = Quaternion.identity; // This would get actual controller rotation
        
        // Update input values
        controller.triggerValue = Input.GetAxis($"VRTrigger{controllerId}");
        controller.triggerPressed = Input.GetButtonDown($"VRTrigger{controllerId}");
        controller.gripPressed = Input.GetButtonDown($"VRGrip{controllerId}");
        controller.menuPressed = Input.GetButtonDown($"VRMenu{controllerId}");
        controller.thumbstick = new Vector2(Input.GetAxis($"VRThumbstickX{controllerId}"), 
                                          Input.GetAxis($"VRThumbstickY{controllerId}"));
        controller.trackpad = new Vector2(Input.GetAxis($"VRTrackpadX{controllerId}"), 
                                        Input.GetAxis($"VRTrackpadY{controllerId}"));
    }
    
    /// <summary>
    /// Handles VR interactions like grabbing and teleportation.
    /// </summary>
    private void HandleVRInteractions()
    {
        // Handle grabbing
        if (enableVRGrab)
        {
            HandleVRGrabbing();
        }
        
        // Handle teleportation
        if (enableVRTeleportation)
        {
            HandleVRTeleportation();
        }
    }
    
    /// <summary>
    /// Handles VR grabbing interactions.
    /// </summary>
    private void HandleVRGrabbing()
    {
        for (int i = 0; i < 2; i++)
        {
            if (vrControllers.ContainsKey(i) && vrControllers[i].triggerPressed)
            {
                // Check for grabbable objects
                RaycastHit hit;
                if (Physics.Raycast(vrControllers[i].position, vrControllers[i].rotation * Vector3.forward, 
                                   out hit, vrInteractionDistance))
                {
                    GameObject grabbable = hit.collider.gameObject;
                    if (grabbable.CompareTag("Grabbable"))
                    {
                        GrabObject(grabbable, i);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Handles VR teleportation.
    /// </summary>
    private void HandleVRTeleportation()
    {
        // Check for teleport input
        if (Input.GetButtonDown("VRTeleport"))
        {
            // Cast ray to find teleport target
            RaycastHit hit;
            if (Physics.Raycast(vrControllers[1].position, vrControllers[1].rotation * Vector3.forward, 
                               out hit, 10f))
            {
                Vector3 teleportPosition = hit.point;
                OnTeleportRequested?.Invoke(teleportPosition);
                
                // Move VR rig to new position
                if (vrRig != null)
                {
                    vrRig.transform.position = teleportPosition;
                }
            }
        }
    }
    
    /// <summary>
    /// Updates AR input handling.
    /// </summary>
    private void UpdateARInput()
    {
        // Handle AR touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                HandleARTouch(touch.position);
            }
        }
        
        // Update AR plane tracking
        if (enableARPlaneDetection)
        {
            UpdateARPlanes();
        }
    }
    
    /// <summary>
    /// Handles AR touch input for object placement.
    /// </summary>
    private void HandleARTouch(Vector2 touchPosition)
    {
        if (!enableARObjectPlacement)
            return;
        
        // Cast ray from camera through touch point
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 placementPosition = hit.point;
            OnARObjectPlaced?.Invoke(placementPosition);
            
            if (enableDebugLogging)
            {
                Debug.Log($"AR object placed at: {placementPosition}");
            }
        }
    }
    
    /// <summary>
    /// Updates AR plane tracking.
    /// </summary>
    private void UpdateARPlanes()
    {
        // This would integrate with AR Foundation plane detection
        // For now, we'll simulate plane detection
        
        // Simulate detecting a horizontal plane
        if (arPlanes.Count == 0)
        {
            ARPlaneData plane = new ARPlaneData("plane_0");
            plane.center = Vector3.zero;
            plane.size = new Vector3(2f, 0.1f, 2f);
            plane.planeType = ARPlaneType.Horizontal;
            plane.isTracked = true;
            
            arPlanes[plane.planeId] = plane;
        }
    }
    
    /// <summary>
    /// Updates hand tracking.
    /// </summary>
    private void UpdateHandTracking()
    {
        if (!enableHandTracking)
            return;
        
        // Update hand positions and gestures
        for (int i = 0; i < maxHands; i++)
        {
            if (trackedHands.ContainsKey(i))
            {
                UpdateHandData(i);
            }
        }
    }
    
    /// <summary>
    /// Updates data for a specific hand.
    /// </summary>
    private void UpdateHandData(int handId)
    {
        HandData hand = trackedHands[handId];
        
        // Simulate hand tracking (this would integrate with hand tracking SDK)
        hand.isTracked = true;
        hand.confidence = handTrackingConfidence;
        
        // Update hand position and rotation
        Vector3 newPosition = Vector3.zero; // This would get actual hand position
        Quaternion newRotation = Quaternion.identity; // This would get actual hand rotation
        
        if (newPosition != hand.position)
        {
            hand.position = newPosition;
            OnHandPositionChanged?.Invoke(newPosition);
        }
        
        if (newRotation != hand.rotation)
        {
            hand.rotation = newRotation;
            OnHandRotationChanged?.Invoke(newRotation);
        }
        
        // Update finger tracking
        if (enableFingerTracking)
        {
            UpdateFingerTracking(hand);
        }
        
        // Detect gestures
        if (enableGestureRecognition)
        {
            DetectHandGesture(hand);
        }
    }
    
    /// <summary>
    /// Updates finger tracking for a hand.
    /// </summary>
    private void UpdateFingerTracking(HandData hand)
    {
        // Simulate finger positions (this would integrate with hand tracking SDK)
        for (int i = 0; i < hand.fingerPositions.Length; i++)
        {
            hand.fingerPositions[i] = hand.position + Vector3.forward * (i * 0.01f);
        }
        
        // Calculate finger bends
        for (int i = 0; i < hand.fingerBends.Length; i++)
        {
            hand.fingerBends[i] = UnityEngine.Random.Range(0f, 1f); // This would calculate actual finger bends
        }
    }
    
    /// <summary>
    /// Detects hand gestures.
    /// </summary>
    private void DetectHandGesture(HandData hand)
    {
        HandGesture newGesture = HandGesture.None;
        
        // Simple gesture detection based on finger bends
        float averageBend = 0f;
        for (int i = 0; i < hand.fingerBends.Length; i++)
        {
            averageBend += hand.fingerBends[i];
        }
        averageBend /= hand.fingerBends.Length;
        
        if (averageBend < 0.2f)
        {
            newGesture = HandGesture.Open;
        }
        else if (averageBend > 0.8f)
        {
            newGesture = HandGesture.Closed;
        }
        else if (hand.fingerBends[0] < 0.3f && hand.fingerBends[1] > 0.7f)
        {
            newGesture = HandGesture.Point;
        }
        
        if (newGesture != hand.currentGesture)
        {
            hand.currentGesture = newGesture;
            OnGestureDetected?.Invoke(newGesture.ToString());
            
            if (enableDebugLogging)
            {
                Debug.Log($"Hand {hand.handId} gesture: {newGesture}");
            }
        }
    }
    
    /// <summary>
    /// Grabs an object with VR controller or hand.
    /// </summary>
    public void GrabObject(GameObject obj, int controllerId = -1)
    {
        if (controllerId >= 0 && vrControllers.ContainsKey(controllerId))
        {
            // VR controller grab
            obj.transform.SetParent(vrControllers[controllerId].transform);
            OnObjectGrabbed?.Invoke(obj);
        }
        else
        {
            // Hand grab
            for (int i = 0; i < maxHands; i++)
            {
                if (trackedHands.ContainsKey(i) && trackedHands[i].isTracked)
                {
                    trackedHands[i].isGrabbing = true;
                    trackedHands[i].grabbedObject = obj;
                    obj.transform.SetParent(transform);
                    OnObjectGrabbed?.Invoke(obj);
                    break;
                }
            }
        }
        
        // Trigger haptic feedback
        if (enableHapticFeedback)
        {
            TriggerHapticFeedback(defaultHapticType);
        }
    }
    
    /// <summary>
    /// Releases a grabbed object.
    /// </summary>
    public void ReleaseObject(GameObject obj)
    {
        obj.transform.SetParent(null);
        OnObjectReleased?.Invoke(obj);
        
        // Clear hand grabbing state
        for (int i = 0; i < maxHands; i++)
        {
            if (trackedHands.ContainsKey(i) && trackedHands[i].grabbedObject == obj)
            {
                trackedHands[i].isGrabbing = false;
                trackedHands[i].grabbedObject = null;
                break;
            }
        }
    }
    
    /// <summary>
    /// Triggers haptic feedback.
    /// </summary>
    public void TriggerHapticFeedback(HapticFeedbackType type, float intensity = -1f, float duration = -1f)
    {
        if (!enableHapticFeedback)
            return;
        
        if (intensity < 0f) intensity = hapticIntensity;
        if (duration < 0f) duration = hapticDuration;
        
        // This would integrate with platform-specific haptic feedback
        // For VR controllers and hand haptics
        
        if (enableDebugLogging)
        {
            Debug.Log($"Haptic feedback: {type}, Intensity: {intensity}, Duration: {duration}");
        }
    }
    
    /// <summary>
    /// Places an object in AR space.
    /// </summary>
    public void PlaceARObject(GameObject obj, Vector3 position, Quaternion rotation)
    {
        if (!isARMode)
            return;
        
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(arSession.transform);
        
        OnARObjectPlaced?.Invoke(position);
        
        if (enableDebugLogging)
        {
            Debug.Log($"AR object placed: {obj.name} at {position}");
        }
    }
    
    /// <summary>
    /// Gets hand data for a specific hand.
    /// </summary>
    public HandData GetHandData(int handId)
    {
        if (trackedHands.ContainsKey(handId))
        {
            return trackedHands[handId];
        }
        return null;
    }
    
    /// <summary>
    /// Gets VR controller data for a specific controller.
    /// </summary>
    public VRControllerData GetVRControllerData(int controllerId)
    {
        if (vrControllers.ContainsKey(controllerId))
        {
            return vrControllers[controllerId];
        }
        return null;
    }
    
    /// <summary>
    /// Gets AR plane data for a specific plane.
    /// </summary>
    public ARPlaneData GetARPlaneData(string planeId)
    {
        if (arPlanes.ContainsKey(planeId))
        {
            return arPlanes[planeId];
        }
        return null;
    }
    
    /// <summary>
    /// Checks if VR mode is active.
    /// </summary>
    public bool IsVRMode()
    {
        return isVRMode;
    }
    
    /// <summary>
    /// Checks if AR mode is active.
    /// </summary>
    public bool IsARMode()
    {
        return isARMode;
    }
    
    /// <summary>
    /// Gets the VR camera.
    /// </summary>
    public Camera GetVRCamera()
    {
        return vrCamera;
    }
    
    /// <summary>
    /// Gets the AR camera.
    /// </summary>
    public Camera GetARCamera()
    {
        return arCamera;
    }
    
    /// <summary>
    /// Logs VR/AR status for debugging.
    /// </summary>
    public void LogVRARStatus()
    {
        if (!enableDebugLogging)
            return;
        
        Debug.Log("=== VR/AR Status ===");
        Debug.Log($"VR Mode: {isVRMode}");
        Debug.Log($"AR Mode: {isARMode}");
        Debug.Log($"Hand Tracking: {enableHandTracking}");
        Debug.Log($"Tracked Hands: {trackedHands.Count}");
        Debug.Log($"VR Controllers: {vrControllers.Count}");
        Debug.Log($"AR Planes: {arPlanes.Count}");
        Debug.Log($"Haptic Feedback: {enableHapticFeedback}");
        Debug.Log($"Spatial Audio: {enableSpatialAudio}");
        Debug.Log("===================");
    }
}

/// <summary>
/// VR Controller component for handling VR controller input and interactions.
/// </summary>
public class VRController : MonoBehaviour
{
    private int controllerId;
    private VRARManager.VRControllerData controllerData;
    
    public void Initialize(int id)
    {
        controllerId = id;
        controllerData = new VRARManager.VRControllerData(id);
    }
    
    private void Update()
    {
        // Update controller data
        controllerData.position = transform.position;
        controllerData.rotation = transform.rotation;
    }
} 