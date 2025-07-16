using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    [Header("Mini Game Settings")]
    public List<MiniGameData> availableMinigames = new List<MiniGameData>();
    public Transform minigameParent;
    public GameObject minigameUI;
    
    [Header("Current Mini Game")]
    public MiniGameBase currentMiniGame;
    public bool isMinigamePlaying = false;
    
    private List<Player> participants = new List<Player>();
    
    [System.Serializable]
    public class MiniGameData
    {
        public string gameName;
        public GameObject gamePrefab;
        public Sprite gameIcon;
        public string description;
        public int minPlayers = 1;
        public int maxPlayers = 4;
        public float duration = 30f;
    }
    
    void Start()
    {
        // Initialize mini-games
        InitializeMiniGames();
    }
    
    void InitializeMiniGames()
    {
        // Add default mini-games if none are set
        if (availableMinigames.Count == 0)
        {
            // These would reference actual prefabs in a real project
            availableMinigames.Add(new MiniGameData
            {
                gameName = "Race to the Finish",
                description = "First player to reach the end wins!",
                minPlayers = 2,
                maxPlayers = 4,
                duration = 45f
            });
            
            availableMinigames.Add(new MiniGameData
            {
                gameName = "Memory Match",
                description = "Match the most pairs to win!",
                minPlayers = 1,
                maxPlayers = 4,
                duration = 60f
            });
            
            availableMinigames.Add(new MiniGameData
            {
                gameName = "Platform Jump",
                description = "Jump on platforms and avoid falling!",
                minPlayers = 1,
                maxPlayers = 4,
                duration = 30f
            });
            
            availableMinigames.Add(new MiniGameData
            {
                gameName = "Color Clash",
                description = "Stand on the correct color when time runs out!",
                minPlayers = 2,
                maxPlayers = 4,
                duration = 20f
            });
            
            availableMinigames.Add(new MiniGameData
            {
                gameName = "Polygon Panic",
                description = "Collect the most polygons before time runs out!",
                minPlayers = 2,
                maxPlayers = 4,
                duration = 45f
            });
        }
    }
    
    public void StartRandomMiniGame()
    {
        if (isMinigamePlaying) return;
        
        // Get all players as participants
        participants = new List<Player>(GameManager.Instance.players);
        
        // Select a random mini-game
        MiniGameData selectedGame = availableMinigames[Random.Range(0, availableMinigames.Count)];
        StartMiniGame(selectedGame);
    }
    
    public void StartMiniGame(MiniGameData gameData)
    {
        if (isMinigamePlaying) return;
        
        isMinigamePlaying = true;
        
        // Show mini-game announcement
        StartCoroutine(ShowMiniGameAnnouncement(gameData));
    }
    
    IEnumerator ShowMiniGameAnnouncement(MiniGameData gameData)
    {
        // Show announcement UI
        if (minigameUI != null)
        {
            minigameUI.SetActive(true);
            // Set announcement text
            UI_MiniGameAnnouncement announcement = minigameUI.GetComponent<UI_MiniGameAnnouncement>();
            if (announcement != null)
            {
                announcement.ShowAnnouncement(gameData.gameName, gameData.description);
            }
        }
        
        yield return new WaitForSeconds(3f);
        
        // Hide announcement and start game
        if (minigameUI != null)
        {
            minigameUI.SetActive(false);
        }
        
        LaunchMiniGame(gameData);
    }
    
    void LaunchMiniGame(MiniGameData gameData)
    {
        // Create mini-game instance
        GameObject minigameObject = null;
        
        // For now, create a simple mini-game based on the name
        switch (gameData.gameName)
        {
            case "Race to the Finish":
                minigameObject = CreateRaceGame();
                break;
            case "Memory Match":
                minigameObject = CreateMemoryGame();
                break;
            case "Platform Jump":
                minigameObject = CreatePlatformGame();
                break;
            case "Color Clash":
                minigameObject = CreateColorGame();
                break;
            case "Polygon Panic":
                minigameObject = CreatePolygonGame();
                break;
        }
        
        if (minigameObject != null)
        {
            currentMiniGame = minigameObject.GetComponent<MiniGameBase>();
            if (currentMiniGame != null)
            {
                currentMiniGame.Initialize(participants, gameData.duration);
                currentMiniGame.OnMiniGameComplete += OnMiniGameComplete;
                currentMiniGame.StartGame();
            }
        }
    }
    
    GameObject CreateRaceGame()
    {
        GameObject gameObj = new GameObject("RaceGame");
        RaceGame raceGame = gameObj.AddComponent<RaceGame>();
        return gameObj;
    }
    
    GameObject CreateMemoryGame()
    {
        GameObject gameObj = new GameObject("MemoryGame");
        MemoryGame memoryGame = gameObj.AddComponent<MemoryGame>();
        return gameObj;
    }
    
    GameObject CreatePlatformGame()
    {
        GameObject gameObj = new GameObject("PlatformGame");
        PlatformGame platformGame = gameObj.AddComponent<PlatformGame>();
        return gameObj;
    }
    
    GameObject CreateColorGame()
    {
        GameObject gameObj = new GameObject("ColorGame");
        ColorGame colorGame = gameObj.AddComponent<ColorGame>();
        return gameObj;
    }
    
    GameObject CreatePolygonGame()
    {
        GameObject gameObj = new GameObject("PolygonGame");
        PolygonGame polygonGame = gameObj.AddComponent<PolygonGame>();
        return gameObj;
    }
    
    void OnMiniGameComplete(int winnerIndex)
    {
        isMinigamePlaying = false;
        
        // Clean up current mini-game
        if (currentMiniGame != null)
        {
            currentMiniGame.OnMiniGameComplete -= OnMiniGameComplete;
            Destroy(currentMiniGame.gameObject);
            currentMiniGame = null;
        }
        
        // Notify game manager
        GameManager.Instance.OnMiniGameComplete(winnerIndex);
    }
    
    public void ForceEndMiniGame()
    {
        if (isMinigamePlaying && currentMiniGame != null)
        {
            currentMiniGame.EndGame(-1); // -1 indicates no winner
        }
    }
    
    public bool IsMinigamePlaying()
    {
        return isMinigamePlaying;
    }
    
    public string GetRandomMiniGameName()
    {
        if (availableMinigames.Count > 0)
        {
            return availableMinigames[Random.Range(0, availableMinigames.Count)].gameName;
        }
        return "Unknown Game";
    }
}