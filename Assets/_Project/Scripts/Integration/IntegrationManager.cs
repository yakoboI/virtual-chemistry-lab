using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages system integration, coordination, and communication between all components.
/// This component handles the overall system architecture and component interactions.
/// </summary>
public class IntegrationManager : MonoBehaviour
{
    [Header("Integration Management")]
    [SerializeField] private bool enableIntegrationManagement = true;
    [SerializeField] private bool enableSystemCoordination = true;
    [SerializeField] private bool enableComponentCommunication = true;
    [SerializeField] private bool enableSystemValidation = true;
    [SerializeField] private bool enableSystemMonitoring = true;
    
    [Header("System Configuration")]
    [SerializeField] private bool enableAutoInitialization = true;
    [SerializeField] private bool enableDependencyResolution = true;
    [SerializeField] private bool enableSystemRecovery = true;
    [SerializeField] private float initializationTimeout = 10f;
    [SerializeField] private float healthCheckInterval = 5f;
    
    [Header("Component Registration")]
    [SerializeField] private List<ComponentInfo> registeredComponents = new List<ComponentInfo>();
    [SerializeField] private Dictionary<string, ComponentStatus> componentStatuses = new Dictionary<string, ComponentStatus>();
    [SerializeField] private Dictionary<string, List<string>> componentDependencies = new Dictionary<string, List<string>>();
    
    [Header("Communication Settings")]
    [SerializeField] private bool enableEventRouting = true;
    [SerializeField] private bool enableMessageQueuing = true;
    [SerializeField] private bool enableAsyncCommunication = true;
    [SerializeField] private int maxMessageQueueSize = 100;
    [SerializeField] private float messageTimeout = 5f;
    
    [Header("System State")]
    [SerializeField] private SystemState currentSystemState = SystemState.Initializing;
    [SerializeField] private bool isSystemReady = false;
    [SerializeField] private bool isSystemHealthy = true;
    [SerializeField] private float systemStartTime = 0f;
    [SerializeField] private float lastHealthCheck = 0f;
    
    [Header("Performance")]
    [SerializeField] private bool enablePerformanceMonitoring = true;
    [SerializeField] private bool enableMemoryMonitoring = true;
    [SerializeField] private bool enableComponentPooling = true;
    [SerializeField] private int maxActiveComponents = 1000;
    [SerializeField] private float cleanupInterval = 30f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logSystemEvents = false;
    [SerializeField] private bool showSystemStatus = false;
    
    private static IntegrationManager instance;
    public static IntegrationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<IntegrationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("IntegrationManager");
                    instance = go.AddComponent<IntegrationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action OnSystemInitialized;
    public event Action OnSystemReady;
    public event Action OnSystemShutdown;
    public event Action<string> OnComponentRegistered;
    public event Action<string> OnComponentInitialized;
    public event Action<string> OnComponentError;
    public event Action<string> OnSystemError;
    public event Action<SystemState> OnSystemStateChanged;
    
    // Private variables
    private Queue<SystemMessage> messageQueue = new Queue<SystemMessage>();
    private Dictionary<string, object> systemData = new Dictionary<string, object>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class ComponentInfo
    {
        public string id;
        public string name;
        public string type;
        public bool isRequired;
        public bool isInitialized;
        public float initializationTime;
        public ComponentPriority priority;
        public string[] dependencies;
    }
    
    [System.Serializable]
    public class ComponentStatus
    {
        public string componentId;
        public bool isActive;
        public bool isHealthy;
        public float lastUpdateTime;
        public string statusMessage;
        public int errorCount;
        public float uptime;
    }
    
    [System.Serializable]
    public class SystemMessage
    {
        public string id;
        public string sender;
        public string receiver;
        public string messageType;
        public object data;
        public float timestamp;
        public bool isProcessed;
    }
    
    [System.Serializable]
    public enum SystemState
    {
        Initializing,
        Starting,
        Running,
        Paused,
        Stopping,
        Error,
        Shutdown
    }
    
    [System.Serializable]
    public enum ComponentPriority
    {
        Critical,
        High,
        Normal,
        Low,
        Optional
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIntegrationManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableAutoInitialization)
        {
            StartSystemInitialization();
        }
    }
    
    private void Update()
    {
        UpdateSystemState();
        ProcessMessageQueue();
        PerformHealthCheck();
    }
    
    /// <summary>
    /// Initializes the integration manager.
    /// </summary>
    private void InitializeIntegrationManager()
    {
        registeredComponents.Clear();
        componentStatuses.Clear();
        componentDependencies.Clear();
        messageQueue.Clear();
        systemData.Clear();
        
        systemStartTime = Time.time;
        currentSystemState = SystemState.Initializing;
        isSystemReady = false;
        isSystemHealthy = true;
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("IntegrationManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Starts system initialization.
    /// </summary>
    public void StartSystemInitialization()
    {
        if (currentSystemState != SystemState.Initializing)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("System initialization already started");
            }
            return;
        }
        
        ChangeSystemState(SystemState.Starting);
        
        if (enableDebugLogging)
        {
            Debug.Log("Starting system initialization...");
        }
        
        // Register core components
        RegisterCoreComponents();
        
        // Initialize components in dependency order
        InitializeComponents();
    }
    
    /// <summary>
    /// Registers core system components.
    /// </summary>
    private void RegisterCoreComponents()
    {
        // Core Systems
        RegisterComponent("GameManager", "Core", ComponentPriority.Critical, new string[] {});
        RegisterComponent("ExperimentStateManager", "Core", ComponentPriority.Critical, new string[] { "GameManager" });
        RegisterComponent("MouseInputHandler", "Core", ComponentPriority.High, new string[] { "GameManager" });
        RegisterComponent("KeyboardInputHandler", "Core", ComponentPriority.High, new string[] { "GameManager" });
        
        // Management Systems
        RegisterComponent("DataManager", "Management", ComponentPriority.Critical, new string[] { "GameManager" });
        RegisterComponent("AudioManager", "Management", ComponentPriority.Normal, new string[] { "GameManager" });
        RegisterComponent("SettingsManager", "Management", ComponentPriority.High, new string[] { "GameManager" });
        RegisterComponent("SceneStateController", "Management", ComponentPriority.High, new string[] { "GameManager" });
        RegisterComponent("UIManager", "Management", ComponentPriority.High, new string[] { "GameManager" });
        RegisterComponent("TextDisplayManager", "Management", ComponentPriority.Normal, new string[] { "UIManager" });
        RegisterComponent("ButtonHandler", "Management", ComponentPriority.Normal, new string[] { "UIManager" });
        
        // Laboratory Components
        RegisterComponent("ChemicalManager", "Laboratory", ComponentPriority.High, new string[] { "GameManager", "DataManager" });
        RegisterComponent("ApparatusManager", "Laboratory", ComponentPriority.High, new string[] { "GameManager", "DataManager" });
        RegisterComponent("SafetyManager", "Laboratory", ComponentPriority.Critical, new string[] { "GameManager", "ChemicalManager" });
        
        // Experiment Components
        RegisterComponent("TitrationManager", "Experiments", ComponentPriority.Normal, new string[] { "ChemicalManager", "MeasurementManager" });
        RegisterComponent("ReactionManager", "Experiments", ComponentPriority.Normal, new string[] { "ChemicalManager", "SafetyManager" });
        RegisterComponent("MeasurementManager", "Experiments", ComponentPriority.High, new string[] { "DataManager" });
    }
    
    /// <summary>
    /// Registers a component with the integration manager.
    /// </summary>
    public void RegisterComponent(string componentId, string type, ComponentPriority priority, string[] dependencies)
    {
        if (registeredComponents.Exists(c => c.id == componentId))
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Component {componentId} is already registered");
            }
            return;
        }
        
        ComponentInfo component = new ComponentInfo
        {
            id = componentId,
            name = componentId,
            type = type,
            isRequired = priority == ComponentPriority.Critical || priority == ComponentPriority.High,
            isInitialized = false,
            initializationTime = 0f,
            priority = priority,
            dependencies = dependencies
        };
        
        registeredComponents.Add(component);
        componentDependencies[componentId] = new List<string>(dependencies);
        
        // Initialize component status
        componentStatuses[componentId] = new ComponentStatus
        {
            componentId = componentId,
            isActive = false,
            isHealthy = true,
            lastUpdateTime = Time.time,
            statusMessage = "Registered",
            errorCount = 0,
            uptime = 0f
        };
        
        OnComponentRegistered?.Invoke(componentId);
        
        if (logSystemEvents)
        {
            Debug.Log($"Registered component: {componentId} ({type})");
        }
    }
    
    /// <summary>
    /// Initializes components in dependency order.
    /// </summary>
    private void InitializeComponents()
    {
        List<string> initializedComponents = new List<string>();
        List<string> failedComponents = new List<string>();
        
        // Initialize critical components first
        foreach (ComponentInfo component in registeredComponents)
        {
            if (component.priority == ComponentPriority.Critical)
            {
                if (InitializeComponent(component, initializedComponents))
                {
                    initializedComponents.Add(component.id);
                }
                else
                {
                    failedComponents.Add(component.id);
                }
            }
        }
        
        // Initialize high priority components
        foreach (ComponentInfo component in registeredComponents)
        {
            if (component.priority == ComponentPriority.High && !initializedComponents.Contains(component.id))
            {
                if (InitializeComponent(component, initializedComponents))
                {
                    initializedComponents.Add(component.id);
                }
                else
                {
                    failedComponents.Add(component.id);
                }
            }
        }
        
        // Initialize remaining components
        foreach (ComponentInfo component in registeredComponents)
        {
            if (!initializedComponents.Contains(component.id) && !failedComponents.Contains(component.id))
            {
                if (InitializeComponent(component, initializedComponents))
                {
                    initializedComponents.Add(component.id);
                }
                else
                {
                    failedComponents.Add(component.id);
                }
            }
        }
        
        // Check if system is ready
        CheckSystemReadiness(initializedComponents, failedComponents);
    }
    
    /// <summary>
    /// Initializes a specific component.
    /// </summary>
    private bool InitializeComponent(ComponentInfo component, List<string> initializedComponents)
    {
        // Check dependencies
        foreach (string dependency in component.dependencies)
        {
            if (!initializedComponents.Contains(dependency))
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"Component {component.id} cannot initialize - dependency {dependency} not ready");
                }
                return false;
            }
        }
        
        // Simulate component initialization
        float startTime = Time.time;
        
        try
        {
            // Check if component instance exists
            bool componentExists = CheckComponentInstance(component.id);
            
            if (componentExists)
            {
                component.isInitialized = true;
                component.initializationTime = Time.time - startTime;
                
                ComponentStatus status = componentStatuses[component.id];
                status.isActive = true;
                status.isHealthy = true;
                status.lastUpdateTime = Time.time;
                status.statusMessage = "Initialized";
                
                OnComponentInitialized?.Invoke(component.id);
                
                if (logSystemEvents)
                {
                    Debug.Log($"Initialized component: {component.id} in {component.initializationTime:F3}s");
                }
                
                return true;
            }
            else
            {
                if (enableDebugLogging)
                {
                    Debug.LogError($"Component instance not found: {component.id}");
                }
                
                ComponentStatus status = componentStatuses[component.id];
                status.isActive = false;
                status.isHealthy = false;
                status.errorCount++;
                status.statusMessage = "Instance not found";
                
                OnComponentError?.Invoke(component.id);
                
                return false;
            }
        }
        catch (Exception e)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error initializing component {component.id}: {e.Message}");
            }
            
            ComponentStatus status = componentStatuses[component.id];
            status.isActive = false;
            status.isHealthy = false;
            status.errorCount++;
            status.statusMessage = $"Error: {e.Message}";
            
            OnComponentError?.Invoke(component.id);
            
            return false;
        }
    }
    
    /// <summary>
    /// Checks if a component instance exists.
    /// </summary>
    private bool CheckComponentInstance(string componentId)
    {
        // Check for component instances based on naming convention
        switch (componentId)
        {
            case "GameManager":
                return GameManager.Instance != null;
            case "ExperimentStateManager":
                return ExperimentStateManager.Instance != null;
            case "MouseInputHandler":
                return MouseInputHandler.Instance != null;
            case "KeyboardInputHandler":
                return KeyboardInputHandler.Instance != null;
            case "DataManager":
                return DataManager.Instance != null;
            case "AudioManager":
                return AudioManager.Instance != null;
            case "SettingsManager":
                return SettingsManager.Instance != null;
            case "SceneStateController":
                return SceneStateController.Instance != null;
            case "UIManager":
                return UIManager.Instance != null;
            case "TextDisplayManager":
                return TextDisplayManager.Instance != null;
            case "ButtonHandler":
                return ButtonHandler.Instance != null;
            case "ChemicalManager":
                return ChemicalManager.Instance != null;
            case "ApparatusManager":
                return ApparatusManager.Instance != null;
            case "SafetyManager":
                return SafetyManager.Instance != null;
            case "TitrationManager":
                return TitrationManager.Instance != null;
            case "ReactionManager":
                return ReactionManager.Instance != null;
            case "MeasurementManager":
                return MeasurementManager.Instance != null;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Checks if the system is ready.
    /// </summary>
    private void CheckSystemReadiness(List<string> initializedComponents, List<string> failedComponents)
    {
        bool allCriticalComponentsReady = true;
        bool allHighPriorityComponentsReady = true;
        
        foreach (ComponentInfo component in registeredComponents)
        {
            if (component.isRequired)
            {
                if (component.priority == ComponentPriority.Critical && !initializedComponents.Contains(component.id))
                {
                    allCriticalComponentsReady = false;
                }
                else if (component.priority == ComponentPriority.High && !initializedComponents.Contains(component.id))
                {
                    allHighPriorityComponentsReady = false;
                }
            }
        }
        
        if (allCriticalComponentsReady)
        {
            ChangeSystemState(SystemState.Running);
            isSystemReady = true;
            
            OnSystemInitialized?.Invoke();
            OnSystemReady?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log($"System initialized successfully. {initializedComponents.Count} components ready, {failedComponents.Count} failed.");
            }
        }
        else
        {
            ChangeSystemState(SystemState.Error);
            isSystemReady = false;
            
            OnSystemError?.Invoke("Critical components failed to initialize");
            
            if (enableDebugLogging)
            {
                Debug.LogError($"System initialization failed. Critical components not ready.");
            }
        }
    }
    
    /// <summary>
    /// Changes the system state.
    /// </summary>
    private void ChangeSystemState(SystemState newState)
    {
        if (currentSystemState == newState) return;
        
        SystemState previousState = currentSystemState;
        currentSystemState = newState;
        
        OnSystemStateChanged?.Invoke(newState);
        
        if (logSystemEvents)
        {
            Debug.Log($"System state changed: {previousState} -> {newState}");
        }
    }
    
    /// <summary>
    /// Updates system state.
    /// </summary>
    private void UpdateSystemState()
    {
        if (!isSystemReady) return;
        
        // Update component uptimes
        foreach (var kvp in componentStatuses)
        {
            if (kvp.Value.isActive)
            {
                kvp.Value.uptime = Time.time - systemStartTime;
            }
        }
        
        // Check for system health
        bool systemHealthy = true;
        foreach (var kvp in componentStatuses)
        {
            if (kvp.Value.isActive && !kvp.Value.isHealthy)
            {
                systemHealthy = false;
                break;
            }
        }
        
        isSystemHealthy = systemHealthy;
    }
    
    /// <summary>
    /// Processes the message queue.
    /// </summary>
    private void ProcessMessageQueue()
    {
        if (!enableMessageQueuing || messageQueue.Count == 0) return;
        
        while (messageQueue.Count > 0)
        {
            SystemMessage message = messageQueue.Dequeue();
            
            if (Time.time - message.timestamp > messageTimeout)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"Message timeout: {message.id}");
                }
                continue;
            }
            
            ProcessMessage(message);
        }
    }
    
    /// <summary>
    /// Processes a system message.
    /// </summary>
    private void ProcessMessage(SystemMessage message)
    {
        message.isProcessed = true;
        
        if (logSystemEvents)
        {
            Debug.Log($"Processing message: {message.messageType} from {message.sender} to {message.receiver}");
        }
        
        // Route message to appropriate component
        RouteMessage(message);
    }
    
    /// <summary>
    /// Routes a message to the appropriate component.
    /// </summary>
    private void RouteMessage(SystemMessage message)
    {
        // This would route messages to specific components based on the receiver
        // For now, we'll just log the routing
        if (enableDebugLogging)
        {
            Debug.Log($"Routing message {message.id} to {message.receiver}");
        }
    }
    
    /// <summary>
    /// Performs system health check.
    /// </summary>
    private void PerformHealthCheck()
    {
        if (!enableSystemMonitoring || Time.time - lastHealthCheck < healthCheckInterval) return;
        
        lastHealthCheck = Time.time;
        
        bool allComponentsHealthy = true;
        
        foreach (var kvp in componentStatuses)
        {
            ComponentStatus status = kvp.Value;
            
            if (status.isActive)
            {
                // Check if component is still responding
                bool componentResponding = CheckComponentHealth(status.componentId);
                
                if (!componentResponding)
                {
                    status.isHealthy = false;
                    status.errorCount++;
                    status.statusMessage = "Not responding";
                    allComponentsHealthy = false;
                    
                    OnComponentError?.Invoke(status.componentId);
                }
                else
                {
                    status.isHealthy = true;
                    status.statusMessage = "Healthy";
                }
                
                status.lastUpdateTime = Time.time;
            }
        }
        
        isSystemHealthy = allComponentsHealthy;
        
        if (showSystemStatus)
        {
            LogSystemStatus();
        }
    }
    
    /// <summary>
    /// Checks component health.
    /// </summary>
    private bool CheckComponentHealth(string componentId)
    {
        // Simple health check - verify component instance exists
        return CheckComponentInstance(componentId);
    }
    
    /// <summary>
    /// Sends a message to another component.
    /// </summary>
    public void SendMessage(string sender, string receiver, string messageType, object data = null)
    {
        if (!enableEventRouting) return;
        
        SystemMessage message = new SystemMessage
        {
            id = GenerateMessageId(),
            sender = sender,
            receiver = receiver,
            messageType = messageType,
            data = data,
            timestamp = Time.time,
            isProcessed = false
        };
        
        if (enableMessageQueuing && messageQueue.Count < maxMessageQueueSize)
        {
            messageQueue.Enqueue(message);
        }
        else
        {
            ProcessMessage(message);
        }
    }
    
    /// <summary>
    /// Gets system data.
    /// </summary>
    public object GetSystemData(string key)
    {
        return systemData.ContainsKey(key) ? systemData[key] : null;
    }
    
    /// <summary>
    /// Sets system data.
    /// </summary>
    public void SetSystemData(string key, object value)
    {
        systemData[key] = value;
    }
    
    /// <summary>
    /// Gets component status.
    /// </summary>
    public ComponentStatus GetComponentStatus(string componentId)
    {
        return componentStatuses.ContainsKey(componentId) ? componentStatuses[componentId] : null;
    }
    
    /// <summary>
    /// Gets all registered components.
    /// </summary>
    public List<ComponentInfo> GetRegisteredComponents()
    {
        return new List<ComponentInfo>(registeredComponents);
    }
    
    /// <summary>
    /// Gets system health status.
    /// </summary>
    public bool IsSystemHealthy()
    {
        return isSystemHealthy;
    }
    
    /// <summary>
    /// Gets system ready status.
    /// </summary>
    public bool IsSystemReady()
    {
        return isSystemReady;
    }
    
    /// <summary>
    /// Gets current system state.
    /// </summary>
    public SystemState GetCurrentSystemState()
    {
        return currentSystemState;
    }
    
    /// <summary>
    /// Shuts down the system.
    /// </summary>
    public void ShutdownSystem()
    {
        ChangeSystemState(SystemState.Stopping);
        
        // Cleanup components
        foreach (var kvp in componentStatuses)
        {
            ComponentStatus status = kvp.Value;
            status.isActive = false;
            status.statusMessage = "Shutdown";
        }
        
        ChangeSystemState(SystemState.Shutdown);
        isSystemReady = false;
        
        OnSystemShutdown?.Invoke();
        
        if (enableDebugLogging)
        {
            Debug.Log("System shutdown completed");
        }
    }
    
    /// <summary>
    /// Generates a unique message ID.
    /// </summary>
    private string GenerateMessageId()
    {
        return $"msg_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Logs system status.
    /// </summary>
    public void LogSystemStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Integration Manager Status ===");
        Debug.Log($"System State: {currentSystemState}");
        Debug.Log($"System Ready: {isSystemReady}");
        Debug.Log($"System Healthy: {isSystemHealthy}");
        Debug.Log($"Registered Components: {registeredComponents.Count}");
        Debug.Log($"Active Components: {componentStatuses.Count}");
        Debug.Log($"Message Queue Size: {messageQueue.Count}");
        Debug.Log($"System Uptime: {Time.time - systemStartTime:F1}s");
        Debug.Log("==================================");
    }
} 