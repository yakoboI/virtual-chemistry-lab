using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages laboratory equipment and apparatus for the virtual chemistry lab.
/// This component handles all equipment-related operations and interactions.
/// </summary>
public class ApparatusManager : MonoBehaviour
{
    [Header("Apparatus Management")]
    [SerializeField] private bool enableApparatusManagement = true;
    [SerializeField] private bool enableEquipmentInteraction = true;
    [SerializeField] private bool enableEquipmentCalibration = true;
    [SerializeField] private bool enableEquipmentMaintenance = true;
    [SerializeField] private bool enableEquipmentSafety = true;
    
    [Header("Apparatus Configuration")]
    [SerializeField] private ApparatusData[] availableApparatus;
    [SerializeField] private string defaultEquipmentSound = "equipment_use";
    [SerializeField] private string calibrationSound = "equipment_calibrate";
    [SerializeField] private string maintenanceSound = "equipment_maintenance";
    
    [Header("Apparatus State")]
    [SerializeField] private Dictionary<string, ApparatusInstance> activeApparatus = new Dictionary<string, ApparatusInstance>();
    [SerializeField] private List<ApparatusInstance> calibratedEquipment = new List<ApparatusInstance>();
    [SerializeField] private List<ApparatusInstance> maintenanceRequired = new List<ApparatusInstance>();
    [SerializeField] private bool isEquipmentInUse = false;
    
    [Header("Equipment Settings")]
    [SerializeField] private bool enableAutoCalibration = true;
    [SerializeField] private bool enablePreventiveMaintenance = true;
    [SerializeField] private float calibrationInterval = 3600f; // 1 hour
    [SerializeField] private float maintenanceInterval = 7200f; // 2 hours
    [SerializeField] private bool enableEquipmentLogging = true;
    
    [Header("Safety Settings")]
    [SerializeField] private bool enableSafetyChecks = true;
    [SerializeField] private bool enableEquipmentWarnings = true;
    [SerializeField] private bool enableUsageTracking = true;
    [SerializeField] private bool enableTemperatureMonitoring = true;
    
    [Header("Performance")]
    [SerializeField] private bool enableEquipmentPooling = true;
    [SerializeField] private int equipmentPoolSize = 20;
    [SerializeField] private bool enableEquipmentCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private int maxActiveEquipment = 50;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logEquipmentChanges = false;
    
    private static ApparatusManager instance;
    public static ApparatusManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ApparatusManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ApparatusManager");
                    instance = go.AddComponent<ApparatusManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnApparatusCreated;
    public event Action<string> OnApparatusDestroyed;
    public event Action<string> OnEquipmentUsed;
    public event Action<string> OnEquipmentCalibrated;
    public event Action<string> OnEquipmentMaintained;
    public event Action<string> OnEquipmentBroken;
    public event Action<string> OnSafetyViolation;
    public event Action<string> OnEquipmentWarning;
    public event Action<ApparatusInstance> OnEquipmentStatusChanged;
    
    // Private variables
    private Dictionary<string, ApparatusData> apparatusDatabase = new Dictionary<string, ApparatusData>();
    private Queue<ApparatusInstance> equipmentPool = new Queue<ApparatusInstance>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class ApparatusData
    {
        public string id;
        public string name;
        public string description;
        public ApparatusType type;
        public EquipmentCategory category;
        public float capacity;
        public float precision;
        public float accuracy;
        public float maxTemperature;
        public float minTemperature;
        public bool requiresCalibration;
        public bool requiresMaintenance;
        public bool isHazardous;
        public bool requiresTraining;
        public string[] safetyNotes;
        public string[] operatingInstructions;
        public string useSound;
        public string calibrationSound;
        public GameObject prefab;
        public Material[] materials;
    }
    
    [System.Serializable]
    public class ApparatusInstance
    {
        public string id;
        public string apparatusId;
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public GameObject gameObject;
        public bool isActive;
        public bool isInUse;
        public bool isCalibrated;
        public bool needsMaintenance;
        public bool isBroken;
        public float currentTemperature;
        public float usageCount;
        public float lastCalibrationTime;
        public float lastMaintenanceTime;
        public float creationTime;
        public ApparatusStatus status;
    }
    
    [System.Serializable]
    public enum ApparatusType
    {
        Burette,
        Pipette,
        Beaker,
        Flask,
        TestTube,
        Thermometer,
        pHMeter,
        Balance,
        Hotplate,
        BunsenBurner,
        FumeHood,
        Centrifuge,
        Microscope,
        Spectrophotometer,
        TitrationSetup,
        DistillationSetup,
        FilterSetup,
        Other
    }
    
    [System.Serializable]
    public enum EquipmentCategory
    {
        Glassware,
        Measuring,
        Heating,
        Safety,
        Analysis,
        Separation,
        Storage,
        Support
    }
    
    [System.Serializable]
    public enum ApparatusStatus
    {
        Available,
        InUse,
        Calibrating,
        Maintaining,
        Broken,
        OutOfService
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeApparatusManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadApparatusDatabase();
        InitializeEquipmentPool();
    }
    
    private void Update()
    {
        UpdateEquipmentStatus();
    }
    
    /// <summary>
    /// Initializes the apparatus manager.
    /// </summary>
    private void InitializeApparatusManager()
    {
        activeApparatus.Clear();
        calibratedEquipment.Clear();
        maintenanceRequired.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("ApparatusManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads apparatus data from available equipment.
    /// </summary>
    private void LoadApparatusDatabase()
    {
        apparatusDatabase.Clear();
        
        foreach (ApparatusData apparatus in availableApparatus)
        {
            if (apparatus != null && !string.IsNullOrEmpty(apparatus.id))
            {
                apparatusDatabase[apparatus.id] = apparatus;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded apparatus: {apparatus.name} ({apparatus.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {apparatusDatabase.Count} apparatus");
        }
    }
    
    /// <summary>
    /// Initializes the equipment pool.
    /// </summary>
    private void InitializeEquipmentPool()
    {
        if (!enableEquipmentPooling) return;
        
        for (int i = 0; i < equipmentPoolSize; i++)
        {
            ApparatusInstance instance = new ApparatusInstance
            {
                id = $"pooled_{i}",
                isActive = false
            };
            
            equipmentPool.Enqueue(instance);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized equipment pool with {equipmentPoolSize} instances");
        }
    }
    
    /// <summary>
    /// Creates a new apparatus instance.
    /// </summary>
    public ApparatusInstance CreateApparatus(string apparatusId, Vector3 position, Quaternion rotation)
    {
        if (!enableApparatusManagement || !apparatusDatabase.ContainsKey(apparatusId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Apparatus not found: {apparatusId}");
            }
            return null;
        }
        
        if (activeApparatus.Count >= maxActiveEquipment)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("Maximum active equipment reached");
            }
            return null;
        }
        
        ApparatusData apparatusData = apparatusDatabase[apparatusId];
        ApparatusInstance instance = GetPooledEquipment();
        
        if (instance == null)
        {
            instance = new ApparatusInstance();
        }
        
        instance.id = GenerateApparatusId();
        instance.apparatusId = apparatusId;
        instance.name = apparatusData.name;
        instance.position = position;
        instance.rotation = rotation;
        instance.isActive = true;
        instance.isInUse = false;
        instance.isCalibrated = !apparatusData.requiresCalibration;
        instance.needsMaintenance = false;
        instance.isBroken = false;
        instance.currentTemperature = 25f; // Room temperature
        instance.usageCount = 0f;
        instance.lastCalibrationTime = Time.time;
        instance.lastMaintenanceTime = Time.time;
        instance.creationTime = Time.time;
        instance.status = ApparatusStatus.Available;
        
        // Create visual representation
        instance.gameObject = CreateApparatusVisual(apparatusData, position, rotation);
        
        activeApparatus[instance.id] = instance;
        
        OnApparatusCreated?.Invoke(instance.id);
        
        if (logEquipmentChanges)
        {
            Debug.Log($"Created apparatus: {apparatusData.name} ({instance.id}) at {position}");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Destroys an apparatus instance.
    /// </summary>
    public void DestroyApparatus(string instanceId)
    {
        if (!activeApparatus.ContainsKey(instanceId)) return;
        
        ApparatusInstance instance = activeApparatus[instanceId];
        
        if (instance.gameObject != null)
        {
            Destroy(instance.gameObject);
        }
        
        activeApparatus.Remove(instanceId);
        
        // Remove from lists
        calibratedEquipment.Remove(instance);
        maintenanceRequired.Remove(instance);
        
        // Return to pool
        if (enableEquipmentPooling)
        {
            instance.isActive = false;
            equipmentPool.Enqueue(instance);
        }
        
        OnApparatusDestroyed?.Invoke(instanceId);
        
        if (logEquipmentChanges)
        {
            Debug.Log($"Destroyed apparatus: {instanceId}");
        }
    }
    
    /// <summary>
    /// Uses an apparatus.
    /// </summary>
    public bool UseApparatus(string instanceId)
    {
        if (!enableEquipmentInteraction || !activeApparatus.ContainsKey(instanceId)) return false;
        
        ApparatusInstance instance = activeApparatus[instanceId];
        ApparatusData data = apparatusDatabase[instance.apparatusId];
        
        // Check if equipment is available
        if (instance.isInUse || instance.isBroken)
        {
            OnEquipmentWarning?.Invoke($"Equipment {instance.name} is not available for use");
            return false;
        }
        
        // Check if calibration is required
        if (data.requiresCalibration && !instance.isCalibrated)
        {
            OnEquipmentWarning?.Invoke($"Equipment {instance.name} requires calibration");
            return false;
        }
        
        // Check if maintenance is needed
        if (instance.needsMaintenance)
        {
            OnEquipmentWarning?.Invoke($"Equipment {instance.name} requires maintenance");
            return false;
        }
        
        // Use the equipment
        instance.isInUse = true;
        instance.usageCount++;
        instance.status = ApparatusStatus.InUse;
        isEquipmentInUse = true;
        
        // Play use sound
        if (AudioManager.Instance != null)
        {
            string soundName = !string.IsNullOrEmpty(data.useSound) ? 
                data.useSound : defaultEquipmentSound;
            AudioManager.Instance.PlaySFX(soundName);
        }
        
        OnEquipmentUsed?.Invoke(instance.id);
        OnEquipmentStatusChanged?.Invoke(instance);
        
        if (logEquipmentChanges)
        {
            Debug.Log($"Used apparatus: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Stops using an apparatus.
    /// </summary>
    public void StopUsingApparatus(string instanceId)
    {
        if (!activeApparatus.ContainsKey(instanceId)) return;
        
        ApparatusInstance instance = activeApparatus[instanceId];
        
        instance.isInUse = false;
        instance.status = ApparatusStatus.Available;
        
        // Check if equipment needs maintenance
        CheckMaintenanceNeeds(instance);
        
        OnEquipmentStatusChanged?.Invoke(instance);
        
        if (logEquipmentChanges)
        {
            Debug.Log($"Stopped using apparatus: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Calibrates an apparatus.
    /// </summary>
    public bool CalibrateApparatus(string instanceId)
    {
        if (!enableEquipmentCalibration || !activeApparatus.ContainsKey(instanceId)) return false;
        
        ApparatusInstance instance = activeApparatus[instanceId];
        ApparatusData data = apparatusDatabase[instance.apparatusId];
        
        if (!data.requiresCalibration)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Equipment {instance.name} does not require calibration");
            }
            return false;
        }
        
        if (instance.isInUse)
        {
            OnEquipmentWarning?.Invoke($"Cannot calibrate equipment {instance.name} while in use");
            return false;
        }
        
        // Perform calibration
        instance.isCalibrated = true;
        instance.lastCalibrationTime = Time.time;
        instance.status = ApparatusStatus.Calibrating;
        
        // Add to calibrated list
        if (!calibratedEquipment.Contains(instance))
        {
            calibratedEquipment.Add(instance);
        }
        
        // Play calibration sound
        if (AudioManager.Instance != null)
        {
            string soundName = !string.IsNullOrEmpty(data.calibrationSound) ? 
                data.calibrationSound : calibrationSound;
            AudioManager.Instance.PlaySFX(soundName);
        }
        
        OnEquipmentCalibrated?.Invoke(instance.id);
        OnEquipmentStatusChanged?.Invoke(instance);
        
        if (logEquipmentChanges)
        {
            Debug.Log($"Calibrated apparatus: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Performs maintenance on an apparatus.
    /// </summary>
    public bool MaintainApparatus(string instanceId)
    {
        if (!enableEquipmentMaintenance || !activeApparatus.ContainsKey(instanceId)) return false;
        
        ApparatusInstance instance = activeApparatus[instanceId];
        ApparatusData data = apparatusDatabase[instance.apparatusId];
        
        if (!data.requiresMaintenance)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Equipment {instance.name} does not require maintenance");
            }
            return false;
        }
        
        if (instance.isInUse)
        {
            OnEquipmentWarning?.Invoke($"Cannot maintain equipment {instance.name} while in use");
            return false;
        }
        
        // Perform maintenance
        instance.needsMaintenance = false;
        instance.lastMaintenanceTime = Time.time;
        instance.status = ApparatusStatus.Maintaining;
        
        // Remove from maintenance list
        maintenanceRequired.Remove(instance);
        
        // Play maintenance sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(maintenanceSound);
        }
        
        OnEquipmentMaintained?.Invoke(instance.id);
        OnEquipmentStatusChanged?.Invoke(instance);
        
        if (logEquipmentChanges)
        {
            Debug.Log($"Maintained apparatus: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks maintenance needs for an apparatus.
    /// </summary>
    private void CheckMaintenanceNeeds(ApparatusInstance instance)
    {
        if (!enablePreventiveMaintenance) return;
        
        ApparatusData data = apparatusDatabase[instance.apparatusId];
        
        if (!data.requiresMaintenance) return;
        
        float timeSinceMaintenance = Time.time - instance.lastMaintenanceTime;
        
        if (timeSinceMaintenance > maintenanceInterval)
        {
            instance.needsMaintenance = true;
            
            if (!maintenanceRequired.Contains(instance))
            {
                maintenanceRequired.Add(instance);
            }
            
            OnEquipmentWarning?.Invoke($"Equipment {instance.name} requires maintenance");
        }
    }
    
    /// <summary>
    /// Updates equipment status.
    /// </summary>
    private void UpdateEquipmentStatus()
    {
        if (!enableApparatusManagement) return;
        
        foreach (var kvp in activeApparatus)
        {
            ApparatusInstance instance = kvp.Value;
            
            if (instance.isActive)
            {
                // Update temperature
                if (enableTemperatureMonitoring)
                {
                    UpdateTemperature(instance);
                }
                
                // Check calibration status
                if (enableAutoCalibration)
                {
                    CheckCalibrationStatus(instance);
                }
                
                // Check safety conditions
                if (enableSafetyChecks)
                {
                    CheckSafetyConditions(instance);
                }
            }
        }
        
        // Update global equipment state
        isEquipmentInUse = false;
        foreach (var kvp in activeApparatus)
        {
            if (kvp.Value.isInUse)
            {
                isEquipmentInUse = true;
                break;
            }
        }
    }
    
    /// <summary>
    /// Updates temperature for an apparatus.
    /// </summary>
    private void UpdateTemperature(ApparatusInstance instance)
    {
        ApparatusData data = apparatusDatabase[instance.apparatusId];
        
        // Simple temperature simulation - can be enhanced
        float targetTemp = 25f; // Room temperature
        float tempChange = (targetTemp - instance.currentTemperature) * Time.deltaTime * 0.1f;
        instance.currentTemperature += tempChange;
        
        // Check temperature limits
        if (instance.currentTemperature > data.maxTemperature || instance.currentTemperature < data.minTemperature)
        {
            OnEquipmentWarning?.Invoke($"Equipment {instance.name} temperature out of range: {instance.currentTemperature:F1}°C");
        }
    }
    
    /// <summary>
    /// Checks calibration status for an apparatus.
    /// </summary>
    private void CheckCalibrationStatus(ApparatusInstance instance)
    {
        ApparatusData data = apparatusDatabase[instance.apparatusId];
        
        if (!data.requiresCalibration) return;
        
        float timeSinceCalibration = Time.time - instance.lastCalibrationTime;
        
        if (timeSinceCalibration > calibrationInterval)
        {
            instance.isCalibrated = false;
            calibratedEquipment.Remove(instance);
            
            OnEquipmentWarning?.Invoke($"Equipment {instance.name} requires recalibration");
        }
    }
    
    /// <summary>
    /// Checks safety conditions for an apparatus.
    /// </summary>
    private void CheckSafetyConditions(ApparatusInstance instance)
    {
        ApparatusData data = apparatusDatabase[instance.apparatusId];
        
        if (data.isHazardous && instance.isInUse)
        {
            // Check for safety violations
            if (instance.currentTemperature > data.maxTemperature)
            {
                OnSafetyViolation?.Invoke($"Safety violation: {instance.name} temperature too high");
            }
        }
    }
    
    /// <summary>
    /// Creates visual representation of an apparatus.
    /// </summary>
    private GameObject CreateApparatusVisual(ApparatusData data, Vector3 position, Quaternion rotation)
    {
        GameObject visual;
        
        if (data.prefab != null)
        {
            visual = Instantiate(data.prefab, position, rotation);
        }
        else
        {
            // Create default visual
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = $"Apparatus_{data.name}";
            visual.transform.position = position;
            visual.transform.rotation = rotation;
            visual.transform.localScale = Vector3.one * 0.2f;
        }
        
        // Set materials if available
        if (data.materials != null && data.materials.Length > 0)
        {
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = data.materials[0];
            }
        }
        
        return visual;
    }
    
    /// <summary>
    /// Gets a pooled equipment instance.
    /// </summary>
    private ApparatusInstance GetPooledEquipment()
    {
        if (!enableEquipmentPooling || equipmentPool.Count == 0)
        {
            return null;
        }
        
        return equipmentPool.Dequeue();
    }
    
    /// <summary>
    /// Generates a unique apparatus instance ID.
    /// </summary>
    private string GenerateApparatusId()
    {
        return $"app_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsEquipmentInUse() => isEquipmentInUse;
    public int GetActiveApparatusCount() => activeApparatus.Count;
    public int GetCalibratedEquipmentCount() => calibratedEquipment.Count;
    public int GetMaintenanceRequiredCount() => maintenanceRequired.Count;
    
    /// <summary>
    /// Gets an apparatus instance by ID.
    /// </summary>
    public ApparatusInstance GetApparatus(string instanceId)
    {
        return activeApparatus.ContainsKey(instanceId) ? activeApparatus[instanceId] : null;
    }
    
    /// <summary>
    /// Gets apparatus data by ID.
    /// </summary>
    public ApparatusData GetApparatusData(string apparatusId)
    {
        return apparatusDatabase.ContainsKey(apparatusId) ? apparatusDatabase[apparatusId] : null;
    }
    
    /// <summary>
    /// Gets all available apparatus IDs.
    /// </summary>
    public List<string> GetAvailableApparatusIds()
    {
        return new List<string>(apparatusDatabase.Keys);
    }
    
    /// <summary>
    /// Gets apparatus by type.
    /// </summary>
    public List<ApparatusInstance> GetApparatusByType(ApparatusType type)
    {
        List<ApparatusInstance> result = new List<ApparatusInstance>();
        
        foreach (var kvp in activeApparatus)
        {
            ApparatusInstance instance = kvp.Value;
            ApparatusData data = apparatusDatabase[instance.apparatusId];
            
            if (data.type == type)
            {
                result.Add(instance);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets available apparatus (not in use).
    /// </summary>
    public List<ApparatusInstance> GetAvailableApparatus()
    {
        List<ApparatusInstance> result = new List<ApparatusInstance>();
        
        foreach (var kvp in activeApparatus)
        {
            ApparatusInstance instance = kvp.Value;
            
            if (!instance.isInUse && !instance.isBroken)
            {
                result.Add(instance);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Sets the calibration interval.
    /// </summary>
    public void SetCalibrationInterval(float interval)
    {
        calibrationInterval = Mathf.Clamp(interval, 60f, 86400f); // 1 minute to 24 hours
    }
    
    /// <summary>
    /// Sets the maintenance interval.
    /// </summary>
    public void SetMaintenanceInterval(float interval)
    {
        maintenanceInterval = Mathf.Clamp(interval, 300f, 86400f); // 5 minutes to 24 hours
    }
    
    /// <summary>
    /// Enables or disables equipment calibration.
    /// </summary>
    public void SetEquipmentCalibrationEnabled(bool enabled)
    {
        enableEquipmentCalibration = enabled;
    }
    
    /// <summary>
    /// Enables or disables equipment maintenance.
    /// </summary>
    public void SetEquipmentMaintenanceEnabled(bool enabled)
    {
        enableEquipmentMaintenance = enabled;
    }
    
    /// <summary>
    /// Logs the current apparatus manager status.
    /// </summary>
    public void LogApparatusStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Apparatus Manager Status ===");
        Debug.Log($"Active Apparatus: {activeApparatus.Count}");
        Debug.Log($"Calibrated Equipment: {calibratedEquipment.Count}");
        Debug.Log($"Maintenance Required: {maintenanceRequired.Count}");
        Debug.Log($"Is Equipment In Use: {isEquipmentInUse}");
        Debug.Log($"Apparatus Database Size: {apparatusDatabase.Count}");
        Debug.Log($"Equipment Calibration: {(enableEquipmentCalibration ? "Enabled" : "Disabled")}");
        Debug.Log($"Equipment Maintenance: {(enableEquipmentMaintenance ? "Enabled" : "Disabled")}");
        Debug.Log($"Auto Calibration: {(enableAutoCalibration ? "Enabled" : "Disabled")}");
        Debug.Log($"Preventive Maintenance: {(enablePreventiveMaintenance ? "Enabled" : "Disabled")}");
        Debug.Log($"Safety Checks: {(enableSafetyChecks ? "Enabled" : "Disabled")}");
        Debug.Log($"Equipment Pooling: {(enableEquipmentPooling ? "Enabled" : "Disabled")}");
        Debug.Log($"Equipment Pool Size: {equipmentPool.Count}");
        Debug.Log("===============================");
    }
} 