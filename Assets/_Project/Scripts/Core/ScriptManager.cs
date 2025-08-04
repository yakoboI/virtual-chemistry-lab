using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple script manager to handle basic Unity script initialization and management.
/// This is a minimal implementation to get the project running.
/// </summary>
public class ScriptManager : MonoBehaviour
{
    [Header("Script Management")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool autoInitializeComponents = true;
    
    [Header("Component References")]
    [SerializeField] private List<MonoBehaviour> managedScripts = new List<MonoBehaviour>();
    
    private static ScriptManager instance;
    public static ScriptManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ScriptManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ScriptManager");
                    instance = go.AddComponent<ScriptManager>();
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
            InitializeScriptManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (autoInitializeComponents)
        {
            InitializeAllComponents();
        }
    }
    
    /// <summary>
    /// Initializes the script manager with basic settings.
    /// </summary>
    private void InitializeScriptManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("ScriptManager initialized successfully");
        }
        
        // Set up basic Unity settings
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
    
    /// <summary>
    /// Initializes all managed script components.
    /// </summary>
    private void InitializeAllComponents()
    {
        foreach (MonoBehaviour script in managedScripts)
        {
            if (script != null)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"Initializing component: {script.GetType().Name}");
                }
                
                // Enable the script if it's disabled
                if (!script.enabled)
                {
                    script.enabled = true;
                }
            }
        }
    }
    
    /// <summary>
    /// Adds a script to the managed list.
    /// </summary>
    public void AddManagedScript(MonoBehaviour script)
    {
        if (script != null && !managedScripts.Contains(script))
        {
            managedScripts.Add(script);
            if (enableDebugLogging)
            {
                Debug.Log($"Added script to management: {script.GetType().Name}");
            }
        }
    }
    
    /// <summary>
    /// Removes a script from the managed list.
    /// </summary>
    public void RemoveManagedScript(MonoBehaviour script)
    {
        if (managedScripts.Contains(script))
        {
            managedScripts.Remove(script);
            if (enableDebugLogging)
            {
                Debug.Log($"Removed script from management: {script.GetType().Name}");
            }
        }
    }
    
    /// <summary>
    /// Gets all managed scripts of a specific type.
    /// </summary>
    public List<T> GetManagedScripts<T>() where T : MonoBehaviour
    {
        List<T> scripts = new List<T>();
        foreach (MonoBehaviour script in managedScripts)
        {
            if (script is T typedScript)
            {
                scripts.Add(typedScript);
            }
        }
        return scripts;
    }
    
    /// <summary>
    /// Enables or disables all managed scripts.
    /// </summary>
    public void SetAllScriptsEnabled(bool enabled)
    {
        foreach (MonoBehaviour script in managedScripts)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Set all managed scripts to: {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Logs the current status of all managed scripts.
    /// </summary>
    public void LogScriptStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Script Manager Status ===");
        Debug.Log($"Total managed scripts: {managedScripts.Count}");
        
        foreach (MonoBehaviour script in managedScripts)
        {
            if (script != null)
            {
                Debug.Log($"- {script.GetType().Name}: {(script.enabled ? "Enabled" : "Disabled")}");
            }
        }
        Debug.Log("=============================");
    }
} 