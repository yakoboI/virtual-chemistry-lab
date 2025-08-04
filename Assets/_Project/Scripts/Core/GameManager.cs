using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple game manager to handle basic game state management and coordination.
/// This is a minimal implementation for the virtual chemistry lab.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private GameState currentGameState = GameState.MainMenu;
    [SerializeField] private string currentExperimentId = "";
    [SerializeField] private bool isPaused = false;
    
    [Header("Game Settings")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 60f;
    [SerializeField] private bool enableDebugMode = false;
    
    [Header("Performance")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool enableVSync = false;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private float lastAutoSave = 0f;
    private Dictionary<string, object> gameData = new Dictionary<string, object>();
    
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    public enum GameState
    {
        MainMenu,
        ExperimentSelection,
        ExperimentActive,
        ExperimentComplete,
        Paused,
        SafetyWarning,
        EmergencyMode
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupGameSettings();
        LoadGameData();
    }
    
    private void Update()
    {
        HandleInput();
        
        if (enableAutoSave && Time.time - lastAutoSave >= autoSaveInterval)
        {
            SaveGameData();
            lastAutoSave = Time.time;
        }
    }
    
    /// <summary>
    /// Initializes the game manager with basic settings.
    /// </summary>
    private void InitializeGameManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("GameManager initialized successfully");
        }
        
        // Initialize game data
        gameData = new Dictionary<string, object>();
        currentGameState = GameState.MainMenu;
    }
    
    /// <summary>
    /// Sets up basic game settings.
    /// </summary>
    private void SetupGameSettings()
    {
        // Set frame rate
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        
        // Set up other Unity settings
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Game settings configured - Frame Rate: {targetFrameRate}, VSync: {enableVSync}");
        }
    }
    
    /// <summary>
    /// Handles basic input for game control.
    /// </summary>
    private void HandleInput()
    {
        // Pause/Resume with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // Debug mode toggle
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugMode();
        }
        
        // Force save with F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGameData();
        }
    }
    
    /// <summary>
    /// Changes the current game state.
    /// </summary>
    public void ChangeGameState(GameState newState)
    {
        if (currentGameState == newState) return;
        
        GameState previousState = currentGameState;
        currentGameState = newState;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Game state changed: {previousState} -> {newState}");
        }
        
        // Handle state-specific actions
        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.ExperimentSelection:
                HandleExperimentSelectionState();
                break;
            case GameState.ExperimentActive:
                HandleExperimentActiveState();
                break;
            case GameState.ExperimentComplete:
                HandleExperimentCompleteState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
            case GameState.SafetyWarning:
                HandleSafetyWarningState();
                break;
            case GameState.EmergencyMode:
                HandleEmergencyModeState();
                break;
        }
    }
    
    /// <summary>
    /// Starts an experiment.
    /// </summary>
    public void StartExperiment(string experimentId)
    {
        if (string.IsNullOrEmpty(experimentId))
        {
            Debug.LogError("Experiment ID is null or empty");
            return;
        }
        
        currentExperimentId = experimentId;
        ChangeGameState(GameState.ExperimentActive);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Started experiment: {experimentId}");
        }
    }
    
    /// <summary>
    /// Completes the current experiment.
    /// </summary>
    public void CompleteExperiment()
    {
        ChangeGameState(GameState.ExperimentComplete);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Completed experiment: {currentExperimentId}");
        }
    }
    
    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseExperiment()
    {
        if (currentGameState == GameState.ExperimentActive)
        {
            isPaused = true;
            ChangeGameState(GameState.Paused);
            
            if (enableDebugLogging)
            {
                Debug.Log("Experiment paused");
            }
        }
    }
    
    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void ResumeExperiment()
    {
        if (isPaused && currentGameState == GameState.Paused)
        {
            isPaused = false;
            ChangeGameState(GameState.ExperimentActive);
            
            if (enableDebugLogging)
            {
                Debug.Log("Experiment resumed");
            }
        }
    }
    
    /// <summary>
    /// Toggles pause state.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeExperiment();
        }
        else
        {
            PauseExperiment();
        }
    }
    
    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        currentExperimentId = "";
        isPaused = false;
        ChangeGameState(GameState.MainMenu);
        
        if (enableDebugLogging)
        {
            Debug.Log("Returned to main menu");
        }
    }
    
    /// <summary>
    /// Handles safety violations.
    /// </summary>
    public void HandleSafetyViolation(string violationDescription)
    {
        if (enableDebugLogging)
        {
            Debug.LogWarning($"Safety violation: {violationDescription}");
        }
        
        // Show safety warning
        ChangeGameState(GameState.SafetyWarning);
        
        // Update UI if available
        if (TextDisplayManager.Instance != null)
        {
            TextDisplayManager.Instance.ShowSafetyWarning(violationDescription);
        }
    }
    
    /// <summary>
    /// Enters emergency mode.
    /// </summary>
    public void EnterEmergencyMode()
    {
        ChangeGameState(GameState.EmergencyMode);
        
        if (enableDebugLogging)
        {
            Debug.LogError("EMERGENCY MODE ACTIVATED");
        }
    }
    
    /// <summary>
    /// Exits emergency mode.
    /// </summary>
    public void ExitEmergencyMode()
    {
        if (currentGameState == GameState.EmergencyMode)
        {
            ChangeGameState(GameState.ExperimentActive);
            
            if (enableDebugLogging)
            {
                Debug.Log("Emergency mode deactivated");
            }
        }
    }
    
    /// <summary>
    /// Toggles debug mode.
    /// </summary>
    public void ToggleDebugMode()
    {
        enableDebugMode = !enableDebugMode;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Debug mode: {(enableDebugMode ? "Enabled" : "Disabled")}");
        }
    }
    
    /// <summary>
    /// Saves game data.
    /// </summary>
    public void SaveGameData()
    {
        gameData["CurrentState"] = currentGameState.ToString();
        gameData["CurrentExperiment"] = currentExperimentId;
        gameData["IsPaused"] = isPaused;
        gameData["SaveTime"] = System.DateTime.Now.ToString();
        
        string jsonData = JsonUtility.ToJson(new GameDataWrapper(gameData));
        PlayerPrefs.SetString("GameData", jsonData);
        PlayerPrefs.Save();
        
        if (enableDebugLogging)
        {
            Debug.Log("Game data saved");
        }
    }
    
    /// <summary>
    /// Loads game data.
    /// </summary>
    public void LoadGameData()
    {
        if (PlayerPrefs.HasKey("GameData"))
        {
            string jsonData = PlayerPrefs.GetString("GameData");
            GameDataWrapper wrapper = JsonUtility.FromJson<GameDataWrapper>(jsonData);
            
            if (wrapper != null)
            {
                gameData = wrapper.data;
                
                // Restore game state
                if (gameData.ContainsKey("CurrentState"))
                {
                    string stateString = gameData["CurrentState"].ToString();
                    if (System.Enum.TryParse(stateString, out GameState savedState))
                    {
                        currentGameState = savedState;
                    }
                }
                
                if (gameData.ContainsKey("CurrentExperiment"))
                {
                    currentExperimentId = gameData["CurrentExperiment"].ToString();
                }
                
                if (gameData.ContainsKey("IsPaused"))
                {
                    isPaused = bool.Parse(gameData["IsPaused"].ToString());
                }
                
                if (enableDebugLogging)
                {
                    Debug.Log("Game data loaded");
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the current game state.
    /// </summary>
    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }
    
    /// <summary>
    /// Gets the current experiment ID.
    /// </summary>
    public string GetCurrentExperimentId()
    {
        return currentExperimentId;
    }
    
    /// <summary>
    /// Checks if the game is paused.
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// Checks if debug mode is enabled.
    /// </summary>
    public bool IsDebugModeEnabled()
    {
        return enableDebugMode;
    }
    
    /// <summary>
    /// Gets game data by key.
    /// </summary>
    public object GetGameData(string key)
    {
        return gameData.ContainsKey(key) ? gameData[key] : null;
    }
    
    /// <summary>
    /// Sets game data by key.
    /// </summary>
    public void SetGameData(string key, object value)
    {
        gameData[key] = value;
    }
    
    // State handling methods
    private void HandleMainMenuState()
    {
        // Show main menu UI
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.ShowMainMenu();
        }
    }
    
    private void HandleExperimentSelectionState()
    {
        // Show experiment selection UI
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.ShowExperimentSelection();
        }
    }
    
    private void HandleExperimentActiveState()
    {
        // Show experiment active UI
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.ShowExperimentActive();
        }
    }
    
    private void HandleExperimentCompleteState()
    {
        // Show experiment results UI
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.ShowExperimentResults();
        }
    }
    
    private void HandlePausedState()
    {
        // Show pause UI
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.ShowPausePanel();
        }
    }
    
    private void HandleSafetyWarningState()
    {
        // Show safety warning UI
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.ShowSafetyWarning();
        }
    }
    
    private void HandleEmergencyModeState()
    {
        // Show emergency UI and stop all experiments
        if (ExperimentManager.Instance != null)
        {
            ExperimentManager.Instance.StopExperiment();
        }
    }
    
    /// <summary>
    /// Logs the current game status.
    /// </summary>
    public void LogGameStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Game Manager Status ===");
        Debug.Log($"Current State: {currentGameState}");
        Debug.Log($"Current Experiment: {currentExperimentId}");
        Debug.Log($"Is Paused: {isPaused}");
        Debug.Log($"Debug Mode: {enableDebugMode}");
        Debug.Log($"Auto Save: {(enableAutoSave ? "Enabled" : "Disabled")}");
        Debug.Log($"Target Frame Rate: {targetFrameRate}");
        Debug.Log($"Game Data Entries: {gameData.Count}");
        Debug.Log("===========================");
    }
}

/// <summary>
/// Wrapper class for serializing game data dictionary.
/// </summary>
[System.Serializable]
public class GameDataWrapper
{
    public Dictionary<string, object> data;
    
    public GameDataWrapper(Dictionary<string, object> data)
    {
        this.data = data;
    }
} 