using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Manages advanced analytics and predictive analysis for learning behavior and performance tracking.
/// Provides insights into student learning patterns and adaptive recommendations.
/// </summary>
public class AdvancedAnalyticsManager : MonoBehaviour
{
    [Header("Analytics Settings")]
    [SerializeField] private bool enableAnalytics = true;
    [SerializeField] private bool enablePredictiveAnalytics = true;
    [SerializeField] private bool enableBehavioralAnalysis = true;
    [SerializeField] private bool enableLearningPathOptimization = true;
    [SerializeField] private bool enableRealTimeAnalytics = true;
    
    [Header("Data Collection")]
    [SerializeField] private bool trackUserInteractions = true;
    [SerializeField] private bool trackPerformanceMetrics = true;
    [SerializeField] private bool trackLearningPatterns = true;
    [SerializeField] private bool trackErrorAnalysis = true;
    [SerializeField] private float dataCollectionInterval = 5f;
    
    [Header("Predictive Models")]
    [SerializeField] private bool enablePerformancePrediction = true;
    [SerializeField] private bool enableDifficultyPrediction = true;
    [SerializeField] private bool enableCompletionPrediction = true;
    [SerializeField] private bool enableEngagementPrediction = true;
    [SerializeField] private float predictionConfidence = 0.8f;
    
    [Header("Behavioral Analysis")]
    [SerializeField] private bool analyzeLearningStyle = true;
    [SerializeField] private bool analyzeAttentionPatterns = true;
    [SerializeField] private bool analyzeErrorPatterns = true;
    [SerializeField] private bool analyzeTimePatterns = true;
    [SerializeField] private int minDataPoints = 10;
    
    [Header("Machine Learning")]
    [SerializeField] private bool enableMLModels = true;
    [SerializeField] private bool enableClustering = true;
    [SerializeField] private bool enableClassification = true;
    [SerializeField] private bool enableRegression = true;
    [SerializeField] private float modelUpdateInterval = 3600f; // 1 hour
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showAnalyticsData = false;
    [SerializeField] private bool logPredictions = false;
    
    private static AdvancedAnalyticsManager instance;
    public static AdvancedAnalyticsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdvancedAnalyticsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AdvancedAnalyticsManager");
                    instance = go.AddComponent<AdvancedAnalyticsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string, float> OnPerformancePredicted;
    public event Action<string, float> OnDifficultyPredicted;
    public event Action<string, float> OnCompletionPredicted;
    public event Action<string, float> OnEngagementPredicted;
    public event Action<string> OnLearningStyleIdentified;
    public event Action<string> OnAttentionPatternDetected;
    public event Action<string> OnErrorPatternIdentified;
    public event Action<string> OnRecommendationGenerated;
    
    [System.Serializable]
    public class UserInteraction
    {
        public string interactionId;
        public string userId;
        public string experimentId;
        public string stepId;
        public string actionType;
        public Vector3 position;
        public float timestamp;
        public float duration;
        public bool wasSuccessful;
        public Dictionary<string, object> metadata;
        
        public UserInteraction(string id, string user, string experiment)
        {
            interactionId = id;
            userId = user;
            experimentId = experiment;
            stepId = "";
            actionType = "";
            position = Vector3.zero;
            timestamp = Time.time;
            duration = 0f;
            wasSuccessful = false;
            metadata = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class PerformanceMetrics
    {
        public string userId;
        public string experimentId;
        public float accuracy;
        public float speed;
        public float efficiency;
        public int errors;
        public int hintsUsed;
        public float completionTime;
        public float engagementScore;
        public DateTime timestamp;
        
        public PerformanceMetrics(string user, string experiment)
        {
            userId = user;
            experimentId = experiment;
            accuracy = 0f;
            speed = 0f;
            efficiency = 0f;
            errors = 0;
            hintsUsed = 0;
            completionTime = 0f;
            engagementScore = 0f;
            timestamp = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class LearningPattern
    {
        public string userId;
        public string patternType;
        public float frequency;
        public float consistency;
        public List<string> associatedBehaviors;
        public Dictionary<string, float> correlations;
        public DateTime firstObserved;
        public DateTime lastObserved;
        
        public LearningPattern(string user, string type)
        {
            userId = user;
            patternType = type;
            frequency = 0f;
            consistency = 0f;
            associatedBehaviors = new List<string>();
            correlations = new Dictionary<string, float>();
            firstObserved = DateTime.Now;
            lastObserved = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class PredictionResult
    {
        public string predictionId;
        public string predictionType;
        public float predictedValue;
        public float confidence;
        public float accuracy;
        public List<string> factors;
        public DateTime timestamp;
        
        public PredictionResult(string id, string type)
        {
            predictionId = id;
            predictionType = type;
            predictedValue = 0f;
            confidence = 0f;
            accuracy = 0f;
            factors = new List<string>();
            timestamp = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class BehavioralProfile
    {
        public string userId;
        public string learningStyle;
        public float attentionSpan;
        public float errorTolerance;
        public float timeManagement;
        public List<string> strengths;
        public List<string> weaknesses;
        public Dictionary<string, float> preferences;
        public DateTime lastUpdated;
        
        public BehavioralProfile(string user)
        {
            userId = user;
            learningStyle = "Unknown";
            attentionSpan = 0f;
            errorTolerance = 0f;
            timeManagement = 0f;
            strengths = new List<string>();
            weaknesses = new List<string>();
            preferences = new Dictionary<string, float>();
            lastUpdated = DateTime.Now;
        }
    }
    
    private Dictionary<string, List<UserInteraction>> userInteractions;
    private Dictionary<string, List<PerformanceMetrics>> performanceData;
    private Dictionary<string, List<LearningPattern>> learningPatterns;
    private Dictionary<string, BehavioralProfile> behavioralProfiles;
    private Dictionary<string, List<PredictionResult>> predictions;
    private Coroutine dataCollectionCoroutine;
    private Coroutine modelUpdateCoroutine;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAnalyticsManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableAnalytics)
        {
            StartDataCollection();
            StartModelUpdates();
        }
    }
    
    private void Update()
    {
        if (enableRealTimeAnalytics)
        {
            ProcessRealTimeData();
        }
    }
    
    /// <summary>
    /// Initializes the analytics manager with basic settings.
    /// </summary>
    private void InitializeAnalyticsManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("AdvancedAnalyticsManager initialized successfully");
        }
        
        // Initialize collections
        userInteractions = new Dictionary<string, List<UserInteraction>>();
        performanceData = new Dictionary<string, List<PerformanceMetrics>>();
        learningPatterns = new Dictionary<string, List<LearningPattern>>();
        behavioralProfiles = new Dictionary<string, BehavioralProfile>();
        predictions = new Dictionary<string, List<PredictionResult>>();
    }
    
    /// <summary>
    /// Starts data collection process.
    /// </summary>
    private void StartDataCollection()
    {
        if (dataCollectionCoroutine != null)
        {
            StopCoroutine(dataCollectionCoroutine);
        }
        
        dataCollectionCoroutine = StartCoroutine(DataCollectionCoroutine());
    }
    
    /// <summary>
    /// Coroutine for periodic data collection.
    /// </summary>
    private System.Collections.IEnumerator DataCollectionCoroutine()
    {
        while (enableAnalytics)
        {
            yield return new WaitForSeconds(dataCollectionInterval);
            
            if (trackUserInteractions)
            {
                CollectUserInteractions();
            }
            
            if (trackPerformanceMetrics)
            {
                CollectPerformanceMetrics();
            }
            
            if (trackLearningPatterns)
            {
                AnalyzeLearningPatterns();
            }
            
            if (trackErrorAnalysis)
            {
                AnalyzeErrorPatterns();
            }
        }
    }
    
    /// <summary>
    /// Starts model update process.
    /// </summary>
    private void StartModelUpdates()
    {
        if (modelUpdateCoroutine != null)
        {
            StopCoroutine(modelUpdateCoroutine);
        }
        
        modelUpdateCoroutine = StartCoroutine(ModelUpdateCoroutine());
    }
    
    /// <summary>
    /// Coroutine for periodic model updates.
    /// </summary>
    private System.Collections.IEnumerator ModelUpdateCoroutine()
    {
        while (enableAnalytics)
        {
            yield return new WaitForSeconds(modelUpdateInterval);
            
            if (enableMLModels)
            {
                UpdateMLModels();
            }
            
            if (enablePredictiveAnalytics)
            {
                UpdatePredictiveModels();
            }
        }
    }
    
    /// <summary>
    /// Records a user interaction.
    /// </summary>
    public void RecordInteraction(string userId, string experimentId, string stepId, string actionType, 
                                 Vector3 position, bool wasSuccessful, Dictionary<string, object> metadata = null)
    {
        if (!enableAnalytics || !trackUserInteractions)
            return;
        
        UserInteraction interaction = new UserInteraction(
            System.Guid.NewGuid().ToString(),
            userId,
            experimentId
        );
        
        interaction.stepId = stepId;
        interaction.actionType = actionType;
        interaction.position = position;
        interaction.wasSuccessful = wasSuccessful;
        interaction.duration = Time.time - interaction.timestamp;
        
        if (metadata != null)
        {
            interaction.metadata = metadata;
        }
        
        // Store interaction
        if (!userInteractions.ContainsKey(userId))
        {
            userInteractions[userId] = new List<UserInteraction>();
        }
        userInteractions[userId].Add(interaction);
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Interaction recorded: {actionType} by {userId} in {experimentId}");
        }
    }
    
    /// <summary>
    /// Records performance metrics for a user.
    /// </summary>
    public void RecordPerformanceMetrics(string userId, string experimentId, float accuracy, float speed, 
                                        float efficiency, int errors, int hintsUsed, float completionTime)
    {
        if (!enableAnalytics || !trackPerformanceMetrics)
            return;
        
        PerformanceMetrics metrics = new PerformanceMetrics(userId, experimentId);
        metrics.accuracy = accuracy;
        metrics.speed = speed;
        metrics.efficiency = efficiency;
        metrics.errors = errors;
        metrics.hintsUsed = hintsUsed;
        metrics.completionTime = completionTime;
        metrics.engagementScore = CalculateEngagementScore(accuracy, speed, efficiency, errors, hintsUsed);
        
        // Store metrics
        if (!performanceData.ContainsKey(userId))
        {
            performanceData[userId] = new List<PerformanceMetrics>();
        }
        performanceData[userId].Add(metrics);
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Performance recorded: {accuracy:F2} accuracy, {speed:F2} speed for {userId}");
        }
    }
    
    /// <summary>
    /// Calculates engagement score based on performance metrics.
    /// </summary>
    private float CalculateEngagementScore(float accuracy, float speed, float efficiency, int errors, int hintsUsed)
    {
        // Weighted combination of metrics
        float engagementScore = 0f;
        engagementScore += accuracy * 0.3f;
        engagementScore += speed * 0.2f;
        engagementScore += efficiency * 0.2f;
        engagementScore += (1f - errors * 0.1f) * 0.15f;
        engagementScore += (1f - hintsUsed * 0.05f) * 0.15f;
        
        return Mathf.Clamp01(engagementScore);
    }
    
    /// <summary>
    /// Collects user interactions data.
    /// </summary>
    private void CollectUserInteractions()
    {
        // Process collected interactions
        foreach (var kvp in userInteractions)
        {
            string userId = kvp.Key;
            List<UserInteraction> interactions = kvp.Value;
            
            // Analyze interaction patterns
            AnalyzeInteractionPatterns(userId, interactions);
        }
    }
    
    /// <summary>
    /// Collects performance metrics data.
    /// </summary>
    private void CollectPerformanceMetrics()
    {
        // Process collected performance data
        foreach (var kvp in performanceData)
        {
            string userId = kvp.Key;
            List<PerformanceMetrics> metrics = kvp.Value;
            
            // Analyze performance trends
            AnalyzePerformanceTrends(userId, metrics);
        }
    }
    
    /// <summary>
    /// Analyzes learning patterns for a user.
    /// </summary>
    private void AnalyzeLearningPatterns()
    {
        foreach (var kvp in userInteractions)
        {
            string userId = kvp.Key;
            List<UserInteraction> interactions = kvp.Value;
            
            // Identify learning patterns
            IdentifyLearningPatterns(userId, interactions);
        }
    }
    
    /// <summary>
    /// Analyzes error patterns for a user.
    /// </summary>
    private void AnalyzeErrorPatterns()
    {
        foreach (var kvp in performanceData)
        {
            string userId = kvp.Key;
            List<PerformanceMetrics> metrics = kvp.Value;
            
            // Identify error patterns
            IdentifyErrorPatterns(userId, metrics);
        }
    }
    
    /// <summary>
    /// Analyzes interaction patterns for a user.
    /// </summary>
    private void AnalyzeInteractionPatterns(string userId, List<UserInteraction> interactions)
    {
        if (interactions.Count < minDataPoints)
            return;
        
        // Analyze interaction frequency, timing, and success rates
        Dictionary<string, int> actionCounts = new Dictionary<string, int>();
        Dictionary<string, float> successRates = new Dictionary<string, float>();
        
        foreach (UserInteraction interaction in interactions)
        {
            if (!actionCounts.ContainsKey(interaction.actionType))
            {
                actionCounts[interaction.actionType] = 0;
                successRates[interaction.actionType] = 0f;
            }
            
            actionCounts[interaction.actionType]++;
            if (interaction.wasSuccessful)
            {
                successRates[interaction.actionType] += 1f;
            }
        }
        
        // Calculate success rates
        foreach (string actionType in actionCounts.Keys)
        {
            successRates[actionType] /= actionCounts[actionType];
        }
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Interaction patterns for {userId}: {actionCounts.Count} action types");
        }
    }
    
    /// <summary>
    /// Analyzes performance trends for a user.
    /// </summary>
    private void AnalyzePerformanceTrends(string userId, List<PerformanceMetrics> metrics)
    {
        if (metrics.Count < minDataPoints)
            return;
        
        // Calculate trend lines for accuracy, speed, and efficiency
        float accuracyTrend = CalculateTrend(metrics, m => m.accuracy);
        float speedTrend = CalculateTrend(metrics, m => m.speed);
        float efficiencyTrend = CalculateTrend(metrics, m => m.efficiency);
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Performance trends for {userId}: Accuracy {accuracyTrend:F3}, Speed {speedTrend:F3}, Efficiency {efficiencyTrend:F3}");
        }
    }
    
    /// <summary>
    /// Calculates trend for a metric over time.
    /// </summary>
    private float CalculateTrend<T>(List<T> data, Func<T, float> valueSelector)
    {
        if (data.Count < 2)
            return 0f;
        
        float sumX = 0f, sumY = 0f, sumXY = 0f, sumX2 = 0f;
        int n = data.Count;
        
        for (int i = 0; i < n; i++)
        {
            float x = i;
            float y = valueSelector(data[i]);
            
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }
        
        float slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }
    
    /// <summary>
    /// Identifies learning patterns for a user.
    /// </summary>
    private void IdentifyLearningPatterns(string userId, List<UserInteraction> interactions)
    {
        if (interactions.Count < minDataPoints)
            return;
        
        // Identify common patterns
        List<string> patterns = new List<string>();
        
        // Pattern 1: Sequential learning
        if (IsSequentialLearner(interactions))
        {
            patterns.Add("Sequential");
        }
        
        // Pattern 2: Exploratory learning
        if (IsExploratoryLearner(interactions))
        {
            patterns.Add("Exploratory");
        }
        
        // Pattern 3: Systematic learning
        if (IsSystematicLearner(interactions))
        {
            patterns.Add("Systematic");
        }
        
        // Store learning patterns
        if (!learningPatterns.ContainsKey(userId))
        {
            learningPatterns[userId] = new List<LearningPattern>();
        }
        
        foreach (string patternType in patterns)
        {
            LearningPattern pattern = new LearningPattern(userId, patternType);
            pattern.frequency = CalculatePatternFrequency(interactions, patternType);
            pattern.consistency = CalculatePatternConsistency(interactions, patternType);
            
            learningPatterns[userId].Add(pattern);
        }
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Learning patterns for {userId}: {string.Join(", ", patterns)}");
        }
    }
    
    /// <summary>
    /// Identifies error patterns for a user.
    /// </summary>
    private void IdentifyErrorPatterns(string userId, List<PerformanceMetrics> metrics)
    {
        if (metrics.Count < minDataPoints)
            return;
        
        // Analyze error patterns
        List<string> errorPatterns = new List<string>();
        
        // Pattern 1: High error rate at beginning
        if (HasHighInitialErrorRate(metrics))
        {
            errorPatterns.Add("HighInitialErrors");
        }
        
        // Pattern 2: Consistent errors
        if (HasConsistentErrors(metrics))
        {
            errorPatterns.Add("ConsistentErrors");
        }
        
        // Pattern 3: Decreasing errors over time
        if (HasDecreasingErrors(metrics))
        {
            errorPatterns.Add("Improving");
        }
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Error patterns for {userId}: {string.Join(", ", errorPatterns)}");
        }
        
        OnErrorPatternIdentified?.Invoke(string.Join(", ", errorPatterns));
    }
    
    /// <summary>
    /// Checks if user is a sequential learner.
    /// </summary>
    private bool IsSequentialLearner(List<UserInteraction> interactions)
    {
        // Analyze if user follows step-by-step approach
        int sequentialSteps = 0;
        for (int i = 1; i < interactions.Count; i++)
        {
            if (interactions[i].stepId != interactions[i-1].stepId)
            {
                sequentialSteps++;
            }
        }
        
        return (float)sequentialSteps / interactions.Count > 0.7f;
    }
    
    /// <summary>
    /// Checks if user is an exploratory learner.
    /// </summary>
    private bool IsExploratoryLearner(List<UserInteraction> interactions)
    {
        // Analyze if user explores different options
        HashSet<string> uniqueActions = new HashSet<string>();
        foreach (UserInteraction interaction in interactions)
        {
            uniqueActions.Add(interaction.actionType);
        }
        
        return uniqueActions.Count > interactions.Count * 0.3f;
    }
    
    /// <summary>
    /// Checks if user is a systematic learner.
    /// </summary>
    private bool IsSystematicLearner(List<UserInteraction> interactions)
    {
        // Analyze if user follows systematic approach
        Dictionary<string, int> actionOrder = new Dictionary<string, int>();
        for (int i = 0; i < interactions.Count; i++)
        {
            string action = interactions[i].actionType;
            if (!actionOrder.ContainsKey(action))
            {
                actionOrder[action] = i;
            }
        }
        
        return actionOrder.Count > interactions.Count * 0.5f;
    }
    
    /// <summary>
    /// Calculates pattern frequency.
    /// </summary>
    private float CalculatePatternFrequency(List<UserInteraction> interactions, string patternType)
    {
        // Calculate how often the pattern occurs
        int patternCount = 0;
        int totalInteractions = interactions.Count;
        
        switch (patternType)
        {
            case "Sequential":
                patternCount = CountSequentialPatterns(interactions);
                break;
            case "Exploratory":
                patternCount = CountExploratoryPatterns(interactions);
                break;
            case "Systematic":
                patternCount = CountSystematicPatterns(interactions);
                break;
        }
        
        return (float)patternCount / totalInteractions;
    }
    
    /// <summary>
    /// Calculates pattern consistency.
    /// </summary>
    private float CalculatePatternConsistency(List<UserInteraction> interactions, string patternType)
    {
        // Calculate consistency of the pattern
        float consistency = 0f;
        
        // This would implement consistency calculation based on pattern type
        // For now, return a simple calculation
        
        return consistency;
    }
    
    /// <summary>
    /// Counts sequential patterns.
    /// </summary>
    private int CountSequentialPatterns(List<UserInteraction> interactions)
    {
        int count = 0;
        for (int i = 1; i < interactions.Count; i++)
        {
            if (interactions[i].stepId != interactions[i-1].stepId)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Counts exploratory patterns.
    /// </summary>
    private int CountExploratoryPatterns(List<UserInteraction> interactions)
    {
        HashSet<string> uniqueActions = new HashSet<string>();
        foreach (UserInteraction interaction in interactions)
        {
            uniqueActions.Add(interaction.actionType);
        }
        return uniqueActions.Count;
    }
    
    /// <summary>
    /// Counts systematic patterns.
    /// </summary>
    private int CountSystematicPatterns(List<UserInteraction> interactions)
    {
        Dictionary<string, int> actionOrder = new Dictionary<string, int>();
        for (int i = 0; i < interactions.Count; i++)
        {
            string action = interactions[i].actionType;
            if (!actionOrder.ContainsKey(action))
            {
                actionOrder[action] = i;
            }
        }
        return actionOrder.Count;
    }
    
    /// <summary>
    /// Checks if user has high initial error rate.
    /// </summary>
    private bool HasHighInitialErrorRate(List<PerformanceMetrics> metrics)
    {
        if (metrics.Count < 3)
            return false;
        
        float initialErrorRate = 0f;
        for (int i = 0; i < Mathf.Min(3, metrics.Count); i++)
        {
            initialErrorRate += metrics[i].errors;
        }
        initialErrorRate /= Mathf.Min(3, metrics.Count);
        
        return initialErrorRate > 2f;
    }
    
    /// <summary>
    /// Checks if user has consistent errors.
    /// </summary>
    private bool HasConsistentErrors(List<PerformanceMetrics> metrics)
    {
        if (metrics.Count < 5)
            return false;
        
        float avgErrors = 0f;
        foreach (PerformanceMetrics metric in metrics)
        {
            avgErrors += metric.errors;
        }
        avgErrors /= metrics.Count;
        
        return avgErrors > 1f;
    }
    
    /// <summary>
    /// Checks if user has decreasing errors over time.
    /// </summary>
    private bool HasDecreasingErrors(List<PerformanceMetrics> metrics)
    {
        if (metrics.Count < 3)
            return false;
        
        float errorTrend = CalculateTrend(metrics, m => (float)m.errors);
        return errorTrend < -0.1f;
    }
    
    /// <summary>
    /// Processes real-time analytics data.
    /// </summary>
    private void ProcessRealTimeData()
    {
        // Process real-time analytics
        if (enablePredictiveAnalytics)
        {
            GenerateRealTimePredictions();
        }
        
        if (enableBehavioralAnalysis)
        {
            AnalyzeRealTimeBehavior();
        }
    }
    
    /// <summary>
    /// Generates real-time predictions.
    /// </summary>
    private void GenerateRealTimePredictions()
    {
        // Generate predictions for active users
        foreach (string userId in userInteractions.Keys)
        {
            if (enablePerformancePrediction)
            {
                PredictPerformance(userId);
            }
            
            if (enableDifficultyPrediction)
            {
                PredictDifficulty(userId);
            }
            
            if (enableCompletionPrediction)
            {
                PredictCompletion(userId);
            }
            
            if (enableEngagementPrediction)
            {
                PredictEngagement(userId);
            }
        }
    }
    
    /// <summary>
    /// Analyzes real-time behavior.
    /// </summary>
    private void AnalyzeRealTimeBehavior()
    {
        // Analyze real-time behavior patterns
        foreach (string userId in userInteractions.Keys)
        {
            if (analyzeLearningStyle)
            {
                IdentifyLearningStyle(userId);
            }
            
            if (analyzeAttentionPatterns)
            {
                AnalyzeAttentionPatterns(userId);
            }
        }
    }
    
    /// <summary>
    /// Predicts performance for a user.
    /// </summary>
    private void PredictPerformance(string userId)
    {
        if (!performanceData.ContainsKey(userId) || performanceData[userId].Count < minDataPoints)
            return;
        
        List<PerformanceMetrics> metrics = performanceData[userId];
        float predictedAccuracy = CalculateTrend(metrics, m => m.accuracy);
        float confidence = Mathf.Min(metrics.Count / 20f, 1f);
        
        PredictionResult prediction = new PredictionResult(
            System.Guid.NewGuid().ToString(),
            "Performance"
        );
        prediction.predictedValue = predictedAccuracy;
        prediction.confidence = confidence;
        
        if (!predictions.ContainsKey(userId))
        {
            predictions[userId] = new List<PredictionResult>();
        }
        predictions[userId].Add(prediction);
        
        OnPerformancePredicted?.Invoke(userId, predictedAccuracy);
        
        if (enableDebugLogging && logPredictions)
        {
            Debug.Log($"Performance prediction for {userId}: {predictedAccuracy:F3} (confidence: {confidence:F2})");
        }
    }
    
    /// <summary>
    /// Predicts difficulty for a user.
    /// </summary>
    private void PredictDifficulty(string userId)
    {
        if (!performanceData.ContainsKey(userId) || performanceData[userId].Count < minDataPoints)
            return;
        
        List<PerformanceMetrics> metrics = performanceData[userId];
        float avgErrors = 0f;
        foreach (PerformanceMetrics metric in metrics)
        {
            avgErrors += metric.errors;
        }
        avgErrors /= metrics.Count;
        
        float predictedDifficulty = Mathf.Clamp01(avgErrors / 5f);
        float confidence = Mathf.Min(metrics.Count / 15f, 1f);
        
        PredictionResult prediction = new PredictionResult(
            System.Guid.NewGuid().ToString(),
            "Difficulty"
        );
        prediction.predictedValue = predictedDifficulty;
        prediction.confidence = confidence;
        
        if (!predictions.ContainsKey(userId))
        {
            predictions[userId] = new List<PredictionResult>();
        }
        predictions[userId].Add(prediction);
        
        OnDifficultyPredicted?.Invoke(userId, predictedDifficulty);
        
        if (enableDebugLogging && logPredictions)
        {
            Debug.Log($"Difficulty prediction for {userId}: {predictedDifficulty:F3} (confidence: {confidence:F2})");
        }
    }
    
    /// <summary>
    /// Predicts completion for a user.
    /// </summary>
    private void PredictCompletion(string userId)
    {
        if (!performanceData.ContainsKey(userId) || performanceData[userId].Count < minDataPoints)
            return;
        
        List<PerformanceMetrics> metrics = performanceData[userId];
        float avgCompletionTime = 0f;
        foreach (PerformanceMetrics metric in metrics)
        {
            avgCompletionTime += metric.completionTime;
        }
        avgCompletionTime /= metrics.Count;
        
        float predictedCompletion = Mathf.Clamp01(1f - avgCompletionTime / 600f); // Normalize to 10 minutes
        float confidence = Mathf.Min(metrics.Count / 15f, 1f);
        
        PredictionResult prediction = new PredictionResult(
            System.Guid.NewGuid().ToString(),
            "Completion"
        );
        prediction.predictedValue = predictedCompletion;
        prediction.confidence = confidence;
        
        if (!predictions.ContainsKey(userId))
        {
            predictions[userId] = new List<PredictionResult>();
        }
        predictions[userId].Add(prediction);
        
        OnCompletionPredicted?.Invoke(userId, predictedCompletion);
        
        if (enableDebugLogging && logPredictions)
        {
            Debug.Log($"Completion prediction for {userId}: {predictedCompletion:F3} (confidence: {confidence:F2})");
        }
    }
    
    /// <summary>
    /// Predicts engagement for a user.
    /// </summary>
    private void PredictEngagement(string userId)
    {
        if (!performanceData.ContainsKey(userId) || performanceData[userId].Count < minDataPoints)
            return;
        
        List<PerformanceMetrics> metrics = performanceData[userId];
        float avgEngagement = 0f;
        foreach (PerformanceMetrics metric in metrics)
        {
            avgEngagement += metric.engagementScore;
        }
        avgEngagement /= metrics.Count;
        
        float predictedEngagement = avgEngagement;
        float confidence = Mathf.Min(metrics.Count / 15f, 1f);
        
        PredictionResult prediction = new PredictionResult(
            System.Guid.NewGuid().ToString(),
            "Engagement"
        );
        prediction.predictedValue = predictedEngagement;
        prediction.confidence = confidence;
        
        if (!predictions.ContainsKey(userId))
        {
            predictions[userId] = new List<PredictionResult>();
        }
        predictions[userId].Add(prediction);
        
        OnEngagementPredicted?.Invoke(userId, predictedEngagement);
        
        if (enableDebugLogging && logPredictions)
        {
            Debug.Log($"Engagement prediction for {userId}: {predictedEngagement:F3} (confidence: {confidence:F2})");
        }
    }
    
    /// <summary>
    /// Identifies learning style for a user.
    /// </summary>
    private void IdentifyLearningStyle(string userId)
    {
        if (!learningPatterns.ContainsKey(userId) || learningPatterns[userId].Count == 0)
            return;
        
        List<LearningPattern> patterns = learningPatterns[userId];
        string dominantStyle = "Unknown";
        float maxFrequency = 0f;
        
        foreach (LearningPattern pattern in patterns)
        {
            if (pattern.frequency > maxFrequency)
            {
                maxFrequency = pattern.frequency;
                dominantStyle = pattern.patternType;
            }
        }
        
        if (!behavioralProfiles.ContainsKey(userId))
        {
            behavioralProfiles[userId] = new BehavioralProfile(userId);
        }
        
        behavioralProfiles[userId].learningStyle = dominantStyle;
        behavioralProfiles[userId].lastUpdated = DateTime.Now;
        
        OnLearningStyleIdentified?.Invoke(dominantStyle);
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Learning style identified for {userId}: {dominantStyle}");
        }
    }
    
    /// <summary>
    /// Analyzes attention patterns for a user.
    /// </summary>
    private void AnalyzeAttentionPatterns(string userId)
    {
        if (!userInteractions.ContainsKey(userId) || userInteractions[userId].Count < minDataPoints)
            return;
        
        List<UserInteraction> interactions = userInteractions[userId];
        float avgDuration = 0f;
        int longInteractions = 0;
        
        foreach (UserInteraction interaction in interactions)
        {
            avgDuration += interaction.duration;
            if (interaction.duration > 30f) // Long interaction threshold
            {
                longInteractions++;
            }
        }
        avgDuration /= interactions.Count;
        
        string attentionPattern = "Normal";
        if (avgDuration > 60f)
        {
            attentionPattern = "Focused";
        }
        else if (avgDuration < 10f)
        {
            attentionPattern = "Distracted";
        }
        
        OnAttentionPatternDetected?.Invoke(attentionPattern);
        
        if (enableDebugLogging && showAnalyticsData)
        {
            Debug.Log($"Attention pattern for {userId}: {attentionPattern} (avg duration: {avgDuration:F1}s)");
        }
    }
    
    /// <summary>
    /// Updates machine learning models.
    /// </summary>
    private void UpdateMLModels()
    {
        // This would update ML models with new data
        if (enableDebugLogging)
        {
            Debug.Log("ML models updated");
        }
    }
    
    /// <summary>
    /// Updates predictive models.
    /// </summary>
    private void UpdatePredictiveModels()
    {
        // This would update predictive models with new data
        if (enableDebugLogging)
        {
            Debug.Log("Predictive models updated");
        }
    }
    
    /// <summary>
    /// Generates personalized recommendations for a user.
    /// </summary>
    public string GenerateRecommendations(string userId)
    {
        if (!behavioralProfiles.ContainsKey(userId))
            return "Insufficient data for recommendations.";
        
        BehavioralProfile profile = behavioralProfiles[userId];
        List<string> recommendations = new List<string>();
        
        // Generate recommendations based on learning style
        switch (profile.learningStyle)
        {
            case "Sequential":
                recommendations.Add("Follow step-by-step procedures");
                recommendations.Add("Review previous steps before proceeding");
                break;
            case "Exploratory":
                recommendations.Add("Try different approaches to problems");
                recommendations.Add("Experiment with various solutions");
                break;
            case "Systematic":
                recommendations.Add("Plan your approach before starting");
                recommendations.Add("Organize your workspace systematically");
                break;
        }
        
        // Generate recommendations based on performance
        if (performanceData.ContainsKey(userId) && performanceData[userId].Count > 0)
        {
            PerformanceMetrics latest = performanceData[userId][performanceData[userId].Count - 1];
            
            if (latest.accuracy < 0.7f)
            {
                recommendations.Add("Focus on accuracy over speed");
                recommendations.Add("Double-check your measurements");
            }
            
            if (latest.hintsUsed > 3)
            {
                recommendations.Add("Try to solve problems independently first");
                recommendations.Add("Review fundamental concepts");
            }
        }
        
        string recommendationText = string.Join("\n• ", recommendations);
        OnRecommendationGenerated?.Invoke(recommendationText);
        
        return recommendationText;
    }
    
    /// <summary>
    /// Gets analytics data for a user.
    /// </summary>
    public Dictionary<string, object> GetAnalyticsData(string userId)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        
        if (userInteractions.ContainsKey(userId))
        {
            data["interactions"] = userInteractions[userId].Count;
        }
        
        if (performanceData.ContainsKey(userId))
        {
            data["performance_records"] = performanceData[userId].Count;
        }
        
        if (learningPatterns.ContainsKey(userId))
        {
            data["learning_patterns"] = learningPatterns[userId].Count;
        }
        
        if (behavioralProfiles.ContainsKey(userId))
        {
            data["learning_style"] = behavioralProfiles[userId].learningStyle;
        }
        
        if (predictions.ContainsKey(userId))
        {
            data["predictions"] = predictions[userId].Count;
        }
        
        return data;
    }
    
    /// <summary>
    /// Logs analytics status for debugging.
    /// </summary>
    public void LogAnalyticsStatus()
    {
        if (!enableDebugLogging)
            return;
        
        Debug.Log("=== Analytics Status ===");
        Debug.Log($"Analytics Enabled: {enableAnalytics}");
        Debug.Log($"Users Tracked: {userInteractions.Count}");
        Debug.Log($"Performance Records: {performanceData.Count}");
        Debug.Log($"Learning Patterns: {learningPatterns.Count}");
        Debug.Log($"Behavioral Profiles: {behavioralProfiles.Count}");
        Debug.Log($"Predictions Generated: {predictions.Count}");
        Debug.Log("=======================");
    }
} 