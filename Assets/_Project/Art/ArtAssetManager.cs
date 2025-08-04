using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages art assets including audio, textures, materials, and 3D models for the Virtual Chemistry Laboratory.
/// </summary>
public class ArtAssetManager : MonoBehaviour
{
    [Header("Art Asset Management")]
    [SerializeField] private bool enableArtAssetManagement = true;
    [SerializeField] private bool enableAssetCaching = true;
    
    [Header("Asset State")]
    [SerializeField] private Dictionary<string, AudioClip> audioAssets = new Dictionary<string, AudioClip>();
    [SerializeField] private Dictionary<string, Texture2D> textureAssets = new Dictionary<string, Texture2D>();
    [SerializeField] private Dictionary<string, Material> materialAssets = new Dictionary<string, Material>();
    [SerializeField] private Dictionary<string, GameObject> modelAssets = new Dictionary<string, GameObject>();
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private static ArtAssetManager instance;
    public static ArtAssetManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ArtAssetManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ArtAssetManager");
                    instance = go.AddComponent<ArtAssetManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events
    public event Action<string> OnAssetLoaded;
    public event Action<string> OnAssetLoadFailed;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeArtAssetManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes the art asset manager.
    /// </summary>
    private void InitializeArtAssetManager()
    {
        audioAssets.Clear();
        textureAssets.Clear();
        materialAssets.Clear();
        modelAssets.Clear();
        
        if (enableDebugLogging)
        {
            Debug.Log("ArtAssetManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Loads an audio clip.
    /// </summary>
    public AudioClip LoadAudioClip(string audioId, string path)
    {
        if (audioAssets.ContainsKey(audioId))
        {
            return audioAssets[audioId];
        }
        
        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip != null)
        {
            audioAssets[audioId] = clip;
            OnAssetLoaded?.Invoke(audioId);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Loaded audio clip: {audioId}");
            }
        }
        else
        {
            OnAssetLoadFailed?.Invoke(audioId);
            
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Failed to load audio clip: {audioId}");
            }
        }
        
        return clip;
    }
    
    /// <summary>
    /// Loads a texture.
    /// </summary>
    public Texture2D LoadTexture(string textureId, string path)
    {
        if (textureAssets.ContainsKey(textureId))
        {
            return textureAssets[textureId];
        }
        
        Texture2D texture = Resources.Load<Texture2D>(path);
        if (texture != null)
        {
            textureAssets[textureId] = texture;
            OnAssetLoaded?.Invoke(textureId);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Loaded texture: {textureId}");
            }
        }
        else
        {
            OnAssetLoadFailed?.Invoke(textureId);
            
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Failed to load texture: {textureId}");
            }
        }
        
        return texture;
    }
    
    /// <summary>
    /// Loads a material.
    /// </summary>
    public Material LoadMaterial(string materialId, string path)
    {
        if (materialAssets.ContainsKey(materialId))
        {
            return materialAssets[materialId];
        }
        
        Material material = Resources.Load<Material>(path);
        if (material != null)
        {
            materialAssets[materialId] = material;
            OnAssetLoaded?.Invoke(materialId);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Loaded material: {materialId}");
            }
        }
        else
        {
            OnAssetLoadFailed?.Invoke(materialId);
            
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Failed to load material: {materialId}");
            }
        }
        
        return material;
    }
    
    /// <summary>
    /// Loads a 3D model.
    /// </summary>
    public GameObject LoadModel(string modelId, string path)
    {
        if (modelAssets.ContainsKey(modelId))
        {
            return modelAssets[modelId];
        }
        
        GameObject model = Resources.Load<GameObject>(path);
        if (model != null)
        {
            modelAssets[modelId] = model;
            OnAssetLoaded?.Invoke(modelId);
            
            if (enableDebugLogging)
            {
                Debug.Log($"Loaded model: {modelId}");
            }
        }
        else
        {
            OnAssetLoadFailed?.Invoke(modelId);
            
            if (enableDebugLogging)
            {
                Debug.LogWarning($"Failed to load model: {modelId}");
            }
        }
        
        return model;
    }
    
    /// <summary>
    /// Gets an audio clip by ID.
    /// </summary>
    public AudioClip GetAudioClip(string audioId)
    {
        return audioAssets.ContainsKey(audioId) ? audioAssets[audioId] : null;
    }
    
    /// <summary>
    /// Gets a texture by ID.
    /// </summary>
    public Texture2D GetTexture(string textureId)
    {
        return textureAssets.ContainsKey(textureId) ? textureAssets[textureId] : null;
    }
    
    /// <summary>
    /// Gets a material by ID.
    /// </summary>
    public Material GetMaterial(string materialId)
    {
        return materialAssets.ContainsKey(materialId) ? materialAssets[materialId] : null;
    }
    
    /// <summary>
    /// Gets a model by ID.
    /// </summary>
    public GameObject GetModel(string modelId)
    {
        return modelAssets.ContainsKey(modelId) ? modelAssets[modelId] : null;
    }
    
    /// <summary>
    /// Clears all cached assets.
    /// </summary>
    public void ClearCache()
    {
        audioAssets.Clear();
        textureAssets.Clear();
        materialAssets.Clear();
        modelAssets.Clear();
        
        Resources.UnloadUnusedAssets();
        
        if (enableDebugLogging)
        {
            Debug.Log("Art asset cache cleared");
        }
    }
    
    /// <summary>
    /// Gets asset counts.
    /// </summary>
    public int GetAudioAssetCount() => audioAssets.Count;
    public int GetTextureAssetCount() => textureAssets.Count;
    public int GetMaterialAssetCount() => materialAssets.Count;
    public int GetModelAssetCount() => modelAssets.Count;
    
    /// <summary>
    /// Logs the current art asset manager status.
    /// </summary>
    public void LogArtAssetStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Art Asset Manager Status ===");
        Debug.Log($"Audio Assets: {audioAssets.Count}");
        Debug.Log($"Texture Assets: {textureAssets.Count}");
        Debug.Log($"Material Assets: {materialAssets.Count}");
        Debug.Log($"Model Assets: {modelAssets.Count}");
        Debug.Log($"Asset Caching: {(enableAssetCaching ? "Enabled" : "Disabled")}");
        Debug.Log("===============================");
    }
} 