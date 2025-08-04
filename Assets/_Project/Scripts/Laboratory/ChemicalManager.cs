using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages chemical properties, reactions, and behavior for the virtual chemistry lab.
/// This component handles all chemical-related operations and interactions.
/// </summary>
public class ChemicalManager : MonoBehaviour
{
    [Header("Chemical Management")]
    [SerializeField] private bool enableChemicalManagement = true;
    [SerializeField] private bool enableChemicalReactions = true;
    [SerializeField] private bool enableChemicalMixing = true;
    [SerializeField] private bool enableChemicalSafety = true;
    
    [Header("Chemical Configuration")]
    [SerializeField] private ChemicalData[] availableChemicals;
    [SerializeField] private string defaultChemicalSound = "chemical_pour";
    [SerializeField] private string reactionSound = "chemical_reaction";
    
    [Header("Chemical State")]
    [SerializeField] private Dictionary<string, ChemicalInstance> activeChemicals = new Dictionary<string, ChemicalInstance>();
    [SerializeField] private bool isReactionInProgress = false;
    
    [Header("Safety Settings")]
    [SerializeField] private bool enableSafetyWarnings = true;
    [SerializeField] private bool enableHazardDetection = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logChemicalChanges = false;
    
    private static ChemicalManager instance;
    public static ChemicalManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ChemicalManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ChemicalManager");
                    instance = go.AddComponent<ChemicalManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnChemicalCreated;
    public event Action<string> OnChemicalDestroyed;
    public event Action<string> OnChemicalMixed;
    public event Action<string> OnChemicalReaction;
    public event Action<string> OnSafetyViolation;
    public event Action<string> OnHazardDetected;
    
    // Private variables
    private Dictionary<string, ChemicalData> chemicalDatabase = new Dictionary<string, ChemicalData>();
    private bool isInitialized = false;
    
    [System.Serializable]
    public class ChemicalData
    {
        public string id;
        public string name;
        public string formula;
        public ChemicalType type;
        public PhysicalState state;
        public Color color = Color.white;
        public float density = 1.0f;
        public float molarMass = 1.0f;
        public float boilingPoint = 100f;
        public float meltingPoint = 0f;
        public float pH = 7.0f;
        public bool isHazardous = false;
        public bool requiresVentilation = false;
        public bool isFlammable = false;
        public bool isCorrosive = false;
        public string[] hazards;
        public string pourSound;
        public string reactionSound;
    }
    
    [System.Serializable]
    public class ChemicalInstance
    {
        public string id;
        public string chemicalId;
        public float volume;
        public float concentration;
        public float temperature;
        public Vector3 position;
        public GameObject gameObject;
        public bool isActive;
        public float creationTime;
    }
    
    [System.Serializable]
    public enum ChemicalType
    {
        Acid,
        Base,
        Salt,
        Organic,
        Inorganic,
        Solvent,
        Catalyst,
        Indicator,
        Buffer
    }
    
    [System.Serializable]
    public enum PhysicalState
    {
        Solid,
        Liquid,
        Gas,
        Aqueous
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeChemicalManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadChemicalDatabase();
    }
    
    /// <summary>
    /// Initializes the chemical manager.
    /// </summary>
    private void InitializeChemicalManager()
    {
        activeChemicals.Clear();
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("ChemicalManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads chemical data from available chemicals.
    /// </summary>
    private void LoadChemicalDatabase()
    {
        chemicalDatabase.Clear();
        
        foreach (ChemicalData chemical in availableChemicals)
        {
            if (chemical != null && !string.IsNullOrEmpty(chemical.id))
            {
                chemicalDatabase[chemical.id] = chemical;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded chemical: {chemical.name} ({chemical.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {chemicalDatabase.Count} chemicals");
        }
    }
    
    /// <summary>
    /// Creates a new chemical instance.
    /// </summary>
    public ChemicalInstance CreateChemical(string chemicalId, float volume, Vector3 position)
    {
        if (!enableChemicalManagement || !chemicalDatabase.ContainsKey(chemicalId))
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Chemical not found: {chemicalId}");
            }
            return null;
        }
        
        ChemicalData chemicalData = chemicalDatabase[chemicalId];
        ChemicalInstance instance = new ChemicalInstance();
        
        instance.id = GenerateChemicalId();
        instance.chemicalId = chemicalId;
        instance.volume = volume;
        instance.concentration = chemicalData.molarMass;
        instance.temperature = 25f; // Room temperature
        instance.position = position;
        instance.isActive = true;
        instance.creationTime = Time.time;
        
        // Create visual representation
        instance.gameObject = CreateChemicalVisual(chemicalData, position);
        
        activeChemicals[instance.id] = instance;
        
        OnChemicalCreated?.Invoke(instance.id);
        
        if (logChemicalChanges)
        {
            Debug.Log($"Created chemical: {chemicalData.name} ({instance.id}) at {position}");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Destroys a chemical instance.
    /// </summary>
    public void DestroyChemical(string instanceId)
    {
        if (!activeChemicals.ContainsKey(instanceId)) return;
        
        ChemicalInstance instance = activeChemicals[instanceId];
        
        if (instance.gameObject != null)
        {
            Destroy(instance.gameObject);
        }
        
        activeChemicals.Remove(instanceId);
        
        OnChemicalDestroyed?.Invoke(instanceId);
        
        if (logChemicalChanges)
        {
            Debug.Log($"Destroyed chemical: {instanceId}");
        }
    }
    
    /// <summary>
    /// Mixes two chemicals.
    /// </summary>
    public void MixChemicals(string chemical1Id, string chemical2Id, float ratio = 0.5f)
    {
        if (!enableChemicalMixing) return;
        
        if (!activeChemicals.ContainsKey(chemical1Id) || !activeChemicals.ContainsKey(chemical2Id))
        {
            if (enableDebugLogging)
            {
                Debug.LogError("One or both chemicals not found for mixing");
            }
            return;
        }
        
        ChemicalInstance chem1 = activeChemicals[chemical1Id];
        ChemicalInstance chem2 = activeChemicals[chemical2Id];
        
        ChemicalData data1 = chemicalDatabase[chem1.chemicalId];
        ChemicalData data2 = chemicalDatabase[chem2.chemicalId];
        
        // Check for safety hazards
        CheckMixingSafety(data1, data2);
        
        OnChemicalMixed?.Invoke($"{data1.name} + {data2.name}");
        
        if (logChemicalChanges)
        {
            Debug.Log($"Mixed chemicals: {data1.name} + {data2.name}");
        }
    }
    
    /// <summary>
    /// Checks for safety hazards when mixing chemicals.
    /// </summary>
    private void CheckMixingSafety(ChemicalData data1, ChemicalData data2)
    {
        if (!enableChemicalSafety) return;
        
        // Check for hazardous combinations
        if (data1.isHazardous && data2.isHazardous)
        {
            OnHazardDetected?.Invoke($"Hazardous combination: {data1.name} + {data2.name}");
            OnSafetyViolation?.Invoke($"Safety violation: mixing hazardous chemicals");
        }
        
        // Check for acid-base reactions
        if ((data1.type == ChemicalType.Acid && data2.type == ChemicalType.Base) ||
            (data1.type == ChemicalType.Base && data2.type == ChemicalType.Acid))
        {
            OnChemicalReaction?.Invoke($"Acid-base reaction: {data1.name} + {data2.name}");
        }
        
        // Check for ventilation requirements
        if (data1.requiresVentilation || data2.requiresVentilation)
        {
            OnSafetyViolation?.Invoke($"Ventilation required for mixing {data1.name} and {data2.name}");
        }
    }
    
    /// <summary>
    /// Creates visual representation of a chemical.
    /// </summary>
    private GameObject CreateChemicalVisual(ChemicalData data, Vector3 position)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = $"Chemical_{data.name}";
        visual.transform.position = position;
        visual.transform.localScale = Vector3.one * 0.1f;
        
        // Set material and color
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = data.color;
            renderer.material = material;
        }
        
        return visual;
    }
    
    /// <summary>
    /// Generates a unique chemical instance ID.
    /// </summary>
    private string GenerateChemicalId()
    {
        return $"chem_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    // Public getters and setters
    public bool IsReactionInProgress() => isReactionInProgress;
    public int GetActiveChemicalCount() => activeChemicals.Count;
    
    /// <summary>
    /// Gets a chemical instance by ID.
    /// </summary>
    public ChemicalInstance GetChemical(string instanceId)
    {
        return activeChemicals.ContainsKey(instanceId) ? activeChemicals[instanceId] : null;
    }
    
    /// <summary>
    /// Gets chemical data by ID.
    /// </summary>
    public ChemicalData GetChemicalData(string chemicalId)
    {
        return chemicalDatabase.ContainsKey(chemicalId) ? chemicalDatabase[chemicalId] : null;
    }
    
    /// <summary>
    /// Gets all available chemical IDs.
    /// </summary>
    public List<string> GetAvailableChemicalIds()
    {
        return new List<string>(chemicalDatabase.Keys);
    }
    
    /// <summary>
    /// Enables or disables chemical reactions.
    /// </summary>
    public void SetChemicalReactionsEnabled(bool enabled)
    {
        enableChemicalReactions = enabled;
    }
    
    /// <summary>
    /// Enables or disables safety warnings.
    /// </summary>
    public void SetSafetyWarningsEnabled(bool enabled)
    {
        enableSafetyWarnings = enabled;
    }
    
    /// <summary>
    /// Logs the current chemical manager status.
    /// </summary>
    public void LogChemicalStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Chemical Manager Status ===");
        Debug.Log($"Active Chemicals: {activeChemicals.Count}");
        Debug.Log($"Is Reaction In Progress: {isReactionInProgress}");
        Debug.Log($"Chemical Database Size: {chemicalDatabase.Count}");
        Debug.Log($"Chemical Reactions: {(enableChemicalReactions ? "Enabled" : "Disabled")}");
        Debug.Log($"Chemical Mixing: {(enableChemicalMixing ? "Enabled" : "Disabled")}");
        Debug.Log($"Safety Warnings: {(enableSafetyWarnings ? "Enabled" : "Disabled")}");
        Debug.Log("===============================");
    }
} 