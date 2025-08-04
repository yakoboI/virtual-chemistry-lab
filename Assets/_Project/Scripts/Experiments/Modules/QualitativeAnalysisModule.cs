using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages qualitative analysis experiments including flame tests, precipitation reactions, and color changes.
/// This module handles all qualitative analysis-related operations and procedures.
/// </summary>
public class QualitativeAnalysisModule : MonoBehaviour
{
    [Header("Qualitative Analysis Management")]
    [SerializeField] private bool enableQualitativeAnalysis = true;
    [SerializeField] private bool enableFlameTests = true;
    [SerializeField] private bool enablePrecipitationTests = true;
    [SerializeField] private bool enableColorChangeTests = true;
    [SerializeField] private bool enableGasEvolutionTests = true;
    
    [Header("Module Configuration")]
    [SerializeField] private QualitativeExperiment[] availableExperiments;
    [SerializeField] private string flameTestSound = "flame_test";
    [SerializeField] private string precipitationSound = "precipitation";
    [SerializeField] private string colorChangeSound = "color_change";
    [SerializeField] private string gasEvolutionSound = "gas_evolution";
    
    [Header("Module State")]
    [SerializeField] private Dictionary<string, QualitativeInstance> activeExperiments = new Dictionary<string, QualitativeInstance>();
    [SerializeField] private List<QualitativeResult> experimentResults = new List<QualitativeResult>();
    [SerializeField] private bool isExperimentInProgress = false;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableRealTimeTests = true;
    [SerializeField] private bool enableAutoDetection = true;
    [SerializeField] private bool enableTestLogging = true;
    [SerializeField] private float testDuration = 5.0f;
    [SerializeField] private float detectionThreshold = 0.05f;
    [SerializeField] private float colorChangeThreshold = 0.02f;
    
    [Header("Analysis Settings")]
    [SerializeField] private bool enableResultAnalysis = true;
    [SerializeField] private bool enableAccuracyAssessment = true;
    [SerializeField] private bool enablePrecisionAnalysis = true;
    [SerializeField] private float acceptableError = 0.1f; // 10%
    [SerializeField] private int minTestsForAnalysis = 3;
    
    [Header("Visual Settings")]
    [SerializeField] private bool enableFlameEffects = true;
    [SerializeField] private bool enablePrecipitationEffects = true;
    [SerializeField] private bool enableColorEffects = true;
    [SerializeField] private bool enableGasEffects = true;
    [SerializeField] private float effectIntensity = 1.0f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logQualitativeEvents = false;
    
    private static QualitativeAnalysisModule instance;
    public static QualitativeAnalysisModule Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<QualitativeAnalysisModule>();
                if (instance == null)
                {
                    GameObject go = new GameObject("QualitativeAnalysisModule");
                    instance = go.AddComponent<QualitativeAnalysisModule>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnQualitativeExperimentStarted;
    public event Action<string> OnQualitativeExperimentCompleted;
    public event Action<string> OnTestCompleted;
    public event Action<string> OnResultDetected;
    public event Action<QualitativeResult> OnQualitativeResult;
    public event Action<string> OnTestTypeChanged;
    public event Action<Color> OnColorChanged;
    public event Action<string> OnQualitativeError;
    
    // Private variables
    private Dictionary<string, QualitativeExperiment> experimentDatabase = new Dictionary<string, QualitativeExperiment>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class QualitativeExperiment
    {
        public string id;
        public string name;
        public string description;
        public QualitativeTestType type;
        public ChemicalData sample;
        public ChemicalData reagent;
        public string expectedResult;
        public string[] possibleResults;
        public float expectedIntensity;
        public float tolerance;
        public string[] learningObjectives;
        public string[] safetyNotes;
    }
    
    [System.Serializable]
    public class QualitativeInstance
    {
        public string id;
        public string experimentId;
        public string name;
        public bool isActive;
        public bool isInProgress;
        public bool isCompleted;
        public float startTime;
        public float completionTime;
        public QualitativeStatus status;
        public string currentTestType;
        public string detectedResult;
        public float resultIntensity;
        public bool resultDetected;
        public List<TestPoint> testPoints;
        public QualitativeData data;
    }
    
    [System.Serializable]
    public class QualitativeResult
    {
        public string id;
        public string experimentId;
        public bool isSuccessful;
        public string detectedResult;
        public string expectedResult;
        public float accuracy;
        public float precision;
        public float resultIntensity;
        public float expectedIntensity;
        public float percentageError;
        public List<TestPoint> testData;
        public QualitativeData data;
        public string grade;
        public List<string> feedback;
    }
    
    [System.Serializable]
    public class TestPoint
    {
        public string testType;
        public string result;
        public float intensity;
        public Color color;
        public bool isPositive;
        public float timestamp;
    }
    
    [System.Serializable]
    public class QualitativeData
    {
        public float testDuration;
        public int numberOfTests;
        public float averageIntensity;
        public List<string> testTypes;
        public List<string> results;
        public List<float> intensities;
        public List<Color> colors;
    }
    
    [System.Serializable]
    public class ChemicalData
    {
        public string id;
        public string name;
        public string formula;
        public string color;
        public float concentration;
        public float volume;
        public string[] properties;
    }
    
    [System.Serializable]
    public enum QualitativeTestType
    {
        FlameTest,
        PrecipitationTest,
        ColorChangeTest,
        GasEvolutionTest,
        pHTest,
        SolubilityTest,
        ComplexationTest,
        OxidationTest
    }
    
    [System.Serializable]
    public enum QualitativeStatus
    {
        Setup,
        Running,
        ResultDetected,
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
            InitializeQualitativeModule();
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
    /// Initializes the qualitative analysis module.
    /// </summary>
    private void InitializeQualitativeModule()
    {
        activeExperiments.Clear();
        experimentResults.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("QualitativeAnalysisModule initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads experiment data from available experiments.
    /// </summary>
    private void LoadExperimentDatabase()
    {
        experimentDatabase.Clear();
        
        foreach (QualitativeExperiment experiment in availableExperiments)
        {
            if (experiment != null && !string.IsNullOrEmpty(experiment.id))
            {
                experimentDatabase[experiment.id] = experiment;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded qualitative experiment: {experiment.name} ({experiment.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {experimentDatabase.Count} qualitative experiments");
        }
    }
    
    /// <summary>
    /// Creates a new qualitative experiment instance.
    /// </summary>
    public QualitativeInstance CreateExperiment(string experimentId)
    {
        if (!enableQualitativeAnalysis || !experimentDatabase.ContainsKey(experimentId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Qualitative experiment not found: {experimentId}");
            }
            return null;
        }
        
        QualitativeExperiment experiment = experimentDatabase[experimentId];
        QualitativeInstance instance = new QualitativeInstance
        {
            id = GenerateExperimentId(),
            experimentId = experimentId,
            name = experiment.name,
            isActive = true,
            isInProgress = false,
            isCompleted = false,
            startTime = 0f,
            completionTime = 0f,
            status = QualitativeStatus.Setup,
            currentTestType = experiment.type.ToString(),
            detectedResult = "",
            resultIntensity = 0f,
            resultDetected = false,
            testPoints = new List<TestPoint>(),
            data = new QualitativeData
            {
                testDuration = 0f,
                numberOfTests = 0,
                averageIntensity = 0f,
                testTypes = new List<string>(),
                results = new List<string>(),
                intensities = new List<float>(),
                colors = new List<Color>()
            }
        };
        
        activeExperiments[instance.id] = instance;
        
        OnQualitativeExperimentStarted?.Invoke(instance.id);
        
        if (logQualitativeEvents)
        {
            Debug.Log($"Created qualitative experiment: {experiment.name} ({instance.id})");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Starts a qualitative experiment.
    /// </summary>
    public bool StartExperiment(string instanceId)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return false;
        
        QualitativeInstance instance = activeExperiments[instanceId];
        QualitativeExperiment experiment = experimentDatabase[instance.experimentId];
        
        if (instance.isCompleted)
        {
            OnQualitativeError?.Invoke($"Experiment {instance.name} is already completed");
            return false;
        }
        
        instance.isInProgress = true;
        instance.startTime = Time.time;
        instance.status = QualitativeStatus.Running;
        
        isExperimentInProgress = true;
        
        if (logQualitativeEvents)
        {
            Debug.Log($"Started qualitative experiment: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Completes a qualitative experiment.
    /// </summary>
    public void CompleteExperiment(string instanceId)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return;
        
        QualitativeInstance instance = activeExperiments[instanceId];
        QualitativeExperiment experiment = experimentDatabase[instance.experimentId];
        
        instance.isInProgress = false;
        instance.isCompleted = true;
        instance.completionTime = Time.time;
        instance.status = QualitativeStatus.Completed;
        
        // Calculate final data
        CalculateFinalData(instance);
        
        // Generate result
        QualitativeResult result = GenerateQualitativeResult(instance);
        experimentResults.Add(result);
        
        isExperimentInProgress = false;
        
        OnQualitativeExperimentCompleted?.Invoke(instance.id);
        OnQualitativeResult?.Invoke(result);
        
        if (logQualitativeEvents)
        {
            Debug.Log($"Completed qualitative experiment: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Performs a specific test type.
    /// </summary>
    public bool PerformTest(string instanceId, QualitativeTestType testType)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return false;
        
        QualitativeInstance instance = activeExperiments[instanceId];
        QualitativeExperiment experiment = experimentDatabase[instance.experimentId];
        
        if (!instance.isInProgress)
        {
            OnQualitativeError?.Invoke($"Cannot perform test - experiment {instance.name} not in progress");
            return false;
        }
        
        instance.currentTestType = testType.ToString();
        instance.data.numberOfTests++;
        instance.data.testTypes.Add(testType.ToString());
        
        // Perform the specific test
        string result = "";
        float intensity = 0f;
        Color color = Color.white;
        bool isPositive = false;
        
        switch (testType)
        {
            case QualitativeTestType.FlameTest:
                result = PerformFlameTest(experiment, out intensity, out color);
                break;
            case QualitativeTestType.PrecipitationTest:
                result = PerformPrecipitationTest(experiment, out intensity, out color);
                break;
            case QualitativeTestType.ColorChangeTest:
                result = PerformColorChangeTest(experiment, out intensity, out color);
                break;
            case QualitativeTestType.GasEvolutionTest:
                result = PerformGasEvolutionTest(experiment, out intensity, out color);
                break;
            default:
                result = "No result";
                break;
        }
        
        // Check if result matches expected
        isPositive = CheckResultMatch(result, experiment.expectedResult);
        
        // Create test point
        TestPoint testPoint = new TestPoint
        {
            testType = testType.ToString(),
            result = result,
            intensity = intensity,
            color = color,
            isPositive = isPositive,
            timestamp = Time.time
        };
        
        instance.testPoints.Add(testPoint);
        instance.data.results.Add(result);
        instance.data.intensities.Add(intensity);
        instance.data.colors.Add(color);
        
        // Update instance data
        if (isPositive)
        {
            instance.detectedResult = result;
            instance.resultIntensity = intensity;
            instance.resultDetected = true;
            instance.status = QualitativeStatus.ResultDetected;
            
            OnResultDetected?.Invoke(instance.id);
        }
        
        OnTestCompleted?.Invoke(instance.id);
        OnTestTypeChanged?.Invoke(testType.ToString());
        OnColorChanged?.Invoke(color);
        
        // Play test sound
        PlayTestSound(testType);
        
        if (logQualitativeEvents)
        {
            Debug.Log($"Performed {testType} test - Result: {result}, Intensity: {intensity:F2}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Performs a flame test.
    /// </summary>
    private string PerformFlameTest(QualitativeExperiment experiment, out float intensity, out Color color)
    {
        // Simulate flame test results
        string[] flameColors = { "Red", "Orange", "Yellow", "Green", "Blue", "Purple", "White" };
        string result = flameColors[UnityEngine.Random.Range(0, flameColors.Length)];
        intensity = UnityEngine.Random.Range(0.3f, 1.0f);
        
        // Set color based on result
        switch (result)
        {
            case "Red": color = Color.red; break;
            case "Orange": color = new Color(1f, 0.5f, 0f); break;
            case "Yellow": color = Color.yellow; break;
            case "Green": color = Color.green; break;
            case "Blue": color = Color.blue; break;
            case "Purple": color = new Color(0.5f, 0f, 0.5f); break;
            case "White": color = Color.white; break;
            default: color = Color.white; break;
        }
        
        return result;
    }
    
    /// <summary>
    /// Performs a precipitation test.
    /// </summary>
    private string PerformPrecipitationTest(QualitativeExperiment experiment, out float intensity, out Color color)
    {
        // Simulate precipitation test results
        string[] precipitates = { "White precipitate", "Yellow precipitate", "Brown precipitate", "Blue precipitate", "No precipitate" };
        string result = precipitates[UnityEngine.Random.Range(0, precipitates.Length)];
        intensity = UnityEngine.Random.Range(0.2f, 0.8f);
        
        // Set color based on result
        switch (result)
        {
            case "White precipitate": color = Color.white; break;
            case "Yellow precipitate": color = Color.yellow; break;
            case "Brown precipitate": color = new Color(0.6f, 0.4f, 0.2f); break;
            case "Blue precipitate": color = Color.blue; break;
            case "No precipitate": color = Color.clear; break;
            default: color = Color.white; break;
        }
        
        return result;
    }
    
    /// <summary>
    /// Performs a color change test.
    /// </summary>
    private string PerformColorChangeTest(QualitativeExperiment experiment, out float intensity, out Color color)
    {
        // Simulate color change test results
        string[] colorChanges = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "No change" };
        string result = colorChanges[UnityEngine.Random.Range(0, colorChanges.Length)];
        intensity = UnityEngine.Random.Range(0.4f, 1.0f);
        
        // Set color based on result
        switch (result)
        {
            case "Red": color = Color.red; break;
            case "Blue": color = Color.blue; break;
            case "Green": color = Color.green; break;
            case "Yellow": color = Color.yellow; break;
            case "Purple": color = new Color(0.5f, 0f, 0.5f); break;
            case "Orange": color = new Color(1f, 0.5f, 0f); break;
            case "No change": color = Color.white; break;
            default: color = Color.white; break;
        }
        
        return result;
    }
    
    /// <summary>
    /// Performs a gas evolution test.
    /// </summary>
    private string PerformGasEvolutionTest(QualitativeExperiment experiment, out float intensity, out Color color)
    {
        // Simulate gas evolution test results
        string[] gases = { "Bubbles", "Fizzing", "Effervescence", "No gas evolution" };
        string result = gases[UnityEngine.Random.Range(0, gases.Length)];
        intensity = UnityEngine.Random.Range(0.3f, 0.9f);
        color = Color.white; // Gas evolution doesn't have a specific color
        
        return result;
    }
    
    /// <summary>
    /// Checks if the detected result matches the expected result.
    /// </summary>
    private bool CheckResultMatch(string detectedResult, string expectedResult)
    {
        // Simple string matching - can be expanded for more complex matching
        return detectedResult.ToLower().Contains(expectedResult.ToLower()) ||
               expectedResult.ToLower().Contains(detectedResult.ToLower());
    }
    
    /// <summary>
    /// Plays test-specific sounds.
    /// </summary>
    private void PlayTestSound(QualitativeTestType testType)
    {
        if (AudioManager.Instance == null) return;
        
        switch (testType)
        {
            case QualitativeTestType.FlameTest:
                AudioManager.Instance.PlaySFX(flameTestSound);
                break;
            case QualitativeTestType.PrecipitationTest:
                AudioManager.Instance.PlaySFX(precipitationSound);
                break;
            case QualitativeTestType.ColorChangeTest:
                AudioManager.Instance.PlaySFX(colorChangeSound);
                break;
            case QualitativeTestType.GasEvolutionTest:
                AudioManager.Instance.PlaySFX(gasEvolutionSound);
                break;
        }
    }
    
    /// <summary>
    /// Calculates final data for the experiment.
    /// </summary>
    private void CalculateFinalData(QualitativeInstance instance)
    {
        instance.data.testDuration = instance.completionTime - instance.startTime;
        
        if (instance.data.intensities.Count > 0)
        {
            float sum = 0f;
            foreach (float intensity in instance.data.intensities)
            {
                sum += intensity;
            }
            instance.data.averageIntensity = sum / instance.data.intensities.Count;
        }
    }
    
    /// <summary>
    /// Generates a qualitative result.
    /// </summary>
    private QualitativeResult GenerateQualitativeResult(QualitativeInstance instance)
    {
        QualitativeExperiment experiment = experimentDatabase[instance.experimentId];
        
        float accuracy = CalculateAccuracy(instance, experiment);
        float precision = CalculatePrecision(instance);
        float percentageError = CalculatePercentageError(instance.resultIntensity, experiment.expectedIntensity);
        string grade = CalculateGrade(percentageError);
        
        QualitativeResult result = new QualitativeResult
        {
            id = GenerateResultId(),
            experimentId = instance.experimentId,
            isSuccessful = instance.resultDetected && percentageError <= acceptableError,
            detectedResult = instance.detectedResult,
            expectedResult = experiment.expectedResult,
            accuracy = accuracy,
            precision = precision,
            resultIntensity = instance.resultIntensity,
            expectedIntensity = experiment.expectedIntensity,
            percentageError = percentageError,
            testData = new List<TestPoint>(instance.testPoints),
            data = new QualitativeData(instance.data),
            grade = grade,
            feedback = GenerateFeedback(instance, experiment, percentageError)
        };
        
        return result;
    }
    
    /// <summary>
    /// Calculates the accuracy of the experiment.
    /// </summary>
    private float CalculateAccuracy(QualitativeInstance instance, QualitativeExperiment experiment)
    {
        if (string.IsNullOrEmpty(instance.detectedResult)) return 0f;
        
        // Check if detected result matches expected result
        bool resultMatch = CheckResultMatch(instance.detectedResult, experiment.expectedResult);
        return resultMatch ? 1f : 0f;
    }
    
    /// <summary>
    /// Calculates the precision of the experiment.
    /// </summary>
    private float CalculatePrecision(QualitativeInstance instance)
    {
        if (instance.data.intensities.Count < 2) return 1f;
        
        // Calculate standard deviation of intensities
        float mean = instance.data.averageIntensity;
        float variance = 0f;
        
        foreach (float intensity in instance.data.intensities)
        {
            variance += Mathf.Pow(intensity - mean, 2);
        }
        variance /= instance.data.intensities.Count;
        
        float standardDeviation = Mathf.Sqrt(variance);
        float precision = 1f - (standardDeviation / mean);
        
        return Mathf.Clamp(precision, 0f, 1f);
    }
    
    /// <summary>
    /// Calculates the percentage error.
    /// </summary>
    private float CalculatePercentageError(float detected, float expected)
    {
        if (expected <= 0f) return 100f;
        
        return Mathf.Abs(detected - expected) / expected * 100f;
    }
    
    /// <summary>
    /// Calculates a grade based on percentage error.
    /// </summary>
    private string CalculateGrade(float percentageError)
    {
        if (percentageError <= 5f) return "A";
        if (percentageError <= 15f) return "B";
        if (percentageError <= 25f) return "C";
        if (percentageError <= 50f) return "D";
        return "F";
    }
    
    /// <summary>
    /// Generates feedback for the experiment.
    /// </summary>
    private List<string> GenerateFeedback(QualitativeInstance instance, QualitativeExperiment experiment, float percentageError)
    {
        List<string> feedback = new List<string>();
        
        if (instance.resultDetected)
        {
            feedback.Add("Successfully detected the expected result.");
        }
        else
        {
            feedback.Add("Failed to detect the expected result. Check your technique.");
        }
        
        if (percentageError <= acceptableError)
        {
            feedback.Add("Excellent work! Your results are very accurate.");
        }
        else if (percentageError <= 25f)
        {
            feedback.Add("Good work! Your results are reasonably accurate.");
        }
        else
        {
            feedback.Add("Your results need improvement. Review the procedure.");
        }
        
        return feedback;
    }
    
    /// <summary>
    /// Generates a unique experiment ID.
    /// </summary>
    private string GenerateExperimentId()
    {
        return $"qual_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique result ID.
    /// </summary>
    private string GenerateResultId()
    {
        return $"qual_res_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsExperimentInProgress() => isExperimentInProgress;
    public int GetActiveExperimentCount() => activeExperiments.Count;
    public int GetExperimentResultCount() => experimentResults.Count;
    
    /// <summary>
    /// Gets an experiment instance by ID.
    /// </summary>
    public QualitativeInstance GetExperiment(string instanceId)
    {
        return activeExperiments.ContainsKey(instanceId) ? activeExperiments[instanceId] : null;
    }
    
    /// <summary>
    /// Gets experiment data by ID.
    /// </summary>
    public QualitativeExperiment GetExperimentData(string experimentId)
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
    public List<QualitativeResult> GetExperimentResults()
    {
        return new List<QualitativeResult>(experimentResults);
    }
    
    /// <summary>
    /// Sets the test duration.
    /// </summary>
    public void SetTestDuration(float duration)
    {
        testDuration = Mathf.Clamp(duration, 1f, 30f);
    }
    
    /// <summary>
    /// Sets the detection threshold.
    /// </summary>
    public void SetDetectionThreshold(float threshold)
    {
        detectionThreshold = Mathf.Clamp(threshold, 0.01f, 1f);
    }
    
    /// <summary>
    /// Enables or disables real-time tests.
    /// </summary>
    public void SetRealTimeTestsEnabled(bool enabled)
    {
        enableRealTimeTests = enabled;
    }
    
    /// <summary>
    /// Generates a qualitative analysis report.
    /// </summary>
    public string GenerateQualitativeReport(string instanceId)
    {
        if (!activeExperiments.ContainsKey(instanceId)) return "";
        
        QualitativeInstance instance = activeExperiments[instanceId];
        QualitativeExperiment experiment = experimentDatabase[instance.experimentId];
        
        string report = "=== Qualitative Analysis Report ===\n";
        report += $"Experiment: {instance.name}\n";
        report += $"Test Type: {experiment.type}\n";
        report += $"Detected Result: {instance.detectedResult}\n";
        report += $"Expected Result: {experiment.expectedResult}\n";
        report += $"Result Intensity: {instance.resultIntensity:F2}\n";
        report += $"Number of Tests: {instance.data.numberOfTests}\n";
        report += $"Test Duration: {instance.data.testDuration:F1}s\n";
        report += "=====================================\n";
        
        return report;
    }
    
    /// <summary>
    /// Logs the current qualitative analysis module status.
    /// </summary>
    public void LogQualitativeStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Qualitative Analysis Module Status ===");
        Debug.Log($"Active Experiments: {activeExperiments.Count}");
        Debug.Log($"Experiment Results: {experimentResults.Count}");
        Debug.Log($"Is Experiment In Progress: {isExperimentInProgress}");
        Debug.Log($"Experiment Database Size: {experimentDatabase.Count}");
        Debug.Log($"Qualitative Analysis: {(enableQualitativeAnalysis ? "Enabled" : "Disabled")}");
        Debug.Log($"Flame Tests: {(enableFlameTests ? "Enabled" : "Disabled")}");
        Debug.Log($"Precipitation Tests: {(enablePrecipitationTests ? "Enabled" : "Disabled")}");
        Debug.Log($"Color Change Tests: {(enableColorChangeTests ? "Enabled" : "Disabled")}");
        Debug.Log($"Gas Evolution Tests: {(enableGasEvolutionTests ? "Enabled" : "Disabled")}");
        Debug.Log($"Real-Time Tests: {(enableRealTimeTests ? "Enabled" : "Disabled")}");
        Debug.Log("==========================================");
    }
} 