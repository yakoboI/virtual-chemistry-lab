using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles keyboard input for shortcuts, text input, navigation, and laboratory controls.
/// This component manages all keyboard-based interactions in the virtual chemistry lab.
/// </summary>
public class KeyboardInputHandler : MonoBehaviour
{
    [Header("Keyboard Settings")]
    [SerializeField] private bool enableKeyboardInput = true;
    [SerializeField] private float keyRepeatDelay = 0.5f;
    [SerializeField] private float keyRepeatRate = 0.1f;
    [SerializeField] private bool enableTextInput = true;
    [SerializeField] private int maxTextLength = 100;
    
    [Header("Shortcuts")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode debugKey = KeyCode.F1;
    [SerializeField] private KeyCode saveKey = KeyCode.F5;
    [SerializeField] private KeyCode loadKey = KeyCode.F9;
    [SerializeField] private KeyCode resetKey = KeyCode.F12;
    [SerializeField] private KeyCode helpKey = KeyCode.F1;
    [SerializeField] private KeyCode fullscreenKey = KeyCode.F11;
    
    [Header("Navigation")]
    [SerializeField] private KeyCode[] navigationKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    [SerializeField] private KeyCode[] cameraKeys = { KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F };
    [SerializeField] private float navigationSpeed = 5f;
    [SerializeField] private float cameraSpeed = 3f;
    
    [Header("Laboratory Controls")]
    [SerializeField] private KeyCode[] equipmentKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };
    [SerializeField] private KeyCode[] chemicalKeys = { KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
    [SerializeField] private KeyCode safetyKey = KeyCode.Space;
    [SerializeField] private KeyCode emergencyKey = KeyCode.X;
    
    [Header("Text Input")]
    [SerializeField] private bool enableNumericInput = true;
    [SerializeField] private bool enableDecimalInput = true;
    [SerializeField] private string allowedCharacters = "0123456789.,+-";
    [SerializeField] private int maxDecimalPlaces = 3;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showKeyPresses = false;
    [SerializeField] private bool logTextInput = false;
    
    private static KeyboardInputHandler instance;
    public static KeyboardInputHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<KeyboardInputHandler>();
                if (instance == null)
                {
                    GameObject go = new GameObject("KeyboardInputHandler");
                    instance = go.AddComponent<KeyboardInputHandler>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<KeyCode> OnKeyPressed;
    public event Action<KeyCode> OnKeyReleased;
    public event Action<KeyCode> OnKeyHeld;
    public event Action<string> OnTextInput;
    public event Action<string> OnNumericInput;
    public event Action<KeyCode> OnShortcutPressed;
    public event Action<Vector3> OnNavigationInput;
    public event Action<Vector3> OnCameraInput;
    public event Action<int> OnEquipmentSelected;
    public event Action<int> OnChemicalSelected;
    public event Action OnSafetyActivated;
    public event Action OnEmergencyActivated;
    public event Action OnPauseToggle;
    public event Action OnDebugToggle;
    public event Action OnSaveRequested;
    public event Action OnLoadRequested;
    public event Action OnResetRequested;
    public event Action OnHelpRequested;
    public event Action OnFullscreenToggle;
    
    // Private variables
    private Dictionary<KeyCode, float> keyPressTimes = new Dictionary<KeyCode, float>();
    private Dictionary<KeyCode, bool> keyStates = new Dictionary<KeyCode, bool>();
    private string currentTextInput = "";
    private bool isTextInputActive = false;
    private bool isNumericInputActive = false;
    private Camera mainCamera;
    private Vector3 navigationInput = Vector3.zero;
    private Vector3 cameraInput = Vector3.zero;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeKeyboardHandler();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupCamera();
    }
    
    private void Update()
    {
        if (!enableKeyboardInput) return;
        
        if (mainCamera == null)
        {
            SetupCamera();
        }
        
        HandleKeyboardInput();
        HandleNavigationInput();
        HandleCameraInput();
        HandleTextInput();
        UpdateKeyStates();
    }
    
    /// <summary>
    /// Initializes the keyboard input handler.
    /// </summary>
    private void InitializeKeyboardHandler()
    {
        // Initialize key states
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            keyStates[key] = false;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("KeyboardInputHandler initialized successfully");
        }
    }
    
    /// <summary>
    /// Sets up the main camera reference.
    /// </summary>
    private void SetupCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
    
    /// <summary>
    /// Handles all keyboard input processing.
    /// </summary>
    private void HandleKeyboardInput()
    {
        // Handle shortcut keys
        HandleShortcutKeys();
        
        // Handle equipment selection
        HandleEquipmentSelection();
        
        // Handle chemical selection
        HandleChemicalSelection();
        
        // Handle safety and emergency
        HandleSafetyControls();
        
        // Handle individual key presses
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                HandleKeyDown(key);
            }
            else if (Input.GetKeyUp(key))
            {
                HandleKeyUp(key);
            }
            else if (Input.GetKey(key))
            {
                HandleKeyHeld(key);
            }
        }
    }
    
    /// <summary>
    /// Handles shortcut key combinations.
    /// </summary>
    private void HandleShortcutKeys()
    {
        // Pause/Resume
        if (Input.GetKeyDown(pauseKey))
        {
            OnShortcutPressed?.Invoke(pauseKey);
            OnPauseToggle?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Pause/Resume shortcut pressed");
            }
        }
        
        // Debug mode
        if (Input.GetKeyDown(debugKey))
        {
            OnShortcutPressed?.Invoke(debugKey);
            OnDebugToggle?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Debug mode shortcut pressed");
            }
        }
        
        // Save
        if (Input.GetKeyDown(saveKey))
        {
            OnShortcutPressed?.Invoke(saveKey);
            OnSaveRequested?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Save shortcut pressed");
            }
        }
        
        // Load
        if (Input.GetKeyDown(loadKey))
        {
            OnShortcutPressed?.Invoke(loadKey);
            OnLoadRequested?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Load shortcut pressed");
            }
        }
        
        // Reset
        if (Input.GetKeyDown(resetKey))
        {
            OnShortcutPressed?.Invoke(resetKey);
            OnResetRequested?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Reset shortcut pressed");
            }
        }
        
        // Help
        if (Input.GetKeyDown(helpKey) && !Input.GetKey(KeyCode.LeftShift))
        {
            OnShortcutPressed?.Invoke(helpKey);
            OnHelpRequested?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Help shortcut pressed");
            }
        }
        
        // Fullscreen
        if (Input.GetKeyDown(fullscreenKey))
        {
            OnShortcutPressed?.Invoke(fullscreenKey);
            OnFullscreenToggle?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Fullscreen shortcut pressed");
            }
        }
    }
    
    /// <summary>
    /// Handles equipment selection via number keys.
    /// </summary>
    private void HandleEquipmentSelection()
    {
        for (int i = 0; i < equipmentKeys.Length; i++)
        {
            if (Input.GetKeyDown(equipmentKeys[i]))
            {
                OnEquipmentSelected?.Invoke(i);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Equipment {i + 1} selected");
                }
            }
        }
    }
    
    /// <summary>
    /// Handles chemical selection via number keys.
    /// </summary>
    private void HandleChemicalSelection()
    {
        for (int i = 0; i < chemicalKeys.Length; i++)
        {
            if (Input.GetKeyDown(chemicalKeys[i]))
            {
                OnChemicalSelected?.Invoke(i);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"Chemical {i + 1} selected");
                }
            }
        }
    }
    
    /// <summary>
    /// Handles safety and emergency controls.
    /// </summary>
    private void HandleSafetyControls()
    {
        // Safety activation
        if (Input.GetKeyDown(safetyKey))
        {
            OnSafetyActivated?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("Safety protocol activated");
            }
        }
        
        // Emergency activation
        if (Input.GetKeyDown(emergencyKey))
        {
            OnEmergencyActivated?.Invoke();
            
            if (enableDebugLogging)
            {
                Debug.Log("EMERGENCY PROTOCOL ACTIVATED");
            }
        }
    }
    
    /// <summary>
    /// Handles key down events.
    /// </summary>
    private void HandleKeyDown(KeyCode key)
    {
        if (!keyStates[key])
        {
            keyStates[key] = true;
            keyPressTimes[key] = Time.time;
            
            OnKeyPressed?.Invoke(key);
            
            if (showKeyPresses)
            {
                Debug.Log($"Key pressed: {key}");
            }
        }
    }
    
    /// <summary>
    /// Handles key up events.
    /// </summary>
    private void HandleKeyUp(KeyCode key)
    {
        if (keyStates[key])
        {
            keyStates[key] = false;
            keyPressTimes.Remove(key);
            
            OnKeyReleased?.Invoke(key);
        }
    }
    
    /// <summary>
    /// Handles key held events with repeat functionality.
    /// </summary>
    private void HandleKeyHeld(KeyCode key)
    {
        if (keyStates[key] && keyPressTimes.ContainsKey(key))
        {
            float timeHeld = Time.time - keyPressTimes[key];
            
            if (timeHeld > keyRepeatDelay)
            {
                float repeatTime = (timeHeld - keyRepeatDelay) / keyRepeatRate;
                if (repeatTime >= 1f)
                {
                    keyPressTimes[key] = Time.time - keyRepeatDelay;
                    OnKeyHeld?.Invoke(key);
                }
            }
        }
    }
    
    /// <summary>
    /// Handles navigation input (WASD).
    /// </summary>
    private void HandleNavigationInput()
    {
        navigationInput = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W))
        {
            navigationInput.z += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            navigationInput.z -= 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            navigationInput.x -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            navigationInput.x += 1f;
        }
        
        if (navigationInput != Vector3.zero)
        {
            navigationInput = navigationInput.normalized * navigationSpeed * Time.deltaTime;
            OnNavigationInput?.Invoke(navigationInput);
        }
    }
    
    /// <summary>
    /// Handles camera input (QERF).
    /// </summary>
    private void HandleCameraInput()
    {
        cameraInput = Vector3.zero;
        
        if (Input.GetKey(KeyCode.Q))
        {
            cameraInput.y += 1f; // Up
        }
        if (Input.GetKey(KeyCode.E))
        {
            cameraInput.y -= 1f; // Down
        }
        if (Input.GetKey(KeyCode.R))
        {
            cameraInput.z += 1f; // Forward
        }
        if (Input.GetKey(KeyCode.F))
        {
            cameraInput.z -= 1f; // Backward
        }
        
        if (cameraInput != Vector3.zero)
        {
            cameraInput = cameraInput.normalized * cameraSpeed * Time.deltaTime;
            OnCameraInput?.Invoke(cameraInput);
        }
    }
    
    /// <summary>
    /// Handles text input processing.
    /// </summary>
    private void HandleTextInput()
    {
        if (!enableTextInput) return;
        
        // Check for text input activation
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isTextInputActive)
            {
                SubmitTextInput();
            }
            else
            {
                ActivateTextInput();
            }
        }
        
        // Handle text input when active
        if (isTextInputActive)
        {
            ProcessTextInput();
        }
        
        // Handle numeric input when active
        if (isNumericInputActive)
        {
            ProcessNumericInput();
        }
    }
    
    /// <summary>
    /// Activates text input mode.
    /// </summary>
    private void ActivateTextInput()
    {
        isTextInputActive = true;
        currentTextInput = "";
        
        if (enableDebugLogging)
        {
            Debug.Log("Text input activated");
        }
    }
    
    /// <summary>
    /// Activates numeric input mode.
    /// </summary>
    public void ActivateNumericInput()
    {
        isNumericInputActive = true;
        currentTextInput = "";
        
        if (enableDebugLogging)
        {
            Debug.Log("Numeric input activated");
        }
    }
    
    /// <summary>
    /// Processes text input from keyboard.
    /// </summary>
    private void ProcessTextInput()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                if (currentTextInput.Length > 0)
                {
                    currentTextInput = currentTextInput.Substring(0, currentTextInput.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r') // Enter
            {
                SubmitTextInput();
            }
            else if (currentTextInput.Length < maxTextLength)
            {
                currentTextInput += c;
            }
        }
        
        if (logTextInput && currentTextInput.Length > 0)
        {
            Debug.Log($"Text input: {currentTextInput}");
        }
    }
    
    /// <summary>
    /// Processes numeric input from keyboard.
    /// </summary>
    private void ProcessNumericInput()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                if (currentTextInput.Length > 0)
                {
                    currentTextInput = currentTextInput.Substring(0, currentTextInput.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r') // Enter
            {
                SubmitNumericInput();
            }
            else if (allowedCharacters.Contains(c.ToString()) && currentTextInput.Length < maxTextLength)
            {
                // Validate decimal places
                if (c == '.' || c == ',')
                {
                    if (!enableDecimalInput || currentTextInput.Contains(".") || currentTextInput.Contains(","))
                    {
                        continue;
                    }
                }
                
                currentTextInput += c;
            }
        }
    }
    
    /// <summary>
    /// Submits the current text input.
    /// </summary>
    private void SubmitTextInput()
    {
        if (currentTextInput.Length > 0)
        {
            OnTextInput?.Invoke(currentTextInput);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Text submitted: {currentTextInput}");
            }
        }
        
        isTextInputActive = false;
        currentTextInput = "";
    }
    
    /// <summary>
    /// Submits the current numeric input.
    /// </summary>
    private void SubmitNumericInput()
    {
        if (currentTextInput.Length > 0)
        {
            OnNumericInput?.Invoke(currentTextInput);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Numeric value submitted: {currentTextInput}");
            }
        }
        
        isNumericInputActive = false;
        currentTextInput = "";
    }
    
    /// <summary>
    /// Updates key states for repeat functionality.
    /// </summary>
    private void UpdateKeyStates()
    {
        // Clean up old key press times
        List<KeyCode> keysToRemove = new List<KeyCode>();
        foreach (var kvp in keyPressTimes)
        {
            if (!Input.GetKey(kvp.Key))
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (KeyCode key in keysToRemove)
        {
            keyPressTimes.Remove(key);
        }
    }
    
    // Public getters and setters
    public bool IsTextInputActive() => isTextInputActive;
    public bool IsNumericInputActive() => isNumericInputActive;
    public string GetCurrentTextInput() => currentTextInput;
    public Vector3 GetNavigationInput() => navigationInput;
    public Vector3 GetCameraInput() => cameraInput;
    
    /// <summary>
    /// Sets keyboard input enabled state.
    /// </summary>
    public void SetKeyboardInputEnabled(bool enabled)
    {
        enableKeyboardInput = enabled;
    }
    
    /// <summary>
    /// Sets text input enabled state.
    /// </summary>
    public void SetTextInputEnabled(bool enabled)
    {
        enableTextInput = enabled;
    }
    
    /// <summary>
    /// Sets navigation speed.
    /// </summary>
    public void SetNavigationSpeed(float speed)
    {
        navigationSpeed = Mathf.Clamp(speed, 0.1f, 20f);
    }
    
    /// <summary>
    /// Sets camera speed.
    /// </summary>
    public void SetCameraSpeed(float speed)
    {
        cameraSpeed = Mathf.Clamp(speed, 0.1f, 10f);
    }
    
    /// <summary>
    /// Clears current text input.
    /// </summary>
    public void ClearTextInput()
    {
        currentTextInput = "";
        isTextInputActive = false;
        isNumericInputActive = false;
    }
    
    /// <summary>
    /// Checks if a key is currently pressed.
    /// </summary>
    public bool IsKeyPressed(KeyCode key)
    {
        return keyStates.ContainsKey(key) && keyStates[key];
    }
    
    /// <summary>
    /// Logs the current keyboard input status.
    /// </summary>
    public void LogKeyboardStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Keyboard Input Handler Status ===");
        Debug.Log($"Keyboard Input: {(enableKeyboardInput ? "Enabled" : "Disabled")}");
        Debug.Log($"Text Input: {(enableTextInput ? "Enabled" : "Disabled")}");
        Debug.Log($"Text Input Active: {isTextInputActive}");
        Debug.Log($"Numeric Input Active: {isNumericInputActive}");
        Debug.Log($"Current Text: {currentTextInput}");
        Debug.Log($"Navigation Input: {navigationInput}");
        Debug.Log($"Camera Input: {cameraInput}");
        Debug.Log($"Navigation Speed: {navigationSpeed}");
        Debug.Log($"Camera Speed: {cameraSpeed}");
        Debug.Log($"Active Keys: {keyStates.Count}");
        Debug.Log("=====================================");
    }
} 