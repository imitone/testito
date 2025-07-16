using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class MiniGameBase : MonoBehaviour
{
    [Header("Mini Game Settings")]
    public string gameName;
    public float gameDuration = 30f;
    public bool isPlaying = false;
    
    [Header("Participants")]
    public List<Player> participants = new List<Player>();
    public List<int> scores = new List<int>();
    
    protected float remainingTime;
    protected int winnerIndex = -1;
    
    public System.Action<int> OnMiniGameComplete;
    
    public virtual void Initialize(List<Player> players, float duration)
    {
        participants = new List<Player>(players);
        gameDuration = duration;
        remainingTime = duration;
        
        // Initialize scores
        scores.Clear();
        for (int i = 0; i < participants.Count; i++)
        {
            scores.Add(0);
        }
        
        SetupGame();
    }
    
    protected abstract void SetupGame();
    
    public virtual void StartGame()
    {
        isPlaying = true;
        StartCoroutine(GameTimer());
        OnGameStart();
    }
    
    protected abstract void OnGameStart();
    
    protected virtual void UpdateGame()
    {
        if (!isPlaying) return;
        
        // Update game logic
        HandleGameLogic();
        
        // Check for win conditions
        CheckWinConditions();
    }
    
    protected abstract void HandleGameLogic();
    
    protected abstract void CheckWinConditions();
    
    IEnumerator GameTimer()
    {
        while (remainingTime > 0 && isPlaying)
        {
            remainingTime -= Time.deltaTime;
            OnTimerUpdate(remainingTime);
            yield return null;
        }
        
        if (isPlaying)
        {
            TimeUp();
        }
    }
    
    protected virtual void OnTimerUpdate(float timeLeft)
    {
        // Override in derived classes for timer-specific logic
    }
    
    protected virtual void TimeUp()
    {
        // Find winner based on scores
        int highestScore = -1;
        int winner = -1;
        
        for (int i = 0; i < scores.Count; i++)
        {
            if (scores[i] > highestScore)
            {
                highestScore = scores[i];
                winner = i;
            }
        }
        
        EndGame(winner);
    }
    
    public virtual void EndGame(int winner)
    {
        isPlaying = false;
        winnerIndex = winner;
        
        OnGameEnd();
        
        // Notify completion
        OnMiniGameComplete?.Invoke(winnerIndex);
    }
    
    protected abstract void OnGameEnd();
    
    protected void AddScore(int playerIndex, int points)
    {
        if (playerIndex >= 0 && playerIndex < scores.Count)
        {
            scores[playerIndex] += points;
        }
    }
    
    protected void SetScore(int playerIndex, int score)
    {
        if (playerIndex >= 0 && playerIndex < scores.Count)
        {
            scores[playerIndex] = score;
        }
    }
    
    public int GetScore(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < scores.Count)
        {
            return scores[playerIndex];
        }
        return 0;
    }
    
    public float GetRemainingTime()
    {
        return remainingTime;
    }
    
    public bool IsGamePlaying()
    {
        return isPlaying;
    }
    
    void Update()
    {
        UpdateGame();
    }
}