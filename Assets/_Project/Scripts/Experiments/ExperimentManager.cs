using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Simple experiment manager to handle basic experiment loading and management.
/// This is a minimal implementation for the virtual chemistry lab.
/// </summary>
public class ExperimentManager : MonoBehaviour
{
    [Header("Experiment Management")]
    [SerializeField] private string currentExperimentId = "";
    [SerializeField] private int currentStepIndex = 0;
    [SerializeField] private bool isExperimentActive = false;
    [SerializeField] private float experimentTimer = 0f;
    
    [Header("Data Loading")]
    [SerializeField] private string experimentsDataPath = "Data/Experiments";
    [SerializeField] private bool autoLoadExperiments = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private ExperimentData currentExperiment;
    private List<ExperimentData> availableExperiments = new List<ExperimentData>();
    private Dictionary<string, ExperimentData> experimentLookup = new Dictionary<string, ExperimentData>();
    
    private static ExperimentManager instance;
    public static ExperimentManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ExperimentManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ExperimentManager");
                    instance = go.AddComponent<ExperimentManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeExperimentManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (autoLoadExperiments)
        {
            LoadAllExperiments();
        }
    }
    
    private void Update()
    {
        if (isExperimentActive)
        {
            experimentTimer += Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Initializes the experiment manager with basic settings.
    /// </summary>
    private void InitializeExperimentManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("ExperimentManager initialized successfully");
        }
        
        // Initialize experiment data structures
        availableExperiments = new List<ExperimentData>();
        experimentLookup = new Dictionary<string, ExperimentData>();
    }
    
    /// <summary>
    /// Loads all available experiments from the Resources folder.
    /// </summary>
    public void LoadAllExperiments()
    {
        if (enableDebugLogging)
        {
            Debug.Log($"Loading experiments from: {experimentsDataPath}");
        }
        
        TextAsset[] experimentFiles = Resources.LoadAll<TextAsset>(experimentsDataPath);
        
        foreach (TextAsset file in experimentFiles)
        {
            try
            {
                ExperimentData experiment = JsonUtility.FromJson<ExperimentData>(file.text);
                if (experiment != null && !string.IsNullOrEmpty(experiment.id))
                {
                    availableExperiments.Add(experiment);
                    experimentLookup[experiment.id] = experiment;
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"Loaded experiment: {experiment.title} (ID: {experiment.id})");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load experiment from {file.name}: {e.Message}");
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Total experiments loaded: {availableExperiments.Count}");
        }
    }
    
    /// <summary>
    /// Starts an experiment by ID.
    /// </summary>
    public void StartExperiment(string experimentId)
    {
        if (string.IsNullOrEmpty(experimentId))
        {
            Debug.LogError("Experiment ID is null or empty");
            return;
        }
        
        if (!experimentLookup.ContainsKey(experimentId))
        {
            Debug.LogError($"Experiment not found: {experimentId}");
            return;
        }
        
        currentExperiment = experimentLookup[experimentId];
        currentExperimentId = experimentId;
        currentStepIndex = 0;
        experimentTimer = 0f;
        isExperimentActive = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Started experiment: {currentExperiment.title}");
        }
        
        // Notify other systems that experiment has started
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartExperiment(experimentId);
        }
    }
    
    /// <summary>
    /// Stops the current experiment.
    /// </summary>
    public void StopExperiment()
    {
        if (!isExperimentActive)
        {
            Debug.LogWarning("No experiment is currently active");
            return;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Stopped experiment: {currentExperiment?.title}");
        }
        
        isExperimentActive = false;
        currentExperiment = null;
        currentExperimentId = "";
        currentStepIndex = 0;
        experimentTimer = 0f;
        
        // Notify other systems that experiment has stopped
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteExperiment();
        }
    }
    
    /// <summary>
    /// Pauses the current experiment.
    /// </summary>
    public void PauseExperiment()
    {
        if (!isExperimentActive)
        {
            Debug.LogWarning("No experiment is currently active");
            return;
        }
        
        isExperimentActive = false;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Paused experiment: {currentExperiment.title}");
        }
    }
    
    /// <summary>
    /// Resumes the current experiment.
    /// </summary>
    public void ResumeExperiment()
    {
        if (currentExperiment == null)
        {
            Debug.LogWarning("No experiment to resume");
            return;
        }
        
        isExperimentActive = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Resumed experiment: {currentExperiment.title}");
        }
    }
    
    /// <summary>
    /// Validates the current step and advances if valid.
    /// </summary>
    public void ValidateCurrentStep()
    {
        if (!isExperimentActive || currentExperiment == null)
        {
            Debug.LogWarning("No experiment is currently active");
            return;
        }
        
        if (currentStepIndex >= currentExperiment.steps.Length)
        {
            Debug.LogWarning("Already at the last step");
            return;
        }
        
        ExperimentStep currentStep = currentExperiment.steps[currentStepIndex];
        
        if (enableDebugLogging)
        {
            Debug.Log($"Validating step {currentStepIndex + 1}: {currentStep.instruction}");
        }
        
        // For now, just advance to the next step
        // In a full implementation, this would check actual experiment conditions
        currentStepIndex++;
        
        if (currentStepIndex >= currentExperiment.steps.Length)
        {
            // Experiment completed
            if (enableDebugLogging)
            {
                Debug.Log("Experiment completed!");
            }
            
            StopExperiment();
        }
    }
    
    /// <summary>
    /// Gets the current experiment data.
    /// </summary>
    public ExperimentData GetCurrentExperiment()
    {
        return currentExperiment;
    }
    
    /// <summary>
    /// Gets the current step index.
    /// </summary>
    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
    
    /// <summary>
    /// Gets the current step data.
    /// </summary>
    public ExperimentStep GetCurrentStep()
    {
        if (currentExperiment != null && currentStepIndex < currentExperiment.steps.Length)
        {
            return currentExperiment.steps[currentStepIndex];
        }
        return null;
    }
    
    /// <summary>
    /// Gets all available experiments.
    /// </summary>
    public List<ExperimentData> GetAvailableExperiments()
    {
        return new List<ExperimentData>(availableExperiments);
    }
    
    /// <summary>
    /// Gets an experiment by ID.
    /// </summary>
    public ExperimentData GetExperimentById(string experimentId)
    {
        if (experimentLookup.ContainsKey(experimentId))
        {
            return experimentLookup[experimentId];
        }
        return null;
    }
    
    /// <summary>
    /// Checks if an experiment is currently active.
    /// </summary>
    public bool IsExperimentActive()
    {
        return isExperimentActive;
    }
    
    /// <summary>
    /// Gets the current experiment timer.
    /// </summary>
    public float GetExperimentTimer()
    {
        return experimentTimer;
    }
    
    /// <summary>
    /// Gets the current experiment ID.
    /// </summary>
    public string GetCurrentExperimentId()
    {
        return currentExperimentId;
    }
    
    /// <summary>
    /// Gets the total number of steps in the current experiment.
    /// </summary>
    public int GetTotalSteps()
    {
        if (currentExperiment != null)
        {
            return currentExperiment.steps.Length;
        }
        return 0;
    }
    
    /// <summary>
    /// Gets the progress percentage of the current experiment.
    /// </summary>
    public float GetExperimentProgress()
    {
        if (currentExperiment == null || currentExperiment.steps.Length == 0)
        {
            return 0f;
        }
        
        return (float)currentStepIndex / currentExperiment.steps.Length;
    }
    
    /// <summary>
    /// Logs the current experiment status.
    /// </summary>
    public void LogExperimentStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Experiment Manager Status ===");
        Debug.Log($"Active: {isExperimentActive}");
        Debug.Log($"Current Experiment: {currentExperiment?.title ?? "None"}");
        Debug.Log($"Current Step: {currentStepIndex + 1}/{GetTotalSteps()}");
        Debug.Log($"Progress: {GetExperimentProgress():P1}");
        Debug.Log($"Timer: {experimentTimer:F1}s");
        Debug.Log($"Available Experiments: {availableExperiments.Count}");
        Debug.Log("================================");
    }
} 