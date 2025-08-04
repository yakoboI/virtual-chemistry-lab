using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages measurement and data collection for the virtual chemistry lab.
/// This component handles all measurement-related operations and calculations.
/// </summary>
public class MeasurementManager : MonoBehaviour
{
    [Header("Measurement Management")]
    [SerializeField] private bool enableMeasurementManagement = true;
    [SerializeField] private bool enableDataCollection = true;
    [SerializeField] private bool enableMeasurementValidation = true;
    [SerializeField] private bool enableMeasurementAnalysis = true;
    [SerializeField] private bool enableMeasurementCalibration = true;
    
    [Header("Measurement Configuration")]
    [SerializeField] private MeasurementData[] availableMeasurements;
    [SerializeField] private string measurementSound = "measurement_taken";
    [SerializeField] private string calibrationSound = "measurement_calibrate";
    [SerializeField] private string errorSound = "measurement_error";
    
    [Header("Measurement State")]
    [SerializeField] private Dictionary<string, MeasurementInstance> activeMeasurements = new Dictionary<string, MeasurementInstance>();
    [SerializeField] private List<MeasurementResult> measurementResults = new List<MeasurementResult>();
    [SerializeField] private bool isMeasurementInProgress = false;
    
    [Header("Measurement Settings")]
    [SerializeField] private bool enableRealTimeMeasurement = true;
    [SerializeField] private bool enableAutoCalibration = true;
    [SerializeField] private bool enableMeasurementLogging = true;
    [SerializeField] private float measurementInterval = 0.1f;
    [SerializeField] private float calibrationInterval = 3600f; // 1 hour
    [SerializeField] private float precision = 0.01f;
    [SerializeField] private float accuracy = 0.1f;
    
    [Header("Data Collection Settings")]
    [SerializeField] private bool enableDataLogging = true;
    [SerializeField] private bool enableDataExport = true;
    [SerializeField] private bool enableDataValidation = true;
    [SerializeField] private int maxDataPoints = 1000;
    [SerializeField] private float dataCollectionInterval = 0.5f;
    
    [Header("Analysis Settings")]
    [SerializeField] private bool enableStatisticalAnalysis = true;
    [SerializeField] private bool enableTrendAnalysis = true;
    [SerializeField] private bool enableErrorAnalysis = true;
    [SerializeField] private bool enableDataVisualization = true;
    [SerializeField] private float outlierThreshold = 2.0f; // Standard deviations
    
    [Header("Performance")]
    [SerializeField] private bool enableMeasurementPooling = true;
    [SerializeField] private int measurementPoolSize = 50;
    [SerializeField] private bool enableMeasurementCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private int maxActiveMeasurements = 100;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logMeasurementChanges = false;
    
    private static MeasurementManager instance;
    public static MeasurementManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MeasurementManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("MeasurementManager");
                    instance = go.AddComponent<MeasurementManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnMeasurementStarted;
    public event Action<string> OnMeasurementCompleted;
    public event Action<string> OnMeasurementCalibrated;
    public event Action<MeasurementResult> OnMeasurementResult;
    public event Action<float> OnValueMeasured;
    public event Action<string> OnMeasurementError;
    public event Action<string> OnDataCollected;
    
    // Private variables
    private Dictionary<string, MeasurementData> measurementDatabase = new Dictionary<string, MeasurementData>();
    private Queue<MeasurementInstance> measurementPool = new Queue<MeasurementInstance>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class MeasurementData
    {
        public string id;
        public string name;
        public string description;
        public MeasurementType type;
        public string unit;
        public float minValue;
        public float maxValue;
        public float precision;
        public float accuracy;
        public bool requiresCalibration;
        public float calibrationInterval;
        public string[] calibrationSteps;
        public string[] safetyNotes;
        public GameObject measurementDevice;
        public Material[] deviceMaterials;
    }
    
    [System.Serializable]
    public class MeasurementInstance
    {
        public string id;
        public string measurementId;
        public string name;
        public Vector3 position;
        public GameObject gameObject;
        public bool isActive;
        public bool isInProgress;
        public bool isCalibrated;
        public float currentValue;
        public float lastCalibrationTime;
        public float startTime;
        public MeasurementStatus status;
        public List<DataPoint> dataPoints;
        public float minValue;
        public float maxValue;
        public float averageValue;
        public float standardDeviation;
    }
    
    [System.Serializable]
    public class MeasurementResult
    {
        public string id;
        public string measurementId;
        public float value;
        public float uncertainty;
        public float timestamp;
        public bool isCalibrated;
        public bool isValid;
        public string errorMessage;
        public List<DataPoint> dataPoints;
        public StatisticalData statistics;
    }
    
    [System.Serializable]
    public class DataPoint
    {
        public float value;
        public float timestamp;
        public bool isValid;
        public string notes;
    }
    
    [System.Serializable]
    public class StatisticalData
    {
        public float mean;
        public float median;
        public float standardDeviation;
        public float variance;
        public float minValue;
        public float maxValue;
        public float range;
        public int sampleCount;
        public List<float> outliers;
    }
    
    [System.Serializable]
    public enum MeasurementType
    {
        Temperature,
        pH,
        Volume,
        Mass,
        Pressure,
        Concentration,
        Conductivity,
        Absorbance,
        Time,
        Length,
        Area,
        Density,
        Viscosity,
        RefractiveIndex,
        Other
    }
    
    [System.Serializable]
    public enum MeasurementStatus
    {
        Setup,
        Calibrating,
        Ready,
        Measuring,
        Completed,
        Error,
        OutOfRange
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMeasurementManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadMeasurementDatabase();
        InitializeMeasurementPool();
    }
    
    private void Update()
    {
        UpdateMeasurementProgress();
    }
    
    /// <summary>
    /// Initializes the measurement manager.
    /// </summary>
    private void InitializeMeasurementManager()
    {
        activeMeasurements.Clear();
        measurementResults.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("MeasurementManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads measurement data from available measurements.
    /// </summary>
    private void LoadMeasurementDatabase()
    {
        measurementDatabase.Clear();
        
        foreach (MeasurementData measurement in availableMeasurements)
        {
            if (measurement != null && !string.IsNullOrEmpty(measurement.id))
            {
                measurementDatabase[measurement.id] = measurement;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded measurement: {measurement.name} ({measurement.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {measurementDatabase.Count} measurements");
        }
    }
    
    /// <summary>
    /// Initializes the measurement pool.
    /// </summary>
    private void InitializeMeasurementPool()
    {
        if (!enableMeasurementPooling) return;
        
        for (int i = 0; i < measurementPoolSize; i++)
        {
            MeasurementInstance instance = new MeasurementInstance
            {
                id = $"pooled_{i}",
                isActive = false
            };
            
            measurementPool.Enqueue(instance);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized measurement pool with {measurementPoolSize} instances");
        }
    }
    
    /// <summary>
    /// Creates a new measurement instance.
    /// </summary>
    public MeasurementInstance CreateMeasurement(string measurementId, Vector3 position)
    {
        if (!enableMeasurementManagement || !measurementDatabase.ContainsKey(measurementId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Measurement not found: {measurementId}");
            }
            return null;
        }
        
        if (activeMeasurements.Count >= maxActiveMeasurements)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("Maximum active measurements reached");
            }
            return null;
        }
        
        MeasurementData measurementData = measurementDatabase[measurementId];
        MeasurementInstance instance = GetPooledMeasurement();
        
        if (instance == null)
        {
            instance = new MeasurementInstance();
        }
        
        instance.id = GenerateMeasurementId();
        instance.measurementId = measurementId;
        instance.name = measurementData.name;
        instance.position = position;
        instance.isActive = true;
        instance.isInProgress = false;
        instance.isCalibrated = !measurementData.requiresCalibration;
        instance.currentValue = 0f;
        instance.lastCalibrationTime = Time.time;
        instance.startTime = 0f;
        instance.status = MeasurementStatus.Setup;
        instance.dataPoints = new List<DataPoint>();
        instance.minValue = float.MaxValue;
        instance.maxValue = float.MinValue;
        instance.averageValue = 0f;
        instance.standardDeviation = 0f;
        
        // Create visual representation
        instance.gameObject = CreateMeasurementVisual(measurementData, position);
        
        activeMeasurements[instance.id] = instance;
        
        OnMeasurementStarted?.Invoke(instance.id);
        
        if (logMeasurementChanges)
        {
            Debug.Log($"Created measurement: {measurementData.name} ({instance.id}) at {position}");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Destroys a measurement instance.
    /// </summary>
    public void DestroyMeasurement(string instanceId)
    {
        if (!activeMeasurements.ContainsKey(instanceId)) return;
        
        MeasurementInstance instance = activeMeasurements[instanceId];
        
        if (instance.gameObject != null)
        {
            Destroy(instance.gameObject);
        }
        
        activeMeasurements.Remove(instanceId);
        
        // Return to pool
        if (enableMeasurementPooling)
        {
            instance.isActive = false;
            measurementPool.Enqueue(instance);
        }
        
        if (logMeasurementChanges)
        {
            Debug.Log($"Destroyed measurement: {instanceId}");
        }
    }
    
    /// <summary>
    /// Starts a measurement.
    /// </summary>
    public bool StartMeasurement(string instanceId)
    {
        if (!enableDataCollection || !activeMeasurements.ContainsKey(instanceId)) return false;
        
        MeasurementInstance instance = activeMeasurements[instanceId];
        MeasurementData data = measurementDatabase[instance.measurementId];
        
        if (instance.isInProgress)
        {
            OnMeasurementError?.Invoke($"Measurement {instance.name} is already in progress");
            return false;
        }
        
        // Check calibration
        if (data.requiresCalibration && !instance.isCalibrated)
        {
            OnMeasurementError?.Invoke($"Measurement {instance.name} requires calibration");
            return false;
        }
        
        // Start measurement
        instance.isInProgress = true;
        instance.startTime = Time.time;
        instance.status = MeasurementStatus.Measuring;
        
        isMeasurementInProgress = true;
        
        if (logMeasurementChanges)
        {
            Debug.Log($"Started measurement: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Stops a measurement.
    /// </summary>
    public void StopMeasurement(string instanceId)
    {
        if (!activeMeasurements.ContainsKey(instanceId)) return;
        
        MeasurementInstance instance = activeMeasurements[instanceId];
        
        if (!instance.isInProgress) return;
        
        instance.isInProgress = false;
        instance.status = MeasurementStatus.Completed;
        
        // Calculate statistics
        CalculateStatistics(instance);
        
        // Create result
        MeasurementResult result = CreateMeasurementResult(instance);
        measurementResults.Add(result);
        
        isMeasurementInProgress = false;
        
        OnMeasurementCompleted?.Invoke(instance.id);
        OnMeasurementResult?.Invoke(result);
        
        if (logMeasurementChanges)
        {
            Debug.Log($"Stopped measurement: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Takes a measurement reading.
    /// </summary>
    public bool TakeMeasurement(string instanceId, float value)
    {
        if (!activeMeasurements.ContainsKey(instanceId)) return false;
        
        MeasurementInstance instance = activeMeasurements[instanceId];
        MeasurementData data = measurementDatabase[instance.measurementId];
        
        // Validate value range
        if (value < data.minValue || value > data.maxValue)
        {
            OnMeasurementError?.Invoke($"Measurement value {value} is out of range for {instance.name}");
            return false;
        }
        
        // Apply precision and accuracy
        float measuredValue = ApplyPrecisionAndAccuracy(value, data);
        instance.currentValue = measuredValue;
        
        // Create data point
        DataPoint dataPoint = new DataPoint
        {
            value = measuredValue,
            timestamp = Time.time,
            isValid = true,
            notes = ""
        };
        
        instance.dataPoints.Add(dataPoint);
        
        // Limit data points
        if (instance.dataPoints.Count > maxDataPoints)
        {
            instance.dataPoints.RemoveAt(0);
        }
        
        // Update statistics
        UpdateStatistics(instance, measuredValue);
        
        // Play measurement sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(measurementSound);
        }
        
        OnValueMeasured?.Invoke(measuredValue);
        OnDataCollected?.Invoke(instance.id);
        
        if (logMeasurementChanges)
        {
            Debug.Log($"Took measurement: {measuredValue} for {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Calibrates a measurement device.
    /// </summary>
    public bool CalibrateMeasurement(string instanceId)
    {
        if (!enableMeasurementCalibration || !activeMeasurements.ContainsKey(instanceId)) return false;
        
        MeasurementInstance instance = activeMeasurements[instanceId];
        MeasurementData data = measurementDatabase[instance.measurementId];
        
        if (!data.requiresCalibration)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Measurement {instance.name} does not require calibration");
            }
            return false;
        }
        
        if (instance.isInProgress)
        {
            OnMeasurementError?.Invoke($"Cannot calibrate measurement {instance.name} while in progress");
            return false;
        }
        
        // Perform calibration
        instance.isCalibrated = true;
        instance.lastCalibrationTime = Time.time;
        instance.status = MeasurementStatus.Calibrating;
        
        // Play calibration sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(calibrationSound);
        }
        
        OnMeasurementCalibrated?.Invoke(instance.id);
        
        if (logMeasurementChanges)
        {
            Debug.Log($"Calibrated measurement: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Applies precision and accuracy to a measurement value.
    /// </summary>
    private float ApplyPrecisionAndAccuracy(float value, MeasurementData data)
    {
        // Apply precision (rounding)
        float precisionFactor = 1f / data.precision;
        value = Mathf.Round(value * precisionFactor) / precisionFactor;
        
        // Apply accuracy (systematic error)
        float accuracyError = UnityEngine.Random.Range(-data.accuracy, data.accuracy);
        value += accuracyError;
        
        return value;
    }
    
    /// <summary>
    /// Updates statistics for a measurement.
    /// </summary>
    private void UpdateStatistics(MeasurementInstance instance, float value)
    {
        // Update min/max
        instance.minValue = Mathf.Min(instance.minValue, value);
        instance.maxValue = Mathf.Max(instance.maxValue, value);
        
        // Update average
        int count = instance.dataPoints.Count;
        instance.averageValue = ((instance.averageValue * (count - 1)) + value) / count;
        
        // Update standard deviation
        if (count > 1)
        {
            float sumSquaredDiff = 0f;
            foreach (DataPoint point in instance.dataPoints)
            {
                sumSquaredDiff += Mathf.Pow(point.value - instance.averageValue, 2);
            }
            instance.standardDeviation = Mathf.Sqrt(sumSquaredDiff / (count - 1));
        }
    }
    
    /// <summary>
    /// Calculates final statistics for a measurement.
    /// </summary>
    private void CalculateStatistics(MeasurementInstance instance)
    {
        if (instance.dataPoints.Count == 0) return;
        
        // Calculate median
        List<float> values = new List<float>();
        foreach (DataPoint point in instance.dataPoints)
        {
            values.Add(point.value);
        }
        values.Sort();
        
        float median;
        if (values.Count % 2 == 0)
        {
            median = (values[values.Count / 2 - 1] + values[values.Count / 2]) / 2f;
        }
        else
        {
            median = values[values.Count / 2];
        }
        
        // Find outliers
        List<float> outliers = new List<float>();
        foreach (float value in values)
        {
            float zScore = Mathf.Abs((value - instance.averageValue) / instance.standardDeviation);
            if (zScore > outlierThreshold)
            {
                outliers.Add(value);
            }
        }
        
        // Store statistics
        instance.averageValue = values.Count > 0 ? values[values.Count - 1] : 0f;
    }
    
    /// <summary>
    /// Creates a measurement result.
    /// </summary>
    private MeasurementResult CreateMeasurementResult(MeasurementInstance instance)
    {
        MeasurementData data = measurementDatabase[instance.measurementId];
        
        MeasurementResult result = new MeasurementResult
        {
            id = GenerateResultId(),
            measurementId = instance.measurementId,
            value = instance.currentValue,
            uncertainty = instance.standardDeviation,
            timestamp = Time.time,
            isCalibrated = instance.isCalibrated,
            isValid = true,
            dataPoints = new List<DataPoint>(instance.dataPoints),
            statistics = new StatisticalData
            {
                mean = instance.averageValue,
                minValue = instance.minValue,
                maxValue = instance.maxValue,
                standardDeviation = instance.standardDeviation,
                variance = instance.standardDeviation * instance.standardDeviation,
                range = instance.maxValue - instance.minValue,
                sampleCount = instance.dataPoints.Count
            }
        };
        
        return result;
    }
    
    /// <summary>
    /// Updates measurement progress.
    /// </summary>
    private void UpdateMeasurementProgress()
    {
        if (!enableMeasurementManagement) return;
        
        // Update global measurement state
        isMeasurementInProgress = false;
        foreach (var kvp in activeMeasurements)
        {
            if (kvp.Value.isInProgress)
            {
                isMeasurementInProgress = true;
                break;
            }
        }
    }
    
    /// <summary>
    /// Creates visual representation of a measurement device.
    /// </summary>
    private GameObject CreateMeasurementVisual(MeasurementData data, Vector3 position)
    {
        GameObject visual;
        
        if (data.measurementDevice != null)
        {
            visual = Instantiate(data.measurementDevice, position, Quaternion.identity);
        }
        else
        {
            // Create default visual
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = $"Measurement_{data.name}";
            visual.transform.position = position;
            visual.transform.localScale = Vector3.one * 0.1f;
        }
        
        // Set material
        if (data.deviceMaterials != null && data.deviceMaterials.Length > 0)
        {
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = data.deviceMaterials[0];
            }
        }
        
        return visual;
    }
    
    /// <summary>
    /// Gets a pooled measurement instance.
    /// </summary>
    private MeasurementInstance GetPooledMeasurement()
    {
        if (!enableMeasurementPooling || measurementPool.Count == 0)
        {
            return null;
        }
        
        return measurementPool.Dequeue();
    }
    
    /// <summary>
    /// Generates a unique measurement instance ID.
    /// </summary>
    private string GenerateMeasurementId()
    {
        return $"meas_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique result ID.
    /// </summary>
    private string GenerateResultId()
    {
        return $"res_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsMeasurementInProgress() => isMeasurementInProgress;
    public int GetActiveMeasurementCount() => activeMeasurements.Count;
    public int GetMeasurementResultCount() => measurementResults.Count;
    
    /// <summary>
    /// Gets a measurement instance by ID.
    /// </summary>
    public MeasurementInstance GetMeasurement(string instanceId)
    {
        return activeMeasurements.ContainsKey(instanceId) ? activeMeasurements[instanceId] : null;
    }
    
    /// <summary>
    /// Gets measurement data by ID.
    /// </summary>
    public MeasurementData GetMeasurementData(string measurementId)
    {
        return measurementDatabase.ContainsKey(measurementId) ? measurementDatabase[measurementId] : null;
    }
    
    /// <summary>
    /// Gets all available measurement IDs.
    /// </summary>
    public List<string> GetAvailableMeasurementIds()
    {
        return new List<string>(measurementDatabase.Keys);
    }
    
    /// <summary>
    /// Gets measurement results.
    /// </summary>
    public List<MeasurementResult> GetMeasurementResults()
    {
        return new List<MeasurementResult>(measurementResults);
    }
    
    /// <summary>
    /// Gets valid measurement results.
    /// </summary>
    public List<MeasurementResult> GetValidResults()
    {
        return measurementResults.FindAll(r => r.isValid);
    }
    
    /// <summary>
    /// Sets the measurement interval.
    /// </summary>
    public void SetMeasurementInterval(float interval)
    {
        measurementInterval = Mathf.Clamp(interval, 0.01f, 10f);
    }
    
    /// <summary>
    /// Sets the precision.
    /// </summary>
    public void SetPrecision(float newPrecision)
    {
        precision = Mathf.Clamp(newPrecision, 0.001f, 1f);
    }
    
    /// <summary>
    /// Sets the accuracy.
    /// </summary>
    public void SetAccuracy(float newAccuracy)
    {
        accuracy = Mathf.Clamp(newAccuracy, 0.01f, 10f);
    }
    
    /// <summary>
    /// Enables or disables data collection.
    /// </summary>
    public void SetDataCollectionEnabled(bool enabled)
    {
        enableDataCollection = enabled;
    }
    
    /// <summary>
    /// Enables or disables statistical analysis.
    /// </summary>
    public void SetStatisticalAnalysisEnabled(bool enabled)
    {
        enableStatisticalAnalysis = enabled;
    }
    
    /// <summary>
    /// Exports measurement data.
    /// </summary>
    public string ExportMeasurementData(string instanceId)
    {
        if (!activeMeasurements.ContainsKey(instanceId)) return "";
        
        MeasurementInstance instance = activeMeasurements[instanceId];
        
        // Create CSV format
        string csv = "Timestamp,Value,Valid,Notes\n";
        
        foreach (DataPoint point in instance.dataPoints)
        {
            csv += $"{point.timestamp},{point.value},{point.isValid},{point.notes}\n";
        }
        
        return csv;
    }
    
    /// <summary>
    /// Logs the current measurement manager status.
    /// </summary>
    public void LogMeasurementStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Measurement Manager Status ===");
        Debug.Log($"Active Measurements: {activeMeasurements.Count}");
        Debug.Log($"Measurement Results: {measurementResults.Count}");
        Debug.Log($"Is Measurement In Progress: {isMeasurementInProgress}");
        Debug.Log($"Measurement Database Size: {measurementDatabase.Count}");
        Debug.Log($"Data Collection: {(enableDataCollection ? "Enabled" : "Disabled")}");
        Debug.Log($"Measurement Validation: {(enableMeasurementValidation ? "Enabled" : "Disabled")}");
        Debug.Log($"Statistical Analysis: {(enableStatisticalAnalysis ? "Enabled" : "Disabled")}");
        Debug.Log($"Measurement Calibration: {(enableMeasurementCalibration ? "Enabled" : "Disabled")}");
        Debug.Log($"Data Export: {(enableDataExport ? "Enabled" : "Disabled")}");
        Debug.Log($"Measurement Pooling: {(enableMeasurementPooling ? "Enabled" : "Disabled")}");
        Debug.Log($"Measurement Pool Size: {measurementPool.Count}");
        Debug.Log("==================================");
    }
} 