using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
    [Header("Network Settings")]
    public int maxPlayers = 4;
    public float networkTickRate = 60f;
    public bool isHost = false;
    public bool isClient = false;
    
    [Header("Connection Settings")]
    public string serverIP = "127.0.0.1";
    public ushort serverPort = 7777;
    public string roomCode = "";
    
    [Header("Player Management")]
    public GameObject networkPlayerPrefab;
    public List<NetworkPlayer> connectedPlayers = new List<NetworkPlayer>();
    public Dictionary<ulong, PlayerData> playerDataDict = new Dictionary<ulong, PlayerData>();
    
    [Header("Game Sync")]
    public GameManager.GameState networkGameState = GameManager.GameState.WaitingForPlayers;
    public int networkCurrentPlayer = 0;
    public int networkCurrentRound = 1;
    
    [Header("Events")]
    public System.Action<ulong> OnPlayerConnected;
    public System.Action<ulong> OnPlayerDisconnected;
    public System.Action<GameManager.GameState> OnGameStateChanged;
    
    private bool networkInitialized = false;
    private Coroutine reconnectCoroutine;
    
    [System.Serializable]
    public class PlayerData
    {
        public ulong clientId;
        public string playerName;
        public int playerColor;
        public int money;
        public int currentSpace;
        public bool isReady;
        
        public PlayerData()
        {
            clientId = 0;
            playerName = "Player";
            playerColor = 0;
            money = 1500;
            currentSpace = 0;
            isReady = false;
        }
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNetworking();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeNetworking()
    {
        // Initialize networking system (simplified version)
        networkInitialized = true;
        Debug.Log("Network Manager initialized (Local Mode)");
    }
    
    #region Server/Host Methods
    
    public bool StartHost()
    {
        if (!networkInitialized) return false;
        
        isHost = true;
        isClient = true;
        roomCode = GenerateRoomCode();
        
        // Simulate host starting
        networkGameState = GameManager.GameState.WaitingForPlayers;
        
        Debug.Log($"Host started successfully! Room Code: {roomCode}");
        return true;
    }
    
    public bool StartServer()
    {
        if (!networkInitialized) return false;
        
        isHost = true;
        roomCode = GenerateRoomCode();
        
        // Simulate server starting
        networkGameState = GameManager.GameState.WaitingForPlayers;
        
        Debug.Log($"Server started successfully! Room Code: {roomCode}");
        return true;
    }
    
    public void StopHost()
    {
        isHost = false;
        isClient = false;
        ClearPlayerData();
        
        Debug.Log("Host stopped");
    }
    
    #endregion
    
    #region Client Methods
    
    public bool StartClient(string targetIP = "", ushort targetPort = 0)
    {
        if (!networkInitialized) return false;
        
        if (!string.IsNullOrEmpty(targetIP))
            serverIP = targetIP;
        if (targetPort > 0)
            serverPort = targetPort;
        
        isClient = true;
        
        // Simulate client connection
        StartCoroutine(SimulateClientConnection());
        
        Debug.Log($"Connecting to server at {serverIP}:{serverPort}");
        return true;
    }
    
    private IEnumerator SimulateClientConnection()
    {
        yield return new WaitForSeconds(1f);
        
        // Simulate successful connection
        ulong clientId = (ulong)Random.Range(1000, 9999);
        SimulateClientConnected(clientId);
    }
    
    public bool JoinRoom(string code)
    {
        roomCode = code;
        return StartClient();
    }
    
    public void DisconnectClient()
    {
        isClient = false;
        ClearPlayerData();
        
        Debug.Log("Client disconnected");
    }
    
    #endregion
    
    #region Network Simulation
    
    private void SimulateClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected (simulated)");
        
        if (isHost)
        {
            // Add player data for the new client
            PlayerData newPlayer = new PlayerData
            {
                clientId = clientId,
                playerName = $"Player {clientId}",
                playerColor = (int)clientId % 4,
                money = GameManager.Instance != null ? GameManager.Instance.startingMoney : 1500,
                currentSpace = 0,
                isReady = false
            };
            
            playerDataDict[clientId] = newPlayer;
            UpdatePlayerList();
            
            OnPlayerConnected?.Invoke(clientId);
        }
    }
    
    private void SimulateClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected (simulated)");
        
        if (isHost)
        {
            if (playerDataDict.ContainsKey(clientId))
            {
                playerDataDict.Remove(clientId);
                UpdatePlayerList();
            }
        }
        
        OnPlayerDisconnected?.Invoke(clientId);
    }
    
    #endregion
    
    #region Game Synchronization
    
    public void SetPlayerReady(bool ready)
    {
        ulong clientId = 0; // In a real implementation, this would be the actual client ID
        
        if (playerDataDict.ContainsKey(clientId))
        {
            playerDataDict[clientId].isReady = ready;
            UpdatePlayerList();
            
            if (AllPlayersReady() && connectedPlayers.Count >= 2)
            {
                StartGame();
            }
        }
    }
    
    public void UpdatePlayerData(PlayerData playerData)
    {
        ulong clientId = playerData.clientId;
        
        if (playerDataDict.ContainsKey(clientId))
        {
            playerDataDict[clientId] = playerData;
            SyncPlayerData(playerData);
        }
    }
    
    public void RequestDiceRoll()
    {
        ulong clientId = 0; // In a real implementation, this would be the actual client ID
        
        if (clientId == (ulong)networkCurrentPlayer)
        {
            int diceResult = Random.Range(1, 7);
            ProcessDiceRoll(diceResult, clientId);
        }
    }
    
    public void ProcessPropertyAction(int spaceIndex, PropertyAction action)
    {
        ulong clientId = 0; // In a real implementation, this would be the actual client ID
        ProcessPropertyActionForPlayer(spaceIndex, action, clientId);
    }
    
    public void StartMiniGame(GameManager.MiniGameType gameType, ulong[] participants)
    {
        StartMiniGameForPlayers(gameType, participants);
    }
    
    #endregion
    
    #region Local Methods
    
    private void UpdatePlayerList()
    {
        if (UI_GameManager.Instance != null)
        {
            UI_GameManager.Instance.UpdatePlayerList(playerDataDict);
        }
    }
    
    private void StartGame()
    {
        networkGameState = GameManager.GameState.Playing;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNetworkGame();
        }
    }
    
    private void ProcessDiceRoll(int diceResult, ulong playerId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessNetworkDiceRoll(diceResult, playerId);
        }
    }
    
    private void ProcessPropertyActionForPlayer(int spaceIndex, PropertyAction action, ulong playerId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessNetworkPropertyAction(spaceIndex, action, playerId);
        }
    }
    
    private void StartMiniGameForPlayers(GameManager.MiniGameType gameType, ulong[] participants)
    {
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.StartNetworkMiniGame(gameType, participants);
        }
    }
    
    private void SyncPlayerData(PlayerData playerData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SyncNetworkPlayerData(playerData);
        }
    }
    
    private void HandlePlayerDisconnection(ulong disconnectedPlayerId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandlePlayerDisconnection(disconnectedPlayerId);
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    private bool AllPlayersReady()
    {
        foreach (var player in playerDataDict.Values)
        {
            if (!player.isReady) return false;
        }
        return true;
    }
    
    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        var result = new char[6];
        
        for (int i = 0; i < 6; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(result);
    }
    
    private void ClearPlayerData()
    {
        connectedPlayers.Clear();
        playerDataDict.Clear();
        roomCode = "";
    }
    
    public bool IsNetworkGame()
    {
        return isHost || isClient;
    }
    
    public PlayerData GetPlayerData(ulong clientId)
    {
        return playerDataDict.ContainsKey(clientId) ? playerDataDict[clientId] : null;
    }
    
    public List<PlayerData> GetAllPlayerData()
    {
        return new List<PlayerData>(playerDataDict.Values);
    }
    
    #endregion
    
    #region Reconnection System
    
    public void StartReconnectionAttempt()
    {
        if (reconnectCoroutine == null)
        {
            reconnectCoroutine = StartCoroutine(ReconnectionCoroutine());
        }
    }
    
    private IEnumerator ReconnectionCoroutine()
    {
        int attempts = 0;
        const int maxAttempts = 5;
        const float retryDelay = 3f;
        
        while (attempts < maxAttempts)
        {
            attempts++;
            Debug.Log($"Reconnection attempt {attempts}/{maxAttempts}");
            
            if (StartClient())
            {
                Debug.Log("Reconnection successful!");
                yield break;
            }
            
            yield return new WaitForSeconds(retryDelay);
        }
        
        Debug.Log("Failed to reconnect after maximum attempts");
        reconnectCoroutine = null;
    }
    
    #endregion
    
    // Test methods for local multiplayer simulation
    public void SimulateLocalMultiplayer()
    {
        if (!IsNetworkGame())
        {
            StartHost();
            
            // Add simulated players
            for (int i = 1; i <= 3; i++)
            {
                ulong clientId = (ulong)(1000 + i);
                SimulateClientConnected(clientId);
            }
        }
    }
    
    void OnDestroy()
    {
        if (reconnectCoroutine != null)
        {
            StopCoroutine(reconnectCoroutine);
        }
    }
}

public enum PropertyAction
{
    Buy,
    Sell,
    Challenge
}

public class NetworkPlayer : MonoBehaviour
{
    public ulong playerId;
    public int playerMoney;
    public int currentSpace;
    public bool isMyTurn;
    
    public void Initialize(ulong id)
    {
        playerId = id;
        playerMoney = 1500;
        currentSpace = 0;
        isMyTurn = false;
    }
    
    public void UpdatePlayerState(int money, int space, bool turn)
    {
        playerMoney = money;
        currentSpace = space;
        isMyTurn = turn;
    }
}