using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages experiment assessment, evaluation, and scoring for the virtual chemistry lab.
/// This component handles all assessment-related operations and evaluation procedures.
/// </summary>
public class AssessmentManager : MonoBehaviour
{
    [Header("Assessment Management")]
    [SerializeField] private bool enableAssessmentManagement = true;
    [SerializeField] private bool enableRealTimeAssessment = true;
    [SerializeField] private bool enableAutomaticScoring = true;
    [SerializeField] private bool enableDetailedFeedback = true;
    [SerializeField] private bool enableProgressTracking = true;
    
    [Header("Assessment Configuration")]
    [SerializeField] private AssessmentCriteria[] availableCriteria;
    [SerializeField] private string assessmentCompleteSound = "assessment_complete";
    [SerializeField] private string scoreUpdateSound = "score_update";
    
    [Header("Assessment State")]
    [SerializeField] private Dictionary<string, AssessmentInstance> activeAssessments = new Dictionary<string, AssessmentInstance>();
    [SerializeField] private List<AssessmentResult> assessmentResults = new List<AssessmentResult>();
    [SerializeField] private bool isAssessmentInProgress = false;
    
    [Header("Scoring Settings")]
    [SerializeField] private bool enableWeightedScoring = true;
    [SerializeField] private bool enablePartialCredit = true;
    [SerializeField] private bool enableBonusPoints = true;
    [SerializeField] private float passingThreshold = 0.7f;
    [SerializeField] private float excellentThreshold = 0.9f;
    [SerializeField] private int maxBonusPoints = 10;
    
    [Header("Feedback Settings")]
    [SerializeField] private bool enableImmediateFeedback = true;
    [SerializeField] private bool enableDetailedComments = true;
    [SerializeField] private bool enableSuggestionSystem = true;
    [SerializeField] private bool enableLearningObjectives = true;
    [SerializeField] private string feedbackLanguage = "en";
    
    [Header("Progress Tracking")]
    [SerializeField] private bool enableProgressSaving = true;
    [SerializeField] private bool enablePerformanceAnalytics = true;
    [SerializeField] private bool enableTrendAnalysis = true;
    [SerializeField] private int maxStoredResults = 100;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logAssessmentEvents = false;
    
    private static AssessmentManager instance;
    public static AssessmentManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AssessmentManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AssessmentManager");
                    instance = go.AddComponent<AssessmentManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnAssessmentStarted;
    public event Action<string> OnAssessmentCompleted;
    public event Action<AssessmentResult> OnAssessmentResult;
    public event Action<float> OnScoreUpdated;
    public event Action<string> OnFeedbackGenerated;
    public event Action<string> OnAssessmentError;
    
    // Private variables
    private Dictionary<string, AssessmentCriteria> criteriaDatabase = new Dictionary<string, AssessmentCriteria>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class AssessmentCriteria
    {
        public string id;
        public string name;
        public string description;
        public AssessmentType type;
        public float weight;
        public float maxScore;
        public string[] learningObjectives;
        public ScoringRubric rubric;
        public string[] requiredSkills;
    }
    
    [System.Serializable]
    public class AssessmentInstance
    {
        public string id;
        public string experimentId;
        public string studentId;
        public bool isActive;
        public bool isCompleted;
        public float startTime;
        public float completionTime;
        public float currentScore;
        public float maxPossibleScore;
        public AssessmentStatus status;
        public List<CriterionResult> criterionResults;
        public List<string> feedback;
        public Dictionary<string, object> assessmentData;
    }
    
    [System.Serializable]
    public class AssessmentResult
    {
        public string id;
        public string experimentId;
        public string studentId;
        public float totalScore;
        public float maxScore;
        public float percentageScore;
        public string grade;
        public AssessmentStatus status;
        public List<CriterionResult> criterionResults;
        public List<string> feedback;
        public List<string> suggestions;
        public float completionTime;
        public DateTime timestamp;
        public PerformanceAnalytics analytics;
    }
    
    [System.Serializable]
    public class CriterionResult
    {
        public string criterionId;
        public string criterionName;
        public float score;
        public float maxScore;
        public float weight;
        public string feedback;
        public bool isCompleted;
        public List<string> errors;
        public List<string> warnings;
    }
    
    [System.Serializable]
    public class ScoringRubric
    {
        public RubricLevel excellent;
        public RubricLevel good;
        public RubricLevel satisfactory;
        public RubricLevel poor;
        public RubricLevel fail;
    }
    
    [System.Serializable]
    public class RubricLevel
    {
        public float minScore;
        public float maxScore;
        public string description;
        public string feedback;
        public int points;
    }
    
    [System.Serializable]
    public class PerformanceAnalytics
    {
        public float averageScore;
        public float bestScore;
        public float worstScore;
        public int totalAttempts;
        public float improvementRate;
        public List<float> scoreHistory;
        public Dictionary<string, float> skillScores;
    }
    
    [System.Serializable]
    public enum AssessmentType
    {
        Accuracy,
        Technique,
        Safety,
        TimeManagement,
        Understanding,
        ProblemSolving,
        Communication,
        Teamwork
    }
    
    [System.Serializable]
    public enum AssessmentStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed,
        PendingReview
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAssessmentManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadCriteriaDatabase();
    }
    
    /// <summary>
    /// Initializes the assessment manager.
    /// </summary>
    private void InitializeAssessmentManager()
    {
        activeAssessments.Clear();
        assessmentResults.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("AssessmentManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads assessment criteria from available criteria.
    /// </summary>
    private void LoadCriteriaDatabase()
    {
        criteriaDatabase.Clear();
        
        foreach (AssessmentCriteria criteria in availableCriteria)
        {
            if (criteria != null && !string.IsNullOrEmpty(criteria.id))
            {
                criteriaDatabase[criteria.id] = criteria;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded assessment criteria: {criteria.name} ({criteria.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {criteriaDatabase.Count} assessment criteria");
        }
    }
    
    /// <summary>
    /// Creates a new assessment instance.
    /// </summary>
    public AssessmentInstance CreateAssessment(string experimentId, string studentId)
    {
        if (!enableAssessmentManagement || string.IsNullOrEmpty(experimentId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError("Assessment creation failed - invalid parameters");
            }
            return null;
        }
        
        AssessmentInstance instance = new AssessmentInstance
        {
            id = GenerateAssessmentId(),
            experimentId = experimentId,
            studentId = studentId,
            isActive = true,
            isCompleted = false,
            startTime = Time.time,
            completionTime = 0f,
            currentScore = 0f,
            maxPossibleScore = CalculateMaxPossibleScore(experimentId),
            status = AssessmentStatus.NotStarted,
            criterionResults = new List<CriterionResult>(),
            feedback = new List<string>(),
            assessmentData = new Dictionary<string, object>()
        };
        
        // Initialize criterion results
        InitializeCriterionResults(instance);
        
        activeAssessments[instance.id] = instance;
        
        OnAssessmentStarted?.Invoke(instance.id);
        
        if (logAssessmentEvents)
        {
            Debug.Log($"Created assessment: {instance.id} for experiment {experimentId}");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Starts an assessment.
    /// </summary>
    public bool StartAssessment(string assessmentId)
    {
        if (!activeAssessments.ContainsKey(assessmentId)) return false;
        
        AssessmentInstance instance = activeAssessments[assessmentId];
        
        if (instance.isCompleted)
        {
            OnAssessmentError?.Invoke($"Assessment {assessmentId} is already completed");
            return false;
        }
        
        instance.status = AssessmentStatus.InProgress;
        isAssessmentInProgress = true;
        
        if (logAssessmentEvents)
        {
            Debug.Log($"Started assessment: {assessmentId}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Completes an assessment.
    /// </summary>
    public void CompleteAssessment(string assessmentId)
    {
        if (!activeAssessments.ContainsKey(assessmentId)) return;
        
        AssessmentInstance instance = activeAssessments[assessmentId];
        
        instance.isActive = false;
        instance.isCompleted = true;
        instance.completionTime = Time.time;
        instance.status = AssessmentStatus.Completed;
        
        // Calculate final score
        CalculateFinalScore(instance);
        
        // Generate assessment result
        AssessmentResult result = GenerateAssessmentResult(instance);
        assessmentResults.Add(result);
        
        isAssessmentInProgress = false;
        
        OnAssessmentCompleted?.Invoke(assessmentId);
        OnAssessmentResult?.Invoke(result);
        
        // Play completion sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(assessmentCompleteSound);
        }
        
        if (logAssessmentEvents)
        {
            Debug.Log($"Completed assessment: {assessmentId} - Score: {result.percentageScore:F1}%");
        }
    }
    
    /// <summary>
    /// Updates a criterion score.
    /// </summary>
    public bool UpdateCriterionScore(string assessmentId, string criterionId, float score, string feedback = "")
    {
        if (!activeAssessments.ContainsKey(assessmentId)) return false;
        
        AssessmentInstance instance = activeAssessments[assessmentId];
        CriterionResult criterionResult = instance.criterionResults.Find(c => c.criterionId == criterionId);
        
        if (criterionResult == null) return false;
        
        float previousScore = criterionResult.score;
        criterionResult.score = Mathf.Clamp(score, 0f, criterionResult.maxScore);
        criterionResult.feedback = feedback;
        criterionResult.isCompleted = true;
        
        // Update total score
        float scoreDifference = criterionResult.score - previousScore;
        instance.currentScore += scoreDifference;
        
        OnScoreUpdated?.Invoke(instance.currentScore);
        
        // Play score update sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(scoreUpdateSound);
        }
        
        if (logAssessmentEvents)
        {
            Debug.Log($"Updated criterion {criterionId} score: {score:F1}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Evaluates a specific action or step.
    /// </summary>
    public bool EvaluateAction(string assessmentId, string actionType, Dictionary<string, object> parameters)
    {
        if (!activeAssessments.ContainsKey(assessmentId)) return false;
        
        AssessmentInstance instance = activeAssessments[assessmentId];
        
        // Find relevant criteria for this action
        List<AssessmentCriteria> relevantCriteria = FindRelevantCriteria(actionType);
        
        foreach (AssessmentCriteria criteria in relevantCriteria)
        {
            float score = EvaluateCriteria(criteria, parameters);
            string feedback = GenerateFeedback(criteria, score, parameters);
            
            UpdateCriterionScore(assessmentId, criteria.id, score, feedback);
        }
        
        return true;
    }
    
    /// <summary>
    /// Generates feedback for an assessment.
    /// </summary>
    public List<string> GenerateFeedback(string assessmentId)
    {
        if (!activeAssessments.ContainsKey(assessmentId)) return new List<string>();
        
        AssessmentInstance instance = activeAssessments[assessmentId];
        List<string> feedback = new List<string>();
        
        foreach (CriterionResult result in instance.criterionResults)
        {
            if (result.score < result.maxScore)
            {
                string criterionFeedback = GenerateCriterionFeedback(result);
                feedback.Add(criterionFeedback);
            }
        }
        
        // Add overall feedback
        if (instance.currentScore < instance.maxPossibleScore * passingThreshold)
        {
            feedback.Add("Overall: You need to improve your performance to pass this assessment.");
        }
        else if (instance.currentScore >= instance.maxPossibleScore * excellentThreshold)
        {
            feedback.Add("Overall: Excellent work! You've demonstrated mastery of the concepts.");
        }
        
        OnFeedbackGenerated?.Invoke(string.Join("\n", feedback));
        
        return feedback;
    }
    
    /// <summary>
    /// Initializes criterion results for an assessment.
    /// </summary>
    private void InitializeCriterionResults(AssessmentInstance instance)
    {
        instance.criterionResults.Clear();
        
        foreach (var kvp in criteriaDatabase)
        {
            AssessmentCriteria criteria = kvp.Value;
            
            CriterionResult result = new CriterionResult
            {
                criterionId = criteria.id,
                criterionName = criteria.name,
                score = 0f,
                maxScore = criteria.maxScore,
                weight = criteria.weight,
                feedback = "",
                isCompleted = false,
                errors = new List<string>(),
                warnings = new List<string>()
            };
            
            instance.criterionResults.Add(result);
        }
    }
    
    /// <summary>
    /// Calculates the maximum possible score for an experiment.
    /// </summary>
    private float CalculateMaxPossibleScore(string experimentId)
    {
        float maxScore = 0f;
        
        foreach (var kvp in criteriaDatabase)
        {
            AssessmentCriteria criteria = kvp.Value;
            maxScore += criteria.maxScore * criteria.weight;
        }
        
        return maxScore;
    }
    
    /// <summary>
    /// Calculates the final score for an assessment.
    /// </summary>
    private void CalculateFinalScore(AssessmentInstance instance)
    {
        float totalScore = 0f;
        float totalWeight = 0f;
        
        foreach (CriterionResult result in instance.criterionResults)
        {
            if (result.isCompleted)
            {
                totalScore += result.score * result.weight;
                totalWeight += result.weight;
            }
        }
        
        if (totalWeight > 0)
        {
            instance.currentScore = totalScore / totalWeight;
        }
    }
    
    /// <summary>
    /// Generates an assessment result.
    /// </summary>
    private AssessmentResult GenerateAssessmentResult(AssessmentInstance instance)
    {
        AssessmentResult result = new AssessmentResult
        {
            id = GenerateResultId(),
            experimentId = instance.experimentId,
            studentId = instance.studentId,
            totalScore = instance.currentScore,
            maxScore = instance.maxPossibleScore,
            percentageScore = instance.maxPossibleScore > 0 ? (instance.currentScore / instance.maxPossibleScore) * 100f : 0f,
            grade = CalculateGrade(instance.currentScore, instance.maxPossibleScore),
            status = instance.status,
            criterionResults = new List<CriterionResult>(instance.criterionResults),
            feedback = new List<string>(instance.feedback),
            suggestions = GenerateSuggestions(instance),
            completionTime = instance.completionTime - instance.startTime,
            timestamp = DateTime.Now,
            analytics = GenerateAnalytics(instance)
        };
        
        return result;
    }
    
    /// <summary>
    /// Calculates a grade based on score.
    /// </summary>
    private string CalculateGrade(float score, float maxScore)
    {
        float percentage = maxScore > 0 ? (score / maxScore) * 100f : 0f;
        
        if (percentage >= 90f) return "A";
        if (percentage >= 80f) return "B";
        if (percentage >= 70f) return "C";
        if (percentage >= 60f) return "D";
        return "F";
    }
    
    /// <summary>
    /// Finds criteria relevant to an action type.
    /// </summary>
    private List<AssessmentCriteria> FindRelevantCriteria(string actionType)
    {
        List<AssessmentCriteria> relevant = new List<AssessmentCriteria>();
        
        foreach (var kvp in criteriaDatabase)
        {
            AssessmentCriteria criteria = kvp.Value;
            
            // Simple matching logic - can be expanded
            if (actionType.Contains("safety") && criteria.type == AssessmentType.Safety)
            {
                relevant.Add(criteria);
            }
            else if (actionType.Contains("accuracy") && criteria.type == AssessmentType.Accuracy)
            {
                relevant.Add(criteria);
            }
            else if (actionType.Contains("technique") && criteria.type == AssessmentType.Technique)
            {
                relevant.Add(criteria);
            }
        }
        
        return relevant;
    }
    
    /// <summary>
    /// Evaluates a specific criterion.
    /// </summary>
    private float EvaluateCriteria(AssessmentCriteria criteria, Dictionary<string, object> parameters)
    {
        // Simple evaluation logic - can be expanded
        float score = 0f;
        
        switch (criteria.type)
        {
            case AssessmentType.Accuracy:
                score = EvaluateAccuracy(parameters);
                break;
            case AssessmentType.Safety:
                score = EvaluateSafety(parameters);
                break;
            case AssessmentType.Technique:
                score = EvaluateTechnique(parameters);
                break;
            case AssessmentType.TimeManagement:
                score = EvaluateTimeManagement(parameters);
                break;
            default:
                score = criteria.maxScore * 0.8f; // Default score
                break;
        }
        
        return Mathf.Clamp(score, 0f, criteria.maxScore);
    }
    
    /// <summary>
    /// Evaluates accuracy.
    /// </summary>
    private float EvaluateAccuracy(Dictionary<string, object> parameters)
    {
        // Simulate accuracy evaluation
        return UnityEngine.Random.Range(0.6f, 1.0f) * 10f;
    }
    
    /// <summary>
    /// Evaluates safety.
    /// </summary>
    private float EvaluateSafety(Dictionary<string, object> parameters)
    {
        // Simulate safety evaluation
        return UnityEngine.Random.Range(0.7f, 1.0f) * 10f;
    }
    
    /// <summary>
    /// Evaluates technique.
    /// </summary>
    private float EvaluateTechnique(Dictionary<string, object> parameters)
    {
        // Simulate technique evaluation
        return UnityEngine.Random.Range(0.5f, 1.0f) * 10f;
    }
    
    /// <summary>
    /// Evaluates time management.
    /// </summary>
    private float EvaluateTimeManagement(Dictionary<string, object> parameters)
    {
        // Simulate time management evaluation
        return UnityEngine.Random.Range(0.6f, 1.0f) * 10f;
    }
    
    /// <summary>
    /// Generates feedback for a criterion.
    /// </summary>
    private string GenerateFeedback(AssessmentCriteria criteria, float score, Dictionary<string, object> parameters)
    {
        float percentage = criteria.maxScore > 0 ? (score / criteria.maxScore) * 100f : 0f;
        
        if (percentage >= 90f)
        {
            return $"Excellent {criteria.name}! You've demonstrated mastery.";
        }
        else if (percentage >= 80f)
        {
            return $"Good {criteria.name}. Minor improvements needed.";
        }
        else if (percentage >= 70f)
        {
            return $"Satisfactory {criteria.name}. Some areas need attention.";
        }
        else
        {
            return $"Needs improvement in {criteria.name}. Review the concepts.";
        }
    }
    
    /// <summary>
    /// Generates criterion-specific feedback.
    /// </summary>
    private string GenerateCriterionFeedback(CriterionResult result)
    {
        float percentage = result.maxScore > 0 ? (result.score / result.maxScore) * 100f : 0f;
        
        return $"{result.criterionName}: {percentage:F1}% - {result.feedback}";
    }
    
    /// <summary>
    /// Generates suggestions for improvement.
    /// </summary>
    private List<string> GenerateSuggestions(AssessmentInstance instance)
    {
        List<string> suggestions = new List<string>();
        
        foreach (CriterionResult result in instance.criterionResults)
        {
            if (result.score < result.maxScore * 0.8f)
            {
                suggestions.Add($"Practice {result.criterionName} to improve your score.");
            }
        }
        
        return suggestions;
    }
    
    /// <summary>
    /// Generates performance analytics.
    /// </summary>
    private PerformanceAnalytics GenerateAnalytics(AssessmentInstance instance)
    {
        PerformanceAnalytics analytics = new PerformanceAnalytics
        {
            averageScore = instance.currentScore,
            bestScore = instance.currentScore,
            worstScore = instance.currentScore,
            totalAttempts = 1,
            improvementRate = 0f,
            scoreHistory = new List<float> { instance.currentScore },
            skillScores = new Dictionary<string, float>()
        };
        
        foreach (CriterionResult result in instance.criterionResults)
        {
            analytics.skillScores[result.criterionName] = result.score;
        }
        
        return analytics;
    }
    
    /// <summary>
    /// Generates a unique assessment ID.
    /// </summary>
    private string GenerateAssessmentId()
    {
        return $"assess_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique result ID.
    /// </summary>
    private string GenerateResultId()
    {
        return $"result_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsAssessmentInProgress() => isAssessmentInProgress;
    public int GetActiveAssessmentCount() => activeAssessments.Count;
    public int GetAssessmentResultCount() => assessmentResults.Count;
    
    /// <summary>
    /// Gets an assessment instance by ID.
    /// </summary>
    public AssessmentInstance GetAssessment(string assessmentId)
    {
        return activeAssessments.ContainsKey(assessmentId) ? activeAssessments[assessmentId] : null;
    }
    
    /// <summary>
    /// Gets assessment results.
    /// </summary>
    public List<AssessmentResult> GetAssessmentResults()
    {
        return new List<AssessmentResult>(assessmentResults);
    }
    
    /// <summary>
    /// Gets assessment results for a specific student.
    /// </summary>
    public List<AssessmentResult> GetStudentResults(string studentId)
    {
        return assessmentResults.FindAll(r => r.studentId == studentId);
    }
    
    /// <summary>
    /// Gets assessment results for a specific experiment.
    /// </summary>
    public List<AssessmentResult> GetExperimentResults(string experimentId)
    {
        return assessmentResults.FindAll(r => r.experimentId == experimentId);
    }
    
    /// <summary>
    /// Sets the passing threshold.
    /// </summary>
    public void SetPassingThreshold(float threshold)
    {
        passingThreshold = Mathf.Clamp(threshold, 0.1f, 1.0f);
    }
    
    /// <summary>
    /// Sets the excellent threshold.
    /// </summary>
    public void SetExcellentThreshold(float threshold)
    {
        excellentThreshold = Mathf.Clamp(threshold, 0.1f, 1.0f);
    }
    
    /// <summary>
    /// Enables or disables real-time assessment.
    /// </summary>
    public void SetRealTimeAssessmentEnabled(bool enabled)
    {
        enableRealTimeAssessment = enabled;
    }
    
    /// <summary>
    /// Generates an assessment report.
    /// </summary>
    public string GenerateAssessmentReport(string assessmentId)
    {
        if (!activeAssessments.ContainsKey(assessmentId)) return "";
        
        AssessmentInstance instance = activeAssessments[assessmentId];
        
        string report = "=== Assessment Report ===\n";
        report += $"Assessment ID: {assessmentId}\n";
        report += $"Experiment: {instance.experimentId}\n";
        report += $"Student: {instance.studentId}\n";
        report += $"Score: {instance.currentScore:F1}/{instance.maxPossibleScore:F1}\n";
        report += $"Percentage: {(instance.currentScore / instance.maxPossibleScore * 100f):F1}%\n";
        report += $"Status: {instance.status}\n";
        report += "=======================\n";
        
        return report;
    }
    
    /// <summary>
    /// Logs the current assessment manager status.
    /// </summary>
    public void LogAssessmentStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Assessment Manager Status ===");
        Debug.Log($"Active Assessments: {activeAssessments.Count}");
        Debug.Log($"Assessment Results: {assessmentResults.Count}");
        Debug.Log($"Is Assessment In Progress: {isAssessmentInProgress}");
        Debug.Log($"Criteria Database Size: {criteriaDatabase.Count}");
        Debug.Log($"Assessment Management: {(enableAssessmentManagement ? "Enabled" : "Disabled")}");
        Debug.Log($"Real-Time Assessment: {(enableRealTimeAssessment ? "Enabled" : "Disabled")}");
        Debug.Log($"Automatic Scoring: {(enableAutomaticScoring ? "Enabled" : "Disabled")}");
        Debug.Log($"Detailed Feedback: {(enableDetailedFeedback ? "Enabled" : "Disabled")}");
        Debug.Log($"Progress Tracking: {(enableProgressTracking ? "Enabled" : "Disabled")}");
        Debug.Log("=================================");
    }
} 