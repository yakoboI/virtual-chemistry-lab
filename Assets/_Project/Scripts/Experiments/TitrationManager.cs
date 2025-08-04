using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages titration experiments and procedures for the virtual chemistry lab.
/// This component handles all titration-related operations and calculations.
/// </summary>
public class TitrationManager : MonoBehaviour
{
    [Header("Titration Management")]
    [SerializeField] private bool enableTitrationManagement = true;
    [SerializeField] private bool enableTitrationSimulation = true;
    [SerializeField] private bool enableEndpointDetection = true;
    [SerializeField] private bool enableTitrationValidation = true;
    [SerializeField] private bool enableTitrationAnalysis = true;
    
    [Header("Titration Configuration")]
    [SerializeField] private TitrationData[] availableTitrations;
    [SerializeField] private string buretteSound = "burette_drop";
    [SerializeField] private string endpointSound = "endpoint_reached";
    [SerializeField] private string indicatorSound = "indicator_change";
    
    [Header("Titration State")]
    [SerializeField] private Dictionary<string, TitrationInstance> activeTitrations = new Dictionary<string, TitrationInstance>();
    [SerializeField] private List<TitrationResult> titrationResults = new List<TitrationResult>();
    [SerializeField] private bool isTitrationInProgress = false;
    
    [Header("Titration Settings")]
    [SerializeField] private bool enableRealTimeTitration = true;
    [SerializeField] private bool enableAutoEndpointDetection = true;
    [SerializeField] private bool enableTitrationLogging = true;
    [SerializeField] private float dropVolume = 0.1f; // mL per drop
    [SerializeField] private float titrationSpeed = 1.0f;
    [SerializeField] private float endpointThreshold = 0.05f;
    [SerializeField] private float colorChangeThreshold = 0.02f;
    
    [Header("Analysis Settings")]
    [SerializeField] private bool enableConcentrationCalculation = true;
    [SerializeField] private bool enableAccuracyAssessment = true;
    [SerializeField] private bool enablePrecisionAnalysis = true;
    [SerializeField] private float acceptableError = 0.5f; // 0.5%
    [SerializeField] private int minTitrationsForAnalysis = 3;
    
    [Header("Visual Settings")]
    [SerializeField] private bool enableColorChanges = true;
    [SerializeField] private bool enableBubbleEffects = true;
    [SerializeField] private bool enableVolumeDisplay = true;
    [SerializeField] private bool enablepHDisplay = true;
    [SerializeField] private float effectIntensity = 1.0f;
    
    [Header("Performance")]
    [SerializeField] private bool enableTitrationPooling = true;
    [SerializeField] private int titrationPoolSize = 10;
    [SerializeField] private bool enableTitrationCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private int maxActiveTitrations = 20;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logTitrationChanges = false;
    
    private static TitrationManager instance;
    public static TitrationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TitrationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TitrationManager");
                    instance = go.AddComponent<TitrationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnTitrationStarted;
    public event Action<string> OnTitrationCompleted;
    public event Action<string> OnEndpointReached;
    public event Action<string> OnIndicatorChanged;
    public event Action<TitrationResult> OnTitrationResult;
    public event Action<float> OnVolumeAdded;
    public event Action<float> OnpHChanged;
    public event Action<string> OnTitrationError;
    
    // Private variables
    private Dictionary<string, TitrationData> titrationDatabase = new Dictionary<string, TitrationData>();
    private Queue<TitrationInstance> titrationPool = new Queue<TitrationInstance>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class TitrationData
    {
        public string id;
        public string name;
        public string description;
        public TitrationType type;
        public string analyteId;
        public string titrantId;
        public string indicatorId;
        public float analyteConcentration;
        public float titrantConcentration;
        public float analyteVolume;
        public float expectedEndpoint;
        public float endpointTolerance;
        public string[] safetyNotes;
        public string[] procedureSteps;
        public GameObject titrationSetup;
        public Material[] indicatorMaterials;
    }
    
    [System.Serializable]
    public class TitrationInstance
    {
        public string id;
        public string titrationId;
        public string name;
        public Vector3 position;
        public GameObject gameObject;
        public bool isActive;
        public bool isInProgress;
        public bool isCompleted;
        public float currentVolume;
        public float totalVolumeAdded;
        public float currentpH;
        public float endpointVolume;
        public float startTime;
        public float completionTime;
        public TitrationStatus status;
        public List<TitrationPoint> titrationCurve;
    }
    
    [System.Serializable]
    public class TitrationResult
    {
        public string id;
        public string titrationId;
        public float endpointVolume;
        public float calculatedConcentration;
        public float actualConcentration;
        public float accuracy;
        public float precision;
        public float completionTime;
        public bool isSuccessful;
        public string errorMessage;
        public List<TitrationPoint> titrationCurve;
    }
    
    [System.Serializable]
    public class TitrationPoint
    {
        public float volume;
        public float pH;
        public Color color;
        public float timestamp;
    }
    
    [System.Serializable]
    public enum TitrationType
    {
        AcidBase,
        Redox,
        Complexometric,
        Precipitation,
        BackTitration,
        DoubleIndicator
    }
    
    [System.Serializable]
    public enum TitrationStatus
    {
        Setup,
        InProgress,
        EndpointReached,
        Completed,
        Failed,
        Cancelled
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTitrationManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadTitrationDatabase();
        InitializeTitrationPool();
    }
    
    private void Update()
    {
        UpdateTitrationProgress();
    }
    
    /// <summary>
    /// Initializes the titration manager.
    /// </summary>
    private void InitializeTitrationManager()
    {
        activeTitrations.Clear();
        titrationResults.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("TitrationManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads titration data from available titrations.
    /// </summary>
    private void LoadTitrationDatabase()
    {
        titrationDatabase.Clear();
        
        foreach (TitrationData titration in availableTitrations)
        {
            if (titration != null && !string.IsNullOrEmpty(titration.id))
            {
                titrationDatabase[titration.id] = titration;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded titration: {titration.name} ({titration.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {titrationDatabase.Count} titrations");
        }
    }
    
    /// <summary>
    /// Initializes the titration pool.
    /// </summary>
    private void InitializeTitrationPool()
    {
        if (!enableTitrationPooling) return;
        
        for (int i = 0; i < titrationPoolSize; i++)
        {
            TitrationInstance instance = new TitrationInstance
            {
                id = $"pooled_{i}",
                isActive = false
            };
            
            titrationPool.Enqueue(instance);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized titration pool with {titrationPoolSize} instances");
        }
    }
    
    /// <summary>
    /// Creates a new titration instance.
    /// </summary>
    public TitrationInstance CreateTitration(string titrationId, Vector3 position)
    {
        if (!enableTitrationManagement || !titrationDatabase.ContainsKey(titrationId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Titration not found: {titrationId}");
            }
            return null;
        }
        
        if (activeTitrations.Count >= maxActiveTitrations)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("Maximum active titrations reached");
            }
            return null;
        }
        
        TitrationData titrationData = titrationDatabase[titrationId];
        TitrationInstance instance = GetPooledTitration();
        
        if (instance == null)
        {
            instance = new TitrationInstance();
        }
        
        instance.id = GenerateTitrationId();
        instance.titrationId = titrationId;
        instance.name = titrationData.name;
        instance.position = position;
        instance.isActive = true;
        instance.isInProgress = false;
        instance.isCompleted = false;
        instance.currentVolume = 0f;
        instance.totalVolumeAdded = 0f;
        instance.currentpH = 7.0f; // Neutral pH
        instance.endpointVolume = 0f;
        instance.startTime = 0f;
        instance.completionTime = 0f;
        instance.status = TitrationStatus.Setup;
        instance.titrationCurve = new List<TitrationPoint>();
        
        // Create visual representation
        instance.gameObject = CreateTitrationVisual(titrationData, position);
        
        activeTitrations[instance.id] = instance;
        
        OnTitrationStarted?.Invoke(instance.id);
        
        if (logTitrationChanges)
        {
            Debug.Log($"Created titration: {titrationData.name} ({instance.id}) at {position}");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Destroys a titration instance.
    /// </summary>
    public void DestroyTitration(string instanceId)
    {
        if (!activeTitrations.ContainsKey(instanceId)) return;
        
        TitrationInstance instance = activeTitrations[instanceId];
        
        if (instance.gameObject != null)
        {
            Destroy(instance.gameObject);
        }
        
        activeTitrations.Remove(instanceId);
        
        // Return to pool
        if (enableTitrationPooling)
        {
            instance.isActive = false;
            titrationPool.Enqueue(instance);
        }
        
        if (logTitrationChanges)
        {
            Debug.Log($"Destroyed titration: {instanceId}");
        }
    }
    
    /// <summary>
    /// Starts a titration.
    /// </summary>
    public bool StartTitration(string instanceId)
    {
        if (!enableTitrationSimulation || !activeTitrations.ContainsKey(instanceId)) return false;
        
        TitrationInstance instance = activeTitrations[instanceId];
        TitrationData data = titrationDatabase[instance.titrationId];
        
        if (instance.isInProgress || instance.isCompleted)
        {
            OnTitrationError?.Invoke($"Titration {instance.name} is already in progress or completed");
            return false;
        }
        
        // Initialize titration
        instance.isInProgress = true;
        instance.startTime = Time.time;
        instance.status = TitrationStatus.InProgress;
        instance.currentVolume = data.analyteVolume;
        instance.currentpH = CalculateInitialpH(data);
        
        // Add initial point to titration curve
        AddTitrationPoint(instance, 0f, instance.currentpH);
        
        isTitrationInProgress = true;
        
        if (logTitrationChanges)
        {
            Debug.Log($"Started titration: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Adds titrant to the titration.
    /// </summary>
    public bool AddTitrant(string instanceId, float volume)
    {
        if (!activeTitrations.ContainsKey(instanceId)) return false;
        
        TitrationInstance instance = activeTitrations[instanceId];
        TitrationData data = titrationDatabase[instance.titrationId];
        
        if (!instance.isInProgress || instance.isCompleted)
        {
            OnTitrationError?.Invoke($"Cannot add titrant to titration {instance.name}");
            return false;
        }
        
        // Add volume
        instance.totalVolumeAdded += volume;
        instance.currentVolume += volume;
        
        // Calculate new pH
        float newpH = CalculatepH(instance, data);
        instance.currentpH = newpH;
        
        // Add point to titration curve
        AddTitrationPoint(instance, instance.totalVolumeAdded, newpH);
        
        // Play drop sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buretteSound);
        }
        
        OnVolumeAdded?.Invoke(volume);
        OnpHChanged?.Invoke(newpH);
        
        // Check for endpoint
        if (enableEndpointDetection && CheckEndpoint(instance, data))
        {
            ReachEndpoint(instance, data);
        }
        
        if (logTitrationChanges)
        {
            Debug.Log($"Added {volume:F2}mL titrant to {instance.name}, pH: {newpH:F2}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Completes a titration.
    /// </summary>
    public void CompleteTitration(string instanceId)
    {
        if (!activeTitrations.ContainsKey(instanceId)) return;
        
        TitrationInstance instance = activeTitrations[instanceId];
        TitrationData data = titrationDatabase[instance.titrationId];
        
        instance.isInProgress = false;
        instance.isCompleted = true;
        instance.completionTime = Time.time;
        instance.status = TitrationStatus.Completed;
        
        // Calculate results
        TitrationResult result = CalculateTitrationResult(instance, data);
        titrationResults.Add(result);
        
        isTitrationInProgress = false;
        
        OnTitrationCompleted?.Invoke(instance.id);
        OnTitrationResult?.Invoke(result);
        
        if (logTitrationChanges)
        {
            Debug.Log($"Completed titration: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Calculates initial pH for a titration.
    /// </summary>
    private float CalculateInitialpH(TitrationData data)
    {
        // Simple pH calculation based on analyte type
        // This can be enhanced with more complex chemistry
        if (data.type == TitrationType.AcidBase)
        {
            // Assume strong acid initially
            return 1.0f;
        }
        
        return 7.0f; // Neutral
    }
    
    /// <summary>
    /// Calculates pH during titration.
    /// </summary>
    private float CalculatepH(TitrationInstance instance, TitrationData data)
    {
        // Simple pH calculation based on titration progress
        // This can be enhanced with more complex chemistry
        
        float progress = instance.totalVolumeAdded / data.expectedEndpoint;
        
        if (data.type == TitrationType.AcidBase)
        {
            if (progress < 0.9f)
            {
                // Before endpoint - pH gradually increases
                return 1.0f + (progress * 6.0f);
            }
            else if (progress < 1.1f)
            {
                // Near endpoint - rapid pH change
                return 7.0f + ((progress - 0.9f) * 50.0f);
            }
            else
            {
                // After endpoint - pH levels off
                return 13.0f;
            }
        }
        
        return 7.0f;
    }
    
    /// <summary>
    /// Checks if endpoint has been reached.
    /// </summary>
    private bool CheckEndpoint(TitrationInstance instance, TitrationData data)
    {
        if (!enableAutoEndpointDetection) return false;
        
        float expectedVolume = data.expectedEndpoint;
        float currentVolume = instance.totalVolumeAdded;
        float difference = Mathf.Abs(currentVolume - expectedVolume);
        
        return difference <= endpointThreshold;
    }
    
    /// <summary>
    /// Handles endpoint reached.
    /// </summary>
    private void ReachEndpoint(TitrationInstance instance, TitrationData data)
    {
        instance.endpointVolume = instance.totalVolumeAdded;
        instance.status = TitrationStatus.EndpointReached;
        
        // Play endpoint sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(endpointSound);
        }
        
        // Change indicator color
        if (enableColorChanges)
        {
            ChangeIndicatorColor(instance, data);
        }
        
        OnEndpointReached?.Invoke(instance.id);
        
        if (logTitrationChanges)
        {
            Debug.Log($"Endpoint reached for {instance.name} at {instance.endpointVolume:F2}mL");
        }
    }
    
    /// <summary>
    /// Changes indicator color at endpoint.
    /// </summary>
    private void ChangeIndicatorColor(TitrationInstance instance, TitrationData data)
    {
        if (instance.gameObject != null && data.indicatorMaterials != null && data.indicatorMaterials.Length > 1)
        {
            Renderer renderer = instance.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = data.indicatorMaterials[1]; // Endpoint color
            }
        }
        
        // Play indicator change sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(indicatorSound);
        }
        
        OnIndicatorChanged?.Invoke(instance.id);
    }
    
    /// <summary>
    /// Adds a point to the titration curve.
    /// </summary>
    private void AddTitrationPoint(TitrationInstance instance, float volume, float pH)
    {
        TitrationPoint point = new TitrationPoint
        {
            volume = volume,
            pH = pH,
            color = CalculateColor(pH),
            timestamp = Time.time
        };
        
        instance.titrationCurve.Add(point);
    }
    
    /// <summary>
    /// Calculates color based on pH.
    /// </summary>
    private Color CalculateColor(float pH)
    {
        if (pH < 3.0f)
        {
            return Color.red; // Acidic
        }
        else if (pH < 6.0f)
        {
            return Color.orange; // Weakly acidic
        }
        else if (pH < 8.0f)
        {
            return Color.yellow; // Neutral
        }
        else if (pH < 11.0f)
        {
            return Color.blue; // Weakly basic
        }
        else
        {
            return Color.purple; // Basic
        }
    }
    
    /// <summary>
    /// Calculates titration result.
    /// </summary>
    private TitrationResult CalculateTitrationResult(TitrationInstance instance, TitrationData data)
    {
        TitrationResult result = new TitrationResult
        {
            id = GenerateResultId(),
            titrationId = instance.titrationId,
            endpointVolume = instance.endpointVolume,
            completionTime = instance.completionTime - instance.startTime,
            titrationCurve = new List<TitrationPoint>(instance.titrationCurve)
        };
        
        if (enableConcentrationCalculation)
        {
            // Calculate concentration using titration formula
            result.calculatedConcentration = CalculateConcentration(instance, data);
            result.actualConcentration = data.analyteConcentration;
            
            if (enableAccuracyAssessment)
            {
                result.accuracy = CalculateAccuracy(result.calculatedConcentration, result.actualConcentration);
            }
            
            result.isSuccessful = result.accuracy <= acceptableError;
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculates concentration from titration data.
    /// </summary>
    private float CalculateConcentration(TitrationInstance instance, TitrationData data)
    {
        // Basic titration formula: C1V1 = C2V2
        float analyteConcentration = (data.titrantConcentration * instance.endpointVolume) / data.analyteVolume;
        return analyteConcentration;
    }
    
    /// <summary>
    /// Calculates accuracy of titration.
    /// </summary>
    private float CalculateAccuracy(float calculated, float actual)
    {
        if (actual == 0f) return 100f;
        
        float error = Mathf.Abs(calculated - actual) / actual * 100f;
        return error;
    }
    
    /// <summary>
    /// Updates titration progress.
    /// </summary>
    private void UpdateTitrationProgress()
    {
        if (!enableTitrationManagement) return;
        
        // Update global titration state
        isTitrationInProgress = false;
        foreach (var kvp in activeTitrations)
        {
            if (kvp.Value.isInProgress)
            {
                isTitrationInProgress = true;
                break;
            }
        }
    }
    
    /// <summary>
    /// Creates visual representation of a titration.
    /// </summary>
    private GameObject CreateTitrationVisual(TitrationData data, Vector3 position)
    {
        GameObject visual;
        
        if (data.titrationSetup != null)
        {
            visual = Instantiate(data.titrationSetup, position, Quaternion.identity);
        }
        else
        {
            // Create default visual
            visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = $"Titration_{data.name}";
            visual.transform.position = position;
            visual.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
        }
        
        // Set initial indicator color
        if (data.indicatorMaterials != null && data.indicatorMaterials.Length > 0)
        {
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = data.indicatorMaterials[0]; // Initial color
            }
        }
        
        return visual;
    }
    
    /// <summary>
    /// Gets a pooled titration instance.
    /// </summary>
    private TitrationInstance GetPooledTitration()
    {
        if (!enableTitrationPooling || titrationPool.Count == 0)
        {
            return null;
        }
        
        return titrationPool.Dequeue();
    }
    
    /// <summary>
    /// Generates a unique titration instance ID.
    /// </summary>
    private string GenerateTitrationId()
    {
        return $"tit_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique result ID.
    /// </summary>
    private string GenerateResultId()
    {
        return $"res_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsTitrationInProgress() => isTitrationInProgress;
    public int GetActiveTitrationCount() => activeTitrations.Count;
    public int GetTitrationResultCount() => titrationResults.Count;
    
    /// <summary>
    /// Gets a titration instance by ID.
    /// </summary>
    public TitrationInstance GetTitration(string instanceId)
    {
        return activeTitrations.ContainsKey(instanceId) ? activeTitrations[instanceId] : null;
    }
    
    /// <summary>
    /// Gets titration data by ID.
    /// </summary>
    public TitrationData GetTitrationData(string titrationId)
    {
        return titrationDatabase.ContainsKey(titrationId) ? titrationDatabase[titrationId] : null;
    }
    
    /// <summary>
    /// Gets all available titration IDs.
    /// </summary>
    public List<string> GetAvailableTitrationIds()
    {
        return new List<string>(titrationDatabase.Keys);
    }
    
    /// <summary>
    /// Gets titration results.
    /// </summary>
    public List<TitrationResult> GetTitrationResults()
    {
        return new List<TitrationResult>(titrationResults);
    }
    
    /// <summary>
    /// Gets successful titration results.
    /// </summary>
    public List<TitrationResult> GetSuccessfulResults()
    {
        return titrationResults.FindAll(r => r.isSuccessful);
    }
    
    /// <summary>
    /// Sets the drop volume.
    /// </summary>
    public void SetDropVolume(float volume)
    {
        dropVolume = Mathf.Clamp(volume, 0.01f, 1.0f);
    }
    
    /// <summary>
    /// Sets the titration speed.
    /// </summary>
    public void SetTitrationSpeed(float speed)
    {
        titrationSpeed = Mathf.Clamp(speed, 0.1f, 10f);
    }
    
    /// <summary>
    /// Sets the endpoint threshold.
    /// </summary>
    public void SetEndpointThreshold(float threshold)
    {
        endpointThreshold = Mathf.Clamp(threshold, 0.01f, 1.0f);
    }
    
    /// <summary>
    /// Enables or disables endpoint detection.
    /// </summary>
    public void SetEndpointDetectionEnabled(bool enabled)
    {
        enableEndpointDetection = enabled;
    }
    
    /// <summary>
    /// Enables or disables color changes.
    /// </summary>
    public void SetColorChangesEnabled(bool enabled)
    {
        enableColorChanges = enabled;
    }
    
    /// <summary>
    /// Logs the current titration manager status.
    /// </summary>
    public void LogTitrationStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Titration Manager Status ===");
        Debug.Log($"Active Titrations: {activeTitrations.Count}");
        Debug.Log($"Titration Results: {titrationResults.Count}");
        Debug.Log($"Is Titration In Progress: {isTitrationInProgress}");
        Debug.Log($"Titration Database Size: {titrationDatabase.Count}");
        Debug.Log($"Titration Simulation: {(enableTitrationSimulation ? "Enabled" : "Disabled")}");
        Debug.Log($"Endpoint Detection: {(enableEndpointDetection ? "Enabled" : "Disabled")}");
        Debug.Log($"Concentration Calculation: {(enableConcentrationCalculation ? "Enabled" : "Disabled")}");
        Debug.Log($"Accuracy Assessment: {(enableAccuracyAssessment ? "Enabled" : "Disabled")}");
        Debug.Log($"Color Changes: {(enableColorChanges ? "Enabled" : "Disabled")}");
        Debug.Log($"Titration Pooling: {(enableTitrationPooling ? "Enabled" : "Disabled")}");
        Debug.Log($"Titration Pool Size: {titrationPool.Count}");
        Debug.Log("===============================");
    }
} 