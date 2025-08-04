using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Manages AI tutoring functionality for intelligent guidance and adaptive learning.
/// Provides personalized hints, explanations, and learning path recommendations.
/// </summary>
public class AITutorManager : MonoBehaviour
{
    [Header("AI Tutor Settings")]
    [SerializeField] private bool enableAITutor = true;
    [SerializeField] private bool enableAdaptiveLearning = true;
    [SerializeField] private bool enablePredictiveAnalytics = true;
    [SerializeField] private float hintCooldown = 30f;
    [SerializeField] private int maxHintsPerStep = 3;
    
    [Header("Learning Analytics")]
    [SerializeField] private bool trackLearningPatterns = true;
    [SerializeField] private bool analyzePerformance = true;
    [SerializeField] private bool generateInsights = true;
    [SerializeField] private float analysisInterval = 60f;
    
    [Header("Personalization")]
    [SerializeField] private bool enablePersonalizedContent = true;
    [SerializeField] private bool enableDifficultyAdjustment = true;
    [SerializeField] private bool enableLearningPathOptimization = true;
    [SerializeField] private string learningStyle = "Visual"; // Visual, Auditory, Kinesthetic
    
    [Header("Knowledge Base")]
    [SerializeField] private bool enableKnowledgeGraph = true;
    [SerializeField] private bool enableConceptMapping = true;
    [SerializeField] private bool enablePrerequisiteChecking = true;
    [SerializeField] private int maxConceptsPerSession = 10;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showTutorSuggestions = false;
    [SerializeField] private bool logLearningAnalytics = false;
    
    private static AITutorManager instance;
    public static AITutorManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AITutorManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AITutorManager");
                    instance = go.AddComponent<AITutorManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnHintProvided;
    public event Action<string> OnExplanationRequested;
    public event Action<string> OnLearningPathUpdated;
    public event Action<string, float> OnPerformanceAnalyzed;
    public event Action<string> OnConceptMastered;
    public event Action<string> OnDifficultyAdjusted;
    public event Action<string> OnPersonalizedContentGenerated;
    
    [System.Serializable]
    public class StudentProfile
    {
        public string studentId;
        public string name;
        public string learningStyle;
        public float overallPerformance;
        public Dictionary<string, float> conceptMastery;
        public List<string> completedExperiments;
        public List<string> preferredTopics;
        public Dictionary<string, int> hintUsage;
        public float averageTimePerStep;
        public int totalErrors;
        public List<string> learningGaps;
        
        public StudentProfile(string id, string studentName)
        {
            studentId = id;
            name = studentName;
            learningStyle = "Visual";
            overallPerformance = 0f;
            conceptMastery = new Dictionary<string, float>();
            completedExperiments = new List<string>();
            preferredTopics = new List<string>();
            hintUsage = new Dictionary<string, int>();
            averageTimePerStep = 0f;
            totalErrors = 0;
            learningGaps = new List<string>();
        }
    }
    
    [System.Serializable]
    public class LearningConcept
    {
        public string conceptId;
        public string name;
        public string description;
        public List<string> prerequisites;
        public List<string> relatedConcepts;
        public float difficulty;
        public List<string> learningObjectives;
        public Dictionary<string, string> explanations;
        public List<string> hints;
        
        public LearningConcept(string id, string conceptName)
        {
            conceptId = id;
            name = conceptName;
            description = "";
            prerequisites = new List<string>();
            relatedConcepts = new List<string>();
            difficulty = 0.5f;
            learningObjectives = new List<string>();
            explanations = new Dictionary<string, string>();
            hints = new List<string>();
        }
    }
    
    [System.Serializable]
    public class TutorResponse
    {
        public string responseId;
        public string type; // Hint, Explanation, Suggestion, Warning
        public string content;
        public float confidence;
        public string targetConcept;
        public bool isPersonalized;
        public DateTime timestamp;
        
        public TutorResponse(string id, string responseType, string responseContent)
        {
            responseId = id;
            type = responseType;
            content = responseContent;
            confidence = 0.8f;
            targetConcept = "";
            isPersonalized = false;
            timestamp = DateTime.Now;
        }
    }
    
    private StudentProfile currentStudent;
    private Dictionary<string, LearningConcept> knowledgeBase;
    private List<TutorResponse> recentResponses;
    private Coroutine analysisCoroutine;
    private Dictionary<string, float> conceptDifficulty;
    private List<string> currentLearningPath;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAITutorManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableAITutor)
        {
            LoadStudentProfile();
            InitializeKnowledgeBase();
            StartLearningAnalytics();
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// Initializes the AI tutor manager with basic settings.
    /// </summary>
    private void InitializeAITutorManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("AITutorManager initialized successfully");
        }
        
        // Initialize collections
        knowledgeBase = new Dictionary<string, LearningConcept>();
        recentResponses = new List<TutorResponse>();
        conceptDifficulty = new Dictionary<string, float>();
        currentLearningPath = new List<string>();
        
        // Load default knowledge base
        LoadDefaultKnowledgeBase();
    }
    
    /// <summary>
    /// Loads the default knowledge base with chemistry concepts.
    /// </summary>
    private void LoadDefaultKnowledgeBase()
    {
        // Acid-Base Chemistry
        LearningConcept acidBase = new LearningConcept("acid_base", "Acid-Base Chemistry");
        acidBase.description = "Understanding acids, bases, pH, and neutralization reactions";
        acidBase.difficulty = 0.6f;
        acidBase.learningObjectives = new List<string> {
            "Define acids and bases",
            "Understand pH scale",
            "Perform acid-base titrations",
            "Calculate concentrations"
        };
        acidBase.explanations["visual"] = "Acids donate H+ ions, bases accept H+ ions. pH scale ranges from 0-14.";
        acidBase.explanations["auditory"] = "Listen to the sound of neutralization reactions.";
        acidBase.hints = new List<string> {
            "Remember: pH + pOH = 14",
            "Strong acids completely dissociate in water",
            "Use indicators to detect pH changes"
        };
        knowledgeBase["acid_base"] = acidBase;
        
        // Titration
        LearningConcept titration = new LearningConcept("titration", "Titration Techniques");
        titration.description = "Volumetric analysis using acid-base reactions";
        titration.prerequisites = new List<string> { "acid_base" };
        titration.difficulty = 0.7f;
        titration.learningObjectives = new List<string> {
            "Set up titration apparatus",
            "Perform accurate measurements",
            "Detect endpoints",
            "Calculate unknown concentrations"
        };
        titration.explanations["visual"] = "Watch the color change at the equivalence point.";
        titration.explanations["kinesthetic"] = "Practice precise volume measurements.";
        titration.hints = new List<string> {
            "Rinse burette with titrant solution",
            "Add indicator before starting titration",
            "Stop at the first permanent color change"
        };
        knowledgeBase["titration"] = titration;
        
        // Spectroscopy
        LearningConcept spectroscopy = new LearningConcept("spectroscopy", "Spectroscopic Analysis");
        spectroscopy.description = "Using light to analyze chemical compounds";
        spectroscopy.difficulty = 0.8f;
        spectroscopy.learningObjectives = new List<string> {
            "Understand Beer's Law",
            "Use spectrophotometers",
            "Create calibration curves",
            "Analyze absorption spectra"
        };
        spectroscopy.explanations["visual"] = "Observe how different compounds absorb different wavelengths.";
        spectroscopy.explanations["mathematical"] = "A = εbc where A is absorbance, ε is molar absorptivity.";
        spectroscopy.hints = new List<string> {
            "Always use a blank for calibration",
            "Choose wavelength of maximum absorption",
            "Ensure linear range for accurate measurements"
        };
        knowledgeBase["spectroscopy"] = spectroscopy;
    }
    
    /// <summary>
    /// Loads or creates a student profile for personalized learning.
    /// </summary>
    private void LoadStudentProfile()
    {
        string studentId = SystemInfo.deviceUniqueIdentifier;
        string studentName = "Student " + studentId.Substring(0, 6);
        
        currentStudent = new StudentProfile(studentId, studentName);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Student profile loaded: {currentStudent.name}");
        }
    }
    
    /// <summary>
    /// Starts the learning analytics system.
    /// </summary>
    private void StartLearningAnalytics()
    {
        if (analysisCoroutine != null)
        {
            StopCoroutine(analysisCoroutine);
        }
        
        analysisCoroutine = StartCoroutine(LearningAnalyticsCoroutine());
    }
    
    /// <summary>
    /// Coroutine for periodic learning analytics.
    /// </summary>
    private System.Collections.IEnumerator LearningAnalyticsCoroutine()
    {
        while (enableAITutor)
        {
            yield return new WaitForSeconds(analysisInterval);
            
            if (analyzePerformance)
            {
                AnalyzeStudentPerformance();
            }
            
            if (generateInsights)
            {
                GenerateLearningInsights();
            }
        }
    }
    
    /// <summary>
    /// Provides a hint for the current experiment step.
    /// </summary>
    public string ProvideHint(string experimentId, string stepId, string conceptId)
    {
        if (!enableAITutor)
        {
            return "";
        }
        
        // Check hint usage limits
        string hintKey = $"{experimentId}_{stepId}";
        if (currentStudent.hintUsage.ContainsKey(hintKey) && 
            currentStudent.hintUsage[hintKey] >= maxHintsPerStep)
        {
            return "You've used all available hints for this step. Try to think through the problem.";
        }
        
        // Get appropriate hint based on learning style and concept
        string hint = GeneratePersonalizedHint(conceptId, currentStudent.learningStyle);
        
        // Update hint usage
        if (!currentStudent.hintUsage.ContainsKey(hintKey))
        {
            currentStudent.hintUsage[hintKey] = 0;
        }
        currentStudent.hintUsage[hintKey]++;
        
        // Create tutor response
        TutorResponse response = new TutorResponse(
            System.Guid.NewGuid().ToString(),
            "Hint",
            hint
        );
        response.targetConcept = conceptId;
        response.isPersonalized = true;
        
        recentResponses.Add(response);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Hint provided: {hint}");
        }
        
        OnHintProvided?.Invoke(hint);
        return hint;
    }
    
    /// <summary>
    /// Generates a personalized hint based on learning style and concept.
    /// </summary>
    private string GeneratePersonalizedHint(string conceptId, string learningStyle)
    {
        if (!knowledgeBase.ContainsKey(conceptId))
        {
            return "Think about the fundamental principles involved in this step.";
        }
        
        LearningConcept concept = knowledgeBase[conceptId];
        
        if (concept.hints.Count == 0)
        {
            return "Consider the key concepts you've learned so far.";
        }
        
        // Select hint based on learning style and performance
        int hintIndex = Mathf.Min(currentStudent.hintUsage.Count, concept.hints.Count - 1);
        string baseHint = concept.hints[hintIndex];
        
        // Personalize based on learning style
        switch (learningStyle.ToLower())
        {
            case "visual":
                return baseHint + " (Try visualizing the process)";
            case "auditory":
                return baseHint + " (Listen for audio cues)";
            case "kinesthetic":
                return baseHint + " (Focus on the physical movements)";
            default:
                return baseHint;
        }
    }
    
    /// <summary>
    /// Provides a detailed explanation for a concept.
    /// </summary>
    public string ProvideExplanation(string conceptId, string explanationType = "general")
    {
        if (!enableAITutor || !knowledgeBase.ContainsKey(conceptId))
        {
            return "Explanation not available for this concept.";
        }
        
        LearningConcept concept = knowledgeBase[conceptId];
        
        string explanation = "";
        if (concept.explanations.ContainsKey(explanationType))
        {
            explanation = concept.explanations[explanationType];
        }
        else if (concept.explanations.ContainsKey("general"))
        {
            explanation = concept.explanations["general"];
        }
        else
        {
            explanation = concept.description;
        }
        
        // Personalize explanation
        explanation = PersonalizeExplanation(explanation, currentStudent.learningStyle);
        
        TutorResponse response = new TutorResponse(
            System.Guid.NewGuid().ToString(),
            "Explanation",
            explanation
        );
        response.targetConcept = conceptId;
        response.isPersonalized = true;
        
        recentResponses.Add(response);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Explanation provided for {conceptId}");
        }
        
        OnExplanationRequested?.Invoke(explanation);
        return explanation;
    }
    
    /// <summary>
    /// Personalizes an explanation based on learning style.
    /// </summary>
    private string PersonalizeExplanation(string explanation, string learningStyle)
    {
        switch (learningStyle.ToLower())
        {
            case "visual":
                return explanation + "\n\nVisual Tip: Try to picture this process in your mind.";
            case "auditory":
                return explanation + "\n\nAudio Tip: Say the steps out loud as you perform them.";
            case "kinesthetic":
                return explanation + "\n\nHands-on Tip: Practice the physical movements involved.";
            default:
                return explanation;
        }
    }
    
    /// <summary>
    /// Analyzes student performance and provides insights.
    /// </summary>
    private void AnalyzeStudentPerformance()
    {
        if (currentStudent == null)
            return;
        
        // Calculate overall performance
        float totalMastery = 0f;
        int conceptCount = 0;
        
        foreach (float mastery in currentStudent.conceptMastery.Values)
        {
            totalMastery += mastery;
            conceptCount++;
        }
        
        if (conceptCount > 0)
        {
            currentStudent.overallPerformance = totalMastery / conceptCount;
        }
        
        // Identify learning gaps
        currentStudent.learningGaps.Clear();
        foreach (var kvp in currentStudent.conceptMastery)
        {
            if (kvp.Value < 0.7f)
            {
                currentStudent.learningGaps.Add(kvp.Key);
            }
        }
        
        if (enableDebugLogging && logLearningAnalytics)
        {
            Debug.Log($"Performance Analysis: {currentStudent.overallPerformance:F2}, Gaps: {currentStudent.learningGaps.Count}");
        }
        
        OnPerformanceAnalyzed?.Invoke(currentStudent.studentId, currentStudent.overallPerformance);
    }
    
    /// <summary>
    /// Generates learning insights and recommendations.
    /// </summary>
    private void GenerateLearningInsights()
    {
        if (currentStudent == null)
            return;
        
        List<string> insights = new List<string>();
        
        // Performance insights
        if (currentStudent.overallPerformance < 0.6f)
        {
            insights.Add("Consider reviewing fundamental concepts before proceeding.");
        }
        
        // Hint usage insights
        int totalHints = 0;
        foreach (int hintCount in currentStudent.hintUsage.Values)
        {
            totalHints += hintCount;
        }
        
        if (totalHints > 10)
        {
            insights.Add("You're using many hints. Try to solve problems independently first.");
        }
        
        // Time management insights
        if (currentStudent.averageTimePerStep > 300f) // 5 minutes
        {
            insights.Add("You're taking longer than average. Consider breaking down complex steps.");
        }
        
        // Error analysis insights
        if (currentStudent.totalErrors > 5)
        {
            insights.Add("Focus on accuracy. Double-check your measurements and calculations.");
        }
        
        if (enableDebugLogging && logLearningAnalytics)
        {
            Debug.Log($"Learning Insights: {insights.Count} recommendations generated");
        }
    }
    
    /// <summary>
    /// Updates concept mastery based on performance.
    /// </summary>
    public void UpdateConceptMastery(string conceptId, float performance)
    {
        if (currentStudent == null)
            return;
        
        currentStudent.conceptMastery[conceptId] = performance;
        
        if (performance >= 0.8f)
        {
            OnConceptMastered?.Invoke(conceptId);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Concept mastery updated: {conceptId} = {performance:F2}");
        }
    }
    
    /// <summary>
    /// Adjusts difficulty based on student performance.
    /// </summary>
    public void AdjustDifficulty(string conceptId, float performance)
    {
        if (!enableDifficultyAdjustment)
            return;
        
        float currentDifficulty = 0.5f;
        if (conceptDifficulty.ContainsKey(conceptId))
        {
            currentDifficulty = conceptDifficulty[conceptId];
        }
        
        // Adjust difficulty based on performance
        if (performance > 0.8f)
        {
            currentDifficulty = Mathf.Min(currentDifficulty + 0.1f, 1.0f);
        }
        else if (performance < 0.4f)
        {
            currentDifficulty = Mathf.Max(currentDifficulty - 0.1f, 0.1f);
        }
        
        conceptDifficulty[conceptId] = currentDifficulty;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Difficulty adjusted for {conceptId}: {currentDifficulty:F2}");
        }
        
        OnDifficultyAdjusted?.Invoke(conceptId);
    }
    
    /// <summary>
    /// Generates personalized learning content.
    /// </summary>
    public string GeneratePersonalizedContent(string conceptId)
    {
        if (!enablePersonalizedContent || currentStudent == null)
        {
            return "";
        }
        
        if (!knowledgeBase.ContainsKey(conceptId))
        {
            return "Personalized content not available for this concept.";
        }
        
        LearningConcept concept = knowledgeBase[conceptId];
        string content = "";
        
        // Generate content based on learning style and performance
        switch (currentStudent.learningStyle.ToLower())
        {
            case "visual":
                content = $"Visual Learning Focus: {concept.name}\n\n";
                content += "Key Visual Elements:\n";
                content += "- Color changes in reactions\n";
                content += "- Equipment setup diagrams\n";
                content += "- Molecular structure models\n";
                break;
                
            case "auditory":
                content = $"Auditory Learning Focus: {concept.name}\n\n";
                content += "Key Audio Elements:\n";
                content += "- Sound of reactions\n";
                content += "- Verbal step-by-step instructions\n";
                content += "- Discussion of concepts\n";
                break;
                
            case "kinesthetic":
                content = $"Hands-on Learning Focus: {concept.name}\n\n";
                content += "Key Physical Elements:\n";
                content += "- Equipment manipulation\n";
                content += "- Precise measurements\n";
                content += "- Physical reactions\n";
                break;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Personalized content generated for {conceptId}");
        }
        
        OnPersonalizedContentGenerated?.Invoke(content);
        return content;
    }
    
    /// <summary>
    /// Handles input for AI tutor features.
    /// </summary>
    private void HandleInput()
    {
        if (!enableAITutor)
            return;
        
        // Request hint
        if (Input.GetKeyDown(KeyCode.H))
        {
            // This would request a hint for the current step
            if (enableDebugLogging)
            {
                Debug.Log("Hint requested");
            }
        }
        
        // Request explanation
        if (Input.GetKeyDown(KeyCode.E))
        {
            // This would request an explanation for the current concept
            if (enableDebugLogging)
            {
                Debug.Log("Explanation requested");
            }
        }
    }
    
    /// <summary>
    /// Gets the current student profile.
    /// </summary>
    public StudentProfile GetCurrentStudent()
    {
        return currentStudent;
    }
    
    /// <summary>
    /// Gets a learning concept from the knowledge base.
    /// </summary>
    public LearningConcept GetConcept(string conceptId)
    {
        if (knowledgeBase.ContainsKey(conceptId))
        {
            return knowledgeBase[conceptId];
        }
        return null;
    }
    
    /// <summary>
    /// Gets recent tutor responses.
    /// </summary>
    public List<TutorResponse> GetRecentResponses()
    {
        return new List<TutorResponse>(recentResponses);
    }
    
    /// <summary>
    /// Logs AI tutor status for debugging.
    /// </summary>
    public void LogAITutorStatus()
    {
        if (!enableDebugLogging)
            return;
        
        Debug.Log("=== AI Tutor Status ===");
        Debug.Log($"Enabled: {enableAITutor}");
        Debug.Log($"Student: {currentStudent?.name}");
        Debug.Log($"Learning Style: {currentStudent?.learningStyle}");
        Debug.Log($"Performance: {currentStudent?.overallPerformance:F2}");
        Debug.Log($"Concepts Mastered: {currentStudent?.conceptMastery.Count}");
        Debug.Log($"Recent Responses: {recentResponses.Count}");
        Debug.Log("======================");
    }
} 