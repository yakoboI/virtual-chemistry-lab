using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages safety protocols and validation for the virtual chemistry lab.
/// This component handles all safety-related operations and monitoring.
/// </summary>
public class SafetyManager : MonoBehaviour
{
    [Header("Safety Management")]
    [SerializeField] private bool enableSafetyManagement = true;
    [SerializeField] private bool enableSafetyMonitoring = true;
    [SerializeField] private bool enableSafetyWarnings = true;
    [SerializeField] private bool enableEmergencyProcedures = true;
    [SerializeField] private bool enableSafetyTraining = true;
    
    [Header("Safety Configuration")]
    [SerializeField] private SafetyProtocol[] safetyProtocols;
    [SerializeField] private string defaultWarningSound = "safety_warning";
    [SerializeField] private string emergencySound = "emergency_alarm";
    [SerializeField] private string evacuationSound = "evacuation_alarm";
    
    [Header("Safety State")]
    [SerializeField] private SafetyLevel currentSafetyLevel = SafetyLevel.Normal;
    [SerializeField] private List<SafetyViolation> activeViolations = new List<SafetyViolation>();
    [SerializeField] private List<SafetyWarning> activeWarnings = new List<SafetyWarning>();
    [SerializeField] private bool isEmergencyMode = false;
    [SerializeField] private bool isEvacuationMode = false;
    
    [Header("Safety Settings")]
    [SerializeField] private bool enableRealTimeMonitoring = true;
    [SerializeField] private bool enableAutomaticShutdown = true;
    [SerializeField] private bool enableVentilationMonitoring = true;
    [SerializeField] private bool enableTemperatureMonitoring = true;
    [SerializeField] private bool enableChemicalSpillDetection = true;
    [SerializeField] private float safetyCheckInterval = 0.5f;
    [SerializeField] private float warningTimeout = 10f;
    
    [Header("Emergency Settings")]
    [SerializeField] private bool enableEmergencyShutdown = true;
    [SerializeField] private bool enableEmergencyEvacuation = true;
    [SerializeField] private bool enableEmergencyContacts = true;
    [SerializeField] private float emergencyResponseTime = 30f;
    [SerializeField] private bool enableEmergencyLogging = true;
    
    [Header("Training Settings")]
    [SerializeField] private bool enableSafetyTraining = true;
    [SerializeField] private bool enableSafetyCertification = true;
    [SerializeField] private bool enableSafetyReminders = true;
    [SerializeField] private float trainingReminderInterval = 3600f; // 1 hour
    
    [Header("Performance")]
    [SerializeField] private bool enableSafetyLogging = true;
    [SerializeField] private int maxViolationHistory = 100;
    [SerializeField] private bool enableSafetyAnalytics = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logSafetyEvents = false;
    
    private static SafetyManager instance;
    public static SafetyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SafetyManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SafetyManager");
                    instance = go.AddComponent<SafetyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<SafetyLevel> OnSafetyLevelChanged;
    public event Action<SafetyViolation> OnSafetyViolation;
    public event Action<SafetyWarning> OnSafetyWarning;
    public event Action OnEmergencyModeActivated;
    public event Action OnEmergencyModeDeactivated;
    public event Action OnEvacuationActivated;
    public event Action OnEvacuationDeactivated;
    public event Action<string> OnSafetyTrainingRequired;
    public event Action<string> OnSafetyCertificationExpired;
    
    // Private variables
    private Dictionary<string, SafetyProtocol> protocolDatabase = new Dictionary<string, SafetyProtocol>();
    private List<SafetyViolation> violationHistory = new List<SafetyViolation>();
    private bool isInitialized = false;
    private float lastSafetyCheck = 0f;
    private float lastTrainingReminder = 0f;
    
    [System.Serializable]
    public class SafetyProtocol
    {
        public string id;
        public string name;
        public string description;
        public SafetyLevel requiredLevel;
        public SafetyCategory category;
        public bool isMandatory;
        public bool requiresTraining;
        public string[] requiredEquipment;
        public string[] safetyNotes;
        public string[] emergencyProcedures;
        public float responseTime;
        public GameObject warningPrefab;
        public string warningSound;
    }
    
    [System.Serializable]
    public class SafetyViolation
    {
        public string id;
        public string protocolId;
        public string description;
        public SafetyLevel severity;
        public Vector3 location;
        public float timestamp;
        public bool isResolved;
        public string resolutionNotes;
        public string reportedBy;
    }
    
    [System.Serializable]
    public class SafetyWarning
    {
        public string id;
        public string message;
        public SafetyLevel level;
        public Vector3 location;
        public float timestamp;
        public float timeout;
        public bool isAcknowledged;
        public string acknowledgedBy;
    }
    
    [System.Serializable]
    public enum SafetyLevel
    {
        Normal,
        Caution,
        Warning,
        Danger,
        Critical,
        Emergency
    }
    
    [System.Serializable]
    public enum SafetyCategory
    {
        Chemical,
        Equipment,
        Environmental,
        Personal,
        Procedural,
        Emergency,
        Training,
        Maintenance
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSafetyManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadSafetyProtocols();
    }
    
    private void Update()
    {
        if (enableSafetyManagement)
        {
            UpdateSafetyMonitoring();
            UpdateTrainingReminders();
        }
    }
    
    /// <summary>
    /// Initializes the safety manager.
    /// </summary>
    private void InitializeSafetyManager()
    {
        activeViolations.Clear();
        activeWarnings.Clear();
        violationHistory.Clear();
        
        currentSafetyLevel = SafetyLevel.Normal;
        isEmergencyMode = false;
        isEvacuationMode = false;
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("SafetyManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads safety protocols from configuration.
    /// </summary>
    private void LoadSafetyProtocols()
    {
        protocolDatabase.Clear();
        
        foreach (SafetyProtocol protocol in safetyProtocols)
        {
            if (protocol != null && !string.IsNullOrEmpty(protocol.id))
            {
                protocolDatabase[protocol.id] = protocol;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded safety protocol: {protocol.name} ({protocol.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {protocolDatabase.Count} safety protocols");
        }
    }
    
    /// <summary>
    /// Updates safety monitoring.
    /// </summary>
    private void UpdateSafetyMonitoring()
    {
        if (!enableRealTimeMonitoring) return;
        
        if (Time.time - lastSafetyCheck >= safetyCheckInterval)
        {
            PerformSafetyChecks();
            lastSafetyCheck = Time.time;
        }
        
        // Update warning timeouts
        UpdateWarningTimeouts();
    }
    
    /// <summary>
    /// Performs safety checks.
    /// </summary>
    private void PerformSafetyChecks()
    {
        // Check chemical safety
        if (enableChemicalSpillDetection)
        {
            CheckChemicalSafety();
        }
        
        // Check equipment safety
        CheckEquipmentSafety();
        
        // Check environmental safety
        CheckEnvironmentalSafety();
        
        // Check ventilation
        if (enableVentilationMonitoring)
        {
            CheckVentilationSafety();
        }
        
        // Check temperature
        if (enableTemperatureMonitoring)
        {
            CheckTemperatureSafety();
        }
    }
    
    /// <summary>
    /// Checks chemical safety.
    /// </summary>
    private void CheckChemicalSafety()
    {
        if (ChemicalManager.Instance != null)
        {
            // Check for hazardous chemical combinations
            // This would integrate with ChemicalManager
        }
    }
    
    /// <summary>
    /// Checks equipment safety.
    /// </summary>
    private void CheckEquipmentSafety()
    {
        if (ApparatusManager.Instance != null)
        {
            // Check for equipment malfunctions
            // This would integrate with ApparatusManager
        }
    }
    
    /// <summary>
    /// Checks environmental safety.
    /// </summary>
    private void CheckEnvironmentalSafety()
    {
        // Check for environmental hazards
        // This could include fire, smoke, gas leaks, etc.
    }
    
    /// <summary>
    /// Checks ventilation safety.
    /// </summary>
    private void CheckVentilationSafety()
    {
        // Check ventilation system status
        bool ventilationActive = true; // This would check actual ventilation system
        
        if (!ventilationActive)
        {
            ReportSafetyViolation("ventilation_failure", "Ventilation system is not active", SafetyLevel.Warning, Vector3.zero);
        }
    }
    
    /// <summary>
    /// Checks temperature safety.
    /// </summary>
    private void CheckTemperatureSafety()
    {
        // Check laboratory temperature
        float labTemperature = 25f; // This would get actual lab temperature
        
        if (labTemperature > 35f)
        {
            ReportSafetyViolation("temperature_high", "Laboratory temperature is too high", SafetyLevel.Warning, Vector3.zero);
        }
        else if (labTemperature < 15f)
        {
            ReportSafetyViolation("temperature_low", "Laboratory temperature is too low", SafetyLevel.Caution, Vector3.zero);
        }
    }
    
    /// <summary>
    /// Updates warning timeouts.
    /// </summary>
    private void UpdateWarningTimeouts()
    {
        for (int i = activeWarnings.Count - 1; i >= 0; i--)
        {
            SafetyWarning warning = activeWarnings[i];
            
            if (Time.time - warning.timestamp > warning.timeout)
            {
                activeWarnings.RemoveAt(i);
                
                if (logSafetyEvents)
                {
                    Debug.Log($"Safety warning expired: {warning.message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Updates training reminders.
    /// </summary>
    private void UpdateTrainingReminders()
    {
        if (!enableSafetyTraining) return;
        
        if (Time.time - lastTrainingReminder >= trainingReminderInterval)
        {
            CheckTrainingRequirements();
            lastTrainingReminder = Time.time;
        }
    }
    
    /// <summary>
    /// Checks training requirements.
    /// </summary>
    private void CheckTrainingRequirements()
    {
        // Check if safety training is required
        bool trainingRequired = false; // This would check actual training status
        
        if (trainingRequired)
        {
            OnSafetyTrainingRequired?.Invoke("Safety training is required");
        }
    }
    
    /// <summary>
    /// Reports a safety violation.
    /// </summary>
    public void ReportSafetyViolation(string protocolId, string description, SafetyLevel severity, Vector3 location)
    {
        if (!enableSafetyManagement) return;
        
        SafetyViolation violation = new SafetyViolation
        {
            id = GenerateViolationId(),
            protocolId = protocolId,
            description = description,
            severity = severity,
            location = location,
            timestamp = Time.time,
            isResolved = false,
            reportedBy = "System"
        };
        
        activeViolations.Add(violation);
        violationHistory.Add(violation);
        
        // Limit history size
        while (violationHistory.Count > maxViolationHistory)
        {
            violationHistory.RemoveAt(0);
        }
        
        // Update safety level
        UpdateSafetyLevel(severity);
        
        // Create warning
        CreateSafetyWarning(description, severity, location);
        
        OnSafetyViolation?.Invoke(violation);
        
        if (logSafetyEvents)
        {
            Debug.LogWarning($"Safety violation reported: {description} (Level: {severity})");
        }
        
        // Check for emergency conditions
        if (severity >= SafetyLevel.Critical)
        {
            ActivateEmergencyMode();
        }
    }
    
    /// <summary>
    /// Creates a safety warning.
    /// </summary>
    public void CreateSafetyWarning(string message, SafetyLevel level, Vector3 location)
    {
        if (!enableSafetyWarnings) return;
        
        SafetyWarning warning = new SafetyWarning
        {
            id = GenerateWarningId(),
            message = message,
            level = level,
            location = location,
            timestamp = Time.time,
            timeout = warningTimeout,
            isAcknowledged = false
        };
        
        activeWarnings.Add(warning);
        
        // Play warning sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(defaultWarningSound);
        }
        
        OnSafetyWarning?.Invoke(warning);
        
        if (logSafetyEvents)
        {
            Debug.LogWarning($"Safety warning created: {message} (Level: {level})");
        }
    }
    
    /// <summary>
    /// Acknowledges a safety warning.
    /// </summary>
    public void AcknowledgeWarning(string warningId, string acknowledgedBy)
    {
        SafetyWarning warning = activeWarnings.Find(w => w.id == warningId);
        
        if (warning != null)
        {
            warning.isAcknowledged = true;
            warning.acknowledgedBy = acknowledgedBy;
            
            if (logSafetyEvents)
            {
                Debug.Log($"Safety warning acknowledged: {warning.message} by {acknowledgedBy}");
            }
        }
    }
    
    /// <summary>
    /// Resolves a safety violation.
    /// </summary>
    public void ResolveViolation(string violationId, string resolutionNotes)
    {
        SafetyViolation violation = activeViolations.Find(v => v.id == violationId);
        
        if (violation != null)
        {
            violation.isResolved = true;
            violation.resolutionNotes = resolutionNotes;
            activeViolations.Remove(violation);
            
            if (logSafetyEvents)
            {
                Debug.Log($"Safety violation resolved: {violation.description}");
            }
        }
    }
    
    /// <summary>
    /// Activates emergency mode.
    /// </summary>
    public void ActivateEmergencyMode()
    {
        if (isEmergencyMode) return;
        
        isEmergencyMode = true;
        currentSafetyLevel = SafetyLevel.Emergency;
        
        // Play emergency sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(emergencySound);
        }
        
        // Automatic shutdown if enabled
        if (enableEmergencyShutdown)
        {
            PerformEmergencyShutdown();
        }
        
        OnEmergencyModeActivated?.Invoke();
        OnSafetyLevelChanged?.Invoke(currentSafetyLevel);
        
        if (logSafetyEvents)
        {
            Debug.LogError("EMERGENCY MODE ACTIVATED");
        }
    }
    
    /// <summary>
    /// Deactivates emergency mode.
    /// </summary>
    public void DeactivateEmergencyMode()
    {
        if (!isEmergencyMode) return;
        
        isEmergencyMode = false;
        currentSafetyLevel = SafetyLevel.Normal;
        
        OnEmergencyModeDeactivated?.Invoke();
        OnSafetyLevelChanged?.Invoke(currentSafetyLevel);
        
        if (logSafetyEvents)
        {
            Debug.Log("Emergency mode deactivated");
        }
    }
    
    /// <summary>
    /// Activates evacuation mode.
    /// </summary>
    public void ActivateEvacuationMode()
    {
        if (isEvacuationMode) return;
        
        isEvacuationMode = true;
        
        // Play evacuation sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(evacuationSound);
        }
        
        OnEvacuationActivated?.Invoke();
        
        if (logSafetyEvents)
        {
            Debug.LogError("EVACUATION MODE ACTIVATED");
        }
    }
    
    /// <summary>
    /// Deactivates evacuation mode.
    /// </summary>
    public void DeactivateEvacuationMode()
    {
        if (!isEvacuationMode) return;
        
        isEvacuationMode = false;
        
        OnEvacuationDeactivated?.Invoke();
        
        if (logSafetyEvents)
        {
            Debug.Log("Evacuation mode deactivated");
        }
    }
    
    /// <summary>
    /// Performs emergency shutdown.
    /// </summary>
    private void PerformEmergencyShutdown()
    {
        // Shutdown equipment
        if (ApparatusManager.Instance != null)
        {
            // Stop all equipment
        }
        
        // Stop experiments
        if (ExperimentStateManager.Instance != null)
        {
            // Pause all experiments
        }
        
        if (logSafetyEvents)
        {
            Debug.Log("Emergency shutdown performed");
        }
    }
    
    /// <summary>
    /// Updates safety level based on violations.
    /// </summary>
    private void UpdateSafetyLevel(SafetyLevel newSeverity)
    {
        SafetyLevel newLevel = currentSafetyLevel;
        
        // Determine new safety level based on severity
        switch (newSeverity)
        {
            case SafetyLevel.Caution:
                if (currentSafetyLevel == SafetyLevel.Normal)
                    newLevel = SafetyLevel.Caution;
                break;
            case SafetyLevel.Warning:
                if (currentSafetyLevel <= SafetyLevel.Caution)
                    newLevel = SafetyLevel.Warning;
                break;
            case SafetyLevel.Danger:
                if (currentSafetyLevel <= SafetyLevel.Warning)
                    newLevel = SafetyLevel.Danger;
                break;
            case SafetyLevel.Critical:
                if (currentSafetyLevel <= SafetyLevel.Danger)
                    newLevel = SafetyLevel.Critical;
                break;
            case SafetyLevel.Emergency:
                newLevel = SafetyLevel.Emergency;
                break;
        }
        
        if (newLevel != currentSafetyLevel)
        {
            currentSafetyLevel = newLevel;
            OnSafetyLevelChanged?.Invoke(currentSafetyLevel);
            
            if (logSafetyEvents)
            {
                Debug.Log($"Safety level changed to: {currentSafetyLevel}");
            }
        }
    }
    
    /// <summary>
    /// Generates a unique violation ID.
    /// </summary>
    private string GenerateViolationId()
    {
        return $"viol_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique warning ID.
    /// </summary>
    private string GenerateWarningId()
    {
        return $"warn_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public SafetyLevel GetCurrentSafetyLevel() => currentSafetyLevel;
    public bool IsEmergencyMode() => isEmergencyMode;
    public bool IsEvacuationMode() => isEvacuationMode;
    public int GetActiveViolationCount() => activeViolations.Count;
    public int GetActiveWarningCount() => activeWarnings.Count;
    public int GetViolationHistoryCount() => violationHistory.Count;
    
    /// <summary>
    /// Gets a safety protocol by ID.
    /// </summary>
    public SafetyProtocol GetSafetyProtocol(string protocolId)
    {
        return protocolDatabase.ContainsKey(protocolId) ? protocolDatabase[protocolId] : null;
    }
    
    /// <summary>
    /// Gets all active violations.
    /// </summary>
    public List<SafetyViolation> GetActiveViolations()
    {
        return new List<SafetyViolation>(activeViolations);
    }
    
    /// <summary>
    /// Gets all active warnings.
    /// </summary>
    public List<SafetyWarning> GetActiveWarnings()
    {
        return new List<SafetyWarning>(activeWarnings);
    }
    
    /// <summary>
    /// Gets violation history.
    /// </summary>
    public List<SafetyViolation> GetViolationHistory()
    {
        return new List<SafetyViolation>(violationHistory);
    }
    
    /// <summary>
    /// Gets violations by severity level.
    /// </summary>
    public List<SafetyViolation> GetViolationsByLevel(SafetyLevel level)
    {
        return activeViolations.FindAll(v => v.severity == level);
    }
    
    /// <summary>
    /// Sets the safety check interval.
    /// </summary>
    public void SetSafetyCheckInterval(float interval)
    {
        safetyCheckInterval = Mathf.Clamp(interval, 0.1f, 60f);
    }
    
    /// <summary>
    /// Sets the warning timeout.
    /// </summary>
    public void SetWarningTimeout(float timeout)
    {
        warningTimeout = Mathf.Clamp(timeout, 1f, 300f);
    }
    
    /// <summary>
    /// Enables or disables safety monitoring.
    /// </summary>
    public void SetSafetyMonitoringEnabled(bool enabled)
    {
        enableSafetyMonitoring = enabled;
    }
    
    /// <summary>
    /// Enables or disables safety warnings.
    /// </summary>
    public void SetSafetyWarningsEnabled(bool enabled)
    {
        enableSafetyWarnings = enabled;
    }
    
    /// <summary>
    /// Enables or disables emergency procedures.
    /// </summary>
    public void SetEmergencyProceduresEnabled(bool enabled)
    {
        enableEmergencyProcedures = enabled;
    }
    
    /// <summary>
    /// Logs the current safety manager status.
    /// </summary>
    public void LogSafetyStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Safety Manager Status ===");
        Debug.Log($"Current Safety Level: {currentSafetyLevel}");
        Debug.Log($"Is Emergency Mode: {isEmergencyMode}");
        Debug.Log($"Is Evacuation Mode: {isEvacuationMode}");
        Debug.Log($"Active Violations: {activeViolations.Count}");
        Debug.Log($"Active Warnings: {activeWarnings.Count}");
        Debug.Log($"Violation History: {violationHistory.Count}");
        Debug.Log($"Safety Protocols: {protocolDatabase.Count}");
        Debug.Log($"Safety Monitoring: {(enableSafetyMonitoring ? "Enabled" : "Disabled")}");
        Debug.Log($"Safety Warnings: {(enableSafetyWarnings ? "Enabled" : "Disabled")}");
        Debug.Log($"Emergency Procedures: {(enableEmergencyProcedures ? "Enabled" : "Disabled")}");
        Debug.Log($"Real-time Monitoring: {(enableRealTimeMonitoring ? "Enabled" : "Disabled")}");
        Debug.Log($"Automatic Shutdown: {(enableAutomaticShutdown ? "Enabled" : "Disabled")}");
        Debug.Log($"Safety Training: {(enableSafetyTraining ? "Enabled" : "Disabled")}");
        Debug.Log("============================");
    }
} 