using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages scene loading, transitions, and state management for the virtual chemistry lab.
/// This component handles scene-specific operations and transitions.
/// </summary>
public class SceneStateController : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private bool enableSceneTransitions = true;
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private bool enableLoadingScreen = true;
    [SerializeField] private bool enableScenePreloading = true;
    [SerializeField] private bool enableSceneCleanup = true;
    
    [Header("Scene Configuration")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string experimentScene = "ExperimentScene";
    [SerializeField] private string loadingScene = "LoadingScene";
    [SerializeField] private string[] preloadScenes = { "MainMenu", "ExperimentScene" };
    
    [Header("Transition Effects")]
    [SerializeField] private bool enableFadeTransition = true;
    [SerializeField] private bool enableSlideTransition = false;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color fadeColor = Color.black;
    
    [Header("Scene State")]
    [SerializeField] private SceneState currentSceneState = SceneState.None;
    [SerializeField] private string currentSceneName = "";
    [SerializeField] private string previousSceneName = "";
    [SerializeField] private bool isTransitioning = false;
    
    [Header("Performance")]
    [SerializeField] private bool enableAsyncLoading = true;
    [SerializeField] private bool enableSceneOptimization = true;
    [SerializeField] private float maxLoadTime = 10f;
    [SerializeField] private bool enableLoadTimeout = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showSceneInfo = false;
    [SerializeField] private bool logSceneChanges = false;
    
    private static SceneStateController instance;
    public static SceneStateController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneStateController>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SceneStateController");
                    instance = go.AddComponent<SceneStateController>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnSceneLoading;
    public event Action<string> OnSceneLoaded;
    public event Action<string> OnSceneUnloading;
    public event Action<string> OnSceneUnloaded;
    public event Action<SceneState> OnSceneStateChanged;
    public event Action<float> OnLoadProgress;
    public event Action OnTransitionStarted;
    public event Action OnTransitionCompleted;
    public event Action<string> OnSceneError;
    
    // Private variables
    private CanvasGroup transitionCanvas;
    private Dictionary<string, bool> preloadedScenes = new Dictionary<string, bool>();
    private Queue<SceneLoadRequest> loadQueue = new Queue<SceneLoadRequest>();
    private SceneLoadRequest currentLoadRequest;
    private Coroutine transitionCoroutine;
    private bool isInitialized = false;
    
    [System.Serializable]
    public enum SceneState
    {
        None,
        Loading,
        MainMenu,
        ExperimentSelection,
        ExperimentActive,
        ExperimentComplete,
        Settings,
        Help,
        LoadingScreen
    }
    
    [System.Serializable]
    public class SceneLoadRequest
    {
        public string sceneName;
        public SceneState targetState;
        public bool useTransition;
        public Action onComplete;
        public Action onError;
        public float timeout;
        
        public SceneLoadRequest(string scene, SceneState state, bool transition = true)
        {
            sceneName = scene;
            targetState = state;
            useTransition = transition;
            timeout = 10f;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSceneController();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupTransitionCanvas();
        PreloadScenes();
        SetInitialScene();
    }
    
    private void Update()
    {
        HandleLoadQueue();
        UpdateSceneInfo();
    }
    
    /// <summary>
    /// Initializes the scene controller.
    /// </summary>
    private void InitializeSceneController()
    {
        currentSceneState = SceneState.None;
        currentSceneName = SceneManager.GetActiveScene().name;
        
        // Subscribe to Unity scene events
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
        SceneManager.sceneUnloaded += OnSceneUnloadedCallback;
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("SceneStateController initialized successfully");
        }
    }
    
    /// <summary>
    /// Sets up the transition canvas for fade effects.
    /// </summary>
    private void SetupTransitionCanvas()
    {
        if (!enableFadeTransition) return;
        
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        
        UnityEngine.UI.Image image = canvasObj.AddComponent<UnityEngine.UI.Image>();
        image.color = fadeColor;
        image.raycastTarget = false;
        
        transitionCanvas = canvasGroup;
    }
    
    /// <summary>
    /// Preloads specified scenes for faster transitions.
    /// </summary>
    private void PreloadScenes()
    {
        if (!enableScenePreloading) return;
        
        foreach (string sceneName in preloadScenes)
        {
            if (SceneUtility.GetBuildIndexByScenePath(sceneName) >= 0)
            {
                StartCoroutine(PreloadScene(sceneName));
            }
        }
    }
    
    /// <summary>
    /// Preloads a single scene.
    /// </summary>
    private IEnumerator PreloadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;
        
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        preloadedScenes[sceneName] = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Scene preloaded: {sceneName}");
        }
    }
    
    /// <summary>
    /// Sets the initial scene state.
    /// </summary>
    private void SetInitialScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == mainMenuScene)
        {
            currentSceneState = SceneState.MainMenu;
        }
        else if (currentScene == experimentScene)
        {
            currentSceneState = SceneState.ExperimentActive;
        }
        else
        {
            currentSceneState = SceneState.None;
        }
        
        currentSceneName = currentScene;
        
        OnSceneStateChanged?.Invoke(currentSceneState);
        
        if (logSceneChanges)
        {
            Debug.Log($"Initial scene set: {currentScene} -> {currentSceneState}");
        }
    }
    
    /// <summary>
    /// Loads a scene with optional transition.
    /// </summary>
    public void LoadScene(string sceneName, SceneState targetState, bool useTransition = true)
    {
        if (isTransitioning)
        {
            // Queue the load request
            SceneLoadRequest request = new SceneLoadRequest(sceneName, targetState, useTransition);
            loadQueue.Enqueue(request);
            return;
        }
        
        StartCoroutine(LoadSceneCoroutine(sceneName, targetState, useTransition));
    }
    
    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene, SceneState.MainMenu);
    }
    
    /// <summary>
    /// Loads the experiment scene.
    /// </summary>
    public void LoadExperimentScene()
    {
        LoadScene(experimentScene, SceneState.ExperimentActive);
    }
    
    /// <summary>
    /// Loads the loading screen.
    /// </summary>
    public void LoadLoadingScreen()
    {
        if (enableLoadingScreen)
        {
            LoadScene(loadingScene, SceneState.LoadingScreen, false);
        }
    }
    
    /// <summary>
    /// Coroutine for loading scenes.
    /// </summary>
    private IEnumerator LoadSceneCoroutine(string sceneName, SceneState targetState, bool useTransition)
    {
        if (isTransitioning) yield break;
        
        isTransitioning = true;
        previousSceneName = currentSceneName;
        
        OnSceneLoading?.Invoke(sceneName);
        OnTransitionStarted?.Invoke();
        
        if (logSceneChanges)
        {
            Debug.Log($"Loading scene: {sceneName} -> {targetState}");
        }
        
        // Start transition out
        if (useTransition && enableSceneTransitions)
        {
            yield return StartCoroutine(TransitionOut());
        }
        
        // Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float loadStartTime = Time.time;
        
        while (asyncLoad.progress < 0.9f)
        {
            OnLoadProgress?.Invoke(asyncLoad.progress);
            
            // Check for timeout
            if (enableLoadTimeout && Time.time - loadStartTime > maxLoadTime)
            {
                OnSceneError?.Invoke($"Scene load timeout: {sceneName}");
                if (enableDebugLogging)
                {
                    Debug.LogError($"Scene load timeout: {sceneName}");
                }
                break;
            }
            
            yield return null;
        }
        
        // Allow scene activation
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to fully load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Update scene state
        currentSceneName = sceneName;
        currentSceneState = targetState;
        
        // Transition in
        if (useTransition && enableSceneTransitions)
        {
            yield return StartCoroutine(TransitionIn());
        }
        
        isTransitioning = false;
        
        OnSceneLoaded?.Invoke(sceneName);
        OnSceneStateChanged?.Invoke(targetState);
        OnTransitionCompleted?.Invoke();
        
        if (logSceneChanges)
        {
            Debug.Log($"Scene loaded: {sceneName} -> {targetState}");
        }
    }
    
    /// <summary>
    /// Transitions out of the current scene.
    /// </summary>
    private IEnumerator TransitionOut()
    {
        if (transitionCanvas != null)
        {
            float elapsed = 0f;
            float duration = transitionDuration * 0.5f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float curveValue = transitionCurve.Evaluate(progress);
                
                transitionCanvas.alpha = curveValue;
                transitionCanvas.blocksRaycasts = true;
                
                yield return null;
            }
            
            transitionCanvas.alpha = 1f;
        }
    }
    
    /// <summary>
    /// Transitions into the new scene.
    /// </summary>
    private IEnumerator TransitionIn()
    {
        if (transitionCanvas != null)
        {
            float elapsed = 0f;
            float duration = transitionDuration * 0.5f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float curveValue = transitionCurve.Evaluate(1f - progress);
                
                transitionCanvas.alpha = curveValue;
                
                yield return null;
            }
            
            transitionCanvas.alpha = 0f;
            transitionCanvas.blocksRaycasts = false;
        }
    }
    
    /// <summary>
    /// Handles the load queue.
    /// </summary>
    private void HandleLoadQueue()
    {
        if (!isTransitioning && loadQueue.Count > 0)
        {
            SceneLoadRequest request = loadQueue.Dequeue();
            StartCoroutine(LoadSceneCoroutine(request.sceneName, request.targetState, request.useTransition));
        }
    }
    
    /// <summary>
    /// Updates scene information display.
    /// </summary>
    private void UpdateSceneInfo()
    {
        if (showSceneInfo)
        {
            Debug.Log($"Current Scene: {currentSceneName} | State: {currentSceneState} | Transitioning: {isTransitioning}");
        }
    }
    
    /// <summary>
    /// Callback for when a scene is loaded.
    /// </summary>
    private void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        if (enableSceneCleanup)
        {
            CleanupScene(scene);
        }
        
        if (enableSceneOptimization)
        {
            OptimizeScene(scene);
        }
    }
    
    /// <summary>
    /// Callback for when a scene is unloaded.
    /// </summary>
    private void OnSceneUnloadedCallback(Scene scene)
    {
        OnSceneUnloaded?.Invoke(scene.name);
        
        if (logSceneChanges)
        {
            Debug.Log($"Scene unloaded: {scene.name}");
        }
    }
    
    /// <summary>
    /// Cleans up a scene after loading.
    /// </summary>
    private void CleanupScene(Scene scene)
    {
        // Remove any unwanted objects or reset scene state
        GameObject[] rootObjects = scene.GetRootGameObjects();
        
        foreach (GameObject obj in rootObjects)
        {
            // Cleanup logic here
        }
    }
    
    /// <summary>
    /// Optimizes a scene for performance.
    /// </summary>
    private void OptimizeScene(Scene scene)
    {
        // Apply performance optimizations
        // This could include LOD adjustments, culling setup, etc.
    }
    
    /// <summary>
    /// Reloads the current scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        LoadScene(currentSceneName, currentSceneState);
    }
    
    /// <summary>
    /// Returns to the previous scene.
    /// </summary>
    public void ReturnToPreviousScene()
    {
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            // Determine the appropriate state for the previous scene
            SceneState previousState = GetSceneState(previousSceneName);
            LoadScene(previousSceneName, previousState);
        }
    }
    
    /// <summary>
    /// Gets the scene state for a given scene name.
    /// </summary>
    private SceneState GetSceneState(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
                return SceneState.MainMenu;
            case "ExperimentScene":
                return SceneState.ExperimentActive;
            case "LoadingScene":
                return SceneState.LoadingScreen;
            default:
                return SceneState.None;
        }
    }
    
    /// <summary>
    /// Checks if a scene is preloaded.
    /// </summary>
    public bool IsScenePreloaded(string sceneName)
    {
        return preloadedScenes.ContainsKey(sceneName) && preloadedScenes[sceneName];
    }
    
    /// <summary>
    /// Gets the current scene state.
    /// </summary>
    public SceneState GetCurrentSceneState()
    {
        return currentSceneState;
    }
    
    /// <summary>
    /// Gets the current scene name.
    /// </summary>
    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }
    
    /// <summary>
    /// Checks if a scene transition is in progress.
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    /// <summary>
    /// Sets the transition duration.
    /// </summary>
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Clamp(duration, 0.1f, 5f);
    }
    
    /// <summary>
    /// Enables or disables scene transitions.
    /// </summary>
    public void SetSceneTransitionsEnabled(bool enabled)
    {
        enableSceneTransitions = enabled;
    }
    
    /// <summary>
    /// Logs the current scene controller status.
    /// </summary>
    public void LogSceneStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Scene State Controller Status ===");
        Debug.Log($"Current Scene: {currentSceneName}");
        Debug.Log($"Current State: {currentSceneState}");
        Debug.Log($"Previous Scene: {previousSceneName}");
        Debug.Log($"Is Transitioning: {isTransitioning}");
        Debug.Log($"Scene Transitions: {(enableSceneTransitions ? "Enabled" : "Disabled")}");
        Debug.Log($"Loading Screen: {(enableLoadingScreen ? "Enabled" : "Disabled")}");
        Debug.Log($"Scene Preloading: {(enableScenePreloading ? "Enabled" : "Disabled")}");
        Debug.Log($"Preloaded Scenes: {preloadedScenes.Count}");
        Debug.Log($"Load Queue Size: {loadQueue.Count}");
        Debug.Log("=====================================");
    }
} 