using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlatformGame : MiniGameBase
{
    [Header("Platform Settings")]
    public float jumpForce = 8f;
    public float platformSpeed = 2f;
    public int numberOfPlatforms = 10;
    
    [Header("Platform Objects")]
    public List<GameObject> playerJumpers = new List<GameObject>();
    public List<GameObject> platforms = new List<GameObject>();
    public GameObject ground;
    
    private List<Vector3> startPositions = new List<Vector3>();
    private List<bool> playerAlive = new List<bool>();
    private List<Rigidbody> playerRigidbodies = new List<Rigidbody>();
    
    protected override void SetupGame()
    {
        gameName = "Platform Jump";
        CreatePlatformLevel();
        CreatePlayerJumpers();
    }
    
    void CreatePlatformLevel()
    {
        // Create ground
        ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = new Vector3(0, -5f, 0);
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
        ground.GetComponent<MeshRenderer>().material.color = Color.red;
        
        // Create moving platforms
        platforms.Clear();
        for (int i = 0; i < numberOfPlatforms; i++)
        {
            float yPosition = i * 2f + 1f;
            float xPosition = Random.Range(-8f, 8f);
            
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.transform.position = new Vector3(xPosition, yPosition, 0);
            platform.transform.localScale = new Vector3(3f, 0.2f, 3f);
            platform.GetComponent<MeshRenderer>().material.color = Color.blue;
            
            // Add platform movement
            PlatformMover mover = platform.AddComponent<PlatformMover>();
            mover.Initialize(platformSpeed, 8f);
            
            platforms.Add(platform);
        }
        
        // Create start positions
        startPositions.Clear();
        for (int i = 0; i < participants.Count; i++)
        {
            float xOffset = (i - (participants.Count - 1) / 2f) * 2f;
            startPositions.Add(new Vector3(xOffset, 0.5f, 0));
        }
    }
    
    void CreatePlayerJumpers()
    {
        playerJumpers.Clear();
        playerAlive.Clear();
        playerRigidbodies.Clear();
        
        for (int i = 0; i < participants.Count; i++)
        {
            // Create jumper object
            GameObject jumper = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            jumper.transform.position = startPositions[i];
            jumper.GetComponent<MeshRenderer>().material.color = participants[i].playerColor;
            jumper.name = $"Jumper_{participants[i].playerName}";
            
            // Add rigidbody for physics
            Rigidbody rb = jumper.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 1f;
            
            // Add collider
            CapsuleCollider collider = jumper.GetComponent<CapsuleCollider>();
            collider.isTrigger = false;
            
            playerJumpers.Add(jumper);
            playerAlive.Add(true);
            playerRigidbodies.Add(rb);
        }
    }
    
    protected override void OnGameStart()
    {
        Debug.Log("Platform game started! Press Space to jump!");
    }
    
    protected override void HandleGameLogic()
    {
        // Handle player input and AI
        for (int i = 0; i < playerJumpers.Count; i++)
        {
            if (!playerAlive[i]) continue;
            
            GameObject jumper = playerJumpers[i];
            Rigidbody rb = playerRigidbodies[i];
            
            // Check if player is on ground or platform
            bool isGrounded = IsGrounded(jumper);
            
            // Player 1 input
            if (i == 0 && Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            
            // AI behavior
            if (i > 0 && isGrounded)
            {
                // AI jumps with some probability
                if (Random.Range(0f, 1f) < 0.05f)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
            }
            
            // Check if player fell
            if (jumper.transform.position.y < -10f)
            {
                playerAlive[i] = false;
                jumper.SetActive(false);
                Debug.Log($"{participants[i].playerName} fell!");
            }
            else
            {
                // Score based on height
                int heightScore = Mathf.Max(0, Mathf.RoundToInt(jumper.transform.position.y * 10));
                SetScore(i, heightScore);
            }
        }
    }
    
    bool IsGrounded(GameObject jumper)
    {
        // Simple ground check
        RaycastHit hit;
        if (Physics.Raycast(jumper.transform.position, Vector3.down, out hit, 1.2f))
        {
            return true;
        }
        return false;
    }
    
    protected override void CheckWinConditions()
    {
        // Check if only one player is alive
        int aliveCount = 0;
        int lastAliveIndex = -1;
        
        for (int i = 0; i < playerAlive.Count; i++)
        {
            if (playerAlive[i])
            {
                aliveCount++;
                lastAliveIndex = i;
            }
        }
        
        if (aliveCount <= 1)
        {
            EndGame(lastAliveIndex);
        }
    }
    
    protected override void OnGameEnd()
    {
        if (winnerIndex >= 0)
        {
            Debug.Log($"{participants[winnerIndex].playerName} won the platform game!");
        }
        else
        {
            Debug.Log("Platform game ended with no winner!");
        }
        
        CleanupPlatformGame();
    }
    
    void CleanupPlatformGame()
    {
        // Destroy game objects
        if (ground != null) Destroy(ground);
        
        foreach (GameObject platform in platforms)
        {
            if (platform != null) Destroy(platform);
        }
        
        foreach (GameObject jumper in playerJumpers)
        {
            if (jumper != null) Destroy(jumper);
        }
        
        platforms.Clear();
        playerJumpers.Clear();
    }
    
    void OnDestroy()
    {
        CleanupPlatformGame();
    }
}

// Helper class for moving platforms
public class PlatformMover : MonoBehaviour
{
    private float speed;
    private float range;
    private float direction = 1f;
    private Vector3 startPosition;
    
    public void Initialize(float moveSpeed, float moveRange)
    {
        speed = moveSpeed;
        range = moveRange;
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Move platform back and forth
        transform.Translate(Vector3.right * speed * direction * Time.deltaTime);
        
        // Change direction if reached range
        if (Mathf.Abs(transform.position.x - startPosition.x) > range)
        {
            direction *= -1f;
        }
    }
}