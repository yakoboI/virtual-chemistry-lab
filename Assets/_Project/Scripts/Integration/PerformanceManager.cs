using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages performance monitoring, optimization, and resource management for the virtual chemistry lab.
/// This component handles all performance-related operations and optimization procedures.
/// </summary>
public class PerformanceManager : MonoBehaviour
{
    [Header("Performance Management")]
    [SerializeField] private bool enablePerformanceManagement = true;
    [SerializeField] private bool enablePerformanceMonitoring = true;
    [SerializeField] private bool enablePerformanceOptimization = true;
    [SerializeField] private bool enableResourceManagement = true;
    [SerializeField] private bool enablePerformanceProfiling = true;
    
    [Header("Performance Configuration")]
    [SerializeField] private PerformanceProfile[] availableProfiles;
    [SerializeField] private string performanceAlertSound = "performance_alert";
    [SerializeField] private string optimizationCompleteSound = "optimization_complete";
    
    [Header("Performance State")]
    [SerializeField] private Dictionary<string, PerformanceMetric> activeMetrics = new Dictionary<string, PerformanceMetric>();
    [SerializeField] private List<PerformanceReport> performanceReports = new List<PerformanceReport>();
    [SerializeField] private bool isPerformanceMonitoring = false;
    
    [Header("Monitoring Settings")]
    [SerializeField] private bool enableRealTimeMonitoring = true;
    [SerializeField] private bool enablePerformanceLogging = true;
    [SerializeField] private bool enablePerformanceReporting = true;
    [SerializeField] private float monitoringInterval = 0.5f;
    [SerializeField] private float alertThreshold = 0.8f;
    [SerializeField] private int maxDataPoints = 1000;
    
    [Header("Optimization Settings")]
    [SerializeField] private bool enableAutoOptimization = false;
    [SerializeField] private bool enableQualityScaling = true;
    [SerializeField] private bool enableLODManagement = true;
    [SerializeField] private bool enableTextureCompression = true;
    [SerializeField] private bool enableObjectPooling = true;
    [SerializeField] private float optimizationThreshold = 0.7f;
    
    [Header("Resource Management")]
    [SerializeField] private bool enableMemoryManagement = true;
    [SerializeField] private bool enableGarbageCollection = true;
    [SerializeField] private bool enableAssetUnloading = true;
    [SerializeField] private float memoryThreshold = 0.9f;
    [SerializeField] private float gcInterval = 30f;
    
    [Header("Profiling Settings")]
    [SerializeField] private bool enableFrameProfiling = true;
    [SerializeField] private bool enableMemoryProfiling = true;
    [SerializeField] private bool enableComponentProfiling = true;
    [SerializeField] private bool enableSceneProfiling = true;
    [SerializeField] private float profilingInterval = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logPerformanceEvents = false;
    
    private static PerformanceManager instance;
    public static PerformanceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PerformanceManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PerformanceManager");
                    instance = go.AddComponent<PerformanceManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnPerformanceAlert;
    public event Action<string> OnOptimizationComplete;
    public event Action<PerformanceReport> OnPerformanceReport;
    public event Action<float> OnFPSChanged;
    public event Action<float> OnMemoryUsageChanged;
    public event Action<string> OnPerformanceError;
    
    // Private variables
    private Dictionary<string, PerformanceProfile> profileDatabase = new Dictionary<string, PerformanceProfile>();
    private bool isInitialized = false;
    private float lastMonitoringTime = 0f;
    private float lastProfilingTime = 0f;
    private float lastGCTime = 0f;
    
    [System.Serializable]
    public class PerformanceProfile
    {
        public string id;
        public string name;
        public string description;
        public PerformanceLevel level;
        public float targetFPS;
        public float maxMemoryUsage;
        public QualitySettings qualitySettings;
        public OptimizationSettings optimizationSettings;
    }
    
    [System.Serializable]
    public class PerformanceMetric
    {
        public string id;
        public string name;
        public float currentValue;
        public float averageValue;
        public float minValue;
        public float maxValue;
        public float threshold;
        public bool isAlerting;
        public List<float> dataPoints;
        public float lastUpdateTime;
    }
    
    [System.Serializable]
    public class PerformanceReport
    {
        public string id;
        public float timestamp;
        public float averageFPS;
        public float memoryUsage;
        public float cpuUsage;
        public int activeObjects;
        public int drawCalls;
        public bool isOptimized;
        public List<PerformanceMetric> metrics;
        public OptimizationResult optimizationResult;
    }
    
    [System.Serializable]
    public class QualitySettings
    {
        public int qualityLevel;
        public bool enableShadows;
        public bool enableAntiAliasing;
        public int shadowResolution;
        public float shadowDistance;
        public int textureQuality;
        public bool enableSoftParticles;
    }
    
    [System.Serializable]
    public class OptimizationSettings
    {
        public bool enableLOD;
        public bool enableOcclusionCulling;
        public bool enableFrustumCulling;
        public float cullingDistance;
        public int maxParticles;
        public bool enableTextureStreaming;
    }
    
    [System.Serializable]
    public class OptimizationResult
    {
        public bool wasOptimized;
        public float performanceGain;
        public string optimizationType;
        public float timestamp;
        public List<string> appliedOptimizations;
    }
    
    [System.Serializable]
    public enum PerformanceLevel
    {
        Low,
        Medium,
        High,
        Ultra,
        Custom
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePerformanceManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadProfileDatabase();
        InitializePerformanceMetrics();
    }
    
    private void Update()
    {
        UpdatePerformanceMonitoring();
        UpdateProfiling();
        UpdateResourceManagement();
    }
    
    /// <summary>
    /// Initializes the performance manager.
    /// </summary>
    private void InitializePerformanceManager()
    {
        activeMetrics.Clear();
        performanceReports.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("PerformanceManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads performance profile data from available profiles.
    /// </summary>
    private void LoadProfileDatabase()
    {
        profileDatabase.Clear();
        
        foreach (PerformanceProfile profile in availableProfiles)
        {
            if (profile != null && !string.IsNullOrEmpty(profile.id))
            {
                profileDatabase[profile.id] = profile;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Loaded performance profile: {profile.name} ({profile.id})");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {profileDatabase.Count} performance profiles");
        }
    }
    
    /// <summary>
    /// Initializes performance metrics.
    /// </summary>
    private void InitializePerformanceMetrics()
    {
        // FPS Metric
        CreateMetric("FPS", "Frames Per Second", 60f, 30f);
        
        // Memory Metric
        CreateMetric("Memory", "Memory Usage (MB)", 1024f, 2048f);
        
        // CPU Metric
        CreateMetric("CPU", "CPU Usage (%)", 50f, 80f);
        
        // Object Count Metric
        CreateMetric("Objects", "Active Objects", 1000f, 2000f);
        
        // Draw Calls Metric
        CreateMetric("DrawCalls", "Draw Calls", 100f, 200f);
        
        isPerformanceMonitoring = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("Performance metrics initialized");
        }
    }
    
    /// <summary>
    /// Creates a performance metric.
    /// </summary>
    public void CreateMetric(string id, string name, float threshold, float maxValue)
    {
        if (activeMetrics.ContainsKey(id))
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Metric {id} already exists");
            }
            return;
        }
        
        PerformanceMetric metric = new PerformanceMetric
        {
            id = id,
            name = name,
            currentValue = 0f,
            averageValue = 0f,
            minValue = float.MaxValue,
            maxValue = 0f,
            threshold = threshold,
            isAlerting = false,
            dataPoints = new List<float>(),
            lastUpdateTime = Time.time
        };
        
        activeMetrics[id] = metric;
        
        if (logPerformanceEvents)
        {
            Debug.Log($"Created performance metric: {name} ({id})");
        }
    }
    
    /// <summary>
    /// Updates performance monitoring.
    /// </summary>
    private void UpdatePerformanceMonitoring()
    {
        if (!enablePerformanceMonitoring || Time.time - lastMonitoringTime < monitoringInterval) return;
        
        lastMonitoringTime = Time.time;
        
        // Update FPS
        float currentFPS = 1f / Time.deltaTime;
        UpdateMetric("FPS", currentFPS);
        OnFPSChanged?.Invoke(currentFPS);
        
        // Update Memory Usage
        float memoryUsage = SystemInfo.systemMemorySize;
        UpdateMetric("Memory", memoryUsage);
        OnMemoryUsageChanged?.Invoke(memoryUsage);
        
        // Update CPU Usage (simulated)
        float cpuUsage = UnityEngine.Random.Range(20f, 80f);
        UpdateMetric("CPU", cpuUsage);
        
        // Update Object Count
        int objectCount = FindObjectsOfType<GameObject>().Length;
        UpdateMetric("Objects", objectCount);
        
        // Update Draw Calls (simulated)
        float drawCalls = UnityEngine.Random.Range(50f, 150f);
        UpdateMetric("DrawCalls", drawCalls);
        
        // Check for performance alerts
        CheckPerformanceAlerts();
        
        // Auto-optimization if enabled
        if (enableAutoOptimization)
        {
            CheckAutoOptimization();
        }
    }
    
    /// <summary>
    /// Updates a performance metric.
    /// </summary>
    private void UpdateMetric(string metricId, float value)
    {
        if (!activeMetrics.ContainsKey(metricId)) return;
        
        PerformanceMetric metric = activeMetrics[metricId];
        
        metric.currentValue = value;
        metric.lastUpdateTime = Time.time;
        
        // Update min/max values
        metric.minValue = Mathf.Min(metric.minValue, value);
        metric.maxValue = Mathf.Max(metric.maxValue, value);
        
        // Add to data points
        metric.dataPoints.Add(value);
        
        // Limit data points
        if (metric.dataPoints.Count > maxDataPoints)
        {
            metric.dataPoints.RemoveAt(0);
        }
        
        // Calculate average
        float sum = 0f;
        foreach (float point in metric.dataPoints)
        {
            sum += point;
        }
        metric.averageValue = sum / metric.dataPoints.Count;
        
        // Check for alerts
        bool wasAlerting = metric.isAlerting;
        metric.isAlerting = value > metric.threshold;
        
        if (metric.isAlerting && !wasAlerting)
        {
            OnPerformanceAlert?.Invoke($"{metric.name} exceeded threshold: {value:F1} > {metric.threshold:F1}");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(performanceAlertSound);
            }
        }
    }
    
    /// <summary>
    /// Checks for performance alerts.
    /// </summary>
    private void CheckPerformanceAlerts()
    {
        foreach (var kvp in activeMetrics)
        {
            PerformanceMetric metric = kvp.Value;
            
            if (metric.isAlerting)
            {
                if (logPerformanceEvents)
                {
                    Debug.LogWarning($"Performance alert: {metric.name} = {metric.currentValue:F1}");
                }
            }
        }
    }
    
    /// <summary>
    /// Checks for auto-optimization opportunities.
    /// </summary>
    private void CheckAutoOptimization()
    {
        float currentFPS = activeMetrics.ContainsKey("FPS") ? activeMetrics["FPS"].currentValue : 60f;
        float memoryUsage = activeMetrics.ContainsKey("Memory") ? activeMetrics["Memory"].currentValue : 0f;
        
        bool needsOptimization = currentFPS < 30f || memoryUsage > memoryThreshold * SystemInfo.systemMemorySize;
        
        if (needsOptimization)
        {
            PerformOptimization("Auto");
        }
    }
    
    /// <summary>
    /// Updates profiling.
    /// </summary>
    private void UpdateProfiling()
    {
        if (!enablePerformanceProfiling || Time.time - lastProfilingTime < profilingInterval) return;
        
        lastProfilingTime = Time.time;
        
        // Generate performance report
        PerformanceReport report = GeneratePerformanceReport();
        performanceReports.Add(report);
        
        OnPerformanceReport?.Invoke(report);
        
        if (logPerformanceEvents)
        {
            Debug.Log($"Performance report generated - FPS: {report.averageFPS:F1}, Memory: {report.memoryUsage:F0}MB");
        }
    }
    
    /// <summary>
    /// Updates resource management.
    /// </summary>
    private void UpdateResourceManagement()
    {
        if (!enableResourceManagement) return;
        
        // Garbage collection
        if (enableGarbageCollection && Time.time - lastGCTime > gcInterval)
        {
            PerformGarbageCollection();
            lastGCTime = Time.time;
        }
        
        // Memory management
        if (enableMemoryManagement)
        {
            CheckMemoryUsage();
        }
    }
    
    /// <summary>
    /// Performs optimization.
    /// </summary>
    public bool PerformOptimization(string optimizationType)
    {
        if (!enablePerformanceOptimization) return false;
        
        bool wasOptimized = false;
        List<string> appliedOptimizations = new List<string>();
        
        // Quality scaling
        if (enableQualityScaling)
        {
            if (OptimizeQualitySettings())
            {
                appliedOptimizations.Add("Quality Scaling");
                wasOptimized = true;
            }
        }
        
        // LOD management
        if (enableLODManagement)
        {
            if (OptimizeLODSettings())
            {
                appliedOptimizations.Add("LOD Management");
                wasOptimized = true;
            }
        }
        
        // Texture compression
        if (enableTextureCompression)
        {
            if (OptimizeTextureSettings())
            {
                appliedOptimizations.Add("Texture Compression");
                wasOptimized = true;
            }
        }
        
        // Object pooling
        if (enableObjectPooling)
        {
            if (OptimizeObjectPooling())
            {
                appliedOptimizations.Add("Object Pooling");
                wasOptimized = true;
            }
        }
        
        if (wasOptimized)
        {
            OptimizationResult result = new OptimizationResult
            {
                wasOptimized = true,
                performanceGain = CalculatePerformanceGain(),
                optimizationType = optimizationType,
                timestamp = Time.time,
                appliedOptimizations = appliedOptimizations
            };
            
            OnOptimizationComplete?.Invoke($"Applied {appliedOptimizations.Count} optimizations");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(optimizationCompleteSound);
            }
            
            if (logPerformanceEvents)
            {
                Debug.Log($"Performance optimization completed: {string.Join(", ", appliedOptimizations)}");
            }
        }
        
        return wasOptimized;
    }
    
    /// <summary>
    /// Optimizes quality settings.
    /// </summary>
    private bool OptimizeQualitySettings()
    {
        int currentQuality = QualitySettings.GetQualityLevel();
        
        if (currentQuality > 0)
        {
            QualitySettings.SetQualityLevel(currentQuality - 1);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Optimizes LOD settings.
    /// </summary>
    private bool OptimizeLODSettings()
    {
        // Simulate LOD optimization
        return true;
    }
    
    /// <summary>
    /// Optimizes texture settings.
    /// </summary>
    private bool OptimizeTextureSettings()
    {
        // Simulate texture optimization
        return true;
    }
    
    /// <summary>
    /// Optimizes object pooling.
    /// </summary>
    private bool OptimizeObjectPooling()
    {
        // Simulate object pooling optimization
        return true;
    }
    
    /// <summary>
    /// Calculates performance gain.
    /// </summary>
    private float CalculatePerformanceGain()
    {
        // Simulate performance gain calculation
        return UnityEngine.Random.Range(5f, 25f);
    }
    
    /// <summary>
    /// Performs garbage collection.
    /// </summary>
    private void PerformGarbageCollection()
    {
        System.GC.Collect();
        
        if (logPerformanceEvents)
        {
            Debug.Log("Garbage collection performed");
        }
    }
    
    /// <summary>
    /// Checks memory usage.
    /// </summary>
    private void CheckMemoryUsage()
    {
        float memoryUsage = SystemInfo.systemMemorySize;
        float memoryThreshold = this.memoryThreshold * SystemInfo.systemMemorySize;
        
        if (memoryUsage > memoryThreshold)
        {
            if (enableAssetUnloading)
            {
                // Simulate asset unloading
                if (logPerformanceEvents)
                {
                    Debug.Log("Memory threshold exceeded - unloading assets");
                }
            }
        }
    }
    
    /// <summary>
    /// Generates a performance report.
    /// </summary>
    private PerformanceReport GeneratePerformanceReport()
    {
        PerformanceReport report = new PerformanceReport
        {
            id = GenerateReportId(),
            timestamp = Time.time,
            averageFPS = activeMetrics.ContainsKey("FPS") ? activeMetrics["FPS"].averageValue : 0f,
            memoryUsage = activeMetrics.ContainsKey("Memory") ? activeMetrics["Memory"].currentValue : 0f,
            cpuUsage = activeMetrics.ContainsKey("CPU") ? activeMetrics["CPU"].currentValue : 0f,
            activeObjects = (int)(activeMetrics.ContainsKey("Objects") ? activeMetrics["Objects"].currentValue : 0f),
            drawCalls = (int)(activeMetrics.ContainsKey("DrawCalls") ? activeMetrics["DrawCalls"].currentValue : 0f),
            isOptimized = false,
            metrics = new List<PerformanceMetric>(activeMetrics.Values),
            optimizationResult = new OptimizationResult
            {
                wasOptimized = false,
                performanceGain = 0f,
                optimizationType = "None",
                timestamp = Time.time,
                appliedOptimizations = new List<string>()
            }
        };
        
        return report;
    }
    
    /// <summary>
    /// Applies a performance profile.
    /// </summary>
    public bool ApplyPerformanceProfile(string profileId)
    {
        if (!profileDatabase.ContainsKey(profileId)) return false;
        
        PerformanceProfile profile = profileDatabase[profileId];
        
        // Apply quality settings
        if (profile.qualitySettings != null)
        {
            QualitySettings.SetQualityLevel(profile.qualitySettings.qualityLevel);
        }
        
        // Apply optimization settings
        if (profile.optimizationSettings != null)
        {
            // Apply optimization settings
        }
        
        if (logPerformanceEvents)
        {
            Debug.Log($"Applied performance profile: {profile.name}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets a performance metric.
    /// </summary>
    public PerformanceMetric GetMetric(string metricId)
    {
        return activeMetrics.ContainsKey(metricId) ? activeMetrics[metricId] : null;
    }
    
    /// <summary>
    /// Gets all performance metrics.
    /// </summary>
    public Dictionary<string, PerformanceMetric> GetAllMetrics()
    {
        return new Dictionary<string, PerformanceMetric>(activeMetrics);
    }
    
    /// <summary>
    /// Gets performance reports.
    /// </summary>
    public List<PerformanceReport> GetPerformanceReports()
    {
        return new List<PerformanceReport>(performanceReports);
    }
    
    /// <summary>
    /// Sets the monitoring interval.
    /// </summary>
    public void SetMonitoringInterval(float interval)
    {
        monitoringInterval = Mathf.Clamp(interval, 0.1f, 10f);
    }
    
    /// <summary>
    /// Sets the alert threshold.
    /// </summary>
    public void SetAlertThreshold(float threshold)
    {
        alertThreshold = Mathf.Clamp(threshold, 0.1f, 1f);
    }
    
    /// <summary>
    /// Enables or disables auto-optimization.
    /// </summary>
    public void SetAutoOptimizationEnabled(bool enabled)
    {
        enableAutoOptimization = enabled;
    }
    
    /// <summary>
    /// Generates a unique report ID.
    /// </summary>
    private string GenerateReportId()
    {
        return $"rep_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
    }
    
    /// <summary>
    /// Logs the current performance manager status.
    /// </summary>
    public void LogPerformanceStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Performance Manager Status ===");
        Debug.Log($"Active Metrics: {activeMetrics.Count}");
        Debug.Log($"Performance Reports: {performanceReports.Count}");
        Debug.Log($"Is Performance Monitoring: {isPerformanceMonitoring}");
        Debug.Log($"Profile Database Size: {profileDatabase.Count}");
        Debug.Log($"Performance Monitoring: {(enablePerformanceMonitoring ? "Enabled" : "Disabled")}");
        Debug.Log($"Performance Optimization: {(enablePerformanceOptimization ? "Enabled" : "Disabled")}");
        Debug.Log($"Resource Management: {(enableResourceManagement ? "Enabled" : "Disabled")}");
        Debug.Log($"Performance Profiling: {(enablePerformanceProfiling ? "Enabled" : "Disabled")}");
        Debug.Log($"Auto Optimization: {(enableAutoOptimization ? "Enabled" : "Disabled")}");
        Debug.Log("==================================");
    }
} 