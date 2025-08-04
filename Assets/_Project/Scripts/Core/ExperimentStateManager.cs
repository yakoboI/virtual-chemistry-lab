using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages the state of individual experiments including steps, progress, and validation.
/// This component handles experiment-specific state separate from general game state.
/// </summary>
public class ExperimentStateManager : MonoBehaviour
{
    [Header("Experiment State")]
    [SerializeField] private string currentExperimentId = "";
    [SerializeField] private ExperimentState currentState = ExperimentState.NotStarted;
    [SerializeField] private int currentStepIndex = 0;
    [SerializeField] private float experimentStartTime = 0f;
    [SerializeField] private float currentStepStartTime = 0f;
    
    [Header("Progress Tracking")]
    [SerializeField] private List<StepResult> completedSteps = new List<StepResult>();
    [SerializeField] private Dictionary<string, object> experimentData = new Dictionary<string, object>();
    [SerializeField] private int totalSteps = 0;
    [SerializeField] private float totalExperimentTime = 0f;
    
    [Header("Validation")]
    [SerializeField] private bool enableStepValidation = true;
    [SerializeField] private bool enableRealTimeFeedback = true;
    [SerializeField] private float validationTimeout = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private static ExperimentStateManager instance;
    public static ExperimentStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ExperimentStateManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ExperimentStateManager");
                    instance = go.AddComponent<ExperimentStateManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnExperimentStarted;
    public event Action<string> OnExperimentCompleted;
    public event Action<int> OnStepChanged;
    public event Action<StepResult> OnStepCompleted;
    public event Action<string> OnValidationFailed;
    public event Action<ExperimentState> OnStateChanged;
    
    public enum ExperimentState
    {
        NotStarted,
        Loading,
        Ready,
        InProgress,
        Paused,
        Completed,
        Failed,
        SafetyViolation
    }
    
    [System.Serializable]
    public class StepResult
    {
        public int stepNumber;
        public string stepTitle;
        public bool isCompleted;
        public float completionTime;
        public float accuracy;
        public string feedback;
        public Dictionary<string, object> stepData;
        
        public StepResult(int step, string title)
        {
            stepNumber = step;
            stepTitle = title;
            isCompleted = false;
            completionTime = 0f;
            accuracy = 0f;
            feedback = "";
            stepData = new Dictionary<string, object>();
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStateManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        UpdateExperimentTime();
        HandleStepTimeout();
    }
    
    /// <summary>
    /// Initializes the experiment state manager.
    /// </summary>
    private void InitializeStateManager()
    {
        ResetExperimentState();
        
        if (enableDebugLogging)
        {
            Debug.Log("ExperimentStateManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Starts a new experiment.
    /// </summary>
    public void StartExperiment(string experimentId, int totalStepsCount = 0)
    {
        if (string.IsNullOrEmpty(experimentId))
        {
            Debug.LogError("Experiment ID is null or empty");
            return;
        }
        
        ResetExperimentState();
        
        currentExperimentId = experimentId;
        totalSteps = totalStepsCount;
        currentState = ExperimentState.Loading;
        
        experimentStartTime = Time.time;
        currentStepStartTime = Time.time;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Starting experiment: {experimentId} with {totalSteps} steps");
        }
        
        OnStateChanged?.Invoke(currentState);
        
        // Load experiment data
        LoadExperimentData(experimentId);
        
        // Transition to ready state
        ChangeState(ExperimentState.Ready);
        
        OnExperimentStarted?.Invoke(experimentId);
    }
    
    /// <summary>
    /// Begins the experiment procedure.
    /// </summary>
    public void BeginExperiment()
    {
        if (currentState == ExperimentState.Ready)
        {
            ChangeState(ExperimentState.InProgress);
            currentStepIndex = 0;
            currentStepStartTime = Time.time;
            
            if (enableDebugLogging)
            {
                Debug.Log($"Experiment {currentExperimentId} began - starting step {currentStepIndex + 1}");
            }
            
            OnStepChanged?.Invoke(currentStepIndex);
        }
    }
    
    /// <summary>
    /// Moves to the next step in the experiment.
    /// </summary>
    public void NextStep()
    {
        if (currentState != ExperimentState.InProgress)
        {
            Debug.LogWarning("Cannot advance step - experiment not in progress");
            return;
        }
        
        // Complete current step
        CompleteCurrentStep();
        
        // Move to next step
        currentStepIndex++;
        currentStepStartTime = Time.time;
        
        if (currentStepIndex >= totalSteps)
        {
            CompleteExperiment();
        }
        else
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Advanced to step {currentStepIndex + 1} of {totalSteps}");
            }
            
            OnStepChanged?.Invoke(currentStepIndex);
        }
    }
    
    /// <summary>
    /// Completes the current step with validation.
    /// </summary>
    public void CompleteCurrentStep()
    {
        if (currentStepIndex < completedSteps.Count)
        {
            StepResult stepResult = completedSteps[currentStepIndex];
            stepResult.isCompleted = true;
            stepResult.completionTime = Time.time - currentStepStartTime;
            
            if (enableDebugLogging)
            {
                Debug.Log($"Completed step {currentStepIndex + 1}: {stepResult.stepTitle}");
            }
            
            OnStepCompleted?.Invoke(stepResult);
        }
    }
    
    /// <summary>
    /// Validates the current step action.
    /// </summary>
    public bool ValidateStepAction(string actionType, Dictionary<string, object> parameters)
    {
        if (!enableStepValidation || currentState != ExperimentState.InProgress)
        {
            return true;
        }
        
        // Store step data for validation
        if (currentStepIndex < completedSteps.Count)
        {
            StepResult currentStep = completedSteps[currentStepIndex];
            currentStep.stepData["actionType"] = actionType;
            currentStep.stepData["parameters"] = parameters;
            currentStep.stepData["validationTime"] = Time.time;
        }
        
        // Basic validation logic - can be extended
        bool isValid = true;
        string validationMessage = "";
        
        // Example validation for titration
        if (actionType == "titration" && parameters.ContainsKey("volume"))
        {
            float volume = Convert.ToSingle(parameters["volume"]);
            if (volume < 0 || volume > 50)
            {
                isValid = false;
                validationMessage = "Invalid titration volume";
            }
        }
        
        if (!isValid)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Step validation failed: {validationMessage}");
            }
            
            OnValidationFailed?.Invoke(validationMessage);
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Completes the entire experiment.
    /// </summary>
    public void CompleteExperiment()
    {
        if (currentState == ExperimentState.InProgress)
        {
            totalExperimentTime = Time.time - experimentStartTime;
            ChangeState(ExperimentState.Completed);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Experiment {currentExperimentId} completed in {totalExperimentTime:F2} seconds");
            }
            
            OnExperimentCompleted?.Invoke(currentExperimentId);
        }
    }
    
    /// <summary>
    /// Pauses the experiment.
    /// </summary>
    public void PauseExperiment()
    {
        if (currentState == ExperimentState.InProgress)
        {
            ChangeState(ExperimentState.Paused);
            
            if (enableDebugLogging)
            {
                Debug.Log("Experiment paused");
            }
        }
    }
    
    /// <summary>
    /// Resumes the experiment.
    /// </summary>
    public void ResumeExperiment()
    {
        if (currentState == ExperimentState.Paused)
        {
            ChangeState(ExperimentState.InProgress);
            currentStepStartTime = Time.time; // Reset step timer
            
            if (enableDebugLogging)
            {
                Debug.Log("Experiment resumed");
            }
        }
    }
    
    /// <summary>
    /// Handles safety violations.
    /// </summary>
    public void HandleSafetyViolation(string violationDescription)
    {
        ChangeState(ExperimentState.SafetyViolation);
        
        if (enableDebugLogging)
        {
            Debug.LogError($"Safety violation in experiment {currentExperimentId}: {violationDescription}");
        }
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleSafetyViolation(violationDescription);
        }
    }
    
    /// <summary>
    /// Resets the experiment state.
    /// </summary>
    public void ResetExperimentState()
    {
        currentExperimentId = "";
        currentState = ExperimentState.NotStarted;
        currentStepIndex = 0;
        experimentStartTime = 0f;
        currentStepStartTime = 0f;
        totalExperimentTime = 0f;
        totalSteps = 0;
        
        completedSteps.Clear();
        experimentData.Clear();
        
        if (enableDebugLogging)
        {
            Debug.Log("Experiment state reset");
        }
    }
    
    /// <summary>
    /// Changes the experiment state.
    /// </summary>
    private void ChangeState(ExperimentState newState)
    {
        if (currentState == newState) return;
        
        ExperimentState previousState = currentState;
        currentState = newState;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Experiment state changed: {previousState} -> {newState}");
        }
        
        OnStateChanged?.Invoke(newState);
    }
    
    /// <summary>
    /// Loads experiment data from JSON.
    /// </summary>
    private void LoadExperimentData(string experimentId)
    {
        // This will be implemented to load from JSON files
        // For now, create placeholder steps
        completedSteps.Clear();
        
        for (int i = 0; i < totalSteps; i++)
        {
            completedSteps.Add(new StepResult(i + 1, $"Step {i + 1}"));
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded experiment data for {experimentId}");
        }
    }
    
    /// <summary>
    /// Updates experiment timing.
    /// </summary>
    private void UpdateExperimentTime()
    {
        if (currentState == ExperimentState.InProgress)
        {
            totalExperimentTime = Time.time - experimentStartTime;
        }
    }
    
    /// <summary>
    /// Handles step timeout validation.
    /// </summary>
    private void HandleStepTimeout()
    {
        if (currentState == ExperimentState.InProgress && 
            Time.time - currentStepStartTime > validationTimeout)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Step {currentStepIndex + 1} timeout - taking too long");
            }
        }
    }
    
    // Public getters for other systems
    public string GetCurrentExperimentId() => currentExperimentId;
    public ExperimentState GetCurrentState() => currentState;
    public int GetCurrentStepIndex() => currentStepIndex;
    public int GetTotalSteps() => totalSteps;
    public float GetExperimentTime() => totalExperimentTime;
    public float GetCurrentStepTime() => Time.time - currentStepStartTime;
    public List<StepResult> GetCompletedSteps() => new List<StepResult>(completedSteps);
    public Dictionary<string, object> GetExperimentData() => new Dictionary<string, object>(experimentData);
    
    /// <summary>
    /// Sets experiment data.
    /// </summary>
    public void SetExperimentData(string key, object value)
    {
        experimentData[key] = value;
    }
    
    /// <summary>
    /// Gets experiment data.
    /// </summary>
    public object GetExperimentData(string key)
    {
        return experimentData.ContainsKey(key) ? experimentData[key] : null;
    }
    
    /// <summary>
    /// Logs the current experiment status.
    /// </summary>
    public void LogExperimentStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Experiment State Manager Status ===");
        Debug.Log($"Current Experiment: {currentExperimentId}");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Current Step: {currentStepIndex + 1} of {totalSteps}");
        Debug.Log($"Experiment Time: {totalExperimentTime:F2}s");
        Debug.Log($"Step Time: {GetCurrentStepTime():F2}s");
        Debug.Log($"Completed Steps: {completedSteps.Count}");
        Debug.Log($"Experiment Data Entries: {experimentData.Count}");
        Debug.Log("=======================================");
    }
} 