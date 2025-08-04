using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages audio playback, sound effects, ambient sounds, and chemical reaction audio.
/// This component handles all audio operations in the virtual chemistry lab.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private bool enableAudio = true;
    [SerializeField] private float masterVolume = 1.0f;
    [SerializeField] private float musicVolume = 0.8f;
    [SerializeField] private float sfxVolume = 1.0f;
    [SerializeField] private float ambientVolume = 0.6f;
    [SerializeField] private float reactionVolume = 0.9f;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource reactionSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource voiceSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private AudioClip[] ambientClips;
    [SerializeField] private AudioClip[] uiClips;
    [SerializeField] private AudioClip[] chemicalClips;
    [SerializeField] private AudioClip[] reactionClips;
    [SerializeField] private AudioClip[] safetyClips;
    [SerializeField] private AudioClip[] equipmentClips;
    
    [Header("Audio Categories")]
    [SerializeField] private AudioCategory[] audioCategories;
    [SerializeField] private Dictionary<string, AudioClip> clipLibrary = new Dictionary<string, AudioClip>();
    [SerializeField] private Dictionary<string, AudioSource> sourceLibrary = new Dictionary<string, AudioSource>();
    
    [Header("3D Audio")]
    [SerializeField] private bool enable3DAudio = true;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private AnimationCurve volumeFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    
    [Header("Audio Effects")]
    [SerializeField] private bool enableReverb = true;
    [SerializeField] private bool enableEcho = false;
    [SerializeField] private bool enableLowPass = false;
    [SerializeField] private float reverbLevel = 0.5f;
    [SerializeField] private float echoDelay = 0.1f;
    [SerializeField] private float lowPassCutoff = 5000f;
    
    [Header("Performance")]
    [SerializeField] private int maxSimultaneousSounds = 10;
    [SerializeField] private bool enableAudioPooling = true;
    [SerializeField] private int audioPoolSize = 20;
    [SerializeField] private bool enableDistanceCulling = true;
    [SerializeField] private float cullingDistance = 100f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showAudioInfo = false;
    [SerializeField] private bool logAudioEvents = false;
    
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnAudioPlayed;
    public event Action<string> OnAudioStopped;
    public event Action<string> OnAudioPaused;
    public event Action<string> OnAudioResumed;
    public event Action<float> OnVolumeChanged;
    public event Action<bool> OnAudioEnabled;
    public event Action<string> OnMusicChanged;
    public event Action<string> OnAmbientChanged;
    
    // Private variables
    private Queue<AudioSource> audioPool = new Queue<AudioSource>();
    private List<AudioSource> activeSources = new List<AudioSource>();
    private Dictionary<string, float> categoryVolumes = new Dictionary<string, float>();
    private AudioListener audioListener;
    private Camera mainCamera;
    private bool isInitialized = false;
    private float fadeTime = 1f;
    private Coroutine fadeCoroutine;
    
    [System.Serializable]
    public class AudioCategory
    {
        public string name;
        public float volume = 1.0f;
        public bool enabled = true;
        public AudioSource source;
        public AudioClip[] clips;
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupAudioSources();
        SetupAudioListener();
        LoadAudioClips();
        InitializeAudioPool();
    }
    
    private void Update()
    {
        if (!enableAudio) return;
        
        UpdateAudioSources();
        HandleDistanceCulling();
        Update3DAudio();
    }
    
    /// <summary>
    /// Initializes the audio manager.
    /// </summary>
    private void InitializeAudioManager()
    {
        // Initialize category volumes
        categoryVolumes["music"] = musicVolume;
        categoryVolumes["sfx"] = sfxVolume;
        categoryVolumes["ambient"] = ambientVolume;
        categoryVolumes["reaction"] = reactionVolume;
        categoryVolumes["ui"] = 1.0f;
        categoryVolumes["voice"] = 1.0f;
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log("AudioManager initialized successfully");
        }
    }
    
    /// <summary>
    /// Sets up audio sources.
    /// </summary>
    private void SetupAudioSources()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        if (ambientSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }
        
        if (reactionSource == null)
        {
            GameObject reactionObj = new GameObject("ReactionSource");
            reactionObj.transform.SetParent(transform);
            reactionSource = reactionObj.AddComponent<AudioSource>();
            reactionSource.loop = false;
            reactionSource.playOnAwake = false;
        }
        
        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.SetParent(transform);
            uiSource = uiObj.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
        }
        
        if (voiceSource == null)
        {
            GameObject voiceObj = new GameObject("VoiceSource");
            voiceObj.transform.SetParent(transform);
            voiceSource = voiceObj.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
        }
        
        // Add to source library
        sourceLibrary["music"] = musicSource;
        sourceLibrary["sfx"] = sfxSource;
        sourceLibrary["ambient"] = ambientSource;
        sourceLibrary["reaction"] = reactionSource;
        sourceLibrary["ui"] = uiSource;
        sourceLibrary["voice"] = voiceSource;
        
        // Apply initial volumes
        UpdateAllVolumes();
    }
    
    /// <summary>
    /// Sets up the audio listener.
    /// </summary>
    private void SetupAudioListener()
    {
        audioListener = FindObjectOfType<AudioListener>();
        if (audioListener == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                audioListener = mainCamera.gameObject.AddComponent<AudioListener>();
            }
        }
    }
    
    /// <summary>
    /// Loads audio clips into the library.
    /// </summary>
    private void LoadAudioClips()
    {
        // Load music clips
        if (musicClips != null)
        {
            foreach (AudioClip clip in musicClips)
            {
                if (clip != null)
                {
                    clipLibrary[clip.name] = clip;
                }
            }
        }
        
        // Load ambient clips
        if (ambientClips != null)
        {
            foreach (AudioClip clip in ambientClips)
            {
                if (clip != null)
                {
                    clipLibrary[clip.name] = clip;
                }
            }
        }
        
        // Load UI clips
        if (uiClips != null)
        {
            foreach (AudioClip clip in uiClips)
            {
                if (clip != null)
                {
                    clipLibrary[clip.name] = clip;
                }
            }
        }
        
        // Load chemical clips
        if (chemicalClips != null)
        {
            foreach (AudioClip clip in chemicalClips)
            {
                if (clip != null)
                {
                    clipLibrary[clip.name] = clip;
                }
            }
        }
        
        // Load reaction clips
        if (reactionClips != null)
        {
            foreach (AudioClip clip in reactionClips)
            {
                if (clip != null)
                {
                    clipLibrary[clip.name] = clip;
                }
            }
        }
        
        // Load safety clips
        if (safetyClips != null)
        {
            foreach (AudioClip clip in safetyClips)
            {
                if (clip != null)
                {
                    clipLibrary[clip.name] = clip;
                }
            }
        }
        
        // Load equipment clips
        if (equipmentClips != null)
        {
            foreach (AudioClip clip in equipmentClips)
            {
                if (clip != null)
                {
                    clipLibrary[clip.name] = clip;
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Loaded {clipLibrary.Count} audio clips");
        }
    }
    
    /// <summary>
    /// Initializes the audio pool.
    /// </summary>
    private void InitializeAudioPool()
    {
        if (!enableAudioPooling) return;
        
        for (int i = 0; i < audioPoolSize; i++)
        {
            GameObject poolObj = new GameObject($"PooledAudio_{i}");
            poolObj.transform.SetParent(transform);
            AudioSource pooledSource = poolObj.AddComponent<AudioSource>();
            pooledSource.playOnAwake = false;
            pooledSource.loop = false;
            
            audioPool.Enqueue(pooledSource);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Initialized audio pool with {audioPoolSize} sources");
        }
    }
    
    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    public void PlaySFX(string clipName, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!enableAudio || string.IsNullOrEmpty(clipName)) return;
        
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null) return;
        
        AudioSource source = GetAudioSource("sfx");
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.pitch = pitch;
            source.Play();
            
            OnAudioPlayed?.Invoke(clipName);
            
            if (logAudioEvents)
            {
                Debug.Log($"Playing SFX: {clipName}");
            }
        }
    }
    
    /// <summary>
    /// Plays a 3D sound effect at a specific position.
    /// </summary>
    public void Play3DSFX(string clipName, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!enableAudio || !enable3DAudio || string.IsNullOrEmpty(clipName)) return;
        
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null) return;
        
        AudioSource source = GetPooledAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.pitch = pitch;
            source.transform.position = position;
            source.spatialBlend = 1.0f;
            source.maxDistance = maxDistance;
            source.minDistance = minDistance;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeFalloff);
            source.Play();
            
            activeSources.Add(source);
            
            OnAudioPlayed?.Invoke(clipName);
            
            if (logAudioEvents)
            {
                Debug.Log($"Playing 3D SFX: {clipName} at {position}");
            }
        }
    }
    
    /// <summary>
    /// Plays background music.
    /// </summary>
    public void PlayMusic(string clipName, bool fadeIn = true)
    {
        if (!enableAudio || string.IsNullOrEmpty(clipName)) return;
        
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null) return;
        
        if (musicSource != null)
        {
            if (fadeIn && musicSource.isPlaying)
            {
                StartCoroutine(FadeOutAndPlay(musicSource, clip, fadeTime));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
            }
            
            OnMusicChanged?.Invoke(clipName);
            
            if (logAudioEvents)
            {
                Debug.Log($"Playing music: {clipName}");
            }
        }
    }
    
    /// <summary>
    /// Plays ambient sounds.
    /// </summary>
    public void PlayAmbient(string clipName, bool fadeIn = true)
    {
        if (!enableAudio || string.IsNullOrEmpty(clipName)) return;
        
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null) return;
        
        if (ambientSource != null)
        {
            if (fadeIn && ambientSource.isPlaying)
            {
                StartCoroutine(FadeOutAndPlay(ambientSource, clip, fadeTime));
            }
            else
            {
                ambientSource.clip = clip;
                ambientSource.volume = ambientVolume * masterVolume;
                ambientSource.Play();
            }
            
            OnAmbientChanged?.Invoke(clipName);
            
            if (logAudioEvents)
            {
                Debug.Log($"Playing ambient: {clipName}");
            }
        }
    }
    
    /// <summary>
    /// Plays a chemical reaction sound.
    /// </summary>
    public void PlayReaction(string clipName, float volume = 1.0f)
    {
        if (!enableAudio || string.IsNullOrEmpty(clipName)) return;
        
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null) return;
        
        if (reactionSource != null)
        {
            reactionSource.clip = clip;
            reactionSource.volume = volume * reactionVolume * masterVolume;
            reactionSource.Play();
            
            OnAudioPlayed?.Invoke(clipName);
            
            if (logAudioEvents)
            {
                Debug.Log($"Playing reaction: {clipName}");
            }
        }
    }
    
    /// <summary>
    /// Plays a UI sound.
    /// </summary>
    public void PlayUI(string clipName, float volume = 1.0f)
    {
        if (!enableAudio || string.IsNullOrEmpty(clipName)) return;
        
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null) return;
        
        if (uiSource != null)
        {
            uiSource.clip = clip;
            uiSource.volume = volume * masterVolume;
            uiSource.Play();
            
            OnAudioPlayed?.Invoke(clipName);
            
            if (logAudioEvents)
            {
                Debug.Log($"Playing UI sound: {clipName}");
            }
        }
    }
    
    /// <summary>
    /// Plays a voice clip.
    /// </summary>
    public void PlayVoice(string clipName, float volume = 1.0f)
    {
        if (!enableAudio || string.IsNullOrEmpty(clipName)) return;
        
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null) return;
        
        if (voiceSource != null)
        {
            voiceSource.clip = clip;
            voiceSource.volume = volume * masterVolume;
            voiceSource.Play();
            
            OnAudioPlayed?.Invoke(clipName);
            
            if (logAudioEvents)
            {
                Debug.Log($"Playing voice: {clipName}");
            }
        }
    }
    
    /// <summary>
    /// Stops all audio.
    /// </summary>
    public void StopAllAudio()
    {
        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        if (ambientSource != null) ambientSource.Stop();
        if (reactionSource != null) reactionSource.Stop();
        if (uiSource != null) uiSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
        
        // Stop pooled sources
        foreach (AudioSource source in activeSources)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
        
        if (logAudioEvents)
        {
            Debug.Log("All audio stopped");
        }
    }
    
    /// <summary>
    /// Pauses all audio.
    /// </summary>
    public void PauseAllAudio()
    {
        if (musicSource != null) musicSource.Pause();
        if (sfxSource != null) sfxSource.Pause();
        if (ambientSource != null) ambientSource.Pause();
        if (reactionSource != null) reactionSource.Pause();
        if (uiSource != null) uiSource.Pause();
        if (voiceSource != null) voiceSource.Pause();
        
        foreach (AudioSource source in activeSources)
        {
            if (source != null)
            {
                source.Pause();
            }
        }
        
        if (logAudioEvents)
        {
            Debug.Log("All audio paused");
        }
    }
    
    /// <summary>
    /// Resumes all audio.
    /// </summary>
    public void ResumeAllAudio()
    {
        if (musicSource != null) musicSource.UnPause();
        if (sfxSource != null) sfxSource.UnPause();
        if (ambientSource != null) ambientSource.UnPause();
        if (reactionSource != null) reactionSource.UnPause();
        if (uiSource != null) uiSource.UnPause();
        if (voiceSource != null) voiceSource.UnPause();
        
        foreach (AudioSource source in activeSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
        
        if (logAudioEvents)
        {
            Debug.Log("All audio resumed");
        }
    }
    
    /// <summary>
    /// Sets the master volume.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnVolumeChanged?.Invoke(masterVolume);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Master volume set to: {masterVolume}");
        }
    }
    
    /// <summary>
    /// Sets the music volume.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        categoryVolumes["music"] = musicVolume;
        UpdateSourceVolume(musicSource, musicVolume);
        OnVolumeChanged?.Invoke(musicVolume);
    }
    
    /// <summary>
    /// Sets the SFX volume.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        categoryVolumes["sfx"] = sfxVolume;
        UpdateSourceVolume(sfxSource, sfxVolume);
        OnVolumeChanged?.Invoke(sfxVolume);
    }
    
    /// <summary>
    /// Sets the ambient volume.
    /// </summary>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        categoryVolumes["ambient"] = ambientVolume;
        UpdateSourceVolume(ambientSource, ambientVolume);
        OnVolumeChanged?.Invoke(ambientVolume);
    }
    
    /// <summary>
    /// Sets the reaction volume.
    /// </summary>
    public void SetReactionVolume(float volume)
    {
        reactionVolume = Mathf.Clamp01(volume);
        categoryVolumes["reaction"] = reactionVolume;
        UpdateSourceVolume(reactionSource, reactionVolume);
        OnVolumeChanged?.Invoke(reactionVolume);
    }
    
    /// <summary>
    /// Enables or disables audio.
    /// </summary>
    public void SetAudioEnabled(bool enabled)
    {
        enableAudio = enabled;
        
        if (!enabled)
        {
            StopAllAudio();
        }
        
        OnAudioEnabled?.Invoke(enabled);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Audio {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Gets an audio clip from the library.
    /// </summary>
    private AudioClip GetAudioClip(string clipName)
    {
        if (clipLibrary.ContainsKey(clipName))
        {
            return clipLibrary[clipName];
        }
        
        if (enableDebugLogging)
        {
            Debug.LogWarning($"Audio clip not found: {clipName}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets an audio source by category.
    /// </summary>
    private AudioSource GetAudioSource(string category)
    {
        if (sourceLibrary.ContainsKey(category))
        {
            return sourceLibrary[category];
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets a pooled audio source.
    /// </summary>
    private AudioSource GetPooledAudioSource()
    {
        if (!enableAudioPooling || audioPool.Count == 0)
        {
            return null;
        }
        
        return audioPool.Dequeue();
    }
    
    /// <summary>
    /// Returns an audio source to the pool.
    /// </summary>
    private void ReturnToPool(AudioSource source)
    {
        if (enableAudioPooling && source != null)
        {
            source.Stop();
            source.clip = null;
            source.spatialBlend = 0f;
            audioPool.Enqueue(source);
            activeSources.Remove(source);
        }
    }
    
    /// <summary>
    /// Updates all audio source volumes.
    /// </summary>
    private void UpdateAllVolumes()
    {
        UpdateSourceVolume(musicSource, musicVolume);
        UpdateSourceVolume(sfxSource, sfxVolume);
        UpdateSourceVolume(ambientSource, ambientVolume);
        UpdateSourceVolume(reactionSource, reactionVolume);
        UpdateSourceVolume(uiSource, 1.0f);
        UpdateSourceVolume(voiceSource, 1.0f);
    }
    
    /// <summary>
    /// Updates a specific audio source volume.
    /// </summary>
    private void UpdateSourceVolume(AudioSource source, float categoryVolume)
    {
        if (source != null)
        {
            source.volume = categoryVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// Updates audio sources.
    /// </summary>
    private void UpdateAudioSources()
    {
        // Check for finished pooled sources
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeSources[i];
            if (source != null && !source.isPlaying)
            {
                ReturnToPool(source);
            }
        }
    }
    
    /// <summary>
    /// Handles distance-based audio culling.
    /// </summary>
    private void HandleDistanceCulling()
    {
        if (!enableDistanceCulling || mainCamera == null) return;
        
        Vector3 cameraPosition = mainCamera.transform.position;
        
        foreach (AudioSource source in activeSources)
        {
            if (source != null)
            {
                float distance = Vector3.Distance(cameraPosition, source.transform.position);
                
                if (distance > cullingDistance)
                {
                    source.mute = true;
                }
                else
                {
                    source.mute = false;
                }
            }
        }
    }
    
    /// <summary>
    /// Updates 3D audio settings.
    /// </summary>
    private void Update3DAudio()
    {
        if (!enable3DAudio) return;
        
        // Update listener position if camera changed
        if (mainCamera != null && audioListener != null)
        {
            audioListener.transform.position = mainCamera.transform.position;
        }
    }
    
    /// <summary>
    /// Fades out current audio and plays new clip.
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndPlay(AudioSource source, AudioClip newClip, float fadeTime)
    {
        float startVolume = source.volume;
        
        // Fade out
        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        
        // Play new clip
        source.clip = newClip;
        source.Play();
        
        // Fade in
        while (source.volume < startVolume)
        {
            source.volume += startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        
        source.volume = startVolume;
    }
    
    // Public getters
    public bool IsAudioEnabled() => enableAudio;
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetAmbientVolume() => ambientVolume;
    public float GetReactionVolume() => reactionVolume;
    public bool IsInitialized() => isInitialized;
    
    /// <summary>
    /// Logs the current audio manager status.
    /// </summary>
    public void LogAudioStatus()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== Audio Manager Status ===");
        Debug.Log($"Audio Enabled: {enableAudio}");
        Debug.Log($"Master Volume: {masterVolume}");
        Debug.Log($"Music Volume: {musicVolume}");
        Debug.Log($"SFX Volume: {sfxVolume}");
        Debug.Log($"Ambient Volume: {ambientVolume}");
        Debug.Log($"Reaction Volume: {reactionVolume}");
        Debug.Log($"3D Audio: {(enable3DAudio ? "Enabled" : "Disabled")}");
        Debug.Log($"Audio Pooling: {(enableAudioPooling ? "Enabled" : "Disabled")}");
        Debug.Log($"Active Sources: {activeSources.Count}");
        Debug.Log($"Pooled Sources: {audioPool.Count}");
        Debug.Log($"Clip Library Size: {clipLibrary.Count}");
        Debug.Log("===========================");
    }
} 