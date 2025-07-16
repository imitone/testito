using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }
    
    [Header("Achievement Settings")]
    public bool enableAchievements = true;
    public bool showAchievementNotifications = true;
    public float notificationDuration = 3f;
    public string saveKey = "PolygonBoardGame_Achievements";
    
    [Header("Achievement Data")]
    public List<Achievement> achievements = new List<Achievement>();
    public List<AchievementCategory> categories = new List<AchievementCategory>();
    
    [Header("Rewards")]
    public List<UnlockableReward> rewards = new List<UnlockableReward>();
    
    [Header("Events")]
    public System.Action<Achievement> OnAchievementUnlocked;
    public System.Action<UnlockableReward> OnRewardUnlocked;
    public System.Action<AchievementProgress> OnProgressUpdated;
    
    private Dictionary<string, Achievement> achievementDict = new Dictionary<string, Achievement>();
    private Dictionary<string, AchievementProgress> progressDict = new Dictionary<string, AchievementProgress>();
    private Dictionary<string, UnlockableReward> rewardDict = new Dictionary<string, UnlockableReward>();
    private AchievementSaveData saveData;
    
    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public Sprite icon;
        public AchievementType type;
        public AchievementCategory category;
        public int targetValue;
        public int points;
        public bool isSecret;
        public bool isUnlocked;
        public DateTime unlockDate;
        public List<string> rewardIds = new List<string>();
        public AchievementCondition[] conditions;
        
        [System.Serializable]
        public class AchievementCondition
        {
            public string parameter;
            public ComparisonType comparison;
            public float value;
            public string stringValue;
        }
    }
    
    [System.Serializable]
    public class AchievementCategory
    {
        public string id;
        public string name;
        public string description;
        public Sprite icon;
        public Color color = Color.white;
        public int totalAchievements;
        public int unlockedAchievements;
    }
    
    [System.Serializable]
    public class UnlockableReward
    {
        public string id;
        public string name;
        public string description;
        public Sprite icon;
        public RewardType type;
        public bool isUnlocked;
        public DateTime unlockDate;
        
        // Reward-specific data
        public Color playerColor;
        public AudioClip musicTrack;
        public GameObject playerModel;
        public Sprite boardTheme;
        public int currency;
        public string customTitle;
    }
    
    [System.Serializable]
    public class AchievementProgress
    {
        public string achievementId;
        public int currentValue;
        public int targetValue;
        public float percentage;
        public bool isCompleted;
        public DateTime lastUpdated;
    }
    
    [System.Serializable]
    public class AchievementSaveData
    {
        public List<string> unlockedAchievements = new List<string>();
        public List<string> unlockedRewards = new List<string>();
        public List<AchievementProgress> progressData = new List<AchievementProgress>();
        public PlayerStats playerStats = new PlayerStats();
    }
    
    [System.Serializable]
    public class PlayerStats
    {
        public int gamesPlayed;
        public int gamesWon;
        public int totalMoney;
        public int propertiesBought;
        public int propertiesSold;
        public int miniGamesWon;
        public int miniGamesPlayed;
        public int totalDiceRolls;
        public int maxMoneyOwned;
        public int bankruptcies;
        public float totalPlayTime;
        public Dictionary<string, int> customStats = new Dictionary<string, int>();
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievementSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadAchievementData();
        SubscribeToGameEvents();
    }
    
    void InitializeAchievementSystem()
    {
        // Build achievement dictionary
        achievementDict.Clear();
        foreach (var achievement in achievements)
        {
            achievementDict[achievement.id] = achievement;
            
            // Initialize progress tracking
            progressDict[achievement.id] = new AchievementProgress
            {
                achievementId = achievement.id,
                currentValue = 0,
                targetValue = achievement.targetValue,
                percentage = 0f,
                isCompleted = false,
                lastUpdated = DateTime.Now
            };
        }
        
        // Build reward dictionary
        rewardDict.Clear();
        foreach (var reward in rewards)
        {
            rewardDict[reward.id] = reward;
        }
        
        // Initialize save data
        if (saveData == null)
        {
            saveData = new AchievementSaveData();
        }
        
        // Create default achievements
        CreateDefaultAchievements();
    }
    
    void CreateDefaultAchievements()
    {
        // Game Progress Achievements
        AddAchievement("first_game", "First Steps", "Complete your first game", AchievementType.GameProgress, 1, 100);
        AddAchievement("play_10_games", "Regular Player", "Play 10 games", AchievementType.GameProgress, 10, 250);
        AddAchievement("play_50_games", "Veteran", "Play 50 games", AchievementType.GameProgress, 50, 500);
        AddAchievement("play_100_games", "Master Player", "Play 100 games", AchievementType.GameProgress, 100, 1000);
        
        // Victory Achievements
        AddAchievement("first_win", "First Victory", "Win your first game", AchievementType.Victory, 1, 200);
        AddAchievement("win_streak_3", "Hat Trick", "Win 3 games in a row", AchievementType.Victory, 3, 300);
        AddAchievement("win_streak_5", "Unstoppable", "Win 5 games in a row", AchievementType.Victory, 5, 500);
        AddAchievement("win_10_games", "Champion", "Win 10 games", AchievementType.Victory, 10, 750);
        
        // Money Achievements
        AddAchievement("millionaire", "Millionaire", "Accumulate $10,000 in a single game", AchievementType.Money, 10000, 400);
        AddAchievement("big_spender", "Big Spender", "Spend $50,000 total across all games", AchievementType.Money, 50000, 300);
        AddAchievement("property_tycoon", "Property Tycoon", "Own 15 properties in a single game", AchievementType.Property, 15, 500);
        
        // Mini-Game Achievements
        AddAchievement("mini_game_master", "Mini-Game Master", "Win 25 mini-games", AchievementType.MiniGame, 25, 400);
        AddAchievement("perfect_memory", "Perfect Memory", "Complete Memory Game without mistakes", AchievementType.MiniGame, 1, 250);
        AddAchievement("speed_demon", "Speed Demon", "Win Race Game in under 30 seconds", AchievementType.MiniGame, 1, 300);
        AddAchievement("platform_master", "Platform Master", "Survive Platform Game for 2 minutes", AchievementType.MiniGame, 120, 350);
        
        // Special Achievements
        AddAchievement("lucky_seven", "Lucky Seven", "Roll seven 6s in a row", AchievementType.Special, 7, 777, true);
        AddAchievement("comeback_king", "Comeback King", "Win a game after being in last place", AchievementType.Special, 1, 600, true);
        AddAchievement("bankruptcy_survivor", "Bankruptcy Survivor", "Recover from bankruptcy and win", AchievementType.Special, 1, 800, true);
        
        // Social Achievements
        AddAchievement("friendly_challenger", "Friendly Challenger", "Challenge opponents to 50 mini-games", AchievementType.Social, 50, 300);
        AddAchievement("property_trader", "Property Trader", "Complete 25 property trades", AchievementType.Social, 25, 250);
        
        // Time-based Achievements
        AddAchievement("marathon_player", "Marathon Player", "Play for 10 hours total", AchievementType.Time, 600, 500); // 10 hours in minutes
        AddAchievement("quick_game", "Quick Game", "Finish a game in under 15 minutes", AchievementType.Time, 15, 200);
        
        // Collection Achievements
        AddAchievement("color_collector", "Color Collector", "Unlock all player colors", AchievementType.Collection, 8, 400);
        AddAchievement("music_lover", "Music Lover", "Unlock all music tracks", AchievementType.Collection, 12, 350);
        AddAchievement("completionist", "Completionist", "Unlock all achievements", AchievementType.Collection, achievements.Count, 2000, true);
    }
    
    void AddAchievement(string id, string title, string description, AchievementType type, int targetValue, int points, bool isSecret = false)
    {
        if (achievementDict.ContainsKey(id)) return;
        
        Achievement achievement = new Achievement
        {
            id = id,
            title = title,
            description = description,
            type = type,
            targetValue = targetValue,
            points = points,
            isSecret = isSecret,
            isUnlocked = false
        };
        
        achievements.Add(achievement);
        achievementDict[id] = achievement;
        
        progressDict[id] = new AchievementProgress
        {
            achievementId = id,
            currentValue = 0,
            targetValue = targetValue,
            percentage = 0f,
            isCompleted = false,
            lastUpdated = DateTime.Now
        };
    }
    
    void SubscribeToGameEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += OnGameStarted;
            GameManager.Instance.OnGameEnded += OnGameEnded;
            GameManager.Instance.OnPlayerTurnChanged += OnPlayerTurnChanged;
            GameManager.Instance.OnPropertyBought += OnPropertyBought;
            GameManager.Instance.OnPropertySold += OnPropertySold;
            GameManager.Instance.OnMiniGameStarted += OnMiniGameStarted;
            GameManager.Instance.OnMiniGameEnded += OnMiniGameEnded;
            GameManager.Instance.OnDiceRolled += OnDiceRolled;
            GameManager.Instance.OnPlayerBankrupt += OnPlayerBankrupt;
        }
    }
    
    #region Public Methods
    
    public void UpdateProgress(string achievementId, int value)
    {
        if (!enableAchievements || !achievementDict.ContainsKey(achievementId)) return;
        
        var achievement = achievementDict[achievementId];
        var progress = progressDict[achievementId];
        
        if (progress.isCompleted) return;
        
        progress.currentValue = value;
        progress.percentage = (float)progress.currentValue / progress.targetValue;
        progress.lastUpdated = DateTime.Now;
        
        OnProgressUpdated?.Invoke(progress);
        
        if (progress.currentValue >= progress.targetValue)
        {
            UnlockAchievement(achievementId);
        }
        
        SaveAchievementData();
    }
    
    public void IncrementProgress(string achievementId, int increment = 1)
    {
        if (!enableAchievements || !progressDict.ContainsKey(achievementId)) return;
        
        var progress = progressDict[achievementId];
        UpdateProgress(achievementId, progress.currentValue + increment);
    }
    
    public void UnlockAchievement(string achievementId)
    {
        if (!enableAchievements || !achievementDict.ContainsKey(achievementId)) return;
        
        var achievement = achievementDict[achievementId];
        if (achievement.isUnlocked) return;
        
        achievement.isUnlocked = true;
        achievement.unlockDate = DateTime.Now;
        
        var progress = progressDict[achievementId];
        progress.isCompleted = true;
        progress.currentValue = progress.targetValue;
        progress.percentage = 1f;
        
        // Unlock associated rewards
        foreach (var rewardId in achievement.rewardIds)
        {
            UnlockReward(rewardId);
        }
        
        OnAchievementUnlocked?.Invoke(achievement);
        
        if (showAchievementNotifications)
        {
            ShowAchievementNotification(achievement);
        }
        
        SaveAchievementData();
    }
    
    public void UnlockReward(string rewardId)
    {
        if (!rewardDict.ContainsKey(rewardId)) return;
        
        var reward = rewardDict[rewardId];
        if (reward.isUnlocked) return;
        
        reward.isUnlocked = true;
        reward.unlockDate = DateTime.Now;
        
        OnRewardUnlocked?.Invoke(reward);
        SaveAchievementData();
    }
    
    public bool IsAchievementUnlocked(string achievementId)
    {
        return achievementDict.ContainsKey(achievementId) && achievementDict[achievementId].isUnlocked;
    }
    
    public bool IsRewardUnlocked(string rewardId)
    {
        return rewardDict.ContainsKey(rewardId) && rewardDict[rewardId].isUnlocked;
    }
    
    public AchievementProgress GetProgress(string achievementId)
    {
        return progressDict.ContainsKey(achievementId) ? progressDict[achievementId] : null;
    }
    
    public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
    {
        return achievements.Where(a => a.category == category).ToList();
    }
    
    public List<Achievement> GetUnlockedAchievements()
    {
        return achievements.Where(a => a.isUnlocked).ToList();
    }
    
    public List<Achievement> GetLockedAchievements()
    {
        return achievements.Where(a => !a.isUnlocked && !a.isSecret).ToList();
    }
    
    public List<UnlockableReward> GetUnlockedRewards()
    {
        return rewards.Where(r => r.isUnlocked).ToList();
    }
    
    public int GetTotalPoints()
    {
        return achievements.Where(a => a.isUnlocked).Sum(a => a.points);
    }
    
    public float GetCompletionPercentage()
    {
        int totalAchievements = achievements.Count;
        int unlockedAchievements = achievements.Count(a => a.isUnlocked);
        return totalAchievements > 0 ? (float)unlockedAchievements / totalAchievements : 0f;
    }
    
    public void ResetAchievements()
    {
        foreach (var achievement in achievements)
        {
            achievement.isUnlocked = false;
            achievement.unlockDate = default;
        }
        
        foreach (var reward in rewards)
        {
            reward.isUnlocked = false;
            reward.unlockDate = default;
        }
        
        foreach (var progress in progressDict.Values)
        {
            progress.currentValue = 0;
            progress.percentage = 0f;
            progress.isCompleted = false;
        }
        
        saveData = new AchievementSaveData();
        SaveAchievementData();
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnGameStarted()
    {
        IncrementProgress("first_game");
        IncrementProgress("play_10_games");
        IncrementProgress("play_50_games");
        IncrementProgress("play_100_games");
    }
    
    private void OnGameEnded(int winnerId, GameEndReason reason)
    {
        if (winnerId == 0) // Human player won
        {
            IncrementProgress("first_win");
            IncrementProgress("win_10_games");
            
            // Check for win streak achievements
            CheckWinStreak();
        }
        
        // Update play time
        float gameTime = Time.time - GameManager.Instance.gameStartTime;
        UpdateProgress("marathon_player", (int)(saveData.playerStats.totalPlayTime + gameTime));
        
        if (gameTime < 15 * 60) // Less than 15 minutes
        {
            UnlockAchievement("quick_game");
        }
    }
    
    private void OnPlayerTurnChanged(int playerId)
    {
        // Track turn-based achievements
    }
    
    private void OnPropertyBought(int playerId, BoardSpace property)
    {
        if (playerId == 0) // Human player
        {
            IncrementProgress("big_spender", property.purchasePrice);
            IncrementProgress("property_tycoon");
        }
    }
    
    private void OnPropertySold(int playerId, BoardSpace property)
    {
        if (playerId == 0) // Human player
        {
            // Track property sales
        }
    }
    
    private void OnMiniGameStarted(MiniGameType gameType)
    {
        IncrementProgress("friendly_challenger");
    }
    
    private void OnMiniGameEnded(MiniGameType gameType, int winnerId)
    {
        if (winnerId == 0) // Human player won
        {
            IncrementProgress("mini_game_master");
            
            // Check for specific mini-game achievements
            CheckMiniGameAchievements(gameType);
        }
    }
    
    private void OnDiceRolled(int playerId, int value)
    {
        if (playerId == 0) // Human player
        {
            if (value == 6)
            {
                IncrementProgress("lucky_seven");
            }
            else
            {
                // Reset lucky seven progress if not 6
                UpdateProgress("lucky_seven", 0);
            }
        }
    }
    
    private void OnPlayerBankrupt(int playerId)
    {
        if (playerId == 0) // Human player
        {
            // Track bankruptcy for recovery achievements
        }
    }
    
    #endregion
    
    #region Private Methods
    
    private void CheckWinStreak()
    {
        // This would require tracking consecutive wins
        // Implementation depends on game persistence
    }
    
    private void CheckMiniGameAchievements(MiniGameType gameType)
    {
        switch (gameType)
        {
            case MiniGameType.Memory:
                // Check for perfect memory achievement
                break;
            case MiniGameType.Race:
                // Check for speed demon achievement
                break;
            case MiniGameType.Platform:
                // Check for platform master achievement
                break;
        }
    }
    
    private void ShowAchievementNotification(Achievement achievement)
    {
        // This would show a UI notification
        Debug.Log($"Achievement Unlocked: {achievement.title} - {achievement.description}");
    }
    
    private void SaveAchievementData()
    {
        try
        {
            saveData.unlockedAchievements = achievements.Where(a => a.isUnlocked).Select(a => a.id).ToList();
            saveData.unlockedRewards = rewards.Where(r => r.isUnlocked).Select(r => r.id).ToList();
            saveData.progressData = progressDict.Values.ToList();
            
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save achievement data: {e.Message}");
        }
    }
    
    private void LoadAchievementData()
    {
        try
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                saveData = JsonUtility.FromJson<AchievementSaveData>(json);
                
                // Apply loaded data
                foreach (var achievementId in saveData.unlockedAchievements)
                {
                    if (achievementDict.ContainsKey(achievementId))
                    {
                        achievementDict[achievementId].isUnlocked = true;
                    }
                }
                
                foreach (var rewardId in saveData.unlockedRewards)
                {
                    if (rewardDict.ContainsKey(rewardId))
                    {
                        rewardDict[rewardId].isUnlocked = true;
                    }
                }
                
                foreach (var progress in saveData.progressData)
                {
                    if (progressDict.ContainsKey(progress.achievementId))
                    {
                        progressDict[progress.achievementId] = progress;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load achievement data: {e.Message}");
            saveData = new AchievementSaveData();
        }
    }
    
    #endregion
    
    void OnDestroy()
    {
        SaveAchievementData();
    }
}

public enum AchievementType
{
    GameProgress,
    Victory,
    Money,
    Property,
    MiniGame,
    Special,
    Social,
    Time,
    Collection
}

public enum RewardType
{
    PlayerColor,
    MusicTrack,
    PlayerModel,
    BoardTheme,
    Currency,
    Title
}

public enum ComparisonType
{
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}