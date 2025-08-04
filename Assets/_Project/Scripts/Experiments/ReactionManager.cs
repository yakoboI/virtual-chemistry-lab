using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages chemical reaction simulation and kinetics for the virtual chemistry lab.
/// This component handles all reaction-related operations and calculations.
/// </summary>
public class ReactionManager : MonoBehaviour
{
    [Header("Reaction Management")]
    [SerializeField] private bool enableReactionManagement = true;
    [SerializeField] private bool enableReactionSimulation = true;
    [SerializeField] private bool enableKineticsCalculation = true;
    [SerializeField] private bool enableReactionValidation = true;
    [SerializeField] private bool enableReactionAnalysis = true;
    
    [Header("Reaction Configuration")]
    [SerializeField] private ReactionData[] availableReactions;
    [SerializeField] private string reactionSound = "chemical_reaction";
    [SerializeField] private string bubbleSound = "bubble_formation";
    [SerializeField] private string heatSound = "heat_generation";
    
    [Header("Reaction State")]
    [SerializeField] private Dictionary<string, ReactionInstance> activeReactions = new Dictionary<string, ReactionInstance>();
    [SerializeField] private List<ReactionResult> reactionResults = new List<ReactionResult>();
    [SerializeField] private bool isReactionInProgress = false;
    
    [Header("Reaction Settings")]
    [SerializeField] private bool enableRealTimeReactions = true;
    [SerializeField] private bool enableAutoReactionCompletion = true;
    [SerializeField] private bool enableReactionLogging = true;
    [SerializeField] private float reactionSpeed = 1.0f;
    [SerializeField] private float temperatureEffect = 1.0f;
    [SerializeField] private float concentrationEffect = 1.0f;
    [SerializeField] private float catalystEffect = 1.0f;
    
    [Header("Kinetics Settings")]
    [SerializeField] private bool enableRateCalculation = true;
    [SerializeField] private bool enableActivationEnergy = true;
    [SerializeField] private bool enableReactionOrder = true;
    [SerializeField] private float defaultActivationEnergy = 50f; // kJ/mol
    [SerializeField] private float defaultReactionOrder = 1f;
    [SerializeField] private float rateConstant = 1.0f;
    
    [Header("Visual Settings")]
    [SerializeField] private bool enableParticleEffects = true;
    [SerializeField] private bool enableColorChanges = true;
    [SerializeField] private bool enableBubbleEffects = true;
    [SerializeField] private bool enableHeatEffects = true;
    [SerializeField] private float effectIntensity = 1.0f;
    
    [Header("Performance")]
    [SerializeField] private bool enableReactionPooling = true;
    [SerializeField] private int reactionPoolSize = 20;
    [SerializeField] private bool enableReactionCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private int maxActiveReactions = 50;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logReactionChanges = false;
    
    private static ReactionManager instance;
    public static ReactionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ReactionManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ReactionManager");
                    instance = go.AddComponent<ReactionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnReactionStarted;
    public event Action<string> OnReactionCompleted;
    public event Action<string> OnReactionProgress;
    public event Action<string> OnReactionStopped;
    public event Action<ReactionResult> OnReactionResult;
    public event Action<float> OnTemperatureChanged;
    public event Action<float> OnConcentrationChanged;
    public event Action<string> OnReactionError;
    
    // Private variables
    private Dictionary<string, ReactionData> reactionDatabase = new Dictionary<string, ReactionData>();
    private Queue<ReactionInstance> reactionPool = new Queue<ReactionInstance>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class ReactionData
    {
        public string id;
        public string name;
        public string description;
        public ReactionType type;
        public string[] reactantIds;
        public string[] productIds;
        public float[] stoichiometricCoefficients;
        public float activationEnergy;
        public float reactionOrder;
        public float rateConstant;
        public bool isExothermic;
        public float heatGenerated;
        public bool isReversible;
        public float equilibriumConstant;
        public string[] catalysts;
        public string[] inhibitors;
        public string[] safetyNotes;
        public GameObject reactionEffect;
        public Material[] reactionMaterials;
    }
    
    [System.Serializable]
    public class ReactionInstance
    {
        public string id;
        public string reactionId;
        public string name;
        public Vector3 position;
        public GameObject gameObject;
        public bool isActive;
        public bool isInProgress;
        public bool isCompleted;
        public float progress;
        public float currentRate;
        public float temperature;
        public float pressure;
        public float startTime;
        public float completionTime;
        public ReactionStatus status;
        public List<ReactionPoint> reactionCurve;
        public Dictionary<string, float> reactantConcentrations;
        public Dictionary<string, float> productConcentrations;
    }
    
    [System.Serializable]
    public class ReactionResult
    {
        public string id;
        public string reactionId;
        public float completionTime;
        public float finalTemperature;
        public float finalPressure;
        public float reactionRate;
        public float yield;
        public bool isSuccessful;
        public string errorMessage;
        public List<ReactionPoint> reactionCurve;
        public Dictionary<string, float> finalConcentrations;
    }
    
    [System.Serializable]
    public class ReactionPoint
    {
        public float time;
        public float progress;
        public float temperature;
        public float rate;
        public Color color;
    }
    
    [System.Serializable]
    public enum ReactionType
    {
        Synthesis,
        Decomposition,
        SingleDisplacement,
        DoubleDisplacement,
        Combustion,
        Oxidation,
        Reduction,
        AcidBase,
        Precipitation,
        Complexation
    }
    
    [System.Serializable]
    public enum ReactionStatus
    {
        Setup,
        InProgress,
        Equilibrium,
        Completed,
        Failed,
        Stopped
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeReactionManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadReactionDatabase();
        InitializeReactionPool();
    }
    
    private void Update()
    {
        UpdateReactionProgress();
    }
    
    /// <summary>
    /// Initializes the reaction manager.
    /// </summary>
    private void InitializeReactionManager()
    {
        activeReactions.Clear();
        reactionResults.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("ReactionManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads reaction data from available reactions.
    /// </summary>
    private void LoadReactionDatabase()
    {
        reactionDatabase.Clear();
        
        foreach (ReactionData reaction in availableReactions)
        {
            if (reaction != null && !string.IsNullOrEmpty(reaction.id))
            {
                reactionDatabase[reaction.id] = reaction;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded reaction: {reaction.name} ({reaction.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {reactionDatabase.Count} reactions");
        }
    }
    
    /// <summary>
    /// Initializes the reaction pool.
    /// </summary>
    private void InitializeReactionPool()
    {
        if (!enableReactionPooling) return;
        
        for (int i = 0; i < reactionPoolSize; i++)
        {
            ReactionInstance instance = new ReactionInstance
            {
                id = $"pooled_{i}",
                isActive = false
            };
            
            reactionPool.Enqueue(instance);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized reaction pool with {reactionPoolSize} instances");
        }
    }
    
    /// <summary>
    /// Creates a new reaction instance.
    /// </summary>
    public ReactionInstance CreateReaction(string reactionId, Vector3 position)
    {
        if (!enableReactionManagement || !reactionDatabase.ContainsKey(reactionId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Reaction not found: {reactionId}");
            }
            return null;
        }
        
        if (activeReactions.Count >= maxActiveReactions)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning("Maximum active reactions reached");
            }
            return null;
        }
        
        ReactionData reactionData = reactionDatabase[reactionId];
        ReactionInstance instance = GetPooledReaction();
        
        if (instance == null)
        {
            instance = new ReactionInstance();
        }
        
        instance.id = GenerateReactionId();
        instance.reactionId = reactionId;
        instance.name = reactionData.name;
        instance.position = position;
        instance.isActive = true;
        instance.isInProgress = false;
        instance.isCompleted = false;
        instance.progress = 0f;
        instance.currentRate = 0f;
        instance.temperature = 25f; // Room temperature
        instance.pressure = 1f; // Atmospheric pressure
        instance.startTime = 0f;
        instance.completionTime = 0f;
        instance.status = ReactionStatus.Setup;
        instance.reactionCurve = new List<ReactionPoint>();
        instance.reactantConcentrations = new Dictionary<string, float>();
        instance.productConcentrations = new Dictionary<string, float>();
        
        // Initialize concentrations
        InitializeConcentrations(instance, reactionData);
        
        // Create visual representation
        instance.gameObject = CreateReactionVisual(reactionData, position);
        
        activeReactions[instance.id] = instance;
        
        OnReactionStarted?.Invoke(instance.id);
        
        if (logReactionChanges)
        {
            Debug.Log($"Created reaction: {reactionData.name} ({instance.id}) at {position}");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Destroys a reaction instance.
    /// </summary>
    public void DestroyReaction(string instanceId)
    {
        if (!activeReactions.ContainsKey(instanceId)) return;
        
        ReactionInstance instance = activeReactions[instanceId];
        
        if (instance.gameObject != null)
        {
            Destroy(instance.gameObject);
        }
        
        activeReactions.Remove(instanceId);
        
        // Return to pool
        if (enableReactionPooling)
        {
            instance.isActive = false;
            reactionPool.Enqueue(instance);
        }
        
        if (logReactionChanges)
        {
            Debug.Log($"Destroyed reaction: {instanceId}");
        }
    }
    
    /// <summary>
    /// Starts a reaction.
    /// </summary>
    public bool StartReaction(string instanceId)
    {
        if (!enableReactionSimulation || !activeReactions.ContainsKey(instanceId)) return false;
        
        ReactionInstance instance = activeReactions[instanceId];
        ReactionData data = reactionDatabase[instance.reactionId];
        
        if (instance.isInProgress || instance.isCompleted)
        {
            OnReactionError?.Invoke($"Reaction {instance.name} is already in progress or completed");
            return false;
        }
        
        // Validate reactants
        if (!ValidateReactants(instance, data))
        {
            OnReactionError?.Invoke($"Insufficient reactants for reaction {instance.name}");
            return false;
        }
        
        // Start reaction
        instance.isInProgress = true;
        instance.startTime = Time.time;
        instance.status = ReactionStatus.InProgress;
        
        // Calculate initial rate
        instance.currentRate = CalculateReactionRate(instance, data);
        
        // Add initial point to reaction curve
        AddReactionPoint(instance);
        
        isReactionInProgress = true;
        
        // Play reaction sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayReaction(reactionSound);
        }
        
        if (logReactionChanges)
        {
            Debug.Log($"Started reaction: {instance.name} ({instance.id})");
        }
        
        return true;
    }
    
    /// <summary>
    /// Stops a reaction.
    /// </summary>
    public void StopReaction(string instanceId)
    {
        if (!activeReactions.ContainsKey(instanceId)) return;
        
        ReactionInstance instance = activeReactions[instanceId];
        
        if (!instance.isInProgress) return;
        
        instance.isInProgress = false;
        instance.status = ReactionStatus.Stopped;
        
        OnReactionStopped?.Invoke(instance.id);
        
        if (logReactionChanges)
        {
            Debug.Log($"Stopped reaction: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Completes a reaction.
    /// </summary>
    public void CompleteReaction(string instanceId)
    {
        if (!activeReactions.ContainsKey(instanceId)) return;
        
        ReactionInstance instance = activeReactions[instanceId];
        ReactionData data = reactionDatabase[instance.reactionId];
        
        instance.isInProgress = false;
        instance.isCompleted = true;
        instance.progress = 1f;
        instance.completionTime = Time.time;
        instance.status = ReactionStatus.Completed;
        
        // Calculate results
        ReactionResult result = CalculateReactionResult(instance, data);
        reactionResults.Add(result);
        
        isReactionInProgress = false;
        
        OnReactionCompleted?.Invoke(instance.id);
        OnReactionResult?.Invoke(result);
        
        if (logReactionChanges)
        {
            Debug.Log($"Completed reaction: {instance.name} ({instance.id})");
        }
    }
    
    /// <summary>
    /// Initializes concentrations for a reaction.
    /// </summary>
    private void InitializeConcentrations(ReactionInstance instance, ReactionData data)
    {
        // Initialize reactant concentrations
        for (int i = 0; i < data.reactantIds.Length; i++)
        {
            string reactantId = data.reactantIds[i];
            float concentration = 1.0f; // Default concentration
            instance.reactantConcentrations[reactantId] = concentration;
        }
        
        // Initialize product concentrations
        for (int i = 0; i < data.productIds.Length; i++)
        {
            string productId = data.productIds[i];
            instance.productConcentrations[productId] = 0f;
        }
    }
    
    /// <summary>
    /// Validates reactants for a reaction.
    /// </summary>
    private bool ValidateReactants(ReactionInstance instance, ReactionData data)
    {
        foreach (string reactantId in data.reactantIds)
        {
            if (!instance.reactantConcentrations.ContainsKey(reactantId) ||
                instance.reactantConcentrations[reactantId] <= 0f)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Calculates reaction rate.
    /// </summary>
    private float CalculateReactionRate(ReactionInstance instance, ReactionData data)
    {
        if (!enableRateCalculation) return rateConstant;
        
        // Basic rate law: rate = k[A]^n
        float rate = data.rateConstant;
        
        // Concentration effect
        if (enableReactionOrder)
        {
            foreach (string reactantId in data.reactantIds)
            {
                if (instance.reactantConcentrations.ContainsKey(reactantId))
                {
                    float concentration = instance.reactantConcentrations[reactantId];
                    rate *= Mathf.Pow(concentration, data.reactionOrder);
                }
            }
        }
        
        // Temperature effect (Arrhenius equation)
        if (enableActivationEnergy)
        {
            float temperatureEffect = Mathf.Exp(-data.activationEnergy / (8.314f * (instance.temperature + 273.15f)));
            rate *= temperatureEffect;
        }
        
        // Apply global effects
        rate *= reactionSpeed * temperatureEffect * concentrationEffect * catalystEffect;
        
        return rate;
    }
    
    /// <summary>
    /// Updates reaction progress.
    /// </summary>
    private void UpdateReactionProgress()
    {
        if (!enableReactionManagement) return;
        
        foreach (var kvp in activeReactions)
        {
            ReactionInstance instance = kvp.Value;
            
            if (instance.isInProgress && !instance.isCompleted)
            {
                UpdateReaction(instance);
            }
        }
        
        // Update global reaction state
        isReactionInProgress = false;
        foreach (var kvp in activeReactions)
        {
            if (kvp.Value.isInProgress)
            {
                isReactionInProgress = true;
                break;
            }
        }
    }
    
    /// <summary>
    /// Updates a specific reaction.
    /// </summary>
    private void UpdateReaction(ReactionInstance instance)
    {
        ReactionData data = reactionDatabase[instance.reactionId];
        
        // Update progress
        float deltaTime = Time.deltaTime;
        float progressIncrement = instance.currentRate * deltaTime;
        instance.progress += progressIncrement;
        
        // Clamp progress
        instance.progress = Mathf.Clamp01(instance.progress);
        
        // Update concentrations
        UpdateConcentrations(instance, data, progressIncrement);
        
        // Update temperature
        UpdateTemperature(instance, data);
        
        // Add point to reaction curve
        AddReactionPoint(instance);
        
        // Check for completion
        if (instance.progress >= 1f && enableAutoReactionCompletion)
        {
            CompleteReaction(instance.id);
        }
        
        OnReactionProgress?.Invoke(instance.id);
    }
    
    /// <summary>
    /// Updates concentrations during reaction.
    /// </summary>
    private void UpdateConcentrations(ReactionInstance instance, ReactionData data, float progressIncrement)
    {
        // Update reactant concentrations
        for (int i = 0; i < data.reactantIds.Length; i++)
        {
            string reactantId = data.reactantIds[i];
            float coefficient = data.stoichiometricCoefficients[i];
            float decrease = coefficient * progressIncrement;
            
            if (instance.reactantConcentrations.ContainsKey(reactantId))
            {
                instance.reactantConcentrations[reactantId] -= decrease;
                instance.reactantConcentrations[reactantId] = Mathf.Max(0f, instance.reactantConcentrations[reactantId]);
            }
        }
        
        // Update product concentrations
        for (int i = 0; i < data.productIds.Length; i++)
        {
            string productId = data.productIds[i];
            float coefficient = data.stoichiometricCoefficients[i + data.reactantIds.Length];
            float increase = coefficient * progressIncrement;
            
            if (instance.productConcentrations.ContainsKey(productId))
            {
                instance.productConcentrations[productId] += increase;
            }
        }
        
        OnConcentrationChanged?.Invoke(instance.progress);
    }
    
    /// <summary>
    /// Updates temperature during reaction.
    /// </summary>
    private void UpdateTemperature(ReactionInstance instance, ReactionData data)
    {
        if (data.isExothermic)
        {
            float heatGenerated = data.heatGenerated * instance.progress;
            float temperatureIncrease = heatGenerated * 0.1f; // Simplified heat capacity
            instance.temperature += temperatureIncrease * Time.deltaTime;
        }
        
        OnTemperatureChanged?.Invoke(instance.temperature);
    }
    
    /// <summary>
    /// Adds a point to the reaction curve.
    /// </summary>
    private void AddReactionPoint(ReactionInstance instance)
    {
        ReactionPoint point = new ReactionPoint
        {
            time = Time.time - instance.startTime,
            progress = instance.progress,
            temperature = instance.temperature,
            rate = instance.currentRate,
            color = CalculateReactionColor(instance.progress)
        };
        
        instance.reactionCurve.Add(point);
    }
    
    /// <summary>
    /// Calculates color based on reaction progress.
    /// </summary>
    private Color CalculateReactionColor(float progress)
    {
        if (progress < 0.3f)
        {
            return Color.red; // Initial
        }
        else if (progress < 0.7f)
        {
            return Color.yellow; // Intermediate
        }
        else
        {
            return Color.green; // Final
        }
    }
    
    /// <summary>
    /// Calculates reaction result.
    /// </summary>
    private ReactionResult CalculateReactionResult(ReactionInstance instance, ReactionData data)
    {
        ReactionResult result = new ReactionResult
        {
            id = GenerateResultId(),
            reactionId = instance.reactionId,
            completionTime = instance.completionTime - instance.startTime,
            finalTemperature = instance.temperature,
            finalPressure = instance.pressure,
            reactionRate = instance.currentRate,
            yield = CalculateYield(instance, data),
            isSuccessful = true,
            reactionCurve = new List<ReactionPoint>(instance.reactionCurve),
            finalConcentrations = new Dictionary<string, float>(instance.productConcentrations)
        };
        
        return result;
    }
    
    /// <summary>
    /// Calculates reaction yield.
    /// </summary>
    private float CalculateYield(ReactionInstance instance, ReactionData data)
    {
        // Simple yield calculation based on progress
        return instance.progress * 100f;
    }
    
    /// <summary>
    /// Creates visual representation of a reaction.
    /// </summary>
    private GameObject CreateReactionVisual(ReactionData data, Vector3 position)
    {
        GameObject visual;
        
        if (data.reactionEffect != null)
        {
            visual = Instantiate(data.reactionEffect, position, Quaternion.identity);
        }
        else
        {
            // Create default visual
            visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = $"Reaction_{data.name}";
            visual.transform.position = position;
            visual.transform.localScale = Vector3.one * 0.15f;
        }
        
        // Set initial material
        if (data.reactionMaterials != null && data.reactionMaterials.Length > 0)
        {
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = data.reactionMaterials[0];
            }
        }
        
        return visual;
    }
    
    /// <summary>
    /// Gets a pooled reaction instance.
    /// </summary>
    private ReactionInstance GetPooledReaction()
    {
        if (!enableReactionPooling || reactionPool.Count == 0)
        {
            return null;
        }
        
        return reactionPool.Dequeue();
    }
    
    /// <summary>
    /// Generates a unique reaction instance ID.
    /// </summary>
    private string GenerateReactionId()
    {
        return $"rxn_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Generates a unique result ID.
    /// </summary>
    private string GenerateResultId()
    {
        return $"res_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsReactionInProgress() => isReactionInProgress;
    public int GetActiveReactionCount() => activeReactions.Count;
    public int GetReactionResultCount() => reactionResults.Count;
    
    /// <summary>
    /// Gets a reaction instance by ID.
    /// </summary>
    public ReactionInstance GetReaction(string instanceId)
    {
        return activeReactions.ContainsKey(instanceId) ? activeReactions[instanceId] : null;
    }
    
    /// <summary>
    /// Gets reaction data by ID.
    /// </summary>
    public ReactionData GetReactionData(string reactionId)
    {
        return reactionDatabase.ContainsKey(reactionId) ? reactionDatabase[reactionId] : null;
    }
    
    /// <summary>
    /// Gets all available reaction IDs.
    /// </summary>
    public List<string> GetAvailableReactionIds()
    {
        return new List<string>(reactionDatabase.Keys);
    }
    
    /// <summary>
    /// Gets reaction results.
    /// </summary>
    public List<ReactionResult> GetReactionResults()
    {
        return new List<ReactionResult>(reactionResults);
    }
    
    /// <summary>
    /// Gets successful reaction results.
    /// </summary>
    public List<ReactionResult> GetSuccessfulResults()
    {
        return reactionResults.FindAll(r => r.isSuccessful);
    }
    
    /// <summary>
    /// Sets the reaction speed.
    /// </summary>
    public void SetReactionSpeed(float speed)
    {
        reactionSpeed = Mathf.Clamp(speed, 0.1f, 10f);
    }
    
    /// <summary>
    /// Sets the temperature effect.
    /// </summary>
    public void SetTemperatureEffect(float effect)
    {
        temperatureEffect = Mathf.Clamp(effect, 0.1f, 5f);
    }
    
    /// <summary>
    /// Sets the concentration effect.
    /// </summary>
    public void SetConcentrationEffect(float effect)
    {
        concentrationEffect = Mathf.Clamp(effect, 0.1f, 5f);
    }
    
    /// <summary>
    /// Sets the catalyst effect.
    /// </summary>
    public void SetCatalystEffect(float effect)
    {
        catalystEffect = Mathf.Clamp(effect, 0.1f, 10f);
    }
    
    /// <summary>
    /// Enables or disables rate calculation.
    /// </summary>
    public void SetRateCalculationEnabled(bool enabled)
    {
        enableRateCalculation = enabled;
    }
    
    /// <summary>
    /// Enables or disables particle effects.
    /// </summary>
    public void SetParticleEffectsEnabled(bool enabled)
    {
        enableParticleEffects = enabled;
    }
    
    /// <summary>
    /// Logs the current reaction manager status.
    /// </summary>
    public void LogReactionStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Reaction Manager Status ===");
        Debug.Log($"Active Reactions: {activeReactions.Count}");
        Debug.Log($"Reaction Results: {reactionResults.Count}");
        Debug.Log($"Is Reaction In Progress: {isReactionInProgress}");
        Debug.Log($"Reaction Database Size: {reactionDatabase.Count}");
        Debug.Log($"Reaction Simulation: {(enableReactionSimulation ? "Enabled" : "Disabled")}");
        Debug.Log($"Rate Calculation: {(enableRateCalculation ? "Enabled" : "Disabled")}");
        Debug.Log($"Activation Energy: {(enableActivationEnergy ? "Enabled" : "Disabled")}");
        Debug.Log($"Reaction Order: {(enableReactionOrder ? "Enabled" : "Disabled")}");
        Debug.Log($"Particle Effects: {(enableParticleEffects ? "Enabled" : "Disabled")}");
        Debug.Log($"Reaction Pooling: {(enableReactionPooling ? "Enabled" : "Disabled")}");
        Debug.Log($"Reaction Pool Size: {reactionPool.Count}");
        Debug.Log("===============================");
    }
} 