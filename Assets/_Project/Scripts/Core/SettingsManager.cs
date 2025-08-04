using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Manages user preferences, graphics settings, accessibility options, and application configuration.
/// This component handles all settings and configuration in the virtual chemistry lab.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Settings Categories")]
    [SerializeField] private bool enableGraphicsSettings = true;
    [SerializeField] private bool enableAudioSettings = true;
    [SerializeField] private bool enableAccessibilitySettings = true;
    [SerializeField] private bool enableGameplaySettings = true;
    [SerializeField] private bool enablePerformanceSettings = true;
    
    [Header("Graphics Settings")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool enableVSync = false;
    [SerializeField] private int qualityLevel = 2;
    [SerializeField] private bool enableFullscreen = false;
    [SerializeField] private Vector2Int resolution = new Vector2Int(1920, 1080);
    [SerializeField] private bool enableAntiAliasing = true;
    [SerializeField] private int antiAliasingLevel = 4;
    [SerializeField] private bool enableShadows = true;
    [SerializeField] private ShadowQuality shadowQuality = ShadowQuality.All;
    [SerializeField] private bool enableParticles = true;
    [SerializeField] private int maxParticles = 1000;
    
    [Header("Audio Settings")]
    [SerializeField] private bool enableAudio = true;
    [SerializeField] private float masterVolume = 1.0f;
    [SerializeField] private float musicVolume = 0.8f;
    [SerializeField] private float sfxVolume = 1.0f;
    [SerializeField] private float ambientVolume = 0.6f;
    [SerializeField] private float reactionVolume = 0.9f;
    [SerializeField] private bool enable3DAudio = true;
    [SerializeField] private bool enableAudioPooling = true;
    
    [Header("Accessibility Settings")]
    [SerializeField] private bool enableHighContrast = false;
    [SerializeField] private bool enableLargeText = false;
    [SerializeField] private bool enableScreenReader = false;
    [SerializeField] private bool enableColorBlindSupport = false;
    [SerializeField] private ColorBlindMode colorBlindMode = ColorBlindMode.None;
    [SerializeField] private bool enableMotionReduction = false;
    [SerializeField] private bool enableSubtitles = true;
    [SerializeField] private float subtitleSize = 1.0f;
    [SerializeField] private bool enableAudioDescriptions = false;
    [SerializeField] private bool enableKeyboardNavigation = true;
    
    [Header("Gameplay Settings")]
    [SerializeField] private float mouseSensitivity = 1.0f;
    [SerializeField] private float cameraSpeed = 3.0f;
    [SerializeField] private bool enableInvertY = false;
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;
    [SerializeField] private bool enableTutorials = true;
    [SerializeField] private bool enableHints = true;
    [SerializeField] private bool enableSafetyWarnings = true;
    [SerializeField] private bool enableRealTimeValidation = true;
    [SerializeField] private string language = "en";
    
    [Header("Performance Settings")]
    [SerializeField] private bool enablePerformanceMode = false;
    [SerializeField] private bool enableLODSystem = true;
    [SerializeField] private float LODDistance = 50f;
    [SerializeField] private bool enableOcclusionCulling = true;
    [SerializeField] private bool enableFrustumCulling = true;
    [SerializeField] private bool enableDistanceCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private bool enableTextureCompression = true;
    [SerializeField] private bool enableMeshCompression = true;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugMode = false;
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool enablePerformanceProfiling = false;
    [SerializeField] private bool enableErrorReporting = true;
    [SerializeField] private bool enableAnalytics = false;
    
    [Header("File Management")]
    [SerializeField] private string settingsFilePath = "Settings/settings.json";
    [SerializeField] private bool enableAutoBackup = true;
    [SerializeField] private int maxBackupFiles = 5;
    [SerializeField] private bool enableSettingsValidation = true;
    
    private static SettingsManager instance;
    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SettingsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SettingsManager");
                    instance = go.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action OnSettingsLoaded;
    public event Action OnSettingsSaved;
    public event Action<string> OnSettingChanged;
    public event Action OnGraphicsSettingsChanged;
    public event Action OnAudioSettingsChanged;
    public event Action OnAccessibilitySettingsChanged;
    public event Action OnGameplaySettingsChanged;
    public event Action OnPerformanceSettingsChanged;
    public event Action<string> OnLanguageChanged;
    public event Action<bool> OnDebugModeChanged;
    
    // Private variables
    private SettingsData currentSettings;
    private bool isInitialized = false;
    private bool isDirty = false;
    private float lastSaveTime = 0f;
    private float autoSaveTimer = 0f;
    
    [System.Serializable]
    public enum ColorBlindMode
    {
        None,
        Protanopia,
        Deuteranopia,
        Tritanopia,
        Monochromacy
    }
    
    [System.Serializable]
    public enum ShadowQuality
    {
        Disabled,
        HardOnly,
        All
    }
    
    [System.Serializable]
    public class SettingsData
    {
        // Graphics
        public int targetFrameRate;
        public bool enableVSync;
        public int qualityLevel;
        public bool enableFullscreen;
        public Vector2Int resolution;
        public bool enableAntiAliasing;
        public int antiAliasingLevel;
        public bool enableShadows;
        public ShadowQuality shadowQuality;
        public bool enableParticles;
        public int maxParticles;
        
        // Audio
        public bool enableAudio;
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float ambientVolume;
        public float reactionVolume;
        public bool enable3DAudio;
        public bool enableAudioPooling;
        
        // Accessibility
        public bool enableHighContrast;
        public bool enableLargeText;
        public bool enableScreenReader;
        public bool enableColorBlindSupport;
        public ColorBlindMode colorBlindMode;
        public bool enableMotionReduction;
        public bool enableSubtitles;
        public float subtitleSize;
        public bool enableAudioDescriptions;
        public bool enableKeyboardNavigation;
        
        // Gameplay
        public float mouseSensitivity;
        public float cameraSpeed;
        public bool enableInvertY;
        public bool enableAutoSave;
        public float autoSaveInterval;
        public bool enableTutorials;
        public bool enableHints;
        public bool enableSafetyWarnings;
        public bool enableRealTimeValidation;
        public string language;
        
        // Performance
        public bool enablePerformanceMode;
        public bool enableLODSystem;
        public float LODDistance;
        public bool enableOcclusionCulling;
        public bool enableFrustumCulling;
        public bool enableDistanceCulling;
        public float cullingDistance;
        public bool enableTextureCompression;
        public bool enableMeshCompression;
        
        // Debug
        public bool enableDebugMode;
        public bool enableDebugLogging;
        public bool enablePerformanceProfiling;
        public bool enableErrorReporting;
        public bool enableAnalytics;
        
        // Metadata
        public DateTime lastModified;
        public string version;
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettingsManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadSettings();
        ApplySettings();
    }
    
    private void Update()
    {
        if (enableAutoSave && isDirty)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                SaveSettings();
                autoSaveTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Initializes the settings manager.
    /// </summary>
    private void InitializeSettingsManager()
    {
        currentSettings = CreateDefaultSettings();
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("SettingsManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Creates default settings.
    /// </summary>
    private SettingsData CreateDefaultSettings()
    {
        return new SettingsData
        {
            // Graphics
            targetFrameRate = targetFrameRate,
            enableVSync = enableVSync,
            qualityLevel = qualityLevel,
            enableFullscreen = enableFullscreen,
            resolution = resolution,
            enableAntiAliasing = enableAntiAliasing,
            antiAliasingLevel = antiAliasingLevel,
            enableShadows = enableShadows,
            shadowQuality = shadowQuality,
            enableParticles = enableParticles,
            maxParticles = maxParticles,
            
            // Audio
            enableAudio = enableAudio,
            masterVolume = masterVolume,
            musicVolume = musicVolume,
            sfxVolume = sfxVolume,
            ambientVolume = ambientVolume,
            reactionVolume = reactionVolume,
            enable3DAudio = enable3DAudio,
            enableAudioPooling = enableAudioPooling,
            
            // Accessibility
            enableHighContrast = enableHighContrast,
            enableLargeText = enableLargeText,
            enableScreenReader = enableScreenReader,
            enableColorBlindSupport = enableColorBlindSupport,
            colorBlindMode = colorBlindMode,
            enableMotionReduction = enableMotionReduction,
            enableSubtitles = enableSubtitles,
            subtitleSize = subtitleSize,
            enableAudioDescriptions = enableAudioDescriptions,
            enableKeyboardNavigation = enableKeyboardNavigation,
            
            // Gameplay
            mouseSensitivity = mouseSensitivity,
            cameraSpeed = cameraSpeed,
            enableInvertY = enableInvertY,
            enableAutoSave = enableAutoSave,
            autoSaveInterval = autoSaveInterval,
            enableTutorials = enableTutorials,
            enableHints = enableHints,
            enableSafetyWarnings = enableSafetyWarnings,
            enableRealTimeValidation = enableRealTimeValidation,
            language = language,
            
            // Performance
            enablePerformanceMode = enablePerformanceMode,
            enableLODSystem = enableLODSystem,
            LODDistance = LODDistance,
            enableOcclusionCulling = enableOcclusionCulling,
            enableFrustumCulling = enableFrustumCulling,
            enableDistanceCulling = enableDistanceCulling,
            cullingDistance = cullingDistance,
            enableTextureCompression = enableTextureCompression,
            enableMeshCompression = enableMeshCompression,
            
            // Debug
            enableDebugMode = enableDebugMode,
            enableDebugLogging = enableDebugLogging,
            enablePerformanceProfiling = enablePerformanceProfiling,
            enableErrorReporting = enableErrorReporting,
            enableAnalytics = enableAnalytics,
            
            // Metadata
            lastModified = DateTime.Now,
            version = Application.version
        };
    }
    
    /// <summary>
    /// Loads settings from file.
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            if (File.Exists(settingsFilePath))
            {
                string jsonData = File.ReadAllText(settingsFilePath);
                SettingsData loadedSettings = JsonUtility.FromJson<SettingsData>(jsonData);
                
                if (loadedSettings != null && ValidateSettings(loadedSettings))
                {
                    currentSettings = loadedSettings;
                    ApplySettingsToInspector();
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log("Settings loaded successfully");
                    }
                }
                else
                {
                    if (enableDebugLogging)
                    {
                        Debug.LogWarning("Invalid settings file, using defaults");
                    }
                }
            }
            else
            {
                if (enableDebugLogging)
                {
                    Debug.Log("No settings file found, using defaults");
                }
            }
        }
        catch (Exception e)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error loading settings: {e.Message}");
            }
        }
        
        OnSettingsLoaded?.Invoke();
    }
    
    /// <summary>
    /// Saves settings to file.
    /// </summary>
    public void SaveSettings()
    {
        if (currentSettings == null) return;
        
        try
        {
            // Create backup if enabled
            if (enableAutoBackup && File.Exists(settingsFilePath))
            {
                CreateSettingsBackup();
            }
            
            // Update metadata
            currentSettings.lastModified = DateTime.Now;
            currentSettings.version = Application.version;
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(settingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string jsonData = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(settingsFilePath, jsonData);
            
            isDirty = false;
            lastSaveTime = Time.time;
            
            OnSettingsSaved?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Settings saved successfully");
            }
        }
        catch (Exception e)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error saving settings: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Creates a backup of the settings file.
    /// </summary>
    private void CreateSettingsBackup()
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = settingsFilePath.Replace(".json", $"_backup_{timestamp}.json");
            File.Copy(settingsFilePath, backupPath);
            
            // Clean up old backups
            CleanupOldBackups();
            
            if (enableDebugLogging)
            {
                Debug.Log($"Settings backup created: {backupPath}");
            }
        }
        catch (Exception e)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error creating settings backup: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Cleans up old backup files.
    /// </summary>
    private void CleanupOldBackups()
    {
        try
        {
            string directory = Path.GetDirectoryName(settingsFilePath);
            string pattern = Path.GetFileNameWithoutExtension(settingsFilePath) + "_backup_*.json";
            string[] backupFiles = Directory.GetFiles(directory, pattern);
            
            if (backupFiles.Length > maxBackupFiles)
            {
                Array.Sort(backupFiles);
                
                for (int i = 0; i < backupFiles.Length - maxBackupFiles; i++)
                {
                    File.Delete(backupFiles[i]);
                }
            }
        }
        catch (Exception e)
        {
            if (enableDebugLogging)
            {
                Debug.LogError($"Error cleaning up backups: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Validates settings data.
    /// </summary>
    private bool ValidateSettings(SettingsData settings)
    {
        if (!enableSettingsValidation) return true;
        
        if (settings == null) return false;
        
        // Basic validation
        if (settings.targetFrameRate < 30 || settings.targetFrameRate > 300) return false;
        if (settings.masterVolume < 0f || settings.masterVolume > 1f) return false;
        if (settings.mouseSensitivity < 0.1f || settings.mouseSensitivity > 5f) return false;
        if (settings.cameraSpeed < 0.1f || settings.cameraSpeed > 10f) return false;
        
        return true;
    }
    
    /// <summary>
    /// Applies current settings to the application.
    /// </summary>
    public void ApplySettings()
    {
        if (currentSettings == null) return;
        
        ApplyGraphicsSettings();
        ApplyAudioSettings();
        ApplyAccessibilitySettings();
        ApplyGameplaySettings();
        ApplyPerformanceSettings();
        ApplyDebugSettings();
        
        if (enableDebugLogging)
        {
            Debug.Log("Settings applied successfully");
        }
    }
    
    /// <summary>
    /// Applies graphics settings.
    /// </summary>
    private void ApplyGraphicsSettings()
    {
        if (!enableGraphicsSettings) return;
        
        Application.targetFrameRate = currentSettings.targetFrameRate;
        QualitySettings.vSyncCount = currentSettings.enableVSync ? 1 : 0;
        QualitySettings.SetQualityLevel(currentSettings.qualityLevel, true);
        
        Screen.fullScreen = currentSettings.enableFullscreen;
        Screen.SetResolution(currentSettings.resolution.x, currentSettings.resolution.y, currentSettings.enableFullscreen);
        
        QualitySettings.antiAliasing = currentSettings.enableAntiAliasing ? currentSettings.antiAliasingLevel : 0;
        QualitySettings.shadowDistance = currentSettings.enableShadows ? 50f : 0f;
        
        OnGraphicsSettingsChanged?.Invoke();
    }
    
    /// <summary>
    /// Applies audio settings.
    /// </summary>
    private void ApplyAudioSettings()
    {
        if (!enableAudioSettings) return;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAudioEnabled(currentSettings.enableAudio);
            AudioManager.Instance.SetMasterVolume(currentSettings.masterVolume);
            AudioManager.Instance.SetMusicVolume(currentSettings.musicVolume);
            AudioManager.Instance.SetSFXVolume(currentSettings.sfxVolume);
            AudioManager.Instance.SetAmbientVolume(currentSettings.ambientVolume);
            AudioManager.Instance.SetReactionVolume(currentSettings.reactionVolume);
        }
        
        OnAudioSettingsChanged?.Invoke();
    }
    
    /// <summary>
    /// Applies accessibility settings.
    /// </summary>
    private void ApplyAccessibilitySettings()
    {
        if (!enableAccessibilitySettings) return;
        
        // Apply color blind support
        if (currentSettings.enableColorBlindSupport)
        {
            ApplyColorBlindMode(currentSettings.colorBlindMode);
        }
        
        // Apply motion reduction
        if (currentSettings.enableMotionReduction)
        {
            Time.timeScale = 0.5f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
        
        OnAccessibilitySettingsChanged?.Invoke();
    }
    
    /// <summary>
    /// Applies gameplay settings.
    /// </summary>
    private void ApplyGameplaySettings()
    {
        if (!enableGameplaySettings) return;
        
        if (MouseInputHandler.Instance != null)
        {
            MouseInputHandler.Instance.SetMouseSensitivity(currentSettings.mouseSensitivity);
        }
        
        if (KeyboardInputHandler.Instance != null)
        {
            KeyboardInputHandler.Instance.SetCameraSpeed(currentSettings.cameraSpeed);
        }
        
        OnGameplaySettingsChanged?.Invoke();
    }
    
    /// <summary>
    /// Applies performance settings.
    /// </summary>
    private void ApplyPerformanceSettings()
    {
        if (!enablePerformanceSettings) return;
        
        QualitySettings.lodBias = currentSettings.enableLODSystem ? 1.0f : 0.0f;
        QualitySettings.maxLODLevel = currentSettings.enableLODSystem ? 2 : 0;
        
        OnPerformanceSettingsChanged?.Invoke();
    }
    
    /// <summary>
    /// Applies debug settings.
    /// </summary>
    private void ApplyDebugSettings()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ToggleDebugMode();
        }
        
        OnDebugModeChanged?.Invoke(currentSettings.enableDebugMode);
    }
    
    /// <summary>
    /// Applies color blind mode.
    /// </summary>
    private void ApplyColorBlindMode(ColorBlindMode mode)
    {
        // This would apply color blind shaders or color adjustments
        // Implementation depends on the specific color blind support system
    }
    
    /// <summary>
    /// Applies current settings to inspector variables.
    /// </summary>
    private void ApplySettingsToInspector()
    {
        if (currentSettings == null) return;
        
        // Graphics
        targetFrameRate = currentSettings.targetFrameRate;
        enableVSync = currentSettings.enableVSync;
        qualityLevel = currentSettings.qualityLevel;
        enableFullscreen = currentSettings.enableFullscreen;
        resolution = currentSettings.resolution;
        enableAntiAliasing = currentSettings.enableAntiAliasing;
        antiAliasingLevel = currentSettings.antiAliasingLevel;
        enableShadows = currentSettings.enableShadows;
        shadowQuality = currentSettings.shadowQuality;
        enableParticles = currentSettings.enableParticles;
        maxParticles = currentSettings.maxParticles;
        
        // Audio
        enableAudio = currentSettings.enableAudio;
        masterVolume = currentSettings.masterVolume;
        musicVolume = currentSettings.musicVolume;
        sfxVolume = currentSettings.sfxVolume;
        ambientVolume = currentSettings.ambientVolume;
        reactionVolume = currentSettings.reactionVolume;
        enable3DAudio = currentSettings.enable3DAudio;
        enableAudioPooling = currentSettings.enableAudioPooling;
        
        // Accessibility
        enableHighContrast = currentSettings.enableHighContrast;
        enableLargeText = currentSettings.enableLargeText;
        enableScreenReader = currentSettings.enableScreenReader;
        enableColorBlindSupport = currentSettings.enableColorBlindSupport;
        colorBlindMode = currentSettings.colorBlindMode;
        enableMotionReduction = currentSettings.enableMotionReduction;
        enableSubtitles = currentSettings.enableSubtitles;
        subtitleSize = currentSettings.subtitleSize;
        enableAudioDescriptions = currentSettings.enableAudioDescriptions;
        enableKeyboardNavigation = currentSettings.enableKeyboardNavigation;
        
        // Gameplay
        mouseSensitivity = currentSettings.mouseSensitivity;
        cameraSpeed = currentSettings.cameraSpeed;
        enableInvertY = currentSettings.enableInvertY;
        enableAutoSave = currentSettings.enableAutoSave;
        autoSaveInterval = currentSettings.autoSaveInterval;
        enableTutorials = currentSettings.enableTutorials;
        enableHints = currentSettings.enableHints;
        enableSafetyWarnings = currentSettings.enableSafetyWarnings;
        enableRealTimeValidation = currentSettings.enableRealTimeValidation;
        language = currentSettings.language;
        
        // Performance
        enablePerformanceMode = currentSettings.enablePerformanceMode;
        enableLODSystem = currentSettings.enableLODSystem;
        LODDistance = currentSettings.LODDistance;
        enableOcclusionCulling = currentSettings.enableOcclusionCulling;
        enableFrustumCulling = currentSettings.enableFrustumCulling;
        enableDistanceCulling = currentSettings.enableDistanceCulling;
        cullingDistance = currentSettings.cullingDistance;
        enableTextureCompression = currentSettings.enableTextureCompression;
        enableMeshCompression = currentSettings.enableMeshCompression;
        
        // Debug
        enableDebugMode = currentSettings.enableDebugMode;
        enableDebugLogging = currentSettings.enableDebugLogging;
        enablePerformanceProfiling = currentSettings.enablePerformanceProfiling;
        enableErrorReporting = currentSettings.enableErrorReporting;
        enableAnalytics = currentSettings.enableAnalytics;
    }
    
    /// <summary>
    /// Resets settings to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        currentSettings = CreateDefaultSettings();
        ApplySettingsToInspector();
        ApplySettings();
        isDirty = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("Settings reset to defaults");
        }
    }
    
    // Public getters and setters
    public SettingsData GetCurrentSettings() => currentSettings;
    public bool IsInitialized() => isInitialized;
    public bool IsDirty() => isDirty;
    
    /// <summary>
    /// Sets a graphics setting.
    /// </summary>
    public void SetGraphicsSetting(string setting, object value)
    {
        if (currentSettings == null) return;
        
        switch (setting)
        {
            case "targetFrameRate":
                currentSettings.targetFrameRate = (int)value;
                break;
            case "enableVSync":
                currentSettings.enableVSync = (bool)value;
                break;
            case "qualityLevel":
                currentSettings.qualityLevel = (int)value;
                break;
            case "enableFullscreen":
                currentSettings.enableFullscreen = (bool)value;
                break;
            case "resolution":
                currentSettings.resolution = (Vector2Int)value;
                break;
        }
        
        isDirty = true;
        OnSettingChanged?.Invoke(setting);
    }
    
    /// <summary>
    /// Sets an audio setting.
    /// </summary>
    public void SetAudioSetting(string setting, object value)
    {
        if (currentSettings == null) return;
        
        switch (setting)
        {
            case "enableAudio":
                currentSettings.enableAudio = (bool)value;
                break;
            case "masterVolume":
                currentSettings.masterVolume = (float)value;
                break;
            case "musicVolume":
                currentSettings.musicVolume = (float)value;
                break;
            case "sfxVolume":
                currentSettings.sfxVolume = (float)value;
                break;
        }
        
        isDirty = true;
        OnSettingChanged?.Invoke(setting);
    }
    
    /// <summary>
    /// Sets a gameplay setting.
    /// </summary>
    public void SetGameplaySetting(string setting, object value)
    {
        if (currentSettings == null) return;
        
        switch (setting)
        {
            case "mouseSensitivity":
                currentSettings.mouseSensitivity = (float)value;
                break;
            case "cameraSpeed":
                currentSettings.cameraSpeed = (float)value;
                break;
            case "language":
                currentSettings.language = (string)value;
                OnLanguageChanged?.Invoke((string)value);
                break;
        }
        
        isDirty = true;
        OnSettingChanged?.Invoke(setting);
    }
    
    /// <summary>
    /// Gets a setting value.
    /// </summary>
    public T GetSetting<T>(string setting)
    {
        if (currentSettings == null) return default(T);
        
        switch (setting)
        {
            case "targetFrameRate":
                return (T)(object)currentSettings.targetFrameRate;
            case "enableAudio":
                return (T)(object)currentSettings.enableAudio;
            case "masterVolume":
                return (T)(object)currentSettings.masterVolume;
            case "mouseSensitivity":
                return (T)(object)currentSettings.mouseSensitivity;
            case "language":
                return (T)(object)currentSettings.language;
            default:
                return default(T);
        }
    }
    
    /// <summary>
    /// Logs the current settings status.
    /// </summary>
    public void LogSettingsStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Settings Manager Status ===");
        Debug.Log($"Initialized: {isInitialized}");
        Debug.Log($"Dirty: {isDirty}");
        Debug.Log($"Auto Save: {(currentSettings?.enableAutoSave ?? false)}");
        Debug.Log($"Target Frame Rate: {currentSettings?.targetFrameRate ?? 60}");
        Debug.Log($"Quality Level: {currentSettings?.qualityLevel ?? 2}");
        Debug.Log($"Master Volume: {currentSettings?.masterVolume ?? 1.0f}");
        Debug.Log($"Mouse Sensitivity: {currentSettings?.mouseSensitivity ?? 1.0f}");
        Debug.Log($"Language: {currentSettings?.language ?? "en"}");
        Debug.Log($"Debug Mode: {currentSettings?.enableDebugMode ?? false}");
        Debug.Log($"Last Modified: {currentSettings?.lastModified}");
        Debug.Log("===============================");
    }
} 