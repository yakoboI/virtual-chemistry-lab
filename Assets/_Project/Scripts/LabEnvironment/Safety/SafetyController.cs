using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages specialized safety controls, equipment, and emergency procedures for the virtual chemistry lab.
/// This component handles safety equipment, emergency systems, and specialized safety protocols.
/// </summary>
public class SafetyController : MonoBehaviour
{
    [Header("Safety Control Management")]
    [SerializeField] private bool enableSafetyControls = true;
    [SerializeField] private bool enableEmergencySystems = true;
    [SerializeField] private bool enableSafetyEquipment = true;
    [SerializeField] private bool enableVentilationControl = true;
    [SerializeField] private bool enableFireSuppression = true;
    
    [Header("Safety Equipment")]
    [SerializeField] private SafetyEquipment[] availableEquipment;
    [SerializeField] private string emergencyAlarmSound = "emergency_alarm";
    [SerializeField] private string ventilationSound = "ventilation_fan";
    [SerializeField] private string fireSuppressionSound = "fire_suppression";
    
    [Header("Safety Control State")]
    [SerializeField] private Dictionary<string, SafetyEquipmentInstance> activeEquipment = new Dictionary<string, SafetyEquipmentInstance>();
    [SerializeField] private List<SafetyEvent> safetyEvents = new List<SafetyEvent>();
    [SerializeField] private bool isEmergencyActive = false;
    
    [Header("Emergency Systems")]
    [SerializeField] private bool enableEmergencyAlarm = true;
    [SerializeField] private bool enableEmergencyShutdown = true;
    [SerializeField] private bool enableEmergencyEvacuation = true;
    [SerializeField] private float emergencyResponseTime = 5f;
    [SerializeField] private float evacuationTime = 30f;
    
    [Header("Ventilation Control")]
    [SerializeField] private bool enableVentilationMonitoring = true;
    [SerializeField] private bool enableAutoVentilation = true;
    [SerializeField] private float ventilationThreshold = 0.8f;
    [SerializeField] private float ventilationSpeed = 1.0f;
    [SerializeField] private float airQualityThreshold = 0.7f;
    
    [Header("Fire Suppression")]
    [SerializeField] private bool enableFireDetection = true;
    [SerializeField] private bool enableAutoSuppression = true;
    [SerializeField] private float fireDetectionThreshold = 0.9f;
    [SerializeField] private float suppressionResponseTime = 3f;
    [SerializeField] private float suppressionDuration = 10f;
    
    [Header("Safety Monitoring")]
    [SerializeField] private bool enableRealTimeMonitoring = true;
    [SerializeField] private bool enableSafetyLogging = true;
    [SerializeField] private bool enableSafetyReporting = true;
    [SerializeField] private float monitoringInterval = 0.5f;
    [SerializeField] private float alertThreshold = 0.8f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logSafetyEvents = false;
    
    private static SafetyController instance;
    public static SafetyController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SafetyController>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SafetyController");
                    instance = go.AddComponent<SafetyController>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnEmergencyActivated;
    public event Action<string> OnEmergencyDeactivated;
    public event Action<string> OnSafetyEquipmentActivated;
    public event Action<SafetyEvent> OnSafetyEvent;
    public event Action<float> OnAirQualityChanged;
    public event Action<string> OnSafetyError;
    
    // Private variables
    private Dictionary<string, SafetyEquipment> equipmentDatabase = new Dictionary<string, SafetyEquipment>();
    private bool isInitialized = false;
    private float lastMonitoringTime = 0f;
    private float emergencyStartTime = 0f;
    
    [System.Serializable]
    public class SafetyEquipment
    {
        public string id;
        public string name;
        public string description;
        public SafetyEquipmentType type;
        public float responseTime;
        public float effectiveness;
        public bool isAutomatic;
        public string[] requiredConditions;
        public SafetyEquipmentStatus status;
    }
    
    [System.Serializable]
    public class SafetyEquipmentInstance
    {
        public string id;
        public string equipmentId;
        public string name;
        public bool isActive;
        public bool isOperational;
        public float activationTime;
        public float lastMaintenance;
        public float effectiveness;
        public SafetyEquipmentStatus status;
        public Dictionary<string, object> operationalData;
    }
    
    [System.Serializable]
    public class SafetyEvent
    {
        public string id;
        public string eventType;
        public string description;
        public SafetyEventSeverity severity;
        public float timestamp;
        public bool isResolved;
        public string equipmentId;
        public Dictionary<string, object> eventData;
    }
    
    [System.Serializable]
    public enum SafetyEquipmentType
    {
        FireExtinguisher,
        EmergencyShower,
        EyeWashStation,
        FumeHood,
        VentilationSystem,
        FireAlarm,
        EmergencyLighting,
        FirstAidKit,
        SpillKit,
        GasDetector
    }
    
    [System.Serializable]
    public enum SafetyEquipmentStatus
    {
        Operational,
        Maintenance,
        Malfunction,
        Offline,
        Emergency
    }
    
    [System.Serializable]
    public enum SafetyEventSeverity
    {
        Low,
        Medium,
        High,
        Critical,
        Emergency
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSafetyController();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadEquipmentDatabase();
    }
    
    private void Update()
    {
        UpdateSafetyMonitoring();
        UpdateEmergencySystems();
    }
    
    /// <summary>
    /// Initializes the safety controller.
    /// </summary>
    private void InitializeSafetyController()
    {
        activeEquipment.Clear();
        safetyEvents.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("SafetyController initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads safety equipment data from available equipment.
    /// </summary>
    private void LoadEquipmentDatabase()
    {
        equipmentDatabase.Clear();
        
        foreach (SafetyEquipment equipment in availableEquipment)
        {
            if (equipment != null && !string.IsNullOrEmpty(equipment.id))
            {
                equipmentDatabase[equipment.id] = equipment;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded safety equipment: {equipment.name} ({equipment.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {equipmentDatabase.Count} safety equipment items");
        }
    }
    
    /// <summary>
    /// Creates a new safety equipment instance.
    /// </summary>
    public SafetyEquipmentInstance CreateEquipment(string equipmentId)
    {
        if (!enableSafetyControls || !equipmentDatabase.ContainsKey(equipmentId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Safety equipment not found: {equipmentId}");
            }
            return null;
        }
        
        SafetyEquipment equipment = equipmentDatabase[equipmentId];
        SafetyEquipmentInstance instance = new SafetyEquipmentInstance
        {
            id = GenerateEquipmentId(),
            equipmentId = equipmentId,
            name = equipment.name,
            isActive = true,
            isOperational = true,
            activationTime = 0f,
            lastMaintenance = Time.time,
            effectiveness = equipment.effectiveness,
            status = SafetyEquipmentStatus.Operational,
            operationalData = new Dictionary<string, object>()
        };
        
        activeEquipment[instance.id] = instance;
        
        OnSafetyEquipmentActivated?.Invoke(instance.id);
        
        if (logSafetyEvents)
        {
            Debug.Log($"Created safety equipment: {equipment.name} ({instance.id})");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Activates safety equipment.
    /// </summary>
    public bool ActivateEquipment(string instanceId)
    {
        if (!activeEquipment.ContainsKey(instanceId)) return false;
        
        SafetyEquipmentInstance instance = activeEquipment[instanceId];
        SafetyEquipment equipment = equipmentDatabase[instance.equipmentId];
        
        if (!instance.isOperational)
        {
            OnSafetyError?.Invoke($"Equipment {instance.name} is not operational");
            return false;
        }
        
        instance.isActive = true;
        instance.activationTime = Time.time;
        instance.status = SafetyEquipmentStatus.Emergency;
        
        // Play activation sound
        PlayEquipmentSound(equipment.type);
        
        // Create safety event
        CreateSafetyEvent("EquipmentActivated", $"Activated {instance.name}", SafetyEventSeverity.Medium, instanceId);
        
        if (logSafetyEvents)
        {
            Debug.Log($"Activated safety equipment: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Deactivates safety equipment.
    /// </summary>
    public void DeactivateEquipment(string instanceId)
    {
        if (!activeEquipment.ContainsKey(instanceId)) return;
        
        SafetyEquipmentInstance instance = activeEquipment[instanceId];
        
        instance.isActive = false;
        instance.status = SafetyEquipmentStatus.Operational;
        
        // Create safety event
        CreateSafetyEvent("EquipmentDeactivated", $"Deactivated {instance.name}", SafetyEventSeverity.Low, instanceId);
        
        if (logSafetyEvents)
        {
            Debug.Log($"Deactivated safety equipment: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Activates emergency mode.
    /// </summary>
    public void ActivateEmergencyMode(string reason)
    {
        if (isEmergencyActive) return;
        
        isEmergencyActive = true;
        emergencyStartTime = Time.time;
        
        // Activate emergency systems
        if (enableEmergencyAlarm)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(emergencyAlarmSound);
            }
        }
        
        // Activate all emergency equipment
        foreach (var kvp in activeEquipment)
        {
            SafetyEquipmentInstance instance = kvp.Value;
            SafetyEquipment equipment = equipmentDatabase[instance.equipmentId];
            
            if (equipment.isAutomatic)
            {
                ActivateEquipment(instance.id);
            }
        }
        
        // Create emergency event
        CreateSafetyEvent("EmergencyActivated", reason, SafetyEventSeverity.Emergency);
        
        OnEmergencyActivated?.Invoke(reason);
        
        if (logSafetyEvents)
        {
            Debug.Log($"Emergency mode activated: {reason}");
        }
    }
    
    /// <summary>
    /// Deactivates emergency mode.
    /// </summary>
    public void DeactivateEmergencyMode()
    {
        if (!isEmergencyActive) return;
        
        isEmergencyActive = false;
        
        // Deactivate emergency equipment
        foreach (var kvp in activeEquipment)
        {
            SafetyEquipmentInstance instance = kvp.Value;
            if (instance.status == SafetyEquipmentStatus.Emergency)
            {
                DeactivateEquipment(instance.id);
            }
        }
        
        // Create emergency deactivation event
        CreateSafetyEvent("EmergencyDeactivated", "Emergency mode deactivated", SafetyEventSeverity.Medium);
        
        OnEmergencyDeactivated?.Invoke("Emergency mode deactivated");
        
        if (logSafetyEvents)
        {
            Debug.Log("Emergency mode deactivated");
        }
    }
    
    /// <summary>
    /// Monitors air quality and controls ventilation.
    /// </summary>
    public void MonitorAirQuality(float airQuality)
    {
        if (!enableVentilationControl) return;
        
        OnAirQualityChanged?.Invoke(airQuality);
        
        if (airQuality < airQualityThreshold)
        {
            // Activate ventilation
            ActivateVentilation();
        }
        
        if (logSafetyEvents)
        {
            Debug.Log($"Air quality monitored: {airQuality:F2}");
        }
    }
    
    /// <summary>
    /// Activates ventilation system.
    /// </summary>
    private void ActivateVentilation()
    {
        if (!enableVentilationMonitoring) return;
        
        // Find ventilation equipment
        foreach (var kvp in activeEquipment)
        {
            SafetyEquipmentInstance instance = kvp.Value;
            SafetyEquipment equipment = equipmentDatabase[instance.equipmentId];
            
            if (equipment.type == SafetyEquipmentType.VentilationSystem)
            {
                ActivateEquipment(instance.id);
                break;
            }
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(ventilationSound);
        }
    }
    
    /// <summary>
    /// Detects fire and activates suppression.
    /// </summary>
    public void DetectFire(float fireLevel)
    {
        if (!enableFireDetection) return;
        
        if (fireLevel > fireDetectionThreshold)
        {
            // Activate fire suppression
            ActivateFireSuppression();
            
            // Activate emergency mode
            ActivateEmergencyMode("Fire detected");
        }
    }
    
    /// <summary>
    /// Activates fire suppression system.
    /// </summary>
    private void ActivateFireSuppression()
    {
        if (!enableFireSuppression) return;
        
        // Find fire suppression equipment
        foreach (var kvp in activeEquipment)
        {
            SafetyEquipmentInstance instance = kvp.Value;
            SafetyEquipment equipment = equipmentDatabase[instance.equipmentId];
            
            if (equipment.type == SafetyEquipmentType.FireExtinguisher)
            {
                ActivateEquipment(instance.id);
                break;
            }
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(fireSuppressionSound);
        }
    }
    
    /// <summary>
    /// Updates safety monitoring.
    /// </summary>
    private void UpdateSafetyMonitoring()
    {
        if (!enableRealTimeMonitoring || Time.time - lastMonitoringTime < monitoringInterval) return;
        
        lastMonitoringTime = Time.time;
        
        // Monitor equipment status
        foreach (var kvp in activeEquipment)
        {
            SafetyEquipmentInstance instance = kvp.Value;
            
            // Check if equipment needs maintenance
            if (Time.time - instance.lastMaintenance > 3600f) // 1 hour
            {
                instance.status = SafetyEquipmentStatus.Maintenance;
                CreateSafetyEvent("MaintenanceRequired", $"{instance.name} requires maintenance", SafetyEventSeverity.Medium, instance.id);
            }
        }
    }
    
    /// <summary>
    /// Updates emergency systems.
    /// </summary>
    private void UpdateEmergencySystems()
    {
        if (!isEmergencyActive) return;
        
        // Check if emergency should be deactivated
        if (Time.time - emergencyStartTime > emergencyResponseTime)
        {
            // Check if all critical issues are resolved
            bool allResolved = true;
            foreach (SafetyEvent safetyEvent in safetyEvents)
            {
                if (safetyEvent.severity == SafetyEventSeverity.Emergency && !safetyEvent.isResolved)
                {
                    allResolved = false;
                    break;
                }
            }
            
            if (allResolved)
            {
                DeactivateEmergencyMode();
            }
        }
    }
    
    /// <summary>
    /// Creates a safety event.
    /// </summary>
    private void CreateSafetyEvent(string eventType, string description, SafetyEventSeverity severity, string equipmentId = "")
    {
        SafetyEvent safetyEvent = new SafetyEvent
        {
            id = GenerateEventId(),
            eventType = eventType,
            description = description,
            severity = severity,
            timestamp = Time.time,
            isResolved = false,
            equipmentId = equipmentId,
            eventData = new Dictionary<string, object>()
        };
        
        safetyEvents.Add(safetyEvent);
        
        OnSafetyEvent?.Invoke(safetyEvent);
        
        if (logSafetyEvents)
        {
            Debug.Log($"Safety event created: {eventType} - {description}");
        }
    }
    
    /// <summary>
    /// Plays equipment-specific sounds.
    /// </summary>
    private void PlayEquipmentSound(SafetyEquipmentType equipmentType)
    {
        if (AudioManager.Instance == null) return;
        
        switch (equipmentType)
        {
            case SafetyEquipmentType.FireAlarm:
                AudioManager.Instance.PlaySFX(emergencyAlarmSound);
                break;
            case SafetyEquipmentType.VentilationSystem:
                AudioManager.Instance.PlaySFX(ventilationSound);
                break;
            case SafetyEquipmentType.FireExtinguisher:
                AudioManager.Instance.PlaySFX(fireSuppressionSound);
                break;
        }
    }
    
    /// <summary>
    /// Generates a unique equipment ID.
    /// </summary>
    private string GenerateEquipmentId()
    {
        return $"safety_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique event ID.
    /// </summary>
    private string GenerateEventId()
    {
        return $"event_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsEmergencyActive() => isEmergencyActive;
    public int GetActiveEquipmentCount() => activeEquipment.Count;
    public int GetSafetyEventCount() => safetyEvents.Count;
    
    /// <summary>
    /// Gets an equipment instance by ID.
    /// </summary>
    public SafetyEquipmentInstance GetEquipment(string instanceId)
    {
        return activeEquipment.ContainsKey(instanceId) ? activeEquipment[instanceId] : null;
    }
    
    /// <summary>
    /// Gets equipment data by ID.
    /// </summary>
    public SafetyEquipment GetEquipmentData(string equipmentId)
    {
        return equipmentDatabase.ContainsKey(equipmentId) ? equipmentDatabase[equipmentId] : null;
    }
    
    /// <summary>
    /// Gets all available equipment IDs.
    /// </summary>
    public List<string> GetAvailableEquipmentIds()
    {
        return new List<string>(equipmentDatabase.Keys);
    }
    
    /// <summary>
    /// Gets safety events.
    /// </summary>
    public List<SafetyEvent> GetSafetyEvents()
    {
        return new List<SafetyEvent>(safetyEvents);
    }
    
    /// <summary>
    /// Gets active safety events.
    /// </summary>
    public List<SafetyEvent> GetActiveSafetyEvents()
    {
        return safetyEvents.FindAll(e => !e.isResolved);
    }
    
    /// <summary>
    /// Resolves a safety event.
    /// </summary>
    public void ResolveSafetyEvent(string eventId)
    {
        SafetyEvent safetyEvent = safetyEvents.Find(e => e.id == eventId);
        if (safetyEvent != null)
        {
            safetyEvent.isResolved = true;
            
            if (logSafetyEvents)
            {
                Debug.Log($"Resolved safety event: {safetyEvent.description}");
            }
        }
    }
    
    /// <summary>
    /// Sets the emergency response time.
    /// </summary>
    public void SetEmergencyResponseTime(float time)
    {
        emergencyResponseTime = Mathf.Clamp(time, 1f, 60f);
    }
    
    /// <summary>
    /// Sets the air quality threshold.
    /// </summary>
    public void SetAirQualityThreshold(float threshold)
    {
        airQualityThreshold = Mathf.Clamp(threshold, 0.1f, 1f);
    }
    
    /// <summary>
    /// Enables or disables emergency systems.
    /// </summary>
    public void SetEmergencySystemsEnabled(bool enabled)
    {
        enableEmergencySystems = enabled;
    }
    
    /// <summary>
    /// Generates a safety report.
    /// </summary>
    public string GenerateSafetyReport()
    {
        string report = "=== Safety Report ===\n";
        report += $"Active Equipment: {activeEquipment.Count}\n";
        report += $"Safety Events: {safetyEvents.Count}\n";
        report += $"Emergency Active: {isEmergencyActive}\n";
        report += $"Total Events: {safetyEvents.Count}\n";
        report += $"Resolved Events: {safetyEvents.FindAll(e => e.isResolved).Count}\n";
        report += "===================\n";
        
        return report;
    }
    
    /// <summary>
    /// Logs the current safety controller status.
    /// </summary>
    public void LogSafetyControllerStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Safety Controller Status ===");
        Debug.Log($"Active Equipment: {activeEquipment.Count}");
        Debug.Log($"Safety Events: {safetyEvents.Count}");
        Debug.Log($"Is Emergency Active: {isEmergencyActive}");
        Debug.Log($"Equipment Database Size: {equipmentDatabase.Count}");
        Debug.Log($"Safety Controls: {(enableSafetyControls ? "Enabled" : "Disabled")}");
        Debug.Log($"Emergency Systems: {(enableEmergencySystems ? "Enabled" : "Disabled")}");
        Debug.Log($"Safety Equipment: {(enableSafetyEquipment ? "Enabled" : "Disabled")}");
        Debug.Log($"Ventilation Control: {(enableVentilationControl ? "Enabled" : "Disabled")}");
        Debug.Log($"Fire Suppression: {(enableFireSuppression ? "Enabled" : "Disabled")}");
        Debug.Log("===============================");
    }
} 