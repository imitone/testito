using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode.Transports.UTP;

public class NetworkManager : NetworkBehaviour
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
    public NetworkVariable<GameState> networkGameState = new NetworkVariable<GameState>(GameState.WaitingForPlayers);
    public NetworkVariable<int> networkCurrentPlayer = new NetworkVariable<int>(0);
    public NetworkVariable<int> networkCurrentRound = new NetworkVariable<int>(1);
    
    [Header("Events")]
    public System.Action<ulong> OnPlayerConnected;
    public System.Action<ulong> OnPlayerDisconnected;
    public System.Action<GameState> OnGameStateChanged;
    
    private Unity.Netcode.NetworkManager netManager;
    private UnityTransport transport;
    private Coroutine reconnectCoroutine;
    
    [System.Serializable]
    public class PlayerData : INetworkSerializable
    {
        public ulong clientId;
        public string playerName;
        public int playerColor;
        public int money;
        public int currentSpace;
        public bool isReady;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerColor);
            serializer.SerializeValue(ref money);
            serializer.SerializeValue(ref currentSpace);
            serializer.SerializeValue(ref isReady);
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
        netManager = GetComponent<Unity.Netcode.NetworkManager>();
        transport = GetComponent<UnityTransport>();
        
        if (netManager != null)
        {
            netManager.OnClientConnectedCallback += OnClientConnected;
            netManager.OnClientDisconnectCallback += OnClientDisconnected;
            netManager.OnServerStarted += OnServerStarted;
        }
        
        networkGameState.OnValueChanged += OnNetworkGameStateChanged;
    }
    
    #region Server/Host Methods
    
    public bool StartHost()
    {
        if (netManager == null) return false;
        
        transport.SetConnectionData(serverIP, serverPort);
        bool result = netManager.StartHost();
        
        if (result)
        {
            isHost = true;
            isClient = true;
            roomCode = GenerateRoomCode();
            Debug.Log($"Host started successfully! Room Code: {roomCode}");
        }
        
        return result;
    }
    
    public bool StartServer()
    {
        if (netManager == null) return false;
        
        transport.SetConnectionData(serverIP, serverPort);
        bool result = netManager.StartServer();
        
        if (result)
        {
            isHost = true;
            roomCode = GenerateRoomCode();
            Debug.Log($"Server started successfully! Room Code: {roomCode}");
        }
        
        return result;
    }
    
    public void StopHost()
    {
        if (netManager != null && netManager.IsHost)
        {
            netManager.Shutdown();
            isHost = false;
            isClient = false;
            ClearPlayerData();
        }
    }
    
    #endregion
    
    #region Client Methods
    
    public bool StartClient(string targetIP = "", ushort targetPort = 0)
    {
        if (netManager == null) return false;
        
        if (!string.IsNullOrEmpty(targetIP))
            serverIP = targetIP;
        if (targetPort > 0)
            serverPort = targetPort;
        
        transport.SetConnectionData(serverIP, serverPort);
        bool result = netManager.StartClient();
        
        if (result)
        {
            isClient = true;
            Debug.Log($"Connecting to server at {serverIP}:{serverPort}");
        }
        
        return result;
    }
    
    public bool JoinRoom(string code)
    {
        // In a full implementation, this would connect to a matchmaking service
        // For now, we'll use direct IP connection
        roomCode = code;
        return StartClient();
    }
    
    public void DisconnectClient()
    {
        if (netManager != null && netManager.IsClient)
        {
            netManager.Shutdown();
            isClient = false;
            ClearPlayerData();
        }
    }
    
    #endregion
    
    #region Network Callbacks
    
    private void OnServerStarted()
    {
        Debug.Log("Server started successfully!");
        networkGameState.Value = GameState.WaitingForPlayers;
    }
    
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
        
        if (IsServer)
        {
            // Add player data for the new client
            PlayerData newPlayer = new PlayerData
            {
                clientId = clientId,
                playerName = $"Player {clientId}",
                playerColor = (int)clientId % 4,
                money = GameManager.Instance.startingMoney,
                currentSpace = 0,
                isReady = false
            };
            
            playerDataDict[clientId] = newPlayer;
            UpdatePlayerListClientRpc();
            
            // Check if we can start the game
            if (connectedPlayers.Count >= 2 && AllPlayersReady())
            {
                StartGameClientRpc();
            }
        }
        
        OnPlayerConnected?.Invoke(clientId);
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
        
        if (IsServer)
        {
            if (playerDataDict.ContainsKey(clientId))
            {
                playerDataDict.Remove(clientId);
                UpdatePlayerListClientRpc();
            }
            
            // Handle player disconnection during game
            if (networkGameState.Value == GameState.Playing)
            {
                HandlePlayerDisconnectionClientRpc(clientId);
            }
        }
        
        OnPlayerDisconnected?.Invoke(clientId);
    }
    
    private void OnNetworkGameStateChanged(GameState previous, GameState current)
    {
        Debug.Log($"Game state changed from {previous} to {current}");
        OnGameStateChanged?.Invoke(current);
    }
    
    #endregion
    
    #region Game Synchronization
    
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(bool ready, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        if (playerDataDict.ContainsKey(clientId))
        {
            playerDataDict[clientId].isReady = ready;
            UpdatePlayerListClientRpc();
            
            if (AllPlayersReady() && connectedPlayers.Count >= 2)
            {
                StartGameClientRpc();
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerDataServerRpc(PlayerData playerData, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        if (playerDataDict.ContainsKey(clientId))
        {
            playerDataDict[clientId] = playerData;
            SyncPlayerDataClientRpc(playerData);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestDiceRollServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        if (clientId == (ulong)networkCurrentPlayer.Value)
        {
            int diceResult = Random.Range(1, 7);
            ProcessDiceRollClientRpc(diceResult, clientId);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ProcessPropertyActionServerRpc(int spaceIndex, PropertyAction action, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        ProcessPropertyActionClientRpc(spaceIndex, action, clientId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void StartMiniGameServerRpc(MiniGameType gameType, ulong[] participants, ServerRpcParams serverRpcParams = default)
    {
        StartMiniGameClientRpc(gameType, participants);
    }
    
    #endregion
    
    #region Client RPCs
    
    [ClientRpc]
    private void UpdatePlayerListClientRpc()
    {
        // Update UI with current player list
        if (UI_GameManager.Instance != null)
        {
            UI_GameManager.Instance.UpdatePlayerList(playerDataDict);
        }
    }
    
    [ClientRpc]
    private void StartGameClientRpc()
    {
        networkGameState.Value = GameState.Playing;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNetworkGame();
        }
    }
    
    [ClientRpc]
    private void ProcessDiceRollClientRpc(int diceResult, ulong playerId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessNetworkDiceRoll(diceResult, playerId);
        }
    }
    
    [ClientRpc]
    private void ProcessPropertyActionClientRpc(int spaceIndex, PropertyAction action, ulong playerId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessNetworkPropertyAction(spaceIndex, action, playerId);
        }
    }
    
    [ClientRpc]
    private void StartMiniGameClientRpc(MiniGameType gameType, ulong[] participants)
    {
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.StartNetworkMiniGame(gameType, participants);
        }
    }
    
    [ClientRpc]
    private void SyncPlayerDataClientRpc(PlayerData playerData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SyncNetworkPlayerData(playerData);
        }
    }
    
    [ClientRpc]
    private void HandlePlayerDisconnectionClientRpc(ulong disconnectedPlayerId)
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
    
    void OnDestroy()
    {
        if (netManager != null)
        {
            netManager.OnClientConnectedCallback -= OnClientConnected;
            netManager.OnClientDisconnectCallback -= OnClientDisconnected;
            netManager.OnServerStarted -= OnServerStarted;
        }
        
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

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<ulong> playerId = new NetworkVariable<ulong>();
    public NetworkVariable<int> playerMoney = new NetworkVariable<int>();
    public NetworkVariable<int> currentSpace = new NetworkVariable<int>();
    public NetworkVariable<bool> isMyTurn = new NetworkVariable<bool>();
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerId.Value = NetworkManager.Singleton.LocalClientId;
        }
    }
    
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(int money, int space, bool turn)
    {
        playerMoney.Value = money;
        currentSpace.Value = space;
        isMyTurn.Value = turn;
    }
}