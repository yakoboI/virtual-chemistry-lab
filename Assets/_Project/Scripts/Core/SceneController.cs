using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple scene controller to handle basic scene management and transitions.
/// This is a minimal implementation for the virtual chemistry lab.
/// </summary>
public class SceneController : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string experimentSceneName = "ExperimentScene";
    [SerializeField] private float transitionDelay = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private static SceneController instance;
    public static SceneController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneController>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SceneController");
                    instance = go.AddComponent<SceneController>();
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
            InitializeSceneController();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes the scene controller with basic settings.
    /// </summary>
    private void InitializeSceneController()
    {
        if (enableDebugLogging)
        {
            Debug.Log("SceneController initialized successfully");
        }
        
        // Set up basic scene management
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
    }
    
    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void LoadMainMenu()
    {
        if (enableDebugLogging)
        {
            Debug.Log($"Loading main menu scene: {mainMenuSceneName}");
        }
        
        StartCoroutine(LoadSceneAsync(mainMenuSceneName));
    }
    
    /// <summary>
    /// Loads the experiment scene.
    /// </summary>
    public void LoadExperimentScene()
    {
        if (enableDebugLogging)
        {
            Debug.Log($"Loading experiment scene: {experimentSceneName}");
        }
        
        StartCoroutine(LoadSceneAsync(experimentSceneName));
    }
    
    /// <summary>
    /// Loads a scene by name.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty");
            return;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loading scene: {sceneName}");
        }
        
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    /// <summary>
    /// Loads a scene by build index.
    /// </summary>
    public void LoadScene(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Invalid build index: {buildIndex}");
            return;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loading scene with build index: {buildIndex}");
        }
        
        StartCoroutine(LoadSceneAsync(buildIndex));
    }
    
    /// <summary>
    /// Reloads the current scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Reloading current scene: {currentSceneName}");
        }
        
        StartCoroutine(LoadSceneAsync(currentSceneName));
    }
    
    /// <summary>
    /// Asynchronously loads a scene by name.
    /// </summary>
    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        // Show loading screen or transition effect here if needed
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        while (asyncLoad.progress < 0.9f)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Loading progress: {asyncLoad.progress * 100:F1}%");
            }
            yield return null;
        }
        
        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);
        
        asyncLoad.allowSceneActivation = true;
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Scene loaded successfully: {sceneName}");
        }
    }
    
    /// <summary>
    /// Asynchronously loads a scene by build index.
    /// </summary>
    private System.Collections.IEnumerator LoadSceneAsync(int buildIndex)
    {
        // Show loading screen or transition effect here if needed
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
        asyncLoad.allowSceneActivation = false;
        
        while (asyncLoad.progress < 0.9f)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"Loading progress: {asyncLoad.progress * 100:F1}%");
            }
            yield return null;
        }
        
        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);
        
        asyncLoad.allowSceneActivation = true;
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Scene loaded successfully with build index: {buildIndex}");
        }
    }
    
    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitApplication()
    {
        if (enableDebugLogging)
        {
            Debug.Log("Quitting application");
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Gets the current scene name.
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Gets the current scene build index.
    /// </summary>
    public int GetCurrentSceneBuildIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
    
    /// <summary>
    /// Checks if a scene is loaded.
    /// </summary>
    public bool IsSceneLoaded(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).isLoaded;
    }
} 