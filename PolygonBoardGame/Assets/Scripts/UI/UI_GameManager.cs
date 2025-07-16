using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UI_GameManager : MonoBehaviour
{
    public static UI_GameManager Instance { get; private set; }
    
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameHUD;
    public GameObject propertyDecisionPanel;
    public GameObject playerTurnPanel;
    public GameObject messagePanel;
    public GameObject scorePanel;
    
    [Header("HUD Elements")]
    public Text currentPlayerText;
    public Text gameStateText;
    public Button rollDiceButton;
    public List<UI_PlayerInfo> playerInfos = new List<UI_PlayerInfo>();
    
    [Header("Property Decision Elements")]
    public Text propertyNameText;
    public Text propertyPriceText;
    public Text propertyOwnerText;
    public Button buyButton;
    public Button sellButton;
    public Button challengeButton;
    public Button skipButton;
    
    [Header("Message Elements")]
    public Text messageText;
    public Button messageOkButton;
    
    [Header("Player Turn Elements")]
    public Text playerTurnText;
    public Image playerTurnImage;
    
    [Header("Score Elements")]
    public Transform scoreContent;
    public GameObject playerScorePrefab;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetupUI();
        SetupButtons();
        ShowMainMenu();
    }
    
    void SetupUI()
    {
        // Initialize UI panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (propertyDecisionPanel != null) propertyDecisionPanel.SetActive(false);
        if (playerTurnPanel != null) playerTurnPanel.SetActive(false);
        if (messagePanel != null) messagePanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
    }
    
    void SetupButtons()
    {
        // Setup button listeners
        if (rollDiceButton != null)
            rollDiceButton.onClick.AddListener(OnRollDiceClicked);
        
        if (buyButton != null)
            buyButton.onClick.AddListener(() => OnPropertyDecision("buy"));
        
        if (sellButton != null)
            sellButton.onClick.AddListener(() => OnPropertyDecision("sell"));
        
        if (challengeButton != null)
            challengeButton.onClick.AddListener(() => OnPropertyDecision("challenge"));
        
        if (skipButton != null)
            skipButton.onClick.AddListener(() => OnPropertyDecision("skip"));
        
        if (messageOkButton != null)
            messageOkButton.onClick.AddListener(HideMessage);
    }
    
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
    
    public void StartGame()
    {
        ShowGameHUD();
    }
    
    public void ShowGameHUD()
    {
        HideAllPanels();
        if (gameHUD != null)
            gameHUD.SetActive(true);
    }
    
    public void UpdateGameState(GameManager.GameState newState)
    {
        if (gameStateText != null)
        {
            gameStateText.text = $"State: {newState}";
        }
        
        // Update UI based on game state
        switch (newState)
        {
            case GameManager.GameState.PlayerTurn:
                ShowPlayerTurn();
                break;
            case GameManager.GameState.PropertyDecision:
                // Property decision UI will be shown separately
                break;
            case GameManager.GameState.MiniGame:
                HideAllPanels();
                break;
            case GameManager.GameState.GameOver:
                ShowScorePanel();
                break;
        }
    }
    
    public void ShowPlayerTurnUI(Player currentPlayer)
    {
        if (playerTurnPanel != null)
        {
            playerTurnPanel.SetActive(true);
            
            if (playerTurnText != null)
            {
                playerTurnText.text = $"{currentPlayer.playerName}'s Turn";
            }
            
            if (playerTurnImage != null)
            {
                playerTurnImage.color = currentPlayer.playerColor;
            }
        }
        
        if (currentPlayerText != null)
        {
            currentPlayerText.text = $"Current Player: {currentPlayer.playerName}";
        }
        
        // Enable dice roll button
        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = true;
        }
        
        UpdatePlayerInfos();
    }
    
    public void ShowPropertyDecisionUI(BoardSpace space)
    {
        if (propertyDecisionPanel != null)
        {
            propertyDecisionPanel.SetActive(true);
            
            if (propertyNameText != null)
                propertyNameText.text = space.propertyName;
            
            if (propertyPriceText != null)
                propertyPriceText.text = $"Price: ${space.price}";
            
            if (propertyOwnerText != null)
            {
                string ownerText = space.owner != null ? $"Owner: {space.owner.playerName}" : "Owner: None";
                propertyOwnerText.text = ownerText;
            }
            
            // Configure buttons based on property state
            Player currentPlayer = GameManager.Instance.GetCurrentPlayer();
            
            if (buyButton != null)
                buyButton.interactable = space.CanBeBought() && currentPlayer.CanAfford(space.price);
            
            if (sellButton != null)
                sellButton.interactable = space.CanBeSold(currentPlayer);
            
            if (challengeButton != null)
                challengeButton.interactable = true;
        }
    }
    
    public void HidePropertyDecisionUI()
    {
        if (propertyDecisionPanel != null)
            propertyDecisionPanel.SetActive(false);
    }
    
    public void ShowMessage(string message)
    {
        if (messagePanel != null && messageText != null)
        {
            messagePanel.SetActive(true);
            messageText.text = message;
        }
    }
    
    public void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
    
    public void ShowScorePanel()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
            UpdateScoreDisplay();
        }
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreContent == null || playerScorePrefab == null) return;
        
        // Clear existing score display
        foreach (Transform child in scoreContent)
        {
            Destroy(child.gameObject);
        }
        
        // Create score entries for each player
        List<Player> players = GameManager.Instance.players;
        
        // Sort players by net worth
        players.Sort((p1, p2) => p2.GetNetWorth().CompareTo(p1.GetNetWorth()));
        
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            GameObject scoreEntry = Instantiate(playerScorePrefab, scoreContent);
            
            Text[] texts = scoreEntry.GetComponentsInChildren<Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = $"#{i + 1}";
                texts[1].text = player.playerName;
                texts[2].text = $"${player.GetNetWorth()}";
                texts[1].color = player.playerColor;
            }
        }
    }
    
    void UpdatePlayerInfos()
    {
        for (int i = 0; i < playerInfos.Count && i < GameManager.Instance.players.Count; i++)
        {
            playerInfos[i].UpdatePlayerInfo(GameManager.Instance.players[i]);
        }
    }
    
    void OnRollDiceClicked()
    {
        if (rollDiceButton != null)
            rollDiceButton.interactable = false;
        
        GameManager.Instance.RollDice();
        
        if (playerTurnPanel != null)
            playerTurnPanel.SetActive(false);
    }
    
    void OnPropertyDecision(string decision)
    {
        HidePropertyDecisionUI();
        GameManager.Instance.OnPropertyDecision(decision);
    }
    
    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (propertyDecisionPanel != null) propertyDecisionPanel.SetActive(false);
        if (playerTurnPanel != null) playerTurnPanel.SetActive(false);
        if (messagePanel != null) messagePanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
    }
    
    void ShowPlayerTurn()
    {
        // Show player turn UI is handled in ShowPlayerTurnUI
    }
    
    // Method needed by NetworkManager
    public void UpdatePlayerList(System.Collections.Generic.Dictionary<ulong, NetworkManager.PlayerData> playerDataDict)
    {
        Debug.Log($"Updating player list with {playerDataDict.Count} players");
        // Update UI to show network players
        // This would typically update a lobby UI or similar
    }
    
    // Method needed by GameManager
    public void ShowGameOverScreen()
    {
        ShowScorePanel();
        ShowMessage("Game Over!");
    }
}

[System.Serializable]
public class UI_PlayerInfo
{
    public Text playerNameText;
    public Text playerMoneyText;
    public Text playerPropertiesText;
    public Image playerColorImage;
    
    public void UpdatePlayerInfo(Player player)
    {
        if (playerNameText != null)
            playerNameText.text = player.playerName;
        
        if (playerMoneyText != null)
            playerMoneyText.text = $"${player.money}";
        
        if (playerPropertiesText != null)
            playerPropertiesText.text = $"Properties: {player.ownedProperties.Count}";
        
        if (playerColorImage != null)
            playerColorImage.color = player.playerColor;
    }
}