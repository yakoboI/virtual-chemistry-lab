using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Simple UI panel manager to handle basic UI panel visibility and transitions.
/// This is a minimal implementation for the virtual chemistry lab.
/// </summary>
public class UIPanelManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject experimentSelectionPanel;
    [SerializeField] private GameObject experimentActivePanel;
    [SerializeField] private GameObject experimentResultsPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject safetyWarningPanel;
    [SerializeField] private GameObject loadingPanel;
    
    [Header("Panel Settings")]
    [SerializeField] private bool enablePanelTransitions = true;
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private bool hideAllOnStart = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private GameObject currentActivePanel;
    private Dictionary<string, GameObject> panelLookup = new Dictionary<string, GameObject>();
    
    private static UIPanelManager instance;
    public static UIPanelManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIPanelManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UIPanelManager");
                    instance = go.AddComponent<UIPanelManager>();
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
            InitializeUIPanelManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializePanels();
        
        if (hideAllOnStart)
        {
            HideAllPanels();
        }
    }
    
    /// <summary>
    /// Initializes the UI panel manager with basic settings.
    /// </summary>
    private void InitializeUIPanelManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("UIPanelManager initialized successfully");
        }
        
        // Initialize panel lookup dictionary
        panelLookup = new Dictionary<string, GameObject>();
    }
    
    /// <summary>
    /// Initializes all UI panels and sets up the lookup dictionary.
    /// </summary>
    private void InitializePanels()
    {
        // Add panels to lookup dictionary
        if (mainMenuPanel != null)
        {
            panelLookup["MainMenu"] = mainMenuPanel;
        }
        
        if (experimentSelectionPanel != null)
        {
            panelLookup["ExperimentSelection"] = experimentSelectionPanel;
        }
        
        if (experimentActivePanel != null)
        {
            panelLookup["ExperimentActive"] = experimentActivePanel;
        }
        
        if (experimentResultsPanel != null)
        {
            panelLookup["ExperimentResults"] = experimentResultsPanel;
        }
        
        if (pausePanel != null)
        {
            panelLookup["Pause"] = pausePanel;
        }
        
        if (safetyWarningPanel != null)
        {
            panelLookup["SafetyWarning"] = safetyWarningPanel;
        }
        
        if (loadingPanel != null)
        {
            panelLookup["Loading"] = loadingPanel;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized {panelLookup.Count} UI panels");
        }
    }
    
    /// <summary>
    /// Shows the main menu panel.
    /// </summary>
    public void ShowMainMenu()
    {
        ShowPanel("MainMenu");
    }
    
    /// <summary>
    /// Shows the experiment selection panel.
    /// </summary>
    public void ShowExperimentSelection()
    {
        ShowPanel("ExperimentSelection");
    }
    
    /// <summary>
    /// Shows the experiment active panel.
    /// </summary>
    public void ShowExperimentActive()
    {
        ShowPanel("ExperimentActive");
    }
    
    /// <summary>
    /// Shows the experiment results panel.
    /// </summary>
    public void ShowExperimentResults()
    {
        ShowPanel("ExperimentResults");
    }
    
    /// <summary>
    /// Shows the pause panel.
    /// </summary>
    public void ShowPausePanel()
    {
        ShowPanel("Pause");
    }
    
    /// <summary>
    /// Shows the safety warning panel.
    /// </summary>
    public void ShowSafetyWarning()
    {
        ShowPanel("SafetyWarning");
    }
    
    /// <summary>
    /// Shows the loading panel.
    /// </summary>
    public void ShowLoadingPanel()
    {
        ShowPanel("Loading");
    }
    
    /// <summary>
    /// Shows a specific panel by name.
    /// </summary>
    public void ShowPanel(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
        {
            Debug.LogError("Panel name is null or empty");
            return;
        }
        
        if (!panelLookup.ContainsKey(panelName))
        {
            Debug.LogError($"Panel not found: {panelName}");
            return;
        }
        
        GameObject panel = panelLookup[panelName];
        if (panel == null)
        {
            Debug.LogError($"Panel GameObject is null: {panelName}");
            return;
        }
        
        // Hide current panel if there is one
        if (currentActivePanel != null && currentActivePanel != panel)
        {
            HidePanel(currentActivePanel);
        }
        
        // Show the new panel
        panel.SetActive(true);
        currentActivePanel = panel;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Showed panel: {panelName}");
        }
        
        // Apply transition if enabled
        if (enablePanelTransitions)
        {
            ApplyPanelTransition(panel, true);
        }
    }
    
    /// <summary>
    /// Hides a specific panel by name.
    /// </summary>
    public void HidePanel(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
        {
            Debug.LogError("Panel name is null or empty");
            return;
        }
        
        if (!panelLookup.ContainsKey(panelName))
        {
            Debug.LogError($"Panel not found: {panelName}");
            return;
        }
        
        GameObject panel = panelLookup[panelName];
        if (panel != null)
        {
            HidePanel(panel);
        }
    }
    
    /// <summary>
    /// Hides a specific panel GameObject.
    /// </summary>
    public void HidePanel(GameObject panel)
    {
        if (panel == null) return;
        
        if (enablePanelTransitions)
        {
            ApplyPanelTransition(panel, false);
        }
        else
        {
            panel.SetActive(false);
        }
        
        if (currentActivePanel == panel)
        {
            currentActivePanel = null;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Hid panel: {panel.name}");
        }
    }
    
    /// <summary>
    /// Hides all panels.
    /// </summary>
    public void HideAllPanels()
    {
        foreach (GameObject panel in panelLookup.Values)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        
        currentActivePanel = null;
        
        if (enableDebugLogging)
        {
            Debug.Log("All panels hidden");
        }
    }
    
    /// <summary>
    /// Applies a transition effect to a panel.
    /// </summary>
    private void ApplyPanelTransition(GameObject panel, bool show)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        if (show)
        {
            // Fade in
            canvasGroup.alpha = 0f;
            panel.SetActive(true);
            StartCoroutine(FadePanel(canvasGroup, 0f, 1f, transitionDuration));
        }
        else
        {
            // Fade out
            StartCoroutine(FadePanel(canvasGroup, 1f, 0f, transitionDuration, () => {
                panel.SetActive(false);
            }));
        }
    }
    
    /// <summary>
    /// Coroutine for fading a panel.
    /// </summary>
    private System.Collections.IEnumerator FadePanel(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Gets the currently active panel.
    /// </summary>
    public GameObject GetCurrentActivePanel()
    {
        return currentActivePanel;
    }
    
    /// <summary>
    /// Gets the name of the currently active panel.
    /// </summary>
    public string GetCurrentActivePanelName()
    {
        if (currentActivePanel == null) return "None";
        
        foreach (var kvp in panelLookup)
        {
            if (kvp.Value == currentActivePanel)
            {
                return kvp.Key;
            }
        }
        
        return "Unknown";
    }
    
    /// <summary>
    /// Checks if a panel is currently visible.
    /// </summary>
    public bool IsPanelVisible(string panelName)
    {
        if (!panelLookup.ContainsKey(panelName)) return false;
        
        GameObject panel = panelLookup[panelName];
        return panel != null && panel.activeInHierarchy;
    }
    
    /// <summary>
    /// Gets all available panel names.
    /// </summary>
    public List<string> GetAvailablePanelNames()
    {
        return new List<string>(panelLookup.Keys);
    }
    
    /// <summary>
    /// Sets the transition duration.
    /// </summary>
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0f, duration);
    }
    
    /// <summary>
    /// Enables or disables panel transitions.
    /// </summary>
    public void SetPanelTransitionsEnabled(bool enabled)
    {
        enablePanelTransitions = enabled;
    }
    
    /// <summary>
    /// Logs the current UI panel status.
    /// </summary>
    public void LogPanelStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== UI Panel Manager Status ===");
        Debug.Log($"Current Active Panel: {GetCurrentActivePanelName()}");
        Debug.Log($"Panel Transitions: {(enablePanelTransitions ? "Enabled" : "Disabled")}");
        Debug.Log($"Transition Duration: {transitionDuration}s");
        Debug.Log($"Total Panels: {panelLookup.Count}");
        
        foreach (var kvp in panelLookup)
        {
            bool isVisible = kvp.Value != null && kvp.Value.activeInHierarchy;
            Debug.Log($"- {kvp.Key}: {(isVisible ? "Visible" : "Hidden")}");
        }
        Debug.Log("===============================");
    }
} 