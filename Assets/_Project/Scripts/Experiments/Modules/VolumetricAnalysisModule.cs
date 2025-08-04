using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages volumetric analysis experiments including titrations, dilutions, and concentration calculations.
/// This module handles all volumetric analysis-related operations and procedures.
/// </summary>
public class VolumetricAnalysisModule : MonoBehaviour
{
    [Header("Volumetric Analysis Management")]
    [SerializeField] private bool enableVolumetricAnalysis = true;
    [SerializeField] private bool enableTitrationSimulation = true;
    [SerializeField] private bool enableDilutionCalculations = true;
    [SerializeField] private bool enableConcentrationAnalysis = true;
    [SerializeField] private bool enableEndpointDetection = true;
    
    [Header("Module Configuration")]
    [SerializeField] private VolumetricExperiment[] availableExperiments;
    [SerializeField] private string titrationSound = "burette_drop";
    [SerializeField] private string endpointSound = "endpoint_reached";
    [SerializeField] private string indicatorSound = "indicator_change";
    
    [Header("Module State")]
    [SerializeField] private Dictionary<string, VolumetricInstance> activeExperiments = new Dictionary<string, VolumetricInstance>();
    [SerializeField] private List<VolumetricResult> experimentResults = new List<VolumetricResult>();
    [SerializeField] private bool isExperimentInProgress = false;
    
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
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logVolumetricEvents = false;
    
    private static VolumetricAnalysisModule instance;
    public static VolumetricAnalysisModule Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<VolumetricAnalysisModule>();
                if (instance == null)
                {
                    GameObject go = new GameObject("VolumetricAnalysisModule");
                    instance = go.AddComponent<VolumetricAnalysisModule>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnVolumetricExperimentStarted;
    public event Action<string> OnVolumetricExperimentCompleted;
    public event Action<string> OnEndpointReached;
    public event Action<string> OnIndicatorChanged;
    public event Action<VolumetricResult> OnVolumetricResult;
    public event Action<float> OnVolumeAdded;
    public event Action<float> OnpHChanged;
    public event Action<string> OnVolumetricError;
    
    // Private variables
    private Dictionary<string, VolumetricExperiment> experimentDatabase = new Dictionary<string, VolumetricExperiment>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class VolumetricExperiment
    {
        public string id;
        public string name;
        public string description;
        public VolumetricType type;
        public ChemicalData analyte;
        public ChemicalData titrant;
        public ChemicalData indicator;
        public float analyteConcentration;
        public float titrantConcentration;
        public float analyteVolume;
        public float expectedEndpoint;
        public float tolerance;
        public string[] learningObjectives;
        public string[] safetyNotes;
    }
    
    [System.Serializable]
    public class VolumetricInstance
    {
        public string id;
        public string experimentId;
        public string name;
        public bool isActive;
        public bool isInProgress;
        public bool isCompleted;
        public float startTime;
        public float completionTime;
        public VolumetricStatus status;
        public float currentVolume;
        public float currentpH;
        public float endpointVolume;
        public bool endpointReached;
        public List<TitrationPoint> titrationPoints;
        public VolumetricData data;
    }
    
    [System.Serializable]
    public class VolumetricResult
    {
        public string id;
        public string experimentId;
        public bool isSuccessful;
        public float experimentalEndpoint;
        public float theoreticalEndpoint;
        public float accuracy;
        public float precision;
        public float calculatedConcentration;
        public float actualConcentration;
        public float percentageError;
        public List<TitrationPoint> titrationCurve;
        public VolumetricData data;
        public string grade;
        public List<string> feedback;
    }
    
    [System.Serializable]
    public class TitrationPoint
    {
        public float volume;
        public float pH;
        public Color color;
        public bool isEndpoint;
        public float timestamp;
    }
    
    [System.Serializable]
    public class VolumetricData
    {
        public float initialVolume;
        public float finalVolume;
        public float totalVolumeAdded;
        public float initialpH;
        public float finalpH;
        public float endpointpH;
        public int numberOfDrops;
        public float titrationTime;
        public List<float> volumeReadings;
        public List<float> pHReadings;
    }
    
    [System.Serializable]
    public class ChemicalData
    {
        public string id;
        public string name;
        public string formula;
        public float concentration;
        public float volume;
        public string color;
        public float pH;
    }
    
    [System.Serializable]
    public enum VolumetricType
    {
        AcidBaseTitration,
        RedoxTitration,
        ComplexometricTitration,
        PrecipitationTitration,
        Dilution,
        Standardization
    }
    
    [System.Serializable]
    public enum VolumetricStatus
    {
        Setup,
        Running,
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
            InitializeVolumetricModule();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadExperimentDatabase();
    }
    
    /// <summary>
    /// Initializes the volumetric analysis module.
    /// </summary>
    private void InitializeVolumetricModule()
    {
        activeExperiments.Clear();
        experimentResults.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("VolumetricAnalysisModule initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads experiment data from available experiments.
    /// </summary>
    private void LoadExperimentDatabase()
    {
        experimentDatabase.Clear();
        
        foreach (VolumetricExperiment experiment in availableExperiments)
        {
            if (experiment != null && !string.IsNullOrEmpty(experiment.id))
            {
                experimentDatabase[experiment.id] = experiment;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded volumetric experiment: {experiment.name} ({experiment.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {experimentDatabase.Count} volumetric experiments");
        }
    }
    
    /// <summary>
    /// Creates a new volumetric experiment instance.
    /// </summary>
    public VolumetricInstance CreateExperiment(string experimentId)
    {
        if (!enableVolumetricAnalysis || !experimentDatabase.ContainsKey(experimentId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Volumetric experiment not found: {experimentId}");
            }
            return null;
        }
        
        VolumetricExperiment experiment = experimentDatabase[experimentId];
        VolumetricInstance instance = new VolumetricInstance
        {
            id = GenerateExperimentId(),
            experimentId = experimentId,
            name = experiment.name,
            isActive = true,
            isInProgress = false,
            isCompleted = false,
            startTime = 0f,
            completionTime = 0f,
            status = VolumetricStatus.Setup,
            currentVolume = 0f,
            currentpH = experiment.analyte.pH,
            endpointVolume = 0f,
            endpointReached = false,
            titrationPoints = new List<TitrationPoint>(),
            data = new VolumetricData
            {
                initialVolume = experiment.analyte.volume,
                finalVolume = 0f,
                totalVolumeAdded = 0f,
                initialpH = experiment.analyte.pH,
                finalpH = 0f,
                endpointpH = 0f,
                numberOfDrops = 0,
                titrationTime = 0f,
                volumeReadings = new List<float>(),
                pHReadings = new List<float>()
            }
        };
        
        activeExperiments[instance.id] = instance;
        
        OnVolumetricExperimentStarted?.Invoke(instance.id);
        
        if (logVolumetricEvents)
        {
            Debug.Log($"Created volumetric experiment: {experiment.name} ({instance.id})");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Starts a volumetric experiment.
    /// </summary>
    public bool StartExperiment(string instanceId)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return false;
        
        VolumetricInstance instance = activeExperiments[instanceId];
        VolumetricExperiment experiment = experimentDatabase[instance.experimentId];
        
        if (instance.isCompleted)
        {
            OnVolumetricError?.Invoke($"Experiment {instance.name} is already completed");
            return false;
        }
        
        instance.isInProgress = true;
        instance.startTime = Time.time;
        instance.status = VolumetricStatus.Running;
        instance.currentVolume = experiment.analyte.volume;
        instance.currentpH = experiment.analyte.pH;
        
        isExperimentInProgress = true;
        
        if (logVolumetricEvents)
        {
            Debug.Log($"Started volumetric experiment: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Completes a volumetric experiment.
    /// </summary>
    public void CompleteExperiment(string instanceId)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return;
        
        VolumetricInstance instance = activeExperiments[instanceId];
        VolumetricExperiment experiment = experimentDatabase[instance.experimentId];
        
        instance.isInProgress = false;
        instance.isCompleted = true;
        instance.completionTime = Time.time;
        instance.status = VolumetricStatus.Completed;
        
        // Calculate final data
        CalculateFinalData(instance);
        
        // Generate result
        VolumetricResult result = GenerateVolumetricResult(instance);
        experimentResults.Add(result);
        
        isExperimentInProgress = false;
        
        OnVolumetricExperimentCompleted?.Invoke(instance.id);
        OnVolumetricResult?.Invoke(result);
        
        if (logVolumetricEvents)
        {
            Debug.Log($"Completed volumetric experiment: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Adds a drop of titrant to the solution.
    /// </summary>
    public bool AddTitrantDrop(string instanceId)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return false;
        
        VolumetricInstance instance = activeExperiments[instanceId];
        VolumetricExperiment experiment = experimentDatabase[instance.experimentId];
        
        if (!instance.isInProgress)
        {
            OnVolumetricError?.Invoke($"Cannot add titrant - experiment {instance.name} not in progress");
            return false;
        }
        
        // Add volume
        instance.currentVolume += dropVolume;
        instance.data.totalVolumeAdded += dropVolume;
        instance.data.numberOfDrops++;
        instance.data.volumeReadings.Add(instance.currentVolume);
        
        // Calculate new pH
        float newpH = CalculateNewpH(instance, experiment);
        instance.currentpH = newpH;
        instance.data.pHReadings.Add(newpH);
        
        // Create titration point
        TitrationPoint point = new TitrationPoint
        {
            volume = instance.currentVolume,
            pH = newpH,
            color = CalculateColor(newpH, experiment.indicator),
            isEndpoint = false,
            timestamp = Time.time
        };
        
        instance.titrationPoints.Add(point);
        
        // Check for endpoint
        if (CheckForEndpoint(instance, experiment))
        {
            instance.endpointReached = true;
            instance.endpointVolume = instance.currentVolume;
            instance.endpointpH = newpH;
            instance.status = VolumetricStatus.EndpointReached;
            point.isEndpoint = true;
            
            OnEndpointReached?.Invoke(instance.id);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(endpointSound);
            }
        }
        
        OnVolumeAdded?.Invoke(dropVolume);
        OnpHChanged?.Invoke(newpH);
        
        if (logVolumetricEvents)
        {
            Debug.Log($"Added titrant drop - Volume: {instance.currentVolume:F2}mL, pH: {newpH:F2}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Calculates the new pH after adding titrant.
    /// </summary>
    private float CalculateNewpH(VolumetricInstance instance, VolumetricExperiment experiment)
    {
        // Simple pH calculation simulation
        float volumeRatio = instance.data.totalVolumeAdded / experiment.analyte.volume;
        float pHChange = volumeRatio * 2f; // Simulate pH change
        
        float newpH = experiment.analyte.pH - pHChange;
        
        // Ensure pH stays within reasonable bounds
        return Mathf.Clamp(newpH, 0f, 14f);
    }
    
    /// <summary>
    /// Calculates the color based on pH and indicator.
    /// </summary>
    private Color CalculateColor(float pH, ChemicalData indicator)
    {
        // Simple color calculation based on pH
        if (pH < 7f)
        {
            return Color.red; // Acidic
        }
        else if (pH > 7f)
        {
            return Color.blue; // Basic
        }
        else
        {
            return Color.green; // Neutral
        }
    }
    
    /// <summary>
    /// Checks if the endpoint has been reached.
    /// </summary>
    private bool CheckForEndpoint(VolumetricInstance instance, VolumetricExperiment experiment)
    {
        if (instance.endpointReached) return false;
        
        // Check if pH is close to expected endpoint
        float pHDifference = Mathf.Abs(instance.currentpH - experiment.expectedEndpoint);
        
        if (pHDifference <= endpointThreshold)
        {
            return true;
        }
        
        // Check for color change
        if (instance.titrationPoints.Count > 1)
        {
            Color previousColor = instance.titrationPoints[instance.titrationPoints.Count - 2].color;
            Color currentColor = instance.titrationPoints[instance.titrationPoints.Count - 1].color;
            
            if (ColorDistance(previousColor, currentColor) > colorChangeThreshold)
            {
                OnIndicatorChanged?.Invoke(instance.id);
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(indicatorSound);
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculates the distance between two colors.
    /// </summary>
    private float ColorDistance(Color color1, Color color2)
    {
        return Vector4.Distance(new Vector4(color1.r, color1.g, color1.b, color1.a),
                               new Vector4(color2.r, color2.g, color2.b, color2.a));
    }
    
    /// <summary>
    /// Calculates final data for the experiment.
    /// </summary>
    private void CalculateFinalData(VolumetricInstance instance)
    {
        instance.data.finalVolume = instance.currentVolume;
        instance.data.finalpH = instance.currentpH;
        instance.data.titrationTime = instance.completionTime - instance.startTime;
    }
    
    /// <summary>
    /// Generates a volumetric result.
    /// </summary>
    private VolumetricResult GenerateVolumetricResult(VolumetricInstance instance)
    {
        VolumetricExperiment experiment = experimentDatabase[instance.experimentId];
        
        float accuracy = CalculateAccuracy(instance, experiment);
        float precision = CalculatePrecision(instance);
        float calculatedConcentration = CalculateConcentration(instance, experiment);
        float percentageError = CalculatePercentageError(calculatedConcentration, experiment.analyte.concentration);
        string grade = CalculateGrade(percentageError);
        
        VolumetricResult result = new VolumetricResult
        {
            id = GenerateResultId(),
            experimentId = instance.experimentId,
            isSuccessful = percentageError <= acceptableError,
            experimentalEndpoint = instance.endpointVolume,
            theoreticalEndpoint = experiment.expectedEndpoint,
            accuracy = accuracy,
            precision = precision,
            calculatedConcentration = calculatedConcentration,
            actualConcentration = experiment.analyte.concentration,
            percentageError = percentageError,
            titrationCurve = new List<TitrationPoint>(instance.titrationPoints),
            data = new VolumetricData(instance.data),
            grade = grade,
            feedback = GenerateFeedback(instance, experiment, percentageError)
        };
        
        return result;
    }
    
    /// <summary>
    /// Calculates the accuracy of the experiment.
    /// </summary>
    private float CalculateAccuracy(VolumetricInstance instance, VolumetricExperiment experiment)
    {
        if (instance.endpointVolume <= 0f) return 0f;
        
        float volumeAccuracy = 1f - Mathf.Abs(instance.endpointVolume - experiment.expectedEndpoint) / experiment.expectedEndpoint;
        return Mathf.Clamp(volumeAccuracy, 0f, 1f);
    }
    
    /// <summary>
    /// Calculates the precision of the experiment.
    /// </summary>
    private float CalculatePrecision(VolumetricInstance instance)
    {
        if (instance.data.volumeReadings.Count < 2) return 1f;
        
        // Calculate standard deviation of volume readings
        float mean = 0f;
        foreach (float reading in instance.data.volumeReadings)
        {
            mean += reading;
        }
        mean /= instance.data.volumeReadings.Count;
        
        float variance = 0f;
        foreach (float reading in instance.data.volumeReadings)
        {
            variance += Mathf.Pow(reading - mean, 2);
        }
        variance /= instance.data.volumeReadings.Count;
        
        float standardDeviation = Mathf.Sqrt(variance);
        float precision = 1f - (standardDeviation / mean);
        
        return Mathf.Clamp(precision, 0f, 1f);
    }
    
    /// <summary>
    /// Calculates the concentration from the experiment.
    /// </summary>
    private float CalculateConcentration(VolumetricInstance instance, VolumetricExperiment experiment)
    {
        if (instance.endpointVolume <= 0f) return 0f;
        
        // Simple concentration calculation: C1V1 = C2V2
        float calculatedConcentration = (experiment.titrant.concentration * instance.endpointVolume) / experiment.analyte.volume;
        
        return calculatedConcentration;
    }
    
    /// <summary>
    /// Calculates the percentage error.
    /// </summary>
    private float CalculatePercentageError(float calculated, float actual)
    {
        if (actual <= 0f) return 100f;
        
        return Mathf.Abs(calculated - actual) / actual * 100f;
    }
    
    /// <summary>
    /// Calculates a grade based on percentage error.
    /// </summary>
    private string CalculateGrade(float percentageError)
    {
        if (percentageError <= 1f) return "A";
        if (percentageError <= 3f) return "B";
        if (percentageError <= 5f) return "C";
        if (percentageError <= 10f) return "D";
        return "F";
    }
    
    /// <summary>
    /// Generates feedback for the experiment.
    /// </summary>
    private List<string> GenerateFeedback(VolumetricInstance instance, VolumetricExperiment experiment, float percentageError)
    {
        List<string> feedback = new List<string>();
        
        if (percentageError <= acceptableError)
        {
            feedback.Add("Excellent work! Your experimental results are very accurate.");
        }
        else if (percentageError <= 5f)
        {
            feedback.Add("Good work! Your results are reasonably accurate with minor improvements needed.");
        }
        else
        {
            feedback.Add("Your results need improvement. Check your technique and calculations.");
        }
        
        if (instance.endpointReached)
        {
            feedback.Add("Successfully detected the endpoint.");
        }
        else
        {
            feedback.Add("Endpoint detection needs improvement.");
        }
        
        return feedback;
    }
    
    /// <summary>
    /// Generates a unique experiment ID.
    /// </summary>
    private string GenerateExperimentId()
    {
        return $"vol_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique result ID.
    /// </summary>
    private string GenerateResultId()
    {
        return $"vol_res_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsExperimentInProgress() => isExperimentInProgress;
    public int GetActiveExperimentCount() => activeExperiments.Count;
    public int GetExperimentResultCount() => experimentResults.Count;
    
    /// <summary>
    /// Gets an experiment instance by ID.
    /// </summary>
    public VolumetricInstance GetExperiment(string instanceId)
    {
        return activeExperiments.ContainsKey(instanceId) ? activeExperiments[instanceId] : null;
    }
    
    /// <summary>
    /// Gets experiment data by ID.
    /// </summary>
    public VolumetricExperiment GetExperimentData(string experimentId)
    {
        return experimentDatabase.ContainsKey(experimentId) ? experimentDatabase[experimentId] : null;
    }
    
    /// <summary>
    /// Gets all available experiment IDs.
    /// </summary>
    public List<string> GetAvailableExperimentIds()
    {
        return new List<string>(experimentDatabase.Keys);
    }
    
    /// <summary>
    /// Gets experiment results.
    /// </summary>
    public List<VolumetricResult> GetExperimentResults()
    {
        return new List<VolumetricResult>(experimentResults);
    }
    
    /// <summary>
    /// Sets the drop volume.
    /// </summary>
    public void SetDropVolume(float volume)
    {
        dropVolume = Mathf.Clamp(volume, 0.01f, 1f);
    }
    
    /// <summary>
    /// Sets the endpoint threshold.
    /// </summary>
    public void SetEndpointThreshold(float threshold)
    {
        endpointThreshold = Mathf.Clamp(threshold, 0.01f, 1f);
    }
    
    /// <summary>
    /// Enables or disables real-time titration.
    /// </summary>
    public void SetRealTimeTitrationEnabled(bool enabled)
    {
        enableRealTimeTitration = enabled;
    }
    
    /// <summary>
    /// Generates a volumetric analysis report.
    /// </summary>
    public string GenerateVolumetricReport(string instanceId)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return "";
        
        VolumetricInstance instance = activeExperiments[instanceId];
        VolumetricExperiment experiment = experimentDatabase[instance.experimentId];
        
        string report = "=== Volumetric Analysis Report ===\n";
        report += $"Experiment: {instance.name}\n";
        report += $"Analyte: {experiment.analyte.name}\n";
        report += $"Titrant: {experiment.titrant.name}\n";
        report += $"Endpoint Volume: {instance.endpointVolume:F2}mL\n";
        report += $"Endpoint pH: {instance.endpointpH:F2}\n";
        report += $"Total Drops: {instance.data.numberOfDrops}\n";
        report += $"Titration Time: {instance.data.titrationTime:F1}s\n";
        report += "====================================\n";
        
        return report;
    }
    
    /// <summary>
    /// Logs the current volumetric analysis module status.
    /// </summary>
    public void LogVolumetricStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Volumetric Analysis Module Status ===");
        Debug.Log($"Active Experiments: {activeExperiments.Count}");
        Debug.Log($"Experiment Results: {experimentResults.Count}");
        Debug.Log($"Is Experiment In Progress: {isExperimentInProgress}");
        Debug.Log($"Experiment Database Size: {experimentDatabase.Count}");
        Debug.Log($"Volumetric Analysis: {(enableVolumetricAnalysis ? "Enabled" : "Disabled")}");
        Debug.Log($"Titration Simulation: {(enableTitrationSimulation ? "Enabled" : "Disabled")}");
        Debug.Log($"Endpoint Detection: {(enableEndpointDetection ? "Enabled" : "Disabled")}");
        Debug.Log($"Real-Time Titration: {(enableRealTimeTitration ? "Enabled" : "Disabled")}");
        Debug.Log($"Auto Endpoint Detection: {(enableAutoEndpointDetection ? "Enabled" : "Disabled")}");
        Debug.Log("=========================================");
    }
} 