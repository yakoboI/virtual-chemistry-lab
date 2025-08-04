using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages text display, instructions, and messaging system for the virtual chemistry lab.
/// This component handles all text-based communication and instruction display.
/// </summary>
public class TextDisplayManager : MonoBehaviour
{
    [Header("Text Display Settings")]
    [SerializeField] private bool enableTextDisplay = true;
    [SerializeField] private bool enableTypewriterEffect = true;
    [SerializeField] private float typewriterSpeed = 50f;
    [SerializeField] private bool enableAutoScroll = true;
    [SerializeField] private bool enableTextFade = true;
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI mainDisplayText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI measurementText;
    [SerializeField] private ScrollRect textScrollRect;
    [SerializeField] private GameObject textContainer;
    
    [Header("Text Categories")]
    [SerializeField] private TextCategory[] textCategories;
    [SerializeField] private Dictionary<string, TextStyle> textStyles = new Dictionary<string, TextStyle>();
    [SerializeField] private Queue<TextMessage> messageQueue = new Queue<TextMessage>();
    
    [Header("Display Settings")]
    [SerializeField] private int maxDisplayLines = 20;
    [SerializeField] private bool enableWordWrap = true;
    [SerializeField] private bool enableRichText = true;
    [SerializeField] private bool enableEmojis = true;
    [SerializeField] private float autoClearDelay = 5f;
    
    [Header("Animation")]
    [SerializeField] private bool enableTextAnimations = true;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool enableBounceEffect = false;
    [SerializeField] private float bounceIntensity = 0.1f;
    
    [Header("Performance")]
    [SerializeField] private bool enableTextPooling = true;
    [SerializeField] private int textPoolSize = 10;
    [SerializeField] private bool enableTextCulling = true;
    [SerializeField] private int maxQueuedMessages = 50;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showTextInfo = false;
    [SerializeField] private bool logTextChanges = false;
    
    private static TextDisplayManager instance;
    public static TextDisplayManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TextDisplayManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TextDisplayManager");
                    instance = go.AddComponent<TextDisplayManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnTextDisplayed;
    public event Action<string> OnTextCleared;
    public event Action<string> OnInstructionShown;
    public event Action<string> OnWarningShown;
    public event Action<string> OnStatusUpdated;
    public event Action OnTextAnimationCompleted;
    public event Action<string> OnTextError;
    
    // Private variables
    private List<string> displayHistory = new List<string>();
    private Coroutine typewriterCoroutine;
    private Coroutine fadeCoroutine;
    private Coroutine autoClearCoroutine;
    private bool isDisplaying = false;
    private bool isInitialized = false;
    private Queue<TextMeshProUGUI> textPool = new Queue<TextMeshProUGUI>();
    
    [System.Serializable]
    public class TextCategory
    {
        public string categoryName;
        public Color textColor = Color.white;
        public int fontSize = 16;
        public FontStyle fontStyle = FontStyle.Normal;
        public bool isBold = false;
        public bool isItalic = false;
        public float displayDuration = 3f;
        public bool autoClear = true;
    }
    
    [System.Serializable]
    public class TextStyle
    {
        public Color color;
        public int fontSize;
        public FontStyle fontStyle;
        public bool isBold;
        public bool isItalic;
        public float duration;
        public bool autoClear;
        
        public TextStyle(TextCategory category)
        {
            color = category.textColor;
            fontSize = category.fontSize;
            fontStyle = category.fontStyle;
            isBold = category.isBold;
            isItalic = category.isItalic;
            duration = category.displayDuration;
            autoClear = category.autoClear;
        }
    }
    
    [System.Serializable]
    public class TextMessage
    {
        public string text;
        public string category;
        public float priority;
        public bool useTypewriter;
        public Action onComplete;
        public float timestamp;
        
        public TextMessage(string message, string cat = "default", float pri = 0f, bool typewriter = true)
        {
            text = message;
            category = cat;
            priority = pri;
            useTypewriter = typewriter;
            timestamp = Time.time;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTextManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupTextStyles();
        InitializeTextPool();
        SetupUIReferences();
    }
    
    private void Update()
    {
        HandleMessageQueue();
        UpdateTextInfo();
    }
    
    /// <summary>
    /// Initializes the text display manager.
    /// </summary>
    private void InitializeTextManager()
    {
        displayHistory.Clear();
        messageQueue.Clear();
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("TextDisplayManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Sets up text styles from categories.
    /// </summary>
    private void SetupTextStyles()
    {
        textStyles.Clear();
        
        foreach (TextCategory category in textCategories)
        {
            textStyles[category.categoryName] = new TextStyle(category);
        }
        
        // Add default style if not present
        if (!textStyles.ContainsKey("default"))
        {
            textStyles["default"] = new TextStyle
            {
                color = Color.white,
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                isBold = false,
                isItalic = false,
                duration = 3f,
                autoClear = true
            };
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Setup {textStyles.Count} text styles");
        }
    }
    
    /// <summary>
    /// Initializes the text pool.
    /// </summary>
    private void InitializeTextPool()
    {
        if (!enableTextPooling) return;
        
        for (int i = 0; i < textPoolSize; i++)
        {
            GameObject textObj = new GameObject($"PooledText_{i}");
            textObj.transform.SetParent(transform);
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.gameObject.SetActive(false);
            
            textPool.Enqueue(textComponent);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized text pool with {textPoolSize} components");
        }
    }
    
    /// <summary>
    /// Sets up UI references.
    /// </summary>
    private void SetupUIReferences()
    {
        // Auto-find UI components if not assigned
        if (mainDisplayText == null)
        {
            mainDisplayText = FindObjectOfType<TextMeshProUGUI>();
        }
        
        if (textScrollRect == null)
        {
            textScrollRect = FindObjectOfType<ScrollRect>();
        }
    }
    
    /// <summary>
    /// Displays text on the main display.
    /// </summary>
    public void DisplayText(string text, string category = "default", bool useTypewriter = true)
    {
        if (!enableTextDisplay || string.IsNullOrEmpty(text)) return;
        
        TextMessage message = new TextMessage(text, category, 0f, useTypewriter);
        messageQueue.Enqueue(message);
        
        if (logTextChanges)
        {
            Debug.Log($"Queued text: {text} (Category: {category})");
        }
    }
    
    /// <summary>
    /// Shows an instruction.
    /// </summary>
    public void ShowInstruction(string instruction, bool useTypewriter = true)
    {
        if (!enableTextDisplay || string.IsNullOrEmpty(instruction)) return;
        
        TextMessage message = new TextMessage(instruction, "instruction", 1f, useTypewriter);
        messageQueue.Enqueue(message);
        
        OnInstructionShown?.Invoke(instruction);
        
        if (logTextChanges)
        {
            Debug.Log($"Instruction: {instruction}");
        }
    }
    
    /// <summary>
    /// Shows a warning message.
    /// </summary>
    public void ShowWarning(string warning, bool useTypewriter = true)
    {
        if (!enableTextDisplay || string.IsNullOrEmpty(warning)) return;
        
        TextMessage message = new TextMessage(warning, "warning", 2f, useTypewriter);
        messageQueue.Enqueue(message);
        
        OnWarningShown?.Invoke(warning);
        
        if (logTextChanges)
        {
            Debug.Log($"Warning: {warning}");
        }
    }
    
    /// <summary>
    /// Shows a safety warning.
    /// </summary>
    public void ShowSafetyWarning(string warning)
    {
        ShowWarning($"⚠️ SAFETY WARNING: {warning}", true);
    }
    
    /// <summary>
    /// Updates status text.
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (!enableTextDisplay || string.IsNullOrEmpty(status)) return;
        
        if (statusText != null)
        {
            statusText.text = status;
            OnStatusUpdated?.Invoke(status);
        }
        
        if (logTextChanges)
        {
            Debug.Log($"Status: {status}");
        }
    }
    
    /// <summary>
    /// Shows measurement text.
    /// </summary>
    public void ShowMeasurement(string measurement, string unit = "")
    {
        if (!enableTextDisplay || string.IsNullOrEmpty(measurement)) return;
        
        string displayText = $"{measurement} {unit}".Trim();
        
        if (measurementText != null)
        {
            measurementText.text = displayText;
        }
        
        if (logTextChanges)
        {
            Debug.Log($"Measurement: {displayText}");
        }
    }
    
    /// <summary>
    /// Clears all text displays.
    /// </summary>
    public void ClearAllText()
    {
        if (mainDisplayText != null)
        {
            mainDisplayText.text = "";
        }
        
        if (instructionText != null)
        {
            instructionText.text = "";
        }
        
        if (warningText != null)
        {
            warningText.text = "";
        }
        
        if (measurementText != null)
        {
            measurementText.text = "";
        }
        
        displayHistory.Clear();
        
        OnTextCleared?.Invoke("all");
        
        if (logTextChanges)
        {
            Debug.Log("All text cleared");
        }
    }
    
    /// <summary>
    /// Clears specific text display.
    /// </summary>
    public void ClearText(string displayType = "main")
    {
        switch (displayType.ToLower())
        {
            case "main":
                if (mainDisplayText != null) mainDisplayText.text = "";
                break;
            case "instruction":
                if (instructionText != null) instructionText.text = "";
                break;
            case "warning":
                if (warningText != null) warningText.text = "";
                break;
            case "measurement":
                if (measurementText != null) measurementText.text = "";
                break;
            case "status":
                if (statusText != null) statusText.text = "";
                break;
        }
        
        OnTextCleared?.Invoke(displayType);
        
        if (logTextChanges)
        {
            Debug.Log($"Cleared {displayType} text");
        }
    }
    
    /// <summary>
    /// Handles the message queue.
    /// </summary>
    private void HandleMessageQueue()
    {
        if (isDisplaying || messageQueue.Count == 0) return;
        
        // Limit queue size
        while (messageQueue.Count > maxQueuedMessages)
        {
            messageQueue.Dequeue();
        }
        
        TextMessage message = messageQueue.Dequeue();
        StartCoroutine(DisplayMessageCoroutine(message));
    }
    
    /// <summary>
    /// Coroutine for displaying a message.
    /// </summary>
    private IEnumerator DisplayMessageCoroutine(TextMessage message)
    {
        isDisplaying = true;
        
        if (logTextChanges)
        {
            Debug.Log($"Displaying: {message.text}");
        }
        
        // Get text style
        TextStyle style = textStyles.ContainsKey(message.category) ? 
            textStyles[message.category] : textStyles["default"];
        
        // Choose target text component
        TextMeshProUGUI targetText = GetTargetTextComponent(message.category);
        if (targetText == null)
        {
            isDisplaying = false;
            yield break;
        }
        
        // Apply style
        ApplyTextStyle(targetText, style);
        
        // Display text
        if (message.useTypewriter && enableTypewriterEffect)
        {
            yield return StartCoroutine(TypewriterEffect(targetText, message.text));
        }
        else
        {
            targetText.text = message.text;
        }
        
        // Add to history
        AddToHistory(message.text);
        
        // Auto-clear if enabled
        if (style.autoClear && style.duration > 0)
        {
            autoClearCoroutine = StartCoroutine(AutoClearText(targetText, style.duration));
        }
        
        // Scroll to bottom if enabled
        if (enableAutoScroll && textScrollRect != null)
        {
            yield return new WaitForEndOfFrame();
            textScrollRect.verticalNormalizedPosition = 0f;
        }
        
        // Trigger events
        OnTextDisplayed?.Invoke(message.text);
        message.onComplete?.Invoke();
        
        isDisplaying = false;
        
        if (logTextChanges)
        {
            Debug.Log($"Displayed: {message.text}");
        }
    }
    
    /// <summary>
    /// Typewriter effect coroutine.
    /// </summary>
    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText)
    {
        textComponent.text = "";
        
        for (int i = 0; i < fullText.Length; i++)
        {
            textComponent.text += fullText[i];
            
            if (enableRichText)
            {
                // Handle rich text tags
                if (fullText[i] == '<')
                {
                    // Skip to end of tag
                    while (i < fullText.Length && fullText[i] != '>')
                    {
                        i++;
                        textComponent.text += fullText[i];
                    }
                }
            }
            
            yield return new WaitForSeconds(1f / typewriterSpeed);
        }
    }
    
    /// <summary>
    /// Auto-clear text coroutine.
    /// </summary>
    private IEnumerator AutoClearText(TextMeshProUGUI textComponent, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (enableTextFade)
        {
            yield return StartCoroutine(FadeOutText(textComponent));
        }
        else
        {
            textComponent.text = "";
        }
    }
    
    /// <summary>
    /// Fade out text coroutine.
    /// </summary>
    private IEnumerator FadeOutText(TextMeshProUGUI textComponent)
    {
        CanvasGroup canvasGroup = textComponent.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textComponent.gameObject.AddComponent<CanvasGroup>();
        }
        
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            float curveValue = fadeCurve.Evaluate(progress);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
            
            yield return null;
        }
        
        textComponent.text = "";
        canvasGroup.alpha = startAlpha;
    }
    
    /// <summary>
    /// Gets the target text component based on category.
    /// </summary>
    private TextMeshProUGUI GetTargetTextComponent(string category)
    {
        switch (category.ToLower())
        {
            case "instruction":
                return instructionText;
            case "warning":
                return warningText;
            case "measurement":
                return measurementText;
            case "status":
                return statusText;
            default:
                return mainDisplayText;
        }
    }
    
    /// <summary>
    /// Applies text style to a text component.
    /// </summary>
    private void ApplyTextStyle(TextMeshProUGUI textComponent, TextStyle style)
    {
        if (textComponent == null) return;
        
        textComponent.color = style.color;
        textComponent.fontSize = style.fontSize;
        textComponent.fontStyle = style.fontStyle;
        textComponent.enableRichText = enableRichText;
        textComponent.enableWordWrapping = enableWordWrap;
    }
    
    /// <summary>
    /// Adds text to display history.
    /// </summary>
    private void AddToHistory(string text)
    {
        displayHistory.Add(text);
        
        // Limit history size
        while (displayHistory.Count > maxDisplayLines)
        {
            displayHistory.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Updates text information display.
    /// </summary>
    private void UpdateTextInfo()
    {
        if (showTextInfo)
        {
            Debug.Log($"Displaying: {isDisplaying} | Queue: {messageQueue.Count} | History: {displayHistory.Count}");
        }
    }
    
    /// <summary>
    /// Gets a pooled text component.
    /// </summary>
    private TextMeshProUGUI GetPooledText()
    {
        if (!enableTextPooling || textPool.Count == 0)
        {
            return null;
        }
        
        return textPool.Dequeue();
    }
    
    /// <summary>
    /// Returns a text component to the pool.
    /// </summary>
    private void ReturnToPool(TextMeshProUGUI textComponent)
    {
        if (enableTextPooling && textComponent != null)
        {
            textComponent.text = "";
            textComponent.gameObject.SetActive(false);
            textPool.Enqueue(textComponent);
        }
    }
    
    // Public getters and setters
    public bool IsDisplaying() => isDisplaying;
    public int GetQueueSize() => messageQueue.Count;
    public int GetHistorySize() => displayHistory.Count;
    public List<string> GetDisplayHistory() => new List<string>(displayHistory);
    
    /// <summary>
    /// Sets the typewriter speed.
    /// </summary>
    public void SetTypewriterSpeed(float speed)
    {
        typewriterSpeed = Mathf.Clamp(speed, 10f, 200f);
    }
    
    /// <summary>
    /// Sets the fade duration.
    /// </summary>
    public void SetFadeDuration(float duration)
    {
        fadeDuration = Mathf.Clamp(duration, 0.1f, 5f);
    }
    
    /// <summary>
    /// Enables or disables typewriter effect.
    /// </summary>
    public void SetTypewriterEnabled(bool enabled)
    {
        enableTypewriterEffect = enabled;
    }
    
    /// <summary>
    /// Enables or disables text fade.
    /// </summary>
    public void SetTextFadeEnabled(bool enabled)
    {
        enableTextFade = enabled;
    }
    
    /// <summary>
    /// Gets the current main display text.
    /// </summary>
    public string GetMainDisplayText()
    {
        return mainDisplayText != null ? mainDisplayText.text : "";
    }
    
    /// <summary>
    /// Gets the current instruction text.
    /// </summary>
    public string GetInstructionText()
    {
        return instructionText != null ? instructionText.text : "";
    }
    
    /// <summary>
    /// Logs the current text display manager status.
    /// </summary>
    public void LogTextStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Text Display Manager Status ===");
        Debug.Log($"Is Displaying: {isDisplaying}");
        Debug.Log($"Message Queue Size: {messageQueue.Count}");
        Debug.Log($"Display History Size: {displayHistory.Count}");
        Debug.Log($"Text Styles: {textStyles.Count}");
        Debug.Log($"Typewriter Effect: {(enableTypewriterEffect ? "Enabled" : "Disabled")}");
        Debug.Log($"Text Fade: {(enableTextFade ? "Enabled" : "Disabled")}");
        Debug.Log($"Auto Scroll: {(enableAutoScroll ? "Enabled" : "Disabled")}");
        Debug.Log($"Text Pooling: {(enableTextPooling ? "Enabled" : "Disabled")}");
        Debug.Log($"Text Pool Size: {textPool.Count}");
        Debug.Log("===================================");
    }
}