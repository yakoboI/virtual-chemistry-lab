using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages UI button interaction and event handling for the virtual chemistry lab.
/// This component handles all button-related operations and user interface interactions.
/// </summary>
public class ButtonHandler : MonoBehaviour
{
    [Header("Button Management")]
    [SerializeField] private bool enableButtonHandling = true;
    [SerializeField] private bool enableButtonAnimations = true;
    [SerializeField] private bool enableButtonSounds = true;
    [SerializeField] private bool enableButtonHoverEffects = true;
    [SerializeField] private bool enableButtonCooldowns = true;
    
    [Header("Button Configuration")]
    [SerializeField] private UIButton[] uiButtons;
    [SerializeField] private string defaultButtonSound = "button_click";
    [SerializeField] private string defaultHoverSound = "button_hover";
    [SerializeField] private float buttonCooldownTime = 0.5f;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;
    
    [Header("Button State")]
    [SerializeField] private Dictionary<string, bool> buttonStates = new Dictionary<string, bool>();
    [SerializeField] private Dictionary<string, float> buttonCooldowns = new Dictionary<string, float>();
    [SerializeField] private string lastClickedButton = "";
    [SerializeField] private bool isProcessingClick = false;
    
    [Header("Performance")]
    [SerializeField] private bool enableButtonPooling = true;
    [SerializeField] private int buttonPoolSize = 20;
    [SerializeField] private bool enableButtonCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showButtonInfo = false;
    [SerializeField] private bool logButtonClicks = false;
    
    private static ButtonHandler instance;
    public static ButtonHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ButtonHandler>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ButtonHandler");
                    instance = go.AddComponent<ButtonHandler>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnButtonClicked;
    public event Action<string> OnButtonHovered;
    public event Action<string> OnButtonPressed;
    public event Action<string> OnButtonReleased;
    public event Action<string> OnButtonEnabled;
    public event Action<string> OnButtonDisabled;
    public event Action<string> OnButtonError;
    
    // Private variables
    private Dictionary<string, UIButton> buttonRegistry = new Dictionary<string, UIButton>();
    private Dictionary<string, Button> unityButtons = new Dictionary<string, Button>();
    private Queue<Button> buttonPool = new Queue<Button>();
    private bool isInitialized = false;
    private Coroutine animationCoroutine;
    
    [System.Serializable]
    public class UIButton
    {
        public string buttonId;
        public Button button;
        public string buttonText;
        public ButtonType buttonType;
        public string actionName;
        public bool isEnabled = true;
        public bool isVisible = true;
        public float cooldownTime = 0.5f;
        public string clickSound;
        public string hoverSound;
        public Color normalColor = Color.white;
        public Color hoverColor = Color.yellow;
        public Color disabledColor = Color.gray;
        public Vector3 originalScale = Vector3.one;
        public Vector3 originalPosition;
    }
    
    [System.Serializable]
    public enum ButtonType
    {
        MainMenu,
        ExperimentSelection,
        ExperimentControl,
        Settings,
        Help,
        Safety,
        Navigation,
        Action,
        Confirmation,
        Cancel
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeButtonHandler();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        RegisterButtons();
        SetupButtonListeners();
        InitializeButtonPool();
    }
    
    private void Update()
    {
        UpdateButtonCooldowns();
        UpdateButtonInfo();
    }
    
    /// <summary>
    /// Initializes the button handler.
    /// </summary>
    private void InitializeButtonHandler()
    {
        buttonStates.Clear();
        buttonCooldowns.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("ButtonHandler initialized successfully");
        }
    }
    
    /// <summary>
    /// Registers all UI buttons.
    /// </summary>
    private void RegisterButtons()
    {
        buttonRegistry.Clear();
        unityButtons.Clear();
        
        foreach (UIButton uiButton in uiButtons)
        {
            if (uiButton != null && !string.IsNullOrEmpty(uiButton.buttonId))
            {
                buttonRegistry[uiButton.buttonId] = uiButton;
                
                if (uiButton.button != null)
                {
                    unityButtons[uiButton.buttonId] = uiButton.button;
                    uiButton.originalScale = uiButton.button.transform.localScale;
                    uiButton.originalPosition = uiButton.button.transform.localPosition;
                }
                
                // Set initial state
                buttonStates[uiButton.buttonId] = uiButton.isEnabled;
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Registered button: {uiButton.buttonId}");
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Registered {buttonRegistry.Count} buttons");
        }
    }
    
    /// <summary>
    /// Sets up button event listeners.
    /// </summary>
    private void SetupButtonListeners()
    {
        foreach (var kvp in buttonRegistry)
        {
            string buttonId = kvp.Key;
            UIButton uiButton = kvp.Value;
            
            if (uiButton.button != null)
            {
                // Remove existing listeners
                uiButton.button.onClick.RemoveAllListeners();
                
                // Add click listener
                string capturedButtonId = buttonId;
                uiButton.button.onClick.AddListener(() => OnButtonClick(capturedButtonId));
                
                // Add hover listeners if enabled
                if (enableButtonHoverEffects)
                {
                    EventTrigger trigger = uiButton.button.gameObject.GetComponent<EventTrigger>();
                    if (trigger == null)
                    {
                        trigger = uiButton.button.gameObject.AddComponent<EventTrigger>();
                    }
                    
                    // Clear existing triggers
                    trigger.triggers.Clear();
                    
                    // Add pointer enter trigger
                    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                    enterEntry.eventID = EventTriggerType.PointerEnter;
                    enterEntry.callback.AddListener((data) => OnButtonHover(capturedButtonId));
                    trigger.triggers.Add(enterEntry);
                    
                    // Add pointer exit trigger
                    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                    exitEntry.eventID = EventTriggerType.PointerExit;
                    exitEntry.callback.AddListener((data) => OnButtonExit(capturedButtonId));
                    trigger.triggers.Add(exitEntry);
                }
            }
        }
    }
    
    /// <summary>
    /// Initializes the button pool.
    /// </summary>
    private void InitializeButtonPool()
    {
        if (!enableButtonPooling) return;
        
        for (int i = 0; i < buttonPoolSize; i++)
        {
            GameObject buttonObj = new GameObject($"PooledButton_{i}");
            buttonObj.transform.SetParent(transform);
            Button button = buttonObj.AddComponent<Button>();
            button.gameObject.SetActive(false);
            
            buttonPool.Enqueue(button);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized button pool with {buttonPoolSize} buttons");
        }
    }
    
    /// <summary>
    /// Handles button click events.
    /// </summary>
    private void OnButtonClick(string buttonId)
    {
        if (!enableButtonHandling || isProcessingClick) return;
        
        if (!buttonRegistry.ContainsKey(buttonId))
        {
            OnButtonError?.Invoke($"Button not found: {buttonId}");
            return;
        }
        
        UIButton uiButton = buttonRegistry[buttonId];
        
        // Check if button is enabled
        if (!uiButton.isEnabled || !buttonStates[buttonId])
        {
            OnButtonError?.Invoke($"Button disabled: {buttonId}");
            return;
        }
        
        // Check cooldown
        if (enableButtonCooldowns && IsButtonOnCooldown(buttonId))
        {
            OnButtonError?.Invoke($"Button on cooldown: {buttonId}");
            return;
        }
        
        isProcessingClick = true;
        lastClickedButton = buttonId;
        
        // Play click sound
        if (enableButtonSounds)
        {
            string soundName = !string.IsNullOrEmpty(uiButton.clickSound) ? 
                uiButton.clickSound : defaultButtonSound;
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUI(soundName);
            }
        }
        
        // Animate button click
        if (enableButtonAnimations)
        {
            StartCoroutine(AnimateButtonClick(uiButton));
        }
        
        // Set cooldown
        if (enableButtonCooldowns)
        {
            SetButtonCooldown(buttonId, uiButton.cooldownTime);
        }
        
        // Trigger events
        OnButtonClicked?.Invoke(buttonId);
        OnButtonPressed?.Invoke(buttonId);
        
        if (logButtonClicks)
        {
            Debug.Log($"Button clicked: {buttonId}");
        }
        
        // Execute button action
        ExecuteButtonAction(buttonId);
        
        isProcessingClick = false;
    }
    
    /// <summary>
    /// Handles button hover events.
    /// </summary>
    private void OnButtonHover(string buttonId)
    {
        if (!enableButtonHandling || !enableButtonHoverEffects) return;
        
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        
        // Play hover sound
        if (enableButtonSounds)
        {
            string soundName = !string.IsNullOrEmpty(uiButton.hoverSound) ? 
                uiButton.hoverSound : defaultHoverSound;
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUI(soundName);
            }
        }
        
        // Animate hover
        if (enableButtonAnimations)
        {
            StartCoroutine(AnimateButtonHover(uiButton, true));
        }
        
        OnButtonHovered?.Invoke(buttonId);
    }
    
    /// <summary>
    /// Handles button exit events.
    /// </summary>
    private void OnButtonExit(string buttonId)
    {
        if (!enableButtonHandling || !enableButtonHoverEffects) return;
        
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        
        // Animate exit
        if (enableButtonAnimations)
        {
            StartCoroutine(AnimateButtonHover(uiButton, false));
        }
        
        OnButtonReleased?.Invoke(buttonId);
    }
    
    /// <summary>
    /// Executes the button action.
    /// </summary>
    private void ExecuteButtonAction(string buttonId)
    {
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        string actionName = uiButton.actionName;
        
        switch (actionName.ToLower())
        {
            case "start":
                if (SceneStateController.Instance != null)
                {
                    SceneStateController.Instance.LoadExperimentScene();
                }
                break;
                
            case "mainmenu":
                if (SceneStateController.Instance != null)
                {
                    SceneStateController.Instance.LoadMainMenu();
                }
                break;
                
            case "settings":
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowSettings();
                }
                break;
                
            case "help":
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowHelp();
                }
                break;
                
            case "pause":
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowPausePanel();
                }
                break;
                
            case "resume":
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideCurrentModal();
                }
                break;
                
            case "quit":
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
                break;
                
            case "back":
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.GoBack();
                }
                break;
                
            case "reset":
                if (ExperimentStateManager.Instance != null)
                {
                    ExperimentStateManager.Instance.ResetExperimentState();
                }
                break;
                
            case "complete":
                if (ExperimentStateManager.Instance != null)
                {
                    ExperimentStateManager.Instance.CompleteExperiment();
                }
                break;
                
            default:
                // Custom action - can be extended
                if (enableDebugLogging)
                {
                    Debug.Log($"Custom button action: {actionName}");
                }
                break;
        }
    }
    
    /// <summary>
    /// Enables a button.
    /// </summary>
    public void EnableButton(string buttonId)
    {
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        uiButton.isEnabled = true;
        buttonStates[buttonId] = true;
        
        if (uiButton.button != null)
        {
            uiButton.button.interactable = true;
            SetButtonColor(uiButton, uiButton.normalColor);
        }
        
        OnButtonEnabled?.Invoke(buttonId);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Button enabled: {buttonId}");
        }
    }
    
    /// <summary>
    /// Disables a button.
    /// </summary>
    public void DisableButton(string buttonId)
    {
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        uiButton.isEnabled = false;
        buttonStates[buttonId] = false;
        
        if (uiButton.button != null)
        {
            uiButton.button.interactable = false;
            SetButtonColor(uiButton, uiButton.disabledColor);
        }
        
        OnButtonDisabled?.Invoke(buttonId);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Button disabled: {buttonId}");
        }
    }
    
    /// <summary>
    /// Shows a button.
    /// </summary>
    public void ShowButton(string buttonId)
    {
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        uiButton.isVisible = true;
        
        if (uiButton.button != null)
        {
            uiButton.button.gameObject.SetActive(true);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Button shown: {buttonId}");
        }
    }
    
    /// <summary>
    /// Hides a button.
    /// </summary>
    public void HideButton(string buttonId)
    {
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        uiButton.isVisible = false;
        
        if (uiButton.button != null)
        {
            uiButton.button.gameObject.SetActive(false);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Button hidden: {buttonId}");
        }
    }
    
    /// <summary>
    /// Sets button text.
    /// </summary>
    public void SetButtonText(string buttonId, string text)
    {
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        uiButton.buttonText = text;
        
        if (uiButton.button != null)
        {
            Text buttonText = uiButton.button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }
    }
    
    /// <summary>
    /// Sets button color.
    /// </summary>
    public void SetButtonColor(string buttonId, Color color)
    {
        if (!buttonRegistry.ContainsKey(buttonId)) return;
        
        UIButton uiButton = buttonRegistry[buttonId];
        SetButtonColor(uiButton, color);
    }
    
    /// <summary>
    /// Sets button color for a UI button.
    /// </summary>
    private void SetButtonColor(UIButton uiButton, Color color)
    {
        if (uiButton.button == null) return;
        
        Image buttonImage = uiButton.button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }
    
    /// <summary>
    /// Checks if a button is on cooldown.
    /// </summary>
    private bool IsButtonOnCooldown(string buttonId)
    {
        if (!buttonCooldowns.ContainsKey(buttonId)) return false;
        
        return Time.time < buttonCooldowns[buttonId];
    }
    
    /// <summary>
    /// Sets button cooldown.
    /// </summary>
    private void SetButtonCooldown(string buttonId, float cooldownTime)
    {
        buttonCooldowns[buttonId] = Time.time + cooldownTime;
    }
    
    /// <summary>
    /// Updates button cooldowns.
    /// </summary>
    private void UpdateButtonCooldowns()
    {
        List<string> expiredCooldowns = new List<string>();
        
        foreach (var kvp in buttonCooldowns)
        {
            if (Time.time >= kvp.Value)
            {
                expiredCooldowns.Add(kvp.Key);
            }
        }
        
        foreach (string buttonId in expiredCooldowns)
        {
            buttonCooldowns.Remove(buttonId);
        }
    }
    
    /// <summary>
    /// Updates button information display.
    /// </summary>
    private void UpdateButtonInfo()
    {
        if (showButtonInfo)
        {
            Debug.Log($"Last Clicked: {lastClickedButton} | Processing: {isProcessingClick} | Cooldowns: {buttonCooldowns.Count}");
        }
    }
    
    /// <summary>
    /// Animates button click.
    /// </summary>
    private IEnumerator AnimateButtonClick(UIButton uiButton)
    {
        if (uiButton.button == null) yield break;
        
        Vector3 originalScale = uiButton.button.transform.localScale;
        Vector3 targetScale = originalScale * clickScale;
        
        float elapsed = 0f;
        
        // Scale down
        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            float curveValue = animationCurve.Evaluate(progress);
            
            uiButton.button.transform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            
            yield return null;
        }
        
        // Scale back up
        elapsed = 0f;
        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            float curveValue = animationCurve.Evaluate(progress);
            
            uiButton.button.transform.localScale = Vector3.Lerp(targetScale, originalScale, curveValue);
            
            yield return null;
        }
        
        uiButton.button.transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Animates button hover.
    /// </summary>
    private IEnumerator AnimateButtonHover(UIButton uiButton, bool isHovering)
    {
        if (uiButton.button == null) yield break;
        
        Vector3 originalScale = uiButton.originalScale;
        Vector3 targetScale = isHovering ? originalScale * hoverScale : originalScale;
        Color originalColor = uiButton.normalColor;
        Color targetColor = isHovering ? uiButton.hoverColor : uiButton.normalColor;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float curveValue = animationCurve.Evaluate(progress);
            
            uiButton.button.transform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            SetButtonColor(uiButton, Color.Lerp(originalColor, targetColor, curveValue));
            
            yield return null;
        }
        
        uiButton.button.transform.localScale = targetScale;
        SetButtonColor(uiButton, targetColor);
    }
    
    /// <summary>
    /// Gets a pooled button.
    /// </summary>
    private Button GetPooledButton()
    {
        if (!enableButtonPooling || buttonPool.Count == 0)
        {
            return null;
        }
        
        return buttonPool.Dequeue();
    }
    
    /// <summary>
    /// Returns a button to the pool.
    /// </summary>
    private void ReturnToPool(Button button)
    {
        if (enableButtonPooling && button != null)
        {
            button.gameObject.SetActive(false);
            buttonPool.Enqueue(button);
        }
    }
    
    // Public getters and setters
    public bool IsButtonEnabled(string buttonId) => buttonStates.ContainsKey(buttonId) && buttonStates[buttonId];
    public bool IsButtonVisible(string buttonId) => buttonRegistry.ContainsKey(buttonId) && buttonRegistry[buttonId].isVisible;
    public bool IsButtonOnCooldown(string buttonId) => IsButtonOnCooldown(buttonId);
    public string GetLastClickedButton() => lastClickedButton;
    public bool IsProcessingClick() => isProcessingClick;
    
    /// <summary>
    /// Gets a button by ID.
    /// </summary>
    public UIButton GetButton(string buttonId)
    {
        return buttonRegistry.ContainsKey(buttonId) ? buttonRegistry[buttonId] : null;
    }
    
    /// <summary>
    /// Gets all registered button IDs.
    /// </summary>
    public List<string> GetButtonIds()
    {
        return new List<string>(buttonRegistry.Keys);
    }
    
    /// <summary>
    /// Sets the button cooldown time.
    /// </summary>
    public void SetButtonCooldownTime(float cooldownTime)
    {
        buttonCooldownTime = Mathf.Clamp(cooldownTime, 0f, 5f);
    }
    
    /// <summary>
    /// Enables or disables button animations.
    /// </summary>
    public void SetButtonAnimationsEnabled(bool enabled)
    {
        enableButtonAnimations = enabled;
    }
    
    /// <summary>
    /// Enables or disables button sounds.
    /// </summary>
    public void SetButtonSoundsEnabled(bool enabled)
    {
        enableButtonSounds = enabled;
    }
    
    /// <summary>
    /// Logs the current button handler status.
    /// </summary>
    public void LogButtonStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Button Handler Status ===");
        Debug.Log($"Registered Buttons: {buttonRegistry.Count}");
        Debug.Log($"Enabled Buttons: {buttonStates.Count}");
        Debug.Log($"Active Cooldowns: {buttonCooldowns.Count}");
        Debug.Log($"Last Clicked: {lastClickedButton}");
        Debug.Log($"Is Processing Click: {isProcessingClick}");
        Debug.Log($"Button Animations: {(enableButtonAnimations ? "Enabled" : "Disabled")}");
        Debug.Log($"Button Sounds: {(enableButtonSounds ? "Enabled" : "Disabled")}");
        Debug.Log($"Button Hover Effects: {(enableButtonHoverEffects ? "Enabled" : "Disabled")}");
        Debug.Log($"Button Cooldowns: {(enableButtonCooldowns ? "Enabled" : "Disabled")}");
        Debug.Log($"Button Pooling: {(enableButtonPooling ? "Enabled" : "Disabled")}");
        Debug.Log($"Button Pool Size: {buttonPool.Count}");
        Debug.Log("============================");
    }
} 