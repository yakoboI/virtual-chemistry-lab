using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages mobile device support and cross-platform compatibility.
/// Handles touch controls, mobile UI, and platform-specific features.
/// </summary>
public class MobileSupportManager : MonoBehaviour
{
    [Header("Mobile Settings")]
    [SerializeField] private bool enableMobileSupport = true;
    [SerializeField] private bool enableTouchControls = true;
    [SerializeField] private bool enableMobileUI = true;
    [SerializeField] private bool enableOfflineMode = true;
    [SerializeField] private bool enableCrossPlatformSync = true;
    
    [Header("Touch Controls")]
    [SerializeField] private float touchSensitivity = 1.0f;
    [SerializeField] private float pinchSensitivity = 1.0f;
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private bool enableHapticFeedback = true;
    [SerializeField] private bool enableGestureRecognition = true;
    
    [Header("Mobile UI")]
    [SerializeField] private bool enableResponsiveLayout = true;
    [SerializeField] private bool enableMobileOptimization = true;
    [SerializeField] private bool enableBatteryOptimization = true;
    [SerializeField] private bool enableLowPowerMode = true;
    [SerializeField] private float targetFrameRate = 30f;
    
    [Header("Platform Features")]
    [SerializeField] private bool enableCloudSync = true;
    [SerializeField] private bool enablePushNotifications = true;
    [SerializeField] private bool enableSocialSharing = true;
    [SerializeField] private bool enableAnalytics = true;
    [SerializeField] private bool enableCrashReporting = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showTouchInfo = false;
    [SerializeField] private bool logPlatformEvents = false;
    
    private static MobileSupportManager instance;
    public static MobileSupportManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MobileSupportManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("MobileSupportManager");
                    instance = go.AddComponent<MobileSupportManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<Vector2> OnTouchTap;
    public event Action<Vector2, Vector2> OnTouchDrag;
    public event Action<float> OnPinchZoom;
    public event Action<Vector2> OnSwipe;
    public event Action<bool> OnOrientationChanged;
    public event Action<bool> OnBatteryLow;
    public event Action<string> OnPlatformEvent;
    
    [System.Serializable]
    public class TouchData
    {
        public int touchId;
        public Vector2 position;
        public Vector2 deltaPosition;
        public float pressure;
        public TouchPhase phase;
        public float startTime;
        
        public TouchData(int id, Vector2 pos)
        {
            touchId = id;
            position = pos;
            deltaPosition = Vector2.zero;
            pressure = 1.0f;
            phase = TouchPhase.Began;
            startTime = Time.time;
        }
    }
    
    [System.Serializable]
    public class MobileSettings
    {
        public bool enableHaptics;
        public bool enableNotifications;
        public bool enableCloudSync;
        public float uiScale;
        public string language;
        public bool lowPowerMode;
        public bool batteryOptimization;
        
        public MobileSettings()
        {
            enableHaptics = true;
            enableNotifications = true;
            enableCloudSync = true;
            uiScale = 1.0f;
            language = "en";
            lowPowerMode = false;
            batteryOptimization = true;
        }
    }
    
    private Dictionary<int, TouchData> activeTouches;
    private MobileSettings mobileSettings;
    private bool isMobileDevice;
    private bool isTablet;
    private float batteryLevel;
    private bool isLowPowerMode;
    private ScreenOrientation currentOrientation;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMobileSupportManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableMobileSupport)
        {
            DetectPlatform();
            SetupMobileFeatures();
            LoadMobileSettings();
        }
    }
    
    private void Update()
    {
        if (enableMobileSupport)
        {
            HandleTouchInput();
            MonitorSystemStatus();
        }
    }
    
    /// <summary>
    /// Initializes the mobile support manager with basic settings.
    /// </summary>
    private void InitializeMobileSupportManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("MobileSupportManager initialized successfully");
        }
        
        // Initialize collections
        activeTouches = new Dictionary<int, TouchData>();
        mobileSettings = new MobileSettings();
        
        // Set target frame rate for mobile
        Application.targetFrameRate = (int)targetFrameRate;
        
        // Enable multi-touch
        Input.multiTouchEnabled = true;
    }
    
    /// <summary>
    /// Detects the current platform and device type.
    /// </summary>
    private void DetectPlatform()
    {
        isMobileDevice = Application.isMobilePlatform;
        isTablet = IsTablet();
        
        if (enableDebugLogging)
        {
            Debug.Log($"Platform: {Application.platform}");
            Debug.Log($"Mobile Device: {isMobileDevice}");
            Debug.Log($"Tablet: {isTablet}");
        }
    }
    
    /// <summary>
    /// Determines if the current device is a tablet.
    /// </summary>
    private bool IsTablet()
    {
        if (!isMobileDevice)
            return false;
        
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenDiagonal = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);
        
        // Convert to inches (assuming 96 DPI)
        float diagonalInches = screenDiagonal / 96f;
        
        return diagonalInches >= 7.0f;
    }
    
    /// <summary>
    /// Sets up mobile-specific features.
    /// </summary>
    private void SetupMobileFeatures()
    {
        if (!isMobileDevice)
            return;
        
        // Setup screen orientation
        SetupScreenOrientation();
        
        // Setup mobile UI scaling
        SetupMobileUI();
        
        // Setup battery monitoring
        SetupBatteryMonitoring();
        
        if (enableDebugLogging)
        {
            Debug.Log("Mobile features setup completed");
        }
    }
    
    /// <summary>
    /// Sets up screen orientation for mobile devices.
    /// </summary>
    private void SetupScreenOrientation()
    {
        if (isTablet)
        {
            // Allow all orientations for tablets
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
        else
        {
            // Prefer portrait for phones
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
        
        currentOrientation = Screen.orientation;
    }
    
    /// <summary>
    /// Sets up mobile UI scaling and optimization.
    /// </summary>
    private void SetupMobileUI()
    {
        if (!enableMobileUI)
            return;
        
        // Adjust UI scale based on device
        if (isTablet)
        {
            mobileSettings.uiScale = 1.2f;
        }
        else
        {
            mobileSettings.uiScale = 1.0f;
        }
        
        // Apply UI scaling
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.scaleFactor = mobileSettings.uiScale;
            }
        }
    }
    
    /// <summary>
    /// Sets up battery monitoring for mobile devices.
    /// </summary>
    private void SetupBatteryMonitoring()
    {
        if (!isMobileDevice)
            return;
        
        batteryLevel = SystemInfo.batteryLevel;
        isLowPowerMode = SystemInfo.batteryStatus == BatteryStatus.Charging ? false : batteryLevel < 0.2f;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Battery Level: {batteryLevel:P0}");
            Debug.Log($"Low Power Mode: {isLowPowerMode}");
        }
    }
    
    /// <summary>
    /// Loads mobile-specific settings.
    /// </summary>
    private void LoadMobileSettings()
    {
        // Load settings from PlayerPrefs or cloud
        mobileSettings.enableHaptics = PlayerPrefs.GetInt("MobileHaptics", 1) == 1;
        mobileSettings.enableNotifications = PlayerPrefs.GetInt("MobileNotifications", 1) == 1;
        mobileSettings.enableCloudSync = PlayerPrefs.GetInt("MobileCloudSync", 1) == 1;
        mobileSettings.uiScale = PlayerPrefs.GetFloat("MobileUIScale", 1.0f);
        mobileSettings.language = PlayerPrefs.GetString("MobileLanguage", "en");
        mobileSettings.lowPowerMode = PlayerPrefs.GetInt("MobileLowPower", 0) == 1;
        mobileSettings.batteryOptimization = PlayerPrefs.GetInt("MobileBatteryOpt", 1) == 1;
        
        if (enableDebugLogging)
        {
            Debug.Log("Mobile settings loaded");
        }
    }
    
    /// <summary>
    /// Saves mobile-specific settings.
    /// </summary>
    public void SaveMobileSettings()
    {
        PlayerPrefs.SetInt("MobileHaptics", mobileSettings.enableHaptics ? 1 : 0);
        PlayerPrefs.SetInt("MobileNotifications", mobileSettings.enableNotifications ? 1 : 0);
        PlayerPrefs.SetInt("MobileCloudSync", mobileSettings.enableCloudSync ? 1 : 0);
        PlayerPrefs.SetFloat("MobileUIScale", mobileSettings.uiScale);
        PlayerPrefs.SetString("MobileLanguage", mobileSettings.language);
        PlayerPrefs.SetInt("MobileLowPower", mobileSettings.lowPowerMode ? 1 : 0);
        PlayerPrefs.SetInt("MobileBatteryOpt", mobileSettings.batteryOptimization ? 1 : 0);
        PlayerPrefs.Save();
        
        if (enableDebugLogging)
        {
            Debug.Log("Mobile settings saved");
        }
    }
    
    /// <summary>
    /// Handles touch input for mobile devices.
    /// </summary>
    private void HandleTouchInput()
    {
        if (!enableTouchControls || !isMobileDevice)
            return;
        
        // Handle touch input
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;
                case TouchPhase.Moved:
                    HandleTouchMoved(touch);
                    break;
                case TouchPhase.Ended:
                    HandleTouchEnded(touch);
                    break;
                case TouchPhase.Canceled:
                    HandleTouchCanceled(touch);
                    break;
            }
        }
        
        // Handle multi-touch gestures
        if (Input.touchCount == 2)
        {
            HandlePinchGesture();
        }
    }
    
    /// <summary>
    /// Handles touch began events.
    /// </summary>
    private void HandleTouchBegan(Touch touch)
    {
        TouchData touchData = new TouchData(touch.fingerId, touch.position);
        activeTouches[touch.fingerId] = touchData;
        
        if (enableHapticFeedback && mobileSettings.enableHaptics)
        {
            TriggerHapticFeedback(HapticFeedbackType.Light);
        }
        
        if (enableDebugLogging && showTouchInfo)
        {
            Debug.Log($"Touch began: {touch.fingerId} at {touch.position}");
        }
    }
    
    /// <summary>
    /// Handles touch moved events.
    /// </summary>
    private void HandleTouchMoved(Touch touch)
    {
        if (!activeTouches.ContainsKey(touch.fingerId))
            return;
        
        TouchData touchData = activeTouches[touch.fingerId];
        Vector2 previousPosition = touchData.position;
        touchData.position = touch.position;
        touchData.deltaPosition = touch.deltaPosition;
        touchData.phase = TouchPhase.Moved;
        
        // Check for swipe gesture
        if (activeTouches.Count == 1)
        {
            CheckForSwipeGesture(touchData, previousPosition);
        }
        
        OnTouchDrag?.Invoke(touchData.position, touchData.deltaPosition);
        
        if (enableDebugLogging && showTouchInfo)
        {
            Debug.Log($"Touch moved: {touch.fingerId} delta: {touch.deltaPosition}");
        }
    }
    
    /// <summary>
    /// Handles touch ended events.
    /// </summary>
    private void HandleTouchEnded(Touch touch)
    {
        if (!activeTouches.ContainsKey(touch.fingerId))
            return;
        
        TouchData touchData = activeTouches[touch.fingerId];
        touchData.phase = TouchPhase.Ended;
        
        // Check for tap gesture
        float touchDuration = Time.time - touchData.startTime;
        if (touchDuration < 0.3f && touchData.deltaPosition.magnitude < swipeThreshold)
        {
            OnTouchTap?.Invoke(touchData.position);
            
            if (enableHapticFeedback && mobileSettings.enableHaptics)
            {
                TriggerHapticFeedback(HapticFeedbackType.Medium);
            }
        }
        
        activeTouches.Remove(touch.fingerId);
        
        if (enableDebugLogging && showTouchInfo)
        {
            Debug.Log($"Touch ended: {touch.fingerId} duration: {touchDuration:F2}s");
        }
    }
    
    /// <summary>
    /// Handles touch canceled events.
    /// </summary>
    private void HandleTouchCanceled(Touch touch)
    {
        if (activeTouches.ContainsKey(touch.fingerId))
        {
            activeTouches.Remove(touch.fingerId);
        }
        
        if (enableDebugLogging && showTouchInfo)
        {
            Debug.Log($"Touch canceled: {touch.fingerId}");
        }
    }
    
    /// <summary>
    /// Checks for swipe gestures.
    /// </summary>
    private void CheckForSwipeGesture(TouchData touchData, Vector2 startPosition)
    {
        Vector2 swipeDelta = touchData.position - startPosition;
        
        if (swipeDelta.magnitude > swipeThreshold)
        {
            OnSwipe?.Invoke(swipeDelta.normalized);
            
            if (enableHapticFeedback && mobileSettings.enableHaptics)
            {
                TriggerHapticFeedback(HapticFeedbackType.Heavy);
            }
        }
    }
    
    /// <summary>
    /// Handles pinch gesture for zooming.
    /// </summary>
    private void HandlePinchGesture()
    {
        if (Input.touchCount != 2)
            return;
        
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        
        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        
        float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
        float touchDeltaMag = (touch0.position - touch1.position).magnitude;
        
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        
        if (Mathf.Abs(deltaMagnitudeDiff) > 10f)
        {
            float zoomFactor = deltaMagnitudeDiff * pinchSensitivity * 0.01f;
            OnPinchZoom?.Invoke(zoomFactor);
        }
    }
    
    /// <summary>
    /// Triggers haptic feedback on mobile devices.
    /// </summary>
    public void TriggerHapticFeedback(HapticFeedbackType type)
    {
        if (!isMobileDevice || !mobileSettings.enableHaptics)
            return;
        
        // This would integrate with platform-specific haptic feedback
        // For now, we'll simulate it
        
        if (enableDebugLogging)
        {
            Debug.Log($"Haptic feedback triggered: {type}");
        }
    }
    
    /// <summary>
    /// Monitors system status for mobile devices.
    /// </summary>
    private void MonitorSystemStatus()
    {
        if (!isMobileDevice)
            return;
        
        // Monitor battery level
        float newBatteryLevel = SystemInfo.batteryLevel;
        if (newBatteryLevel != batteryLevel)
        {
            batteryLevel = newBatteryLevel;
            
            bool newLowPowerMode = batteryLevel < 0.2f;
            if (newLowPowerMode != isLowPowerMode)
            {
                isLowPowerMode = newLowPowerMode;
                OnBatteryLow?.Invoke(isLowPowerMode);
                
                if (isLowPowerMode && mobileSettings.batteryOptimization)
                {
                    EnableLowPowerMode();
                }
            }
        }
        
        // Monitor orientation changes
        ScreenOrientation newOrientation = Screen.orientation;
        if (newOrientation != currentOrientation)
        {
            currentOrientation = newOrientation;
            OnOrientationChanged?.Invoke(true);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Orientation changed to: {currentOrientation}");
            }
        }
    }
    
    /// <summary>
    /// Enables low power mode for mobile devices.
    /// </summary>
    private void EnableLowPowerMode()
    {
        if (!isMobileDevice)
            return;
        
        // Reduce frame rate
        Application.targetFrameRate = 15;
        
        // Disable non-essential features
        enableHapticFeedback = false;
        
        if (enableDebugLogging)
        {
            Debug.Log("Low power mode enabled");
        }
    }
    
    /// <summary>
    /// Disables low power mode for mobile devices.
    /// </summary>
    public void DisableLowPowerMode()
    {
        if (!isMobileDevice)
            return;
        
        // Restore normal frame rate
        Application.targetFrameRate = (int)targetFrameRate;
        
        // Re-enable features
        enableHapticFeedback = mobileSettings.enableHaptics;
        
        if (enableDebugLogging)
        {
            Debug.Log("Low power mode disabled");
        }
    }
    
    /// <summary>
    /// Sends a push notification to mobile devices.
    /// </summary>
    public void SendPushNotification(string title, string message, string data = "")
    {
        if (!isMobileDevice || !mobileSettings.enableNotifications)
            return;
        
        // This would integrate with platform-specific push notification services
        // For now, we'll simulate it
        
        if (enableDebugLogging)
        {
            Debug.Log($"Push notification: {title} - {message}");
        }
        
        OnPlatformEvent?.Invoke("PushNotification");
    }
    
    /// <summary>
    /// Syncs data with cloud storage.
    /// </summary>
    public void SyncWithCloud()
    {
        if (!isMobileDevice || !mobileSettings.enableCloudSync)
            return;
        
        // This would integrate with cloud storage services
        // For now, we'll simulate it
        
        if (enableDebugLogging)
        {
            Debug.Log("Cloud sync initiated");
        }
        
        OnPlatformEvent?.Invoke("CloudSync");
    }
    
    /// <summary>
    /// Shares content on social media platforms.
    /// </summary>
    public void ShareContent(string title, string message, string url = "")
    {
        if (!isMobileDevice || !enableSocialSharing)
            return;
        
        // This would integrate with platform-specific sharing APIs
        // For now, we'll simulate it
        
        if (enableDebugLogging)
        {
            Debug.Log($"Content shared: {title} - {message}");
        }
        
        OnPlatformEvent?.Invoke("SocialShare");
    }
    
    /// <summary>
    /// Gets the current mobile settings.
    /// </summary>
    public MobileSettings GetMobileSettings()
    {
        return mobileSettings;
    }
    
    /// <summary>
    /// Updates mobile settings.
    /// </summary>
    public void UpdateMobileSettings(MobileSettings newSettings)
    {
        mobileSettings = newSettings;
        SaveMobileSettings();
        
        // Apply new settings
        SetupMobileUI();
        
        if (enableDebugLogging)
        {
            Debug.Log("Mobile settings updated");
        }
    }
    
    /// <summary>
    /// Checks if the current device is mobile.
    /// </summary>
    public bool IsMobileDevice()
    {
        return isMobileDevice;
    }
    
    /// <summary>
    /// Checks if the current device is a tablet.
    /// </summary>
    public bool IsTablet()
    {
        return isTablet;
    }
    
    /// <summary>
    /// Gets the current battery level.
    /// </summary>
    public float GetBatteryLevel()
    {
        return batteryLevel;
    }
    
    /// <summary>
    /// Checks if low power mode is active.
    /// </summary>
    public bool IsLowPowerMode()
    {
        return isLowPowerMode;
    }
    
    /// <summary>
    /// Logs mobile support status for debugging.
    /// </summary>
    public void LogMobileSupportStatus()
    {
        if (!enableDebugLogging)
            return;
        
        Debug.Log("=== Mobile Support Status ===");
        Debug.Log($"Mobile Device: {isMobileDevice}");
        Debug.Log($"Tablet: {isTablet}");
        Debug.Log($"Battery Level: {batteryLevel:P0}");
        Debug.Log($"Low Power Mode: {isLowPowerMode}");
        Debug.Log($"Orientation: {currentOrientation}");
        Debug.Log($"UI Scale: {mobileSettings.uiScale}");
        Debug.Log($"Active Touches: {activeTouches.Count}");
        Debug.Log("============================");
    }
}

/// <summary>
/// Types of haptic feedback for mobile devices.
/// </summary>
public enum HapticFeedbackType
{
    Light,
    Medium,
    Heavy
} 