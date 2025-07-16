using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int numberOfPlayers = 4;
    public int startingMoney = 1500;
    public int passStartBonus = 200;
    
    [Header("Game References")]
    public Transform boardParent;
    public GameObject playerPrefab;
    public UI_GameManager uiManager;
    public MiniGameManager miniGameManager;
    
    [Header("Game State")]
    public GameState currentState;
    public int currentPlayerIndex = 0;
    public List<Player> players = new List<Player>();
    
    private BoardManager boardManager;
    private bool gameStarted = false;
    
    public enum GameState
    {
        MainMenu,
        GameSetup,
        PlayerTurn,
        Rolling,
        Moving,
        PropertyDecision,
        MiniGame,
        GameOver
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        InitializeGame();
    }
    
    void InitializeGame()
    {
        SetGameState(GameState.GameSetup);
        CreatePlayers();
        SetGameState(GameState.PlayerTurn);
    }
    
    void CreatePlayers()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject playerObj = Instantiate(playerPrefab);
            Player player = playerObj.GetComponent<Player>();
            player.Initialize(i, startingMoney, GetPlayerColor(i));
            players.Add(player);
        }
    }
    
    Color GetPlayerColor(int index)
    {
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
        return colors[index % colors.Length];
    }
    
    public void SetGameState(GameState newState)
    {
        currentState = newState;
        uiManager?.UpdateGameState(newState);
        
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
        }
    }
    
    void StartPlayerTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];
        uiManager?.ShowPlayerTurnUI(currentPlayer);
    }
    
    public void RollDice()
    {
        if (currentState != GameState.PlayerTurn) return;
        
        SetGameState(GameState.Rolling);
        int diceRoll = Random.Range(1, 7);
        
        Player currentPlayer = players[currentPlayerIndex];
        StartCoroutine(MovePlayer(currentPlayer, diceRoll));
    }
    
    IEnumerator MovePlayer(Player player, int spaces)
    {
        SetGameState(GameState.Moving);
        
        for (int i = 0; i < spaces; i++)
        {
            player.MoveToNextSpace();
            yield return new WaitForSeconds(0.5f);
        }
        
        BoardSpace currentSpace = boardManager.GetSpaceAt(player.currentSpaceIndex);
        if (currentSpace.spaceType == BoardSpace.SpaceType.Property)
        {
            SetGameState(GameState.PropertyDecision);
        }
        else
        {
            EndPlayerTurn();
        }
    }
    
    void ShowPropertyDecisionUI()
    {
        Player currentPlayer = players[currentPlayerIndex];
        BoardSpace currentSpace = boardManager.GetSpaceAt(currentPlayer.currentSpaceIndex);
        uiManager?.ShowPropertyDecisionUI(currentSpace);
    }
    
    public void OnPropertyDecision(string decision)
    {
        Player currentPlayer = players[currentPlayerIndex];
        BoardSpace currentSpace = boardManager.GetSpaceAt(currentPlayer.currentSpaceIndex);
        
        switch (decision.ToLower())
        {
            case "buy":
                BuyProperty(currentPlayer, currentSpace);
                break;
            case "sell":
                SellProperty(currentPlayer, currentSpace);
                break;
            case "challenge":
                SetGameState(GameState.MiniGame);
                break;
        }
    }
    
    void BuyProperty(Player player, BoardSpace space)
    {
        if (player.money >= space.price && space.owner == null)
        {
            player.money -= space.price;
            space.owner = player;
            player.ownedProperties.Add(space);
            uiManager?.ShowMessage($"{player.playerName} bought {space.propertyName} for ${space.price}!");
        }
        else
        {
            uiManager?.ShowMessage("Cannot buy this property!");
        }
        
        EndPlayerTurn();
    }
    
    void SellProperty(Player player, BoardSpace space)
    {
        if (space.owner == player)
        {
            player.money += space.price / 2;
            space.owner = null;
            player.ownedProperties.Remove(space);
            uiManager?.ShowMessage($"{player.playerName} sold {space.propertyName} for ${space.price / 2}!");
        }
        else
        {
            uiManager?.ShowMessage("You don't own this property!");
        }
        
        EndPlayerTurn();
    }
    
    void StartMiniGame()
    {
        miniGameManager?.StartRandomMiniGame();
    }
    
    public void OnMiniGameComplete(int winner)
    {
        if (winner == currentPlayerIndex)
        {
            players[currentPlayerIndex].money += 500;
            uiManager?.ShowMessage($"{players[currentPlayerIndex].playerName} won the mini game and earned $500!");
        }
        else
        {
            uiManager?.ShowMessage($"{players[currentPlayerIndex].playerName} lost the mini game!");
        }
        
        EndPlayerTurn();
    }
    
    void EndPlayerTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % numberOfPlayers;
        SetGameState(GameState.PlayerTurn);
    }
    
    public Player GetCurrentPlayer()
    {
        return players[currentPlayerIndex];
    }
}