using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Manages data loading, saving, and caching for experiments, chemicals, user progress, and settings.
/// This component handles all data operations in the virtual chemistry lab.
/// </summary>
public class DataManager : MonoBehaviour
{
    [Header("Data Settings")]
    [SerializeField] private bool enableDataCaching = true;
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;
    [SerializeField] private int maxCacheSize = 100;
    [SerializeField] private bool enableDataCompression = false;
    
    [Header("File Paths")]
    [SerializeField] private string experimentsPath = "Assets/_Project/Data/Experiments/";
    [SerializeField] private string chemicalsPath = "Assets/_Project/Data/Chemicals/";
    [SerializeField] private string userDataPath = "UserData/";
    [SerializeField] private string settingsPath = "Settings/";
    [SerializeField] private string backupPath = "Backups/";
    
    [Header("Data Validation")]
    [SerializeField] private bool enableDataValidation = true;
    [SerializeField] private bool enableBackupOnSave = true;
    [SerializeField] private int maxBackupFiles = 10;
    [SerializeField] private bool enableErrorRecovery = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showLoadTimes = false;
    [SerializeField] private bool logDataOperations = false;
    
    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DataManager");
                    instance = go.AddComponent<DataManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnDataLoaded;
    public event Action<string> OnDataSaved;
    public event Action<string> OnDataError;
    public event Action<string> OnCacheUpdated;
    public event Action OnAutoSaveCompleted;
    public event Action<string> OnBackupCreated;
    public event Action<string> OnDataValidated;
    
    // Data structures
    private Dictionary<string, object> dataCache = new Dictionary<string, object>();
    private Dictionary<string, DateTime> cacheTimestamps = new Dictionary<string, DateTime>();
    private Dictionary<string, ExperimentData> experimentCache = new Dictionary<string, ExperimentData>();
    private Dictionary<string, ChemicalData> chemicalCache = new Dictionary<string, ChemicalData>();
    private UserProgressData userProgress;
    private SettingsData settings;
    private float lastAutoSave = 0f;
    private bool isInitialized = false;
    
    [System.Serializable]
    public class ExperimentData
    {
        public string id;
        public string title;
        public string description;
        public string category;
        public string difficulty;
        public int estimatedDuration;
        public List<ChemicalRequirement> requiredChemicals;
        public List<ApparatusRequirement> requiredApparatus;
        public ExperimentProcedure procedure;
        public AssessmentCriteria assessment;
        public List<string> learningObjectives;
        public List<string> safetyNotes;
        public List<string> tips;
    }
    
    [System.Serializable]
    public class ChemicalRequirement
    {
        public string id;
        public string name;
        public float volume;
        public float concentration;
    }
    
    [System.Serializable]
    public class ApparatusRequirement
    {
        public string id;
        public string name;
        public float capacity;
        public float precision;
        public string purpose;
    }
    
    [System.Serializable]
    public class ExperimentProcedure
    {
        public List<ExperimentStep> steps;
    }
    
    [System.Serializable]
    public class ExperimentStep
    {
        public int stepNumber;
        public string title;
        public string description;
        public string instructions;
        public int expectedDuration;
        public string safetyNotes;
        public StepValidation validation;
    }
    
    [System.Serializable]
    public class StepValidation
    {
        public string type;
        public Dictionary<string, object> parameters;
    }
    
    [System.Serializable]
    public class AssessmentCriteria
    {
        public List<Criterion> criteria;
        public ExpectedResults expectedResults;
        public List<Calculation> calculations;
    }
    
    [System.Serializable]
    public class Criterion
    {
        public string criterion;
        public string description;
        public int weight;
        public Scoring scoring;
    }
    
    [System.Serializable]
    public class Scoring
    {
        public Excellent excellent;
        public Good good;
        public Satisfactory satisfactory;
        public Poor poor;
    }
    
    [System.Serializable]
    public class Excellent
    {
        public float[] range;
        public int score;
        public string description;
    }
    
    [System.Serializable]
    public class Good
    {
        public float[] range;
        public int score;
        public string description;
    }
    
    [System.Serializable]
    public class Satisfactory
    {
        public float[] range;
        public int score;
        public string description;
    }
    
    [System.Serializable]
    public class Poor
    {
        public float[] range;
        public int score;
        public string description;
    }
    
    [System.Serializable]
    public class ExpectedResults
    {
        public FirstEndpoint firstEndpoint;
        public SecondEndpoint secondEndpoint;
        public Concentration concentration;
    }
    
    [System.Serializable]
    public class FirstEndpoint
    {
        public float volume;
        public float tolerance;
        public string description;
    }
    
    [System.Serializable]
    public class SecondEndpoint
    {
        public float volume;
        public float tolerance;
        public string description;
    }
    
    [System.Serializable]
    public class Concentration
    {
        public float value;
        public float tolerance;
        public string description;
    }
    
    [System.Serializable]
    public class Calculation
    {
        public string name;
        public string formula;
        public string description;
    }
    
    [System.Serializable]
    public class ChemicalData
    {
        public string id;
        public string name;
        public string formula;
        public string type;
        public string state;
        public PhysicalProperties physicalProperties;
        public ChemicalProperties chemicalProperties;
        public SafetyData safety;
        public BehaviorData behavior;
        public VisualSettings visualSettings;
    }
    
    [System.Serializable]
    public class PhysicalProperties
    {
        public float molarMass;
        public float density;
        public string color;
        public float meltingPoint;
        public float boilingPoint;
        public float solubility;
    }
    
    [System.Serializable]
    public class ChemicalProperties
    {
        public float concentration;
        public float pH;
        public bool isAcid;
        public bool isBase;
        public bool isOxidizing;
        public bool isReducing;
    }
    
    [System.Serializable]
    public class SafetyData
    {
        public List<string> hazards;
        public string safetyNotes;
        public bool requiresVentilation;
        public bool requiresGloves;
    }
    
    [System.Serializable]
    public class BehaviorData
    {
        public bool canReact;
        public bool canMix;
        public bool canEvaporate;
        public float evaporationRate;
    }
    
    [System.Serializable]
    public class VisualSettings
    {
        public string materialColor;
        public string particleEffect;
        public string pourSound;
        public string reactionSound;
    }
    
    [System.Serializable]
    public class UserProgressData
    {
        public string userId;
        public DateTime lastSaveTime;
        public List<ExperimentProgress> experimentProgress;
        public Dictionary<string, object> userSettings;
        public List<string> completedExperiments;
        public float totalExperimentTime;
        public int totalScore;
    }
    
    [System.Serializable]
    public class ExperimentProgress
    {
        public string experimentId;
        public bool isCompleted;
        public float completionTime;
        public int score;
        public DateTime lastAttempt;
        public List<StepProgress> stepProgress;
    }
    
    [System.Serializable]
    public class StepProgress
    {
        public int stepNumber;
        public bool isCompleted;
        public float completionTime;
        public float accuracy;
        public string feedback;
    }
    
    [System.Serializable]
    public class SettingsData
    {
        public float mouseSensitivity;
        public float cameraSpeed;
        public bool enableSound;
        public float soundVolume;
        public bool enableParticles;
        public bool enableSafetyWarnings;
        public string language;
        public bool enableAutoSave;
        public float autoSaveInterval;
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDataManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadSettings();
        LoadUserProgress();
        CreateDirectories();
    }
    
    private void Update()
    {
        if (enableAutoSave && Time.time - lastAutoSave >= autoSaveInterval)
        {
            AutoSave();
            lastAutoSave = Time.time;
        }
    }
    
    /// <summary>
    /// Initializes the data manager.
    /// </summary>
    private void InitializeDataManager()
    {
        dataCache.Clear();
        cacheTimestamps.Clear();
        experimentCache.Clear();
        chemicalCache.Clear();
        
        userProgress = new UserProgressData
        {
            userId = SystemInfo.deviceUniqueIdentifier,
            lastSaveTime = DateTime.Now,
            experimentProgress = new List<ExperimentProgress>(),
            userSettings = new Dictionary<string, object>(),
            completedExperiments = new List<string>(),
            totalExperimentTime = 0f,
            totalScore = 0
        };
        
        settings = new SettingsData
        {
            mouseSensitivity = 1.0f,
            cameraSpeed = 3.0f,
            enableSound = true,
            soundVolume = 0.8f,
            enableParticles = true,
            enableSafetyWarnings = true,
            language = "en",
            enableAutoSave = true,
            autoSaveInterval = 30f
        };
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("DataManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Creates necessary directories.
    /// </summary>
    private void CreateDirectories()
    {
        string[] directories = { userDataPath, settingsPath, backupPath };
        
        foreach (string dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Created directory: {dir}");
                }
            }
        }
    }
    
    /// <summary>
    /// Loads experiment data from JSON file.
    /// </summary>
    public ExperimentData LoadExperiment(string experimentId)
    {
        if (string.IsNullOrEmpty(experimentId))
        {
            LogError("Experiment ID is null or empty");
            return null;
        }
        
        // Check cache first
        if (enableDataCaching && experimentCache.ContainsKey(experimentId))
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Experiment {experimentId} loaded from cache");
            }
            return experimentCache[experimentId];
        }
        
        string filePath = Path.Combine(experimentsPath, $"{experimentId}.json");
        
        try
        {
            if (showLoadTimes)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }
            
            if (!File.Exists(filePath))
            {
                LogError($"Experiment file not found: {filePath}");
                return null;
            }
            
            string jsonData = File.ReadAllText(filePath);
            ExperimentData experimentData = JsonUtility.FromJson<ExperimentData>(jsonData);
            
            if (experimentData == null)
            {
                LogError($"Failed to parse experiment data: {experimentId}");
                return null;
            }
            
            // Validate data
            if (enableDataValidation)
            {
                if (!ValidateExperimentData(experimentData))
                {
                    LogError($"Experiment data validation failed: {experimentId}");
                    return null;
                }
            }
            
            // Cache the data
            if (enableDataCaching)
            {
                experimentCache[experimentId] = experimentData;
                
                // Manage cache size
                if (experimentCache.Count > maxCacheSize)
                {
                    RemoveOldestCacheEntry();
                }
            }
            
            if (showLoadTimes)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Debug.Log($"Experiment {experimentId} loaded in {stopwatch.ElapsedMilliseconds}ms");
            }
            
            OnDataLoaded?.Invoke(experimentId);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Experiment {experimentId} loaded successfully");
            }
            
            return experimentData;
        }
        catch (Exception e)
        {
            LogError($"Error loading experiment {experimentId}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads chemical data from JSON file.
    /// </summary>
    public ChemicalData LoadChemical(string chemicalId)
    {
        if (string.IsNullOrEmpty(chemicalId))
        {
            LogError("Chemical ID is null or empty");
            return null;
        }
        
        // Check cache first
        if (enableDataCaching && chemicalCache.ContainsKey(chemicalId))
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Chemical {chemicalId} loaded from cache");
            }
            return chemicalCache[chemicalId];
        }
        
        string filePath = Path.Combine(chemicalsPath, $"{chemicalId}.json");
        
        try
        {
            if (!File.Exists(filePath))
            {
                LogError($"Chemical file not found: {filePath}");
                return null;
            }
            
            string jsonData = File.ReadAllText(filePath);
            ChemicalData chemicalData = JsonUtility.FromJson<ChemicalData>(jsonData);
            
            if (chemicalData == null)
            {
                LogError($"Failed to parse chemical data: {chemicalId}");
                return null;
            }
            
            // Validate data
            if (enableDataValidation)
            {
                if (!ValidateChemicalData(chemicalData))
                {
                    LogError($"Chemical data validation failed: {chemicalId}");
                    return null;
                }
            }
            
            // Cache the data
            if (enableDataCaching)
            {
                chemicalCache[chemicalId] = chemicalData;
                
                // Manage cache size
                if (chemicalCache.Count > maxCacheSize)
                {
                    RemoveOldestCacheEntry();
                }
            }
            
            OnDataLoaded?.Invoke(chemicalId);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Chemical {chemicalId} loaded successfully");
            }
            
            return chemicalData;
        }
        catch (Exception e)
        {
            LogError($"Error loading chemical {chemicalId}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads user progress data.
    /// </summary>
    public void LoadUserProgress()
    {
        string filePath = Path.Combine(userDataPath, "user_progress.json");
        
        try
        {
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                UserProgressData loadedProgress = JsonUtility.FromJson<UserProgressData>(jsonData);
                
                if (loadedProgress != null)
                {
                    userProgress = loadedProgress;
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log("User progress loaded successfully");
                    }
                }
            }
            else
            {
                if (enableDebugLogging)
                {
                    Debug.Log("No existing user progress found, using default");
                }
            }
        }
        catch (Exception e)
        {
            LogError($"Error loading user progress: {e.Message}");
        }
    }
    
    /// <summary>
    /// Saves user progress data.
    /// </summary>
    public void SaveUserProgress()
    {
        if (userProgress == null) return;
        
        userProgress.lastSaveTime = DateTime.Now;
        string filePath = Path.Combine(userDataPath, "user_progress.json");
        
        try
        {
            // Create backup if enabled
            if (enableBackupOnSave && File.Exists(filePath))
            {
                CreateBackup(filePath, "user_progress");
            }
            
            string jsonData = JsonUtility.ToJson(userProgress, true);
            File.WriteAllText(filePath, jsonData);
            
            OnDataSaved?.Invoke("user_progress");
            
            if (enableDebugLogging)
            {
                Debug.Log("User progress saved successfully");
            }
        }
        catch (Exception e)
        {
            LogError($"Error saving user progress: {e.Message}");
        }
    }
    
    /// <summary>
    /// Loads settings data.
    /// </summary>
    public void LoadSettings()
    {
        string filePath = Path.Combine(settingsPath, "settings.json");
        
        try
        {
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                SettingsData loadedSettings = JsonUtility.FromJson<SettingsData>(jsonData);
                
                if (loadedSettings != null)
                {
                    settings = loadedSettings;
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log("Settings loaded successfully");
                    }
                }
            }
            else
            {
                if (enableDebugLogging)
                {
                    Debug.Log("No existing settings found, using default");
                }
            }
        }
        catch (Exception e)
        {
            LogError($"Error loading settings: {e.Message}");
        }
    }
    
    /// <summary>
    /// Saves settings data.
    /// </summary>
    public void SaveSettings()
    {
        if (settings == null) return;
        
        string filePath = Path.Combine(settingsPath, "settings.json");
        
        try
        {
            // Create backup if enabled
            if (enableBackupOnSave && File.Exists(filePath))
            {
                CreateBackup(filePath, "settings");
            }
            
            string jsonData = JsonUtility.ToJson(settings, true);
            File.WriteAllText(filePath, jsonData);
            
            OnDataSaved?.Invoke("settings");
            
            if (enableDebugLogging)
            {
                Debug.Log("Settings saved successfully");
            }
        }
        catch (Exception e)
        {
            LogError($"Error saving settings: {e.Message}");
        }
    }
    
    /// <summary>
    /// Creates a backup of a file.
    /// </summary>
    private void CreateBackup(string originalPath, string backupName)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"{backupName}_{timestamp}.json";
            string backupFilePath = Path.Combine(backupPath, backupFileName);
            
            File.Copy(originalPath, backupFilePath);
            
            // Clean up old backups
            CleanupOldBackups(backupName);
            
            OnBackupCreated?.Invoke(backupFileName);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Backup created: {backupFileName}");
            }
        }
        catch (Exception e)
        {
            LogError($"Error creating backup: {e.Message}");
        }
    }
    
    /// <summary>
    /// Cleans up old backup files.
    /// </summary>
    private void CleanupOldBackups(string backupName)
    {
        try
        {
            string[] backupFiles = Directory.GetFiles(backupPath, $"{backupName}_*.json");
            
            if (backupFiles.Length > maxBackupFiles)
            {
                Array.Sort(backupFiles);
                
                for (int i = 0; i < backupFiles.Length - maxBackupFiles; i++)
                {
                    File.Delete(backupFiles[i]);
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"Deleted old backup: {Path.GetFileName(backupFiles[i])}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            LogError($"Error cleaning up backups: {e.Message}");
        }
    }
    
    /// <summary>
    /// Validates experiment data.
    /// </summary>
    private bool ValidateExperimentData(ExperimentData data)
    {
        if (data == null) return false;
        
        bool isValid = true;
        
        // Basic validation
        if (string.IsNullOrEmpty(data.id) || string.IsNullOrEmpty(data.title))
        {
            isValid = false;
        }
        
        if (data.requiredChemicals == null || data.requiredApparatus == null)
        {
            isValid = false;
        }
        
        if (data.procedure == null || data.procedure.steps == null || data.procedure.steps.Count == 0)
        {
            isValid = false;
        }
        
        OnDataValidated?.Invoke(data.id);
        
        return isValid;
    }
    
    /// <summary>
    /// Validates chemical data.
    /// </summary>
    private bool ValidateChemicalData(ChemicalData data)
    {
        if (data == null) return false;
        
        bool isValid = true;
        
        // Basic validation
        if (string.IsNullOrEmpty(data.id) || string.IsNullOrEmpty(data.name))
        {
            isValid = false;
        }
        
        if (data.physicalProperties == null || data.chemicalProperties == null)
        {
            isValid = false;
        }
        
        OnDataValidated?.Invoke(data.id);
        
        return isValid;
    }
    
    /// <summary>
    /// Removes the oldest cache entry.
    /// </summary>
    private void RemoveOldestCacheEntry()
    {
        string oldestKey = null;
        DateTime oldestTime = DateTime.MaxValue;
        
        foreach (var kvp in cacheTimestamps)
        {
            if (kvp.Value < oldestTime)
            {
                oldestTime = kvp.Value;
                oldestKey = kvp.Key;
            }
        }
        
        if (oldestKey != null)
        {
            dataCache.Remove(oldestKey);
            cacheTimestamps.Remove(oldestKey);
            experimentCache.Remove(oldestKey);
            chemicalCache.Remove(oldestKey);
            
            OnCacheUpdated?.Invoke(oldestKey);
        }
    }
    
    /// <summary>
    /// Performs automatic save.
    /// </summary>
    private void AutoSave()
    {
        SaveUserProgress();
        SaveSettings();
        
        OnAutoSaveCompleted?.Invoke();
        
        if (enableDebugLogging)
        {
            Debug.Log("Auto save completed");
        }
    }
    
    /// <summary>
    /// Logs an error message.
    /// </summary>
    private void LogError(string message)
    {
        Debug.LogError($"[DataManager] {message}");
        OnDataError?.Invoke(message);
    }
    
    // Public getters and setters
    public UserProgressData GetUserProgress() => userProgress;
    public SettingsData GetSettings() => settings;
    public bool IsInitialized() => isInitialized;
    
    /// <summary>
    /// Updates user progress.
    /// </summary>
    public void UpdateUserProgress(UserProgressData newProgress)
    {
        userProgress = newProgress;
        SaveUserProgress();
    }
    
    /// <summary>
    /// Updates settings.
    /// </summary>
    public void UpdateSettings(SettingsData newSettings)
    {
        settings = newSettings;
        SaveSettings();
    }
    
    /// <summary>
    /// Clears all caches.
    /// </summary>
    public void ClearCache()
    {
        dataCache.Clear();
        cacheTimestamps.Clear();
        experimentCache.Clear();
        chemicalCache.Clear();
        
        if (enableDebugLogging)
        {
            Debug.Log("All caches cleared");
        }
    }
    
    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public Dictionary<string, int> GetCacheStats()
    {
        return new Dictionary<string, int>
        {
            { "DataCache", dataCache.Count },
            { "ExperimentCache", experimentCache.Count },
            { "ChemicalCache", chemicalCache.Count },
            { "CacheTimestamps", cacheTimestamps.Count }
        };
    }
    
    /// <summary>
    /// Logs the current data manager status.
    /// </summary>
    public void LogDataManagerStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Data Manager Status ===");
        Debug.Log($"Initialized: {isInitialized}");
        Debug.Log($"Data Caching: {(enableDataCaching ? "Enabled" : "Disabled")}");
        Debug.Log($"Auto Save: {(enableAutoSave ? "Enabled" : "Disabled")}");
        Debug.Log($"Data Validation: {(enableDataValidation ? "Enabled" : "Disabled")}");
        Debug.Log($"Backup on Save: {(enableBackupOnSave ? "Enabled" : "Disabled")}");
        Debug.Log($"Data Cache Size: {dataCache.Count}");
        Debug.Log($"Experiment Cache Size: {experimentCache.Count}");
        Debug.Log($"Chemical Cache Size: {chemicalCache.Count}");
        Debug.Log($"User Progress: {(userProgress != null ? "Loaded" : "Not Loaded")}");
        Debug.Log($"Settings: {(settings != null ? "Loaded" : "Not Loaded")}");
        Debug.Log("===========================");
    }
} 