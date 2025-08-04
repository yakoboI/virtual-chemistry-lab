using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Manages multiplayer functionality for collaborative laboratory sessions.
/// Handles real-time synchronization, user management, and collaborative features.
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    [SerializeField] private bool enableMultiplayer = true;
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private float syncInterval = 0.1f;
    [SerializeField] private bool enableVoiceChat = true;
    [SerializeField] private bool enableTextChat = true;
    
    [Header("Session Management")]
    [SerializeField] private string sessionId = "";
    [SerializeField] private string currentExperimentId = "";
    [SerializeField] private bool isHost = false;
    [SerializeField] private bool isSessionActive = false;
    
    [Header("Player Management")]
    [SerializeField] private List<PlayerData> connectedPlayers = new List<PlayerData>();
    [SerializeField] private Dictionary<string, PlayerData> playerLookup = new Dictionary<string, PlayerData>();
    [SerializeField] private string localPlayerId = "";
    
    [Header("Collaboration Features")]
    [SerializeField] private bool enableSharedEquipment = true;
    [SerializeField] private bool enableSharedData = true;
    [SerializeField] private bool enablePeerAssessment = true;
    [SerializeField] private bool enableTeacherOverride = true;
    
    [Header("Network Settings")]
    [SerializeField] private string serverAddress = "localhost";
    [SerializeField] private int serverPort = 7777;
    [SerializeField] private bool useReliableTransport = true;
    [SerializeField] private float connectionTimeout = 30f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showPlayerInfo = false;
    [SerializeField] private bool logNetworkEvents = false;
    
    private static MultiplayerManager instance;
    public static MultiplayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MultiplayerManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("MultiplayerManager");
                    instance = go.AddComponent<MultiplayerManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events for other systems to listen to
    public event Action<string> OnPlayerJoined;
    public event Action<string> OnPlayerLeft;
    public event Action<string, string> OnPlayerMessage;
    public event Action<string, object> OnDataShared;
    public event Action<string> OnEquipmentShared;
    public event Action<string, float> OnPeerAssessment;
    public event Action<bool> OnSessionStateChanged;
    public event Action<string> OnConnectionError;
    
    [System.Serializable]
    public class PlayerData
    {
        public string playerId;
        public string playerName;
        public string role; // Student, Teacher, Observer
        public bool isOnline;
        public float lastSeen;
        public Vector3 position;
        public string currentEquipment;
        public Dictionary<string, object> sharedData;
        public float assessmentScore;
        public List<string> completedSteps;
        
        public PlayerData(string id, string name, string playerRole)
        {
            playerId = id;
            playerName = name;
            role = playerRole;
            isOnline = true;
            lastSeen = Time.time;
            position = Vector3.zero;
            currentEquipment = "";
            sharedData = new Dictionary<string, object>();
            assessmentScore = 0f;
            completedSteps = new List<string>();
        }
    }
    
    [System.Serializable]
    public class SessionData
    {
        public string sessionId;
        public string experimentId;
        public string hostId;
        public List<string> playerIds;
        public Dictionary<string, object> sharedExperimentData;
        public float sessionStartTime;
        public bool isActive;
        
        public SessionData(string id, string expId, string host)
        {
            sessionId = id;
            experimentId = expId;
            hostId = host;
            playerIds = new List<string>();
            sharedExperimentData = new Dictionary<string, object>();
            sessionStartTime = Time.time;
            isActive = true;
        }
    }
    
    private SessionData currentSession;
    private Coroutine syncCoroutine;
    private Dictionary<string, object> pendingSyncData = new Dictionary<string, object>();
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMultiplayerManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enableMultiplayer)
        {
            SetupNetworkConnection();
        }
    }
    
    private void Update()
    {
        HandleInput();
        UpdatePlayerStatus();
    }
    
    /// <summary>
    /// Initializes the multiplayer manager with basic settings.
    /// </summary>
    private void InitializeMultiplayerManager()
    {
        if (enableDebugLogging)
        {
            Debug.Log("MultiplayerManager initialized successfully");
        }
        
        // Generate local player ID
        localPlayerId = System.Guid.NewGuid().ToString();
        
        // Initialize collections
        connectedPlayers = new List<PlayerData>();
        playerLookup = new Dictionary<string, PlayerData>();
        pendingSyncData = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Sets up network connection for multiplayer functionality.
    /// </summary>
    private void SetupNetworkConnection()
    {
        if (enableDebugLogging)
        {
            Debug.Log($"Setting up network connection to {serverAddress}:{serverPort}");
        }
        
        // Initialize network transport
        // This would integrate with Unity's networking system or third-party networking solution
        // For now, we'll simulate network functionality
        
        StartCoroutine(SimulateNetworkConnection());
    }
    
    /// <summary>
    /// Simulates network connection for development purposes.
    /// </summary>
    private System.Collections.IEnumerator SimulateNetworkConnection()
    {
        yield return new WaitForSeconds(1f);
        
        if (enableDebugLogging)
        {
            Debug.Log("Network connection established (simulated)");
        }
        
        // Add local player to connected players
        AddPlayer(localPlayerId, "Local Player", "Student");
    }
    
    /// <summary>
    /// Creates a new multiplayer session.
    /// </summary>
    public void CreateSession(string experimentId, string sessionName = "")
    {
        if (isSessionActive)
        {
            Debug.LogWarning("Session already active. Cannot create new session.");
            return;
        }
        
        sessionId = System.Guid.NewGuid().ToString();
        currentExperimentId = experimentId;
        isHost = true;
        
        currentSession = new SessionData(sessionId, experimentId, localPlayerId);
        currentSession.playerIds.Add(localPlayerId);
        
        isSessionActive = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Created session: {sessionId} for experiment: {experimentId}");
        }
        
        OnSessionStateChanged?.Invoke(true);
    }
    
    /// <summary>
    /// Joins an existing multiplayer session.
    /// </summary>
    public void JoinSession(string sessionId, string playerName, string role = "Student")
    {
        if (isSessionActive)
        {
            Debug.LogWarning("Already in a session. Cannot join another session.");
            return;
        }
        
        this.sessionId = sessionId;
        isHost = false;
        
        // Simulate joining session
        StartCoroutine(SimulateJoinSession(sessionId, playerName, role));
    }
    
    /// <summary>
    /// Simulates joining a session.
    /// </summary>
    private System.Collections.IEnumerator SimulateJoinSession(string sessionId, string playerName, string role)
    {
        yield return new WaitForSeconds(0.5f);
        
        currentSession = new SessionData(sessionId, "unknown", "host");
        currentSession.playerIds.Add(localPlayerId);
        
        AddPlayer(localPlayerId, playerName, role);
        
        isSessionActive = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Joined session: {sessionId} as {playerName}");
        }
        
        OnSessionStateChanged?.Invoke(true);
    }
    
    /// <summary>
    /// Adds a player to the session.
    /// </summary>
    public void AddPlayer(string playerId, string playerName, string role)
    {
        if (playerLookup.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player {playerId} already exists.");
            return;
        }
        
        PlayerData player = new PlayerData(playerId, playerName, role);
        connectedPlayers.Add(player);
        playerLookup[playerId] = player;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Player added: {playerName} ({role})");
        }
        
        OnPlayerJoined?.Invoke(playerId);
    }
    
    /// <summary>
    /// Removes a player from the session.
    /// </summary>
    public void RemovePlayer(string playerId)
    {
        if (!playerLookup.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player {playerId} not found.");
            return;
        }
        
        PlayerData player = playerLookup[playerId];
        connectedPlayers.Remove(player);
        playerLookup.Remove(playerId);
        
        if (enableDebugLogging)
        {
            Debug.Log($"Player removed: {player.playerName}");
        }
        
        OnPlayerLeft?.Invoke(playerId);
    }
    
    /// <summary>
    /// Sends a message to all players in the session.
    /// </summary>
    public void SendMessage(string message)
    {
        if (!isSessionActive)
        {
            Debug.LogWarning("No active session. Cannot send message.");
            return;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Sending message: {message}");
        }
        
        // Simulate message broadcast
        foreach (PlayerData player in connectedPlayers)
        {
            if (player.playerId != localPlayerId)
            {
                OnPlayerMessage?.Invoke(player.playerId, message);
            }
        }
    }
    
    /// <summary>
    /// Shares data with all players in the session.
    /// </summary>
    public void ShareData(string dataKey, object dataValue)
    {
        if (!isSessionActive)
        {
            Debug.LogWarning("No active session. Cannot share data.");
            return;
        }
        
        if (currentSession != null)
        {
            currentSession.sharedExperimentData[dataKey] = dataValue;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Sharing data: {dataKey} = {dataValue}");
        }
        
        OnDataShared?.Invoke(dataKey, dataValue);
    }
    
    /// <summary>
    /// Shares equipment usage with other players.
    /// </summary>
    public void ShareEquipment(string equipmentId, string action)
    {
        if (!isSessionActive || !enableSharedEquipment)
        {
            return;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"Sharing equipment: {equipmentId} - {action}");
        }
        
        OnEquipmentShared?.Invoke(equipmentId);
    }
    
    /// <summary>
    /// Submits peer assessment for another player.
    /// </summary>
    public void SubmitPeerAssessment(string targetPlayerId, float score, string feedback = "")
    {
        if (!isSessionActive || !enablePeerAssessment)
        {
            Debug.LogWarning("Peer assessment not enabled or no active session.");
            return;
        }
        
        if (!playerLookup.ContainsKey(targetPlayerId))
        {
            Debug.LogWarning($"Target player {targetPlayerId} not found.");
            return;
        }
        
        PlayerData targetPlayer = playerLookup[targetPlayerId];
        targetPlayer.assessmentScore = score;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Peer assessment submitted for {targetPlayer.playerName}: {score}");
        }
        
        OnPeerAssessment?.Invoke(targetPlayerId, score);
    }
    
    /// <summary>
    /// Updates player status and handles disconnections.
    /// </summary>
    private void UpdatePlayerStatus()
    {
        float currentTime = Time.time;
        
        foreach (PlayerData player in connectedPlayers.ToArray())
        {
            if (player.playerId != localPlayerId)
            {
                // Check for player timeout
                if (currentTime - player.lastSeen > connectionTimeout)
                {
                    player.isOnline = false;
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"Player {player.playerName} timed out");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Handles input for multiplayer features.
    /// </summary>
    private void HandleInput()
    {
        if (!isSessionActive)
            return;
        
        // Chat input
        if (enableTextChat && Input.GetKeyDown(KeyCode.Return))
        {
            // This would open chat input UI
            if (enableDebugLogging)
            {
                Debug.Log("Chat input activated");
            }
        }
        
        // Voice chat toggle
        if (enableVoiceChat && Input.GetKeyDown(KeyCode.V))
        {
            ToggleVoiceChat();
        }
    }
    
    /// <summary>
    /// Toggles voice chat functionality.
    /// </summary>
    public void ToggleVoiceChat()
    {
        if (enableDebugLogging)
        {
            Debug.Log("Voice chat toggled");
        }
        
        // This would integrate with voice chat system
    }
    
    /// <summary>
    /// Starts data synchronization between players.
    /// </summary>
    public void StartDataSync()
    {
        if (syncCoroutine != null)
        {
            StopCoroutine(syncCoroutine);
        }
        
        syncCoroutine = StartCoroutine(SyncDataCoroutine());
    }
    
    /// <summary>
    /// Coroutine for periodic data synchronization.
    /// </summary>
    private System.Collections.IEnumerator SyncDataCoroutine()
    {
        while (isSessionActive)
        {
            yield return new WaitForSeconds(syncInterval);
            
            // Sync player positions and equipment usage
            SyncPlayerData();
            
            // Sync experiment data
            SyncExperimentData();
        }
    }
    
    /// <summary>
    /// Synchronizes player data across the network.
    /// </summary>
    private void SyncPlayerData()
    {
        if (currentSession == null)
            return;
        
        // This would send player data to other clients
        if (enableDebugLogging && logNetworkEvents)
        {
            Debug.Log("Player data synchronized");
        }
    }
    
    /// <summary>
    /// Synchronizes experiment data across the network.
    /// </summary>
    private void SyncExperimentData()
    {
        if (currentSession == null)
            return;
        
        // This would send experiment data to other clients
        if (enableDebugLogging && logNetworkEvents)
        {
            Debug.Log("Experiment data synchronized");
        }
    }
    
    /// <summary>
    /// Ends the current multiplayer session.
    /// </summary>
    public void EndSession()
    {
        if (!isSessionActive)
        {
            Debug.LogWarning("No active session to end.");
            return;
        }
        
        if (syncCoroutine != null)
        {
            StopCoroutine(syncCoroutine);
            syncCoroutine = null;
        }
        
        isSessionActive = false;
        isHost = false;
        currentSession = null;
        
        // Clear player data
        connectedPlayers.Clear();
        playerLookup.Clear();
        
        if (enableDebugLogging)
        {
            Debug.Log("Session ended");
        }
        
        OnSessionStateChanged?.Invoke(false);
    }
    
    /// <summary>
    /// Gets the current session information.
    /// </summary>
    public SessionData GetCurrentSession()
    {
        return currentSession;
    }
    
    /// <summary>
    /// Gets all connected players.
    /// </summary>
    public List<PlayerData> GetConnectedPlayers()
    {
        return new List<PlayerData>(connectedPlayers);
    }
    
    /// <summary>
    /// Gets a specific player by ID.
    /// </summary>
    public PlayerData GetPlayer(string playerId)
    {
        if (playerLookup.ContainsKey(playerId))
        {
            return playerLookup[playerId];
        }
        return null;
    }
    
    /// <summary>
    /// Gets the local player data.
    /// </summary>
    public PlayerData GetLocalPlayer()
    {
        return GetPlayer(localPlayerId);
    }
    
    /// <summary>
    /// Checks if the local player is the host.
    /// </summary>
    public bool IsHost()
    {
        return isHost;
    }
    
    /// <summary>
    /// Checks if a session is currently active.
    /// </summary>
    public bool IsSessionActive()
    {
        return isSessionActive;
    }
    
    /// <summary>
    /// Gets the current session ID.
    /// </summary>
    public string GetSessionId()
    {
        return sessionId;
    }
    
    /// <summary>
    /// Logs multiplayer status for debugging.
    /// </summary>
    public void LogMultiplayerStatus()
    {
        if (!enableDebugLogging)
            return;
        
        Debug.Log("=== Multiplayer Status ===");
        Debug.Log($"Session Active: {isSessionActive}");
        Debug.Log($"Session ID: {sessionId}");
        Debug.Log($"Is Host: {isHost}");
        Debug.Log($"Connected Players: {connectedPlayers.Count}");
        
        foreach (PlayerData player in connectedPlayers)
        {
            Debug.Log($"- {player.playerName} ({player.role}) - Online: {player.isOnline}");
        }
        
        Debug.Log("========================");
    }
} 