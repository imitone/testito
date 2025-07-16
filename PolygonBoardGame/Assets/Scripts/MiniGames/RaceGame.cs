using UnityEngine;
using System.Collections.Generic;

public class RaceGame : MiniGameBase
{
    [Header("Race Settings")]
    public float trackLength = 20f;
    public float baseSpeed = 2f;
    public float speedVariation = 1f;
    
    [Header("Race Objects")]
    public List<GameObject> playerRacers = new List<GameObject>();
    public GameObject finishLine;
    public GameObject track;
    
    private List<Vector3> startPositions = new List<Vector3>();
    private List<float> playerProgress = new List<float>();
    private List<float> playerSpeeds = new List<float>();
    
    protected override void SetupGame()
    {
        gameName = "Race to the Finish";
        CreateRaceTrack();
        CreatePlayerRacers();
    }
    
    void CreateRaceTrack()
    {
        // Create track
        track = GameObject.CreatePrimitive(PrimitiveType.Plane);
        track.transform.position = Vector3.zero;
        track.transform.localScale = new Vector3(2f, 1f, 4f);
        track.GetComponent<MeshRenderer>().material.color = Color.gray;
        
        // Create finish line
        finishLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finishLine.transform.position = new Vector3(0, 0.5f, trackLength);
        finishLine.transform.localScale = new Vector3(10f, 1f, 0.2f);
        finishLine.GetComponent<MeshRenderer>().material.color = Color.red;
        
        // Create start positions
        startPositions.Clear();
        for (int i = 0; i < participants.Count; i++)
        {
            float xOffset = (i - (participants.Count - 1) / 2f) * 2f;
            startPositions.Add(new Vector3(xOffset, 0.5f, -trackLength + 2f));
        }
    }
    
    void CreatePlayerRacers()
    {
        playerRacers.Clear();
        playerProgress.Clear();
        playerSpeeds.Clear();
        
        for (int i = 0; i < participants.Count; i++)
        {
            // Create racer object
            GameObject racer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            racer.transform.position = startPositions[i];
            racer.GetComponent<MeshRenderer>().material.color = participants[i].playerColor;
            racer.name = $"Racer_{participants[i].playerName}";
            
            playerRacers.Add(racer);
            playerProgress.Add(0f);
            playerSpeeds.Add(baseSpeed + Random.Range(-speedVariation, speedVariation));
        }
    }
    
    protected override void OnGameStart()
    {
        // Show countdown or start message
        Debug.Log("Race started! Press Space to boost!");
    }
    
    protected override void HandleGameLogic()
    {
        // Update player positions
        for (int i = 0; i < playerRacers.Count; i++)
        {
            float speed = playerSpeeds[i];
            
            // Add boost if player presses space (simplified input)
            if (Input.GetKeyDown(KeyCode.Space) && i == 0) // Only for player 1 in this example
            {
                speed *= 2f;
            }
            
            // AI behavior for other players
            if (i > 0)
            {
                // Random chance to boost
                if (Random.Range(0f, 1f) < 0.02f)
                {
                    speed *= 1.5f;
                }
            }
            
            // Update progress
            playerProgress[i] += speed * Time.deltaTime;
            
            // Move racer
            Vector3 newPosition = Vector3.Lerp(startPositions[i], 
                new Vector3(startPositions[i].x, 0.5f, trackLength - 1f), 
                playerProgress[i] / trackLength);
            playerRacers[i].transform.position = newPosition;
            
            // Update score based on progress
            SetScore(i, Mathf.RoundToInt(playerProgress[i] * 10));
        }
    }
    
    protected override void CheckWinConditions()
    {
        // Check if any player reached the finish line
        for (int i = 0; i < playerProgress.Count; i++)
        {
            if (playerProgress[i] >= trackLength)
            {
                EndGame(i);
                return;
            }
        }
    }
    
    protected override void OnGameEnd()
    {
        // Show winner announcement
        if (winnerIndex >= 0)
        {
            Debug.Log($"{participants[winnerIndex].playerName} won the race!");
        }
        else
        {
            Debug.Log("Race ended with no winner!");
        }
        
        // Clean up race objects
        CleanupRace();
    }
    
    void CleanupRace()
    {
        // Destroy race objects
        if (track != null) Destroy(track);
        if (finishLine != null) Destroy(finishLine);
        
        foreach (GameObject racer in playerRacers)
        {
            if (racer != null) Destroy(racer);
        }
        
        playerRacers.Clear();
    }
    
    void OnDestroy()
    {
        CleanupRace();
    }
}