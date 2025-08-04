using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages central UI coordination, panel management, and interface control.
/// This component handles all UI operations and panel states in the virtual chemistry lab.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Management")]
    [SerializeField] private bool enableUIManagement = true;
    [SerializeField] private bool enablePanelStacking = true;
    [SerializeField] private bool enableAutoHide = true;
    [SerializeField] private bool enablePanelAnimations = true;
    [SerializeField] private float animationDuration = 0.3f;
    
    [Header("Panel Configuration")]
    [SerializeField] private UIPanel[] uiPanels;
    [SerializeField] private string defaultPanel = "MainMenu";
    [SerializeField] private string[] alwaysVisiblePanels = { "HUD", "Loading" };
    [SerializeField] private string[] modalPanels = { "Settings", "Help", "Pause" };
    
    [Header("UI State")]
    [SerializeField] private string currentPanel = "";
    [SerializeField] private string previousPanel = "";
    [SerializeField] private bool isTransitioning = false;
    [SerializeField] private bool isModalOpen = false;
    
    [Header("Panel Stack")]
    [SerializeField] private Stack<string> panelStack = new Stack<string>();
    [SerializeField] private int maxStackSize = 10;
    [SerializeField] private bool enableStackHistory = true;
    
    [Header("UI Effects")]
    [SerializeField] private bool enableFadeEffects = true;
    [SerializeField] private bool enableSlideEffects = true;
    [SerializeField] private bool enableScaleEffects = false;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Performance")]
    [SerializeField] private bool enablePanelPooling = true;
    [SerializeField] private bool enableLazyLoading = true;
    [SerializeField] private bool enablePanelCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showPanelInfo = false;
    [SerializeField] private bool logPanelChanges = false;
    
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    instance = go.AddComponent<UIManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnPanelShown;
    public event Action<string> OnPanelHidden;
    public event Action<string> OnPanelTransitionStarted;
    public event Action<string> OnPanelTransitionCompleted;
    public event Action<string> OnModalOpened;
    public event Action<string> OnModalClosed;
    public event Action<string> OnPanelError;
    public event Action<bool> OnUIVisibleChanged;
    
    // Private variables
    private Dictionary<string, UIPanel> panelRegistry = new Dictionary<string, UIPanel>();
    private Dictionary<string, GameObject> panelObjects = new Dictionary<string, GameObject>();
    private List<string> visiblePanels = new List<string>();
    private List<string> modalPanelsList = new List<string>();
    private bool isInitialized = false;
    private Coroutine transitionCoroutine;
    
    [System.Serializable]
    public class UIPanel
    {
        public string panelName;
        public GameObject panelObject;
        public bool isModal;
        public bool isAlwaysVisible;
        public bool isPreloaded;
        public UIPanelType panelType;
        public AnimationType animationType;
        public Vector2 slideDirection = Vector2.right;
        public float animationSpeed = 1f;
    }
    
    [System.Serializable]
    public enum UIPanelType
    {
        MainMenu,
        ExperimentSelection,
        ExperimentActive,
        ExperimentComplete,
        Settings,
        Help,
        Pause,
        Loading,
        HUD,
        Safety,
        Results,
        Tutorial
    }
    
    [System.Serializable]
    public enum AnimationType
    {
        None,
        Fade,
        Slide,
        Scale,
        SlideAndFade
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUIManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        RegisterPanels();
        SetupInitialState();
    }
    
    private void Update()
    {
        HandleInput();
        UpdatePanelInfo();
    }
    
    /// <summary>
    /// Initializes the UI manager.
    /// </summary>
    private void InitializeUIManager()
    {
        panelStack.Clear();
        visiblePanels.Clear();
        modalPanelsList.Clear();
        
        // Initialize modal panels list
        foreach (string modalPanel in modalPanels)
        {
            modalPanelsList.Add(modalPanel);
        }
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("UIManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Registers all UI panels.
    /// </summary>
    private void RegisterPanels()
    {
        panelRegistry.Clear();
        panelObjects.Clear();
        
        foreach (UIPanel panel in uiPanels)
        {
            if (panel != null && !string.IsNullOrEmpty(panel.panelName))
            {
                panelRegistry[panel.panelName] = panel;
                
                if (panel.panelObject != null)
                {
                    panelObjects[panel.panelName] = panel.panelObject;
                    
                    // Set initial state
                    if (panel.isAlwaysVisible)
                    {
                        ShowPanel(panel.panelName, false);
                    }
                    else
                    {
                        HidePanel(panel.panelName, false);
                    }
                }
                
                if (panel.isModal)
                {
                    modalPanelsList.Add(panel.panelName);
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Registered {panelRegistry.Count} UI panels");
        }
    }
    
    /// <summary>
    /// Sets up the initial UI state.
    /// </summary>
    private void SetupInitialState()
    {
        if (!string.IsNullOrEmpty(defaultPanel) && panelRegistry.ContainsKey(defaultPanel))
        {
            ShowPanel(defaultPanel);
        }
        
        // Show always visible panels
        foreach (string panelName in alwaysVisiblePanels)
        {
            if (panelRegistry.ContainsKey(panelName))
            {
                ShowPanel(panelName, false);
            }
        }
    }
    
    /// <summary>
    /// Shows a UI panel.
    /// </summary>
    public void ShowPanel(string panelName, bool useAnimation = true)
    {
        if (!enableUIManagement || string.IsNullOrEmpty(panelName)) return;
        
        if (!panelRegistry.ContainsKey(panelName))
        {
            OnPanelError?.Invoke($"Panel not found: {panelName}");
            if (enableDebugLogging)
            {
                Debug.LogError($"Panel not found: {panelName}");
            }
            return;
        }
        
        if (isTransitioning)
        {
            // Queue the show request
            StartCoroutine(QueueShowPanel(panelName, useAnimation));
            return;
        }
        
        StartCoroutine(ShowPanelCoroutine(panelName, useAnimation));
    }
    
    /// <summary>
    /// Hides a UI panel.
    /// </summary>
    public void HidePanel(string panelName, bool useAnimation = true)
    {
        if (!enableUIManagement || string.IsNullOrEmpty(panelName)) return;
        
        if (!panelRegistry.ContainsKey(panelName))
        {
            OnPanelError?.Invoke($"Panel not found: {panelName}");
            return;
        }
        
        if (isTransitioning)
        {
            // Queue the hide request
            StartCoroutine(QueueHidePanel(panelName, useAnimation));
            return;
        }
        
        StartCoroutine(HidePanelCoroutine(panelName, useAnimation));
    }
    
    /// <summary>
    /// Shows a modal panel.
    /// </summary>
    public void ShowModal(string panelName, bool useAnimation = true)
    {
        if (isModalOpen)
        {
            // Hide current modal first
            HideCurrentModal();
        }
        
        ShowPanel(panelName, useAnimation);
        isModalOpen = true;
        OnModalOpened?.Invoke(panelName);
        
        if (logPanelChanges)
        {
            Debug.Log($"Modal opened: {panelName}");
        }
    }
    
    /// <summary>
    /// Hides the current modal panel.
    /// </summary>
    public void HideCurrentModal()
    {
        if (!isModalOpen) return;
        
        foreach (string modalPanel in modalPanelsList)
        {
            if (visiblePanels.Contains(modalPanel))
            {
                HidePanel(modalPanel);
                isModalOpen = false;
                OnModalClosed?.Invoke(modalPanel);
                
                if (logPanelChanges)
                {
                    Debug.Log($"Modal closed: {modalPanel}");
                }
                break;
            }
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
        ShowPanel("ExperimentComplete");
    }
    
    /// <summary>
    /// Shows the settings panel.
    /// </summary>
    public void ShowSettings()
    {
        ShowModal("Settings");
    }
    
    /// <summary>
    /// Shows the help panel.
    /// </summary>
    public void ShowHelp()
    {
        ShowModal("Help");
    }
    
    /// <summary>
    /// Shows the pause panel.
    /// </summary>
    public void ShowPausePanel()
    {
        ShowModal("Pause");
    }
    
    /// <summary>
    /// Shows the safety warning panel.
    /// </summary>
    public void ShowSafetyWarning()
    {
        ShowModal("Safety");
    }
    
    /// <summary>
    /// Shows the loading panel.
    /// </summary>
    public void ShowLoading()
    {
        ShowPanel("Loading", false);
    }
    
    /// <summary>
    /// Hides the loading panel.
    /// </summary>
    public void HideLoading()
    {
        HidePanel("Loading", false);
    }
    
    /// <summary>
    /// Goes back to the previous panel.
    /// </summary>
    public void GoBack()
    {
        if (panelStack.Count > 1)
        {
            string currentPanel = panelStack.Pop();
            string previousPanel = panelStack.Peek();
            
            HidePanel(currentPanel);
            ShowPanel(previousPanel);
        }
    }
    
    /// <summary>
    /// Clears all panels except always visible ones.
    /// </summary>
    public void ClearAllPanels()
    {
        List<string> panelsToHide = new List<string>(visiblePanels);
        
        foreach (string panelName in panelsToHide)
        {
            UIPanel panel = panelRegistry[panelName];
            if (!panel.isAlwaysVisible)
            {
                HidePanel(panelName, false);
            }
        }
    }
    
    /// <summary>
    /// Coroutine for showing a panel.
    /// </summary>
    private System.Collections.IEnumerator ShowPanelCoroutine(string panelName, bool useAnimation)
    {
        if (isTransitioning) yield break;
        
        isTransitioning = true;
        previousPanel = currentPanel;
        
        OnPanelTransitionStarted?.Invoke(panelName);
        
        if (logPanelChanges)
        {
            Debug.Log($"Showing panel: {panelName}");
        }
        
        UIPanel panel = panelRegistry[panelName];
        GameObject panelObj = panel.panelObject;
        
        if (panelObj != null)
        {
            // Set active
            panelObj.SetActive(true);
            
            // Get canvas group for animations
            CanvasGroup canvasGroup = panelObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panelObj.AddComponent<CanvasGroup>();
            }
            
            // Apply animation
            if (useAnimation && enablePanelAnimations)
            {
                yield return StartCoroutine(AnimatePanelIn(panelObj, canvasGroup, panel));
            }
            else
            {
                // Set final state immediately
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            // Update state
            if (!visiblePanels.Contains(panelName))
            {
                visiblePanels.Add(panelName);
            }
            
            currentPanel = panelName;
            
            // Add to stack
            if (enablePanelStacking && !panel.isAlwaysVisible)
            {
                AddToPanelStack(panelName);
            }
        }
        
        isTransitioning = false;
        
        OnPanelShown?.Invoke(panelName);
        OnPanelTransitionCompleted?.Invoke(panelName);
        
        if (logPanelChanges)
        {
            Debug.Log($"Panel shown: {panelName}");
        }
    }
    
    /// <summary>
    /// Coroutine for hiding a panel.
    /// </summary>
    private System.Collections.IEnumerator HidePanelCoroutine(string panelName, bool useAnimation)
    {
        if (isTransitioning) yield break;
        
        isTransitioning = true;
        
        OnPanelTransitionStarted?.Invoke(panelName);
        
        if (logPanelChanges)
        {
            Debug.Log($"Hiding panel: {panelName}");
        }
        
        UIPanel panel = panelRegistry[panelName];
        GameObject panelObj = panel.panelObject;
        
        if (panelObj != null)
        {
            CanvasGroup canvasGroup = panelObj.GetComponent<CanvasGroup>();
            
            // Apply animation
            if (useAnimation && enablePanelAnimations)
            {
                yield return StartCoroutine(AnimatePanelOut(panelObj, canvasGroup, panel));
            }
            
            // Set inactive
            panelObj.SetActive(false);
            
            // Update state
            visiblePanels.Remove(panelName);
            
            // Remove from stack
            if (enablePanelStacking)
            {
                RemoveFromPanelStack(panelName);
            }
        }
        
        isTransitioning = false;
        
        OnPanelHidden?.Invoke(panelName);
        OnPanelTransitionCompleted?.Invoke(panelName);
        
        if (logPanelChanges)
        {
            Debug.Log($"Panel hidden: {panelName}");
        }
    }
    
    /// <summary>
    /// Animates a panel in.
    /// </summary>
    private System.Collections.IEnumerator AnimatePanelIn(GameObject panelObj, CanvasGroup canvasGroup, UIPanel panel)
    {
        float duration = animationDuration * panel.animationSpeed;
        float elapsed = 0f;
        
        // Set initial state
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // Set initial transform based on animation type
        SetInitialTransform(panelObj, panel.animationType, true);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = animationCurve.Evaluate(progress);
            
            // Animate based on type
            switch (panel.animationType)
            {
                case AnimationType.Fade:
                    canvasGroup.alpha = curveValue;
                    break;
                case AnimationType.Slide:
                    AnimateSlide(panelObj, curveValue, true);
                    canvasGroup.alpha = curveValue;
                    break;
                case AnimationType.Scale:
                    AnimateScale(panelObj, curveValue, true);
                    canvasGroup.alpha = curveValue;
                    break;
                case AnimationType.SlideAndFade:
                    AnimateSlide(panelObj, curveValue, true);
                    canvasGroup.alpha = curveValue;
                    break;
            }
            
            yield return null;
        }
        
        // Set final state
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        ResetTransform(panelObj);
    }
    
    /// <summary>
    /// Animates a panel out.
    /// </summary>
    private System.Collections.IEnumerator AnimatePanelOut(GameObject panelObj, CanvasGroup canvasGroup, UIPanel panel)
    {
        float duration = animationDuration * panel.animationSpeed;
        float elapsed = 0f;
        
        // Set initial transform based on animation type
        SetInitialTransform(panelObj, panel.animationType, false);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = animationCurve.Evaluate(1f - progress);
            
            // Animate based on type
            switch (panel.animationType)
            {
                case AnimationType.Fade:
                    canvasGroup.alpha = curveValue;
                    break;
                case AnimationType.Slide:
                    AnimateSlide(panelObj, curveValue, false);
                    canvasGroup.alpha = curveValue;
                    break;
                case AnimationType.Scale:
                    AnimateScale(panelObj, curveValue, false);
                    canvasGroup.alpha = curveValue;
                    break;
                case AnimationType.SlideAndFade:
                    AnimateSlide(panelObj, curveValue, false);
                    canvasGroup.alpha = curveValue;
                    break;
            }
            
            yield return null;
        }
        
        // Set final state
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// Sets the initial transform for animations.
    /// </summary>
    private void SetInitialTransform(GameObject panelObj, AnimationType animationType, bool isIn)
    {
        RectTransform rectTransform = panelObj.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        switch (animationType)
        {
            case AnimationType.Slide:
            case AnimationType.SlideAndFade:
                if (isIn)
                {
                    rectTransform.anchoredPosition = rectTransform.anchoredPosition + (Vector2)(rectTransform.right * 1000f);
                }
                break;
            case AnimationType.Scale:
                if (isIn)
                {
                    rectTransform.localScale = Vector3.zero;
                }
                break;
        }
    }
    
    /// <summary>
    /// Animates slide movement.
    /// </summary>
    private void AnimateSlide(GameObject panelObj, float progress, bool isIn)
    {
        RectTransform rectTransform = panelObj.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        Vector2 targetPosition = Vector2.zero;
        Vector2 startPosition = rectTransform.anchoredPosition + (Vector2)(rectTransform.right * 1000f);
        
        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, progress);
    }
    
    /// <summary>
    /// Animates scale.
    /// </summary>
    private void AnimateScale(GameObject panelObj, float progress, bool isIn)
    {
        RectTransform rectTransform = panelObj.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        Vector3 targetScale = Vector3.one;
        Vector3 startScale = Vector3.zero;
        
        rectTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
    }
    
    /// <summary>
    /// Resets transform to default.
    /// </summary>
    private void ResetTransform(GameObject panelObj)
    {
        RectTransform rectTransform = panelObj.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// Adds a panel to the stack.
    /// </summary>
    private void AddToPanelStack(string panelName)
    {
        if (panelStack.Count >= maxStackSize)
        {
            panelStack.Dequeue(); // Remove oldest
        }
        
        panelStack.Push(panelName);
    }
    
    /// <summary>
    /// Removes a panel from the stack.
    /// </summary>
    private void RemoveFromPanelStack(string panelName)
    {
        // Create a temporary stack to rebuild without the removed panel
        Stack<string> tempStack = new Stack<string>();
        
        while (panelStack.Count > 0)
        {
            string panel = panelStack.Pop();
            if (panel != panelName)
            {
                tempStack.Push(panel);
            }
        }
        
        // Rebuild the original stack
        while (tempStack.Count > 0)
        {
            panelStack.Push(tempStack.Pop());
        }
    }
    
    /// <summary>
    /// Handles input for UI navigation.
    /// </summary>
    private void HandleInput()
    {
        // Handle back button (Escape key)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isModalOpen)
            {
                HideCurrentModal();
            }
            else if (panelStack.Count > 1)
            {
                GoBack();
            }
        }
    }
    
    /// <summary>
    /// Updates panel information display.
    /// </summary>
    private void UpdatePanelInfo()
    {
        if (showPanelInfo)
        {
            Debug.Log($"Current Panel: {currentPanel} | Visible: {visiblePanels.Count} | Modal: {isModalOpen}");
        }
    }
    
    /// <summary>
    /// Queues a show panel request.
    /// </summary>
    private System.Collections.IEnumerator QueueShowPanel(string panelName, bool useAnimation)
    {
        yield return new WaitUntil(() => !isTransitioning);
        ShowPanel(panelName, useAnimation);
    }
    
    /// <summary>
    /// Queues a hide panel request.
    /// </summary>
    private System.Collections.IEnumerator QueueHidePanel(string panelName, bool useAnimation)
    {
        yield return new WaitUntil(() => !isTransitioning);
        HidePanel(panelName, useAnimation);
    }
    
    // Public getters and setters
    public string GetCurrentPanel() => currentPanel;
    public string GetPreviousPanel() => previousPanel;
    public bool IsTransitioning() => isTransitioning;
    public bool IsModalOpen() => isModalOpen;
    public List<string> GetVisiblePanels() => new List<string>(visiblePanels);
    public bool IsPanelVisible(string panelName) => visiblePanels.Contains(panelName);
    
    /// <summary>
    /// Gets a panel by name.
    /// </summary>
    public UIPanel GetPanel(string panelName)
    {
        return panelRegistry.ContainsKey(panelName) ? panelRegistry[panelName] : null;
    }
    
    /// <summary>
    /// Sets the animation duration.
    /// </summary>
    public void SetAnimationDuration(float duration)
    {
        animationDuration = Mathf.Clamp(duration, 0.1f, 2f);
    }
    
    /// <summary>
    /// Enables or disables panel animations.
    /// </summary>
    public void SetPanelAnimationsEnabled(bool enabled)
    {
        enablePanelAnimations = enabled;
    }
    
    /// <summary>
    /// Logs the current UI manager status.
    /// </summary>
    public void LogUIStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== UI Manager Status ===");
        Debug.Log($"Current Panel: {currentPanel}");
        Debug.Log($"Previous Panel: {previousPanel}");
        Debug.Log($"Is Transitioning: {isTransitioning}");
        Debug.Log($"Is Modal Open: {isModalOpen}");
        Debug.Log($"Visible Panels: {visiblePanels.Count}");
        Debug.Log($"Panel Stack Size: {panelStack.Count}");
        Debug.Log($"Registered Panels: {panelRegistry.Count}");
        Debug.Log($"Panel Animations: {(enablePanelAnimations ? "Enabled" : "Disabled")}");
        Debug.Log($"Animation Duration: {animationDuration}");
        Debug.Log("=========================");
    }
} 