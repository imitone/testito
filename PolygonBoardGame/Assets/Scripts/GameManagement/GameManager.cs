using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int numberOfPlayers = 4;
    public int startingMoney = 1500;
    public int passStartBonus = 200;
    public int maxRounds = 20;
    public int winConditionMoney = 5000;
    
    [Header("Game References")]
    public Transform boardParent;
    public GameObject playerPrefab;
    public UI_GameManager uiManager;
    public MiniGameManager miniGameManager;
    public AudioManager audioManager;
    public CameraController cameraController;
    
    [Header("Game State")]
    public GameState currentState;
    public int currentPlayerIndex = 0;
    public int currentRound = 1;
    public List<Player> players = new List<Player>();
    public List<int> playerRanking = new List<int>();
    
    [Header("Game Statistics")]
    public int totalMiniGamesPlayed = 0;
    public int totalPropertiesBought = 0;
    public float gameStartTime;
    public float gameEndTime;
    
    private BoardManager boardManager;
    private bool gameStarted = false;
    private bool gameEnded = false;
    private Coroutine gameTimer;
    
    // Enums needed by other systems
    public enum GameEndReason
    {
        Victory,
        Elimination,
        TimeLimit,
        Forfeit
    }
    
    public enum MiniGameType
    {
        Race,
        Memory,
        Platform,
        Color,
        Polygon
    }
    
    public enum GameState
    {
        MainMenu,
        GameSetup,
        PlayerTurn,
        Rolling,
        Moving,
        PropertyDecision,
        MiniGame,
        GameOver,
        Paused,
        WaitingForPlayers,
        Playing
    }
    
    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<Player> OnPlayerTurnChanged;
    public System.Action<int> OnRoundChanged;
    public System.Action<Player> OnGameWon;
    
    // Additional events needed by other systems
    public System.Action OnGameStarted;
    public System.Action<int, GameEndReason> OnGameEnded;
    public System.Action<int> OnPlayerTurnChangedById;
    public System.Action<int, BoardSpace> OnPropertyBought;
    public System.Action<int, BoardSpace> OnPropertySold;
    public System.Action<MiniGameType> OnMiniGameStarted;
    public System.Action<MiniGameType, int> OnMiniGameEnded;
    public System.Action<int, int> OnDiceRolled;
    public System.Action<int> OnPlayerBankrupt;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameStartTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogError("BoardManager not found! Please add a BoardManager to the scene.");
            return;
        }
        
        SetupAudio();
        InitializeGame();
    }
    
    void SetupAudio()
    {
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }
        
        if (audioManager != null)
        {
            audioManager.PlayBackgroundMusic();
        }
    }
    
    void InitializeGame()
    {
        SetGameState(GameState.GameSetup);
        CreatePlayers();
        SetupPlayerRanking();
        SetGameState(GameState.PlayerTurn);
        
        // Trigger game started event
        OnGameStarted?.Invoke();
        gameStarted = true;
        
        // Start game timer
        gameTimer = StartCoroutine(GameTimer());
    }
    
    void CreatePlayers()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject playerObj = Instantiate(playerPrefab);
            Player player = playerObj.GetComponent<Player>();
            
            if (player != null)
            {
                string playerName = GetPlayerName(i);
                player.Initialize(i, startingMoney, GetPlayerColor(i), playerName);
                players.Add(player);
                
                // Position player at start
                if (boardManager != null)
                {
                    Vector3 startPos = boardManager.GetSpacePosition(0);
                    startPos.x += (i - (numberOfPlayers - 1) / 2f) * 0.5f; // Offset players
                    player.transform.position = startPos;
                }
            }
        }
    }
    
    string GetPlayerName(int index)
    {
        string[] names = { "Red Player", "Blue Player", "Green Player", "Yellow Player" };
        return index < names.Length ? names[index] : $"Player {index + 1}";
    }
    
    Color GetPlayerColor(int index)
    {
        Color[] colors = { 
            new Color(1f, 0.2f, 0.2f), // Red
            new Color(0.2f, 0.4f, 1f), // Blue
            new Color(0.2f, 0.8f, 0.2f), // Green
            new Color(1f, 0.8f, 0.2f)  // Yellow
        };
        return colors[index % colors.Length];
    }
    
    void SetupPlayerRanking()
    {
        playerRanking.Clear();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            playerRanking.Add(i);
        }
    }
    
    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState previousState = currentState;
        currentState = newState;
        
        OnGameStateChanged?.Invoke(newState);
        uiManager?.UpdateGameState(newState);
        
        // Handle state-specific logic
        switch (newState)
        {
            case GameState.PlayerTurn:
                StartPlayerTurn();
                break;
            case GameState.PropertyDecision:
                ShowPropertyDecisionUI();
                break;
            case GameState.MiniGame:
                StartMiniGame();
                break;
            case GameState.GameOver:
                EndGame();
                break;
            case GameState.Paused:
                PauseGame();
                break;
        }
        
        // Audio feedback
        if (audioManager != null)
        {
            audioManager.PlayStateChangeSound(newState);
        }
        
        Debug.Log($"Game State Changed: {previousState} -> {newState}");
    }
    
    void StartPlayerTurn()
    {
        if (gameEnded) return;
        
        Player currentPlayer = players[currentPlayerIndex];
        OnPlayerTurnChanged?.Invoke(currentPlayer);
        
        // Check for bankruptcy
        if (currentPlayer.money <= 0 && currentPlayer.ownedProperties.Count == 0)
        {
            HandlePlayerBankruptcy(currentPlayer);
            return;
        }
        
        // Camera follow current player
        if (cameraController != null)
        {
            cameraController.FocusOnPlayer(currentPlayer);
        }
        
        uiManager?.ShowPlayerTurnUI(currentPlayer);
        
        // Auto-roll for AI players (simple implementation)
        if (currentPlayerIndex > 0) // Assuming player 0 is human
        {
            StartCoroutine(AIPlayerTurn());
        }
    }
    
    IEnumerator AIPlayerTurn()
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        RollDice();
    }
    
    void HandlePlayerBankruptcy(Player player)
    {
        Debug.Log($"{player.playerName} is bankrupt!");
        
        // Transfer properties back to bank
        foreach (var property in player.ownedProperties.ToList())
        {
            property.RemoveOwner();
            player.ownedProperties.Remove(property);
        }
        
        // Remove from active players
        players.RemoveAt(currentPlayerIndex);
        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }
        
        // Check for game end
        if (players.Count <= 1)
        {
            SetGameState(GameState.GameOver);
            return;
        }
        
        SetGameState(GameState.PlayerTurn);
    }
    
    public void RollDice()
    {
        if (currentState != GameState.PlayerTurn) return;
        
        SetGameState(GameState.Rolling);
        
        // Enhanced dice rolling with sound
        if (audioManager != null)
        {
            audioManager.PlayDiceRoll();
        }
        
        int diceRoll = Random.Range(1, 7);
        
        // Add dice roll animation delay
        StartCoroutine(DelayedMove(diceRoll));
    }
    
    IEnumerator DelayedMove(int diceRoll)
    {
        yield return new WaitForSeconds(1f); // Wait for dice animation
        
        Player currentPlayer = players[currentPlayerIndex];
        StartCoroutine(MovePlayer(currentPlayer, diceRoll));
    }
    
    IEnumerator MovePlayer(Player player, int spaces)
    {
        SetGameState(GameState.Moving);
        
        int startPosition = player.currentSpaceIndex;
        bool passedStart = false;
        
        for (int i = 0; i < spaces; i++)
        {
            int nextIndex = boardManager.GetNextSpaceIndex(player.currentSpaceIndex);
            
            // Check if passing start
            if (nextIndex < player.currentSpaceIndex)
            {
                passedStart = true;
            }
            
            player.MoveToNextSpace();
            
            if (audioManager != null)
            {
                audioManager.PlayMoveSound();
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        // Handle passing start
        if (passedStart)
        {
            player.AddMoney(passStartBonus);
            uiManager?.ShowMessage($"{player.playerName} passed START and collected ${passStartBonus}!");
            
            if (audioManager != null)
            {
                audioManager.PlayMoneySound();
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        // Handle landing on space
        HandleSpaceLanding(player);
    }
    
    void HandleSpaceLanding(Player player)
    {
        BoardSpace currentSpace = boardManager.GetSpaceAt(player.currentSpaceIndex);
        
        if (currentSpace == null)
        {
            EndPlayerTurn();
            return;
        }
        
        switch (currentSpace.spaceType)
        {
            case BoardSpace.SpaceType.Property:
                SetGameState(GameState.PropertyDecision);
                break;
                
            case BoardSpace.SpaceType.Start:
                // Already handled in MovePlayer
                EndPlayerTurn();
                break;
                
            case BoardSpace.SpaceType.Special:
                HandleSpecialSpace(player, currentSpace);
                break;
                
            case BoardSpace.SpaceType.Corner:
                HandleCornerSpace(player, currentSpace);
                break;
                
            default:
                EndPlayerTurn();
                break;
        }
    }
    
    void HandleSpecialSpace(Player player, BoardSpace space)
    {
        // Special spaces trigger events
        int eventType = Random.Range(0, 3);
        
        switch (eventType)
        {
            case 0:
                // Bonus money
                int bonus = Random.Range(50, 200);
                player.AddMoney(bonus);
                uiManager?.ShowMessage($"{player.playerName} found ${bonus}!");
                break;
                
            case 1:
                // Pay tax
                int tax = Random.Range(50, 150);
                player.SpendMoney(tax);
                uiManager?.ShowMessage($"{player.playerName} paid ${tax} in taxes!");
                break;
                
            case 2:
                // Mini-game challenge
                SetGameState(GameState.MiniGame);
                return;
        }
        
        StartCoroutine(DelayedEndTurn());
    }
    
    void HandleCornerSpace(Player player, BoardSpace space)
    {
        // Corner spaces are safe - just rest
        uiManager?.ShowMessage($"{player.playerName} rests at the corner.");
        StartCoroutine(DelayedEndTurn());
    }
    
    IEnumerator DelayedEndTurn()
    {
        yield return new WaitForSeconds(2f);
        EndPlayerTurn();
    }
    
    void ShowPropertyDecisionUI()
    {
        Player currentPlayer = players[currentPlayerIndex];
        BoardSpace currentSpace = boardManager.GetSpaceAt(currentPlayer.currentSpaceIndex);
        
        if (currentSpace != null)
        {
            uiManager?.ShowPropertyDecisionUI(currentSpace);
        }
    }
    
    public void OnPropertyDecision(string decision)
    {
        Player currentPlayer = players[currentPlayerIndex];
        BoardSpace currentSpace = boardManager.GetSpaceAt(currentPlayer.currentSpaceIndex);
        
        if (currentSpace == null)
        {
            EndPlayerTurn();
            return;
        }
        
        switch (decision.ToLower())
        {
            case "buy":
                BuyProperty(currentPlayer, currentSpace);
                break;
            case "sell":
                ShowSellPropertyUI(currentPlayer);
                break;
            case "challenge":
                SetGameState(GameState.MiniGame);
                break;
            case "skip":
                PayRentOrSkip(currentPlayer, currentSpace);
                break;
        }
    }
    
    void BuyProperty(Player player, BoardSpace space)
    {
        if (player.money >= space.price && space.owner == null)
        {
            player.SpendMoney(space.price);
            space.SetOwner(player);
            player.AddProperty(space);
            totalPropertiesBought++;
            
            uiManager?.ShowMessage($"{player.playerName} bought {space.propertyName} for ${space.price}!");
            
            if (audioManager != null)
            {
                audioManager.PlayPurchaseSound();
            }
        }
        else
        {
            uiManager?.ShowMessage("Cannot buy this property!");
            
            if (audioManager != null)
            {
                audioManager.PlayErrorSound();
            }
        }
        
        EndPlayerTurn();
    }
    
    void ShowSellPropertyUI(Player player)
    {
        // This would show a UI to select which property to sell
        // For now, just sell the first property
        if (player.ownedProperties.Count > 0)
        {
            BoardSpace propertyToSell = player.ownedProperties[0];
            SellProperty(player, propertyToSell);
        }
        else
        {
            uiManager?.ShowMessage("You don't own any properties!");
            EndPlayerTurn();
        }
    }
    
    void SellProperty(Player player, BoardSpace space)
    {
        if (space.owner == player)
        {
            int sellPrice = space.price / 2;
            player.AddMoney(sellPrice);
            space.RemoveOwner();
            player.RemoveProperty(space);
            
            uiManager?.ShowMessage($"{player.playerName} sold {space.propertyName} for ${sellPrice}!");
            
            if (audioManager != null)
            {
                audioManager.PlaySellSound();
            }
        }
        else
        {
            uiManager?.ShowMessage("You don't own this property!");
        }
        
        EndPlayerTurn();
    }
    
    void PayRentOrSkip(Player player, BoardSpace space)
    {
        if (space.owner != null && space.owner != player)
        {
            int rentAmount = Mathf.Min(space.rent, player.money);
            player.SpendMoney(rentAmount);
            space.owner.AddMoney(rentAmount);
            
            uiManager?.ShowMessage($"{player.playerName} paid ${rentAmount} rent to {space.owner.playerName}!");
            
            if (audioManager != null)
            {
                audioManager.PlayRentSound();
            }
        }
        
        EndPlayerTurn();
    }
    
    void StartMiniGame()
    {
        totalMiniGamesPlayed++;
        
        if (miniGameManager != null)
        {
            miniGameManager.StartRandomMiniGame();
        }
        else
        {
            Debug.LogError("MiniGameManager not found!");
            EndPlayerTurn();
        }
    }
    
    public void OnMiniGameComplete(int winnerIndex)
    {
        if (winnerIndex >= 0 && winnerIndex < players.Count)
        {
            Player winner = players[winnerIndex];
            int reward = Random.Range(200, 500);
            winner.AddMoney(reward);
            
            uiManager?.ShowMessage($"{winner.playerName} won the mini-game and earned ${reward}!");
            
            if (audioManager != null)
            {
                audioManager.PlayVictorySound();
            }
        }
        else
        {
            uiManager?.ShowMessage("Mini-game ended with no winner!");
        }
        
        // Return camera to board
        if (cameraController != null)
        {
            cameraController.ReturnToBoard();
        }
        
        EndPlayerTurn();
    }
    
    void EndPlayerTurn()
    {
        // Check for win conditions
        if (CheckWinConditions())
        {
            return;
        }
        
        // Next player
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        
        // Check for new round
        if (currentPlayerIndex == 0)
        {
            currentRound++;
            OnRoundChanged?.Invoke(currentRound);
            
            if (currentRound > maxRounds)
            {
                SetGameState(GameState.GameOver);
                return;
            }
        }
        
        SetGameState(GameState.PlayerTurn);
    }
    
    bool CheckWinConditions()
    {
        // Check if any player reached win condition money
        foreach (var player in players)
        {
            if (player.GetNetWorth() >= winConditionMoney)
            {
                OnGameWon?.Invoke(player);
                SetGameState(GameState.GameOver);
                return true;
            }
        }
        
        return false;
    }
    
    void EndGame()
    {
        gameEnded = true;
        gameEndTime = Time.time;
        
        if (gameTimer != null)
        {
            StopCoroutine(gameTimer);
        }
        
        // Calculate final rankings
        CalculateFinalRanking();
        
        // Show game over screen
        uiManager?.ShowGameOverScreen();
        
        if (audioManager != null)
        {
            audioManager.PlayGameOverSound();
        }
    }
    
    void CalculateFinalRanking()
    {
        playerRanking = players
            .Select((player, index) => new { player, index })
            .OrderByDescending(x => x.player.GetNetWorth())
            .Select(x => x.index)
            .ToList();
    }
    
    IEnumerator GameTimer()
    {
        while (!gameEnded)
        {
            yield return new WaitForSeconds(1f);
            
            // Update any time-based mechanics here
            UpdatePlayerRanking();
        }
    }
    
    void UpdatePlayerRanking()
    {
        playerRanking = players
            .Select((player, index) => new { player, index })
            .OrderByDescending(x => x.player.GetNetWorth())
            .Select(x => x.index)
            .ToList();
    }
    
    void PauseGame()
    {
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SetGameState(GameState.PlayerTurn);
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public Player GetCurrentPlayer()
    {
        if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
        {
            return players[currentPlayerIndex];
        }
        return null;
    }
    
    public Player GetPlayerByIndex(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            return players[index];
        }
        return null;
    }
    
    public float GetGameDuration()
    {
        return gameEnded ? gameEndTime - gameStartTime : Time.time - gameStartTime;
    }
    
    public GameStatistics GetGameStatistics()
    {
        return new GameStatistics
        {
            totalMiniGamesPlayed = this.totalMiniGamesPlayed,
            totalPropertiesBought = this.totalPropertiesBought,
            gameDuration = GetGameDuration(),
            currentRound = this.currentRound,
            playersRemaining = players.Count
        };
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentState != GameState.Paused)
        {
            SetGameState(GameState.Paused);
        }
    }
    
    // Additional methods needed by other systems
    
    // Network methods
    public void StartNetworkGame()
    {
        Debug.Log("Starting network game");
        StartGame();
    }
    
    public void ProcessNetworkDiceRoll(int diceResult, ulong playerId)
    {
        Debug.Log($"Processing network dice roll: {diceResult} for player {playerId}");
        // Handle network dice roll
    }
    
    public void ProcessNetworkPropertyAction(int spaceIndex, PropertyAction action, ulong playerId)
    {
        Debug.Log($"Processing network property action: {action} on space {spaceIndex} for player {playerId}");
        // Handle network property action
    }
    
    public void SyncNetworkPlayerData(NetworkManager.PlayerData playerData)
    {
        Debug.Log($"Syncing network player data for player {playerData.playerId}");
        // Handle network player data sync
    }
    
    public void HandlePlayerDisconnection(ulong disconnectedPlayerId)
    {
        Debug.Log($"Handling player disconnection: {disconnectedPlayerId}");
        // Handle player disconnection
    }
    
    // Property-related methods
    public void BuyProperty(int playerId, BoardSpace property)
    {
        OnPropertyBought?.Invoke(playerId, property);
        totalPropertiesBought++;
    }
    
    public void SellProperty(int playerId, BoardSpace property)
    {
        OnPropertySold?.Invoke(playerId, property);
    }
    
    public void StartMiniGame(MiniGameType gameType)
    {
        OnMiniGameStarted?.Invoke(gameType);
        SetGameState(GameState.MiniGame);
    }
    
    public void EndMiniGame(MiniGameType gameType, int winnerId)
    {
        OnMiniGameEnded?.Invoke(gameType, winnerId);
        SetGameState(GameState.PlayerTurn);
    }
    
    public void PlayerBankrupt(int playerId)
    {
        OnPlayerBankrupt?.Invoke(playerId);
        
        // Remove player from game
        if (playerId >= 0 && playerId < players.Count)
        {
            players.RemoveAt(playerId);
            
            // Check if game should end
            if (players.Count <= 1)
            {
                EndGame(GameEndReason.Elimination);
            }
        }
    }
    
    public void EndGame(GameEndReason reason)
    {
        if (gameEnded) return;
        
        gameEnded = true;
        gameEndTime = Time.time;
        
        // Determine winner
        int winnerId = 0;
        if (players.Count > 0)
        {
            // Find player with highest net worth
            Player winner = players.OrderByDescending(p => p.GetNetWorth()).FirstOrDefault();
            winnerId = winner != null ? winner.playerId : 0;
        }
        
        // Trigger game ended event
        OnGameEnded?.Invoke(winnerId, reason);
        
        SetGameState(GameState.GameOver);
    }
    
    public void StartGame()
    {
        if (!gameStarted)
        {
            InitializeGame();
        }
    }
    
    public void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        
        // Check if we've completed a round
        if (currentPlayerIndex == 0)
        {
            currentRound++;
            OnRoundChanged?.Invoke(currentRound);
            
            // Check for round limit
            if (currentRound > maxRounds)
            {
                EndGame(GameEndReason.TimeLimit);
                return;
            }
        }
        
        // Trigger player turn changed events
        Player currentPlayer = GetCurrentPlayer();
        if (currentPlayer != null)
        {
            OnPlayerTurnChanged?.Invoke(currentPlayer);
            OnPlayerTurnChangedById?.Invoke(currentPlayer.playerId);
        }
        
        SetGameState(GameState.PlayerTurn);
    }
    
    void OnDestroy()
    {
        OnGameStateChanged = null;
        OnPlayerTurnChanged = null;
        OnRoundChanged = null;
        OnGameWon = null;
        OnPlayerTurnChangedById = null;
    }
}

[System.Serializable]
public class GameStatistics
{
    public int totalMiniGamesPlayed;
    public int totalPropertiesBought;
    public float gameDuration;
    public int currentRound;
    public int playersRemaining;
}