using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PolygonGame : MiniGameBase
{
    [Header("Polygon Game Settings")]
    public int polygonCount = 20;
    public float spawnRadius = 10f;
    public float respawnTime = 3f;
    
    [Header("Polygon Objects")]
    public List<GameObject> polygons = new List<GameObject>();
    public List<GameObject> playerCollectors = new List<GameObject>();
    
    private List<Vector3> polygonPositions = new List<Vector3>();
    private List<bool> polygonActive = new List<bool>();
    private int[] polygonTypes = { 3, 4, 5, 6, 8 }; // Triangle, Square, Pentagon, Hexagon, Octagon
    
    protected override void SetupGame()
    {
        gameName = "Polygon Panic";
        CreatePolygons();
        CreatePlayerCollectors();
    }
    
    void CreatePolygons()
    {
        polygons.Clear();
        polygonPositions.Clear();
        polygonActive.Clear();
        
        for (int i = 0; i < polygonCount; i++)
        {
            Vector3 randomPosition = GetRandomSpawnPosition();
            GameObject polygon = CreatePolygonObject(randomPosition, i);
            
            polygons.Add(polygon);
            polygonPositions.Add(randomPosition);
            polygonActive.Add(true);
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return new Vector3(randomCircle.x, 0.5f, randomCircle.y);
    }
    
    GameObject CreatePolygonObject(Vector3 position, int index)
    {
        GameObject polygon = new GameObject($"Polygon_{index}");
        polygon.transform.position = position;
        
        // Create visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.SetParent(polygon.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.5f;
        
        // Randomize polygon type and color
        int polygonType = polygonTypes[Random.Range(0, polygonTypes.Length)];
        Color polygonColor = GetPolygonColor(polygonType);
        visual.GetComponent<MeshRenderer>().material.color = polygonColor;
        
        // Add rotation animation
        PolygonRotator rotator = polygon.AddComponent<PolygonRotator>();
        rotator.Initialize();
        
        // Add collector component
        PolygonCollector collector = polygon.AddComponent<PolygonCollector>();
        collector.Initialize(index, polygonType, this);
        
        // Add trigger collider
        SphereCollider collider = polygon.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.8f;
        
        return polygon;
    }
    
    Color GetPolygonColor(int sides)
    {
        switch (sides)
        {
            case 3: return Color.red;      // Triangle
            case 4: return Color.blue;     // Square
            case 5: return Color.green;    // Pentagon
            case 6: return Color.yellow;   // Hexagon
            case 8: return Color.magenta;  // Octagon
            default: return Color.white;
        }
    }
    
    void CreatePlayerCollectors()
    {
        playerCollectors.Clear();
        
        for (int i = 0; i < participants.Count; i++)
        {
            GameObject collector = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            collector.transform.position = Vector3.zero;
            collector.GetComponent<MeshRenderer>().material.color = participants[i].playerColor;
            collector.name = $"Collector_{participants[i].playerName}";
            
            // Add rigidbody for movement
            Rigidbody rb = collector.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 5f;
            
            // Add player controller
            PolygonPlayerController controller = collector.AddComponent<PolygonPlayerController>();
            controller.Initialize(i);
            
            playerCollectors.Add(collector);
        }
    }
    
    protected override void OnGameStart()
    {
        Debug.Log("Polygon Panic started! Collect polygons to score points!");
        StartCoroutine(SpawnPolygons());
    }
    
    IEnumerator SpawnPolygons()
    {
        while (isPlaying)
        {
            yield return new WaitForSeconds(respawnTime);
            
            // Respawn collected polygons
            for (int i = 0; i < polygons.Count; i++)
            {
                if (!polygonActive[i])
                {
                    RespawnPolygon(i);
                }
            }
        }
    }
    
    void RespawnPolygon(int index)
    {
        if (index >= 0 && index < polygons.Count)
        {
            GameObject polygon = polygons[index];
            polygon.transform.position = GetRandomSpawnPosition();
            polygon.SetActive(true);
            polygonActive[index] = true;
        }
    }
    
    public void CollectPolygon(int polygonIndex, int playerIndex, int polygonType)
    {
        if (polygonIndex >= 0 && polygonIndex < polygons.Count && polygonActive[polygonIndex])
        {
            polygonActive[polygonIndex] = false;
            polygons[polygonIndex].SetActive(false);
            
            // Award points based on polygon type
            int points = GetPolygonPoints(polygonType);
            AddScore(playerIndex, points);
            
            Debug.Log($"{participants[playerIndex].playerName} collected a {polygonType}-sided polygon for {points} points!");
        }
    }
    
    int GetPolygonPoints(int sides)
    {
        switch (sides)
        {
            case 3: return 10;   // Triangle
            case 4: return 20;   // Square
            case 5: return 30;   // Pentagon
            case 6: return 40;   // Hexagon
            case 8: return 50;   // Octagon
            default: return 10;
        }
    }
    
    protected override void HandleGameLogic()
    {
        // Game logic is handled through collision detection and coroutines
    }
    
    protected override void CheckWinConditions()
    {
        // Game continues until time runs out
    }
    
    protected override void OnGameEnd()
    {
        if (winnerIndex >= 0)
        {
            Debug.Log($"{participants[winnerIndex].playerName} won Polygon Panic with {scores[winnerIndex]} points!");
        }
        else
        {
            Debug.Log("Polygon Panic ended!");
        }
        
        CleanupPolygonGame();
    }
    
    void CleanupPolygonGame()
    {
        foreach (GameObject polygon in polygons)
        {
            if (polygon != null) Destroy(polygon);
        }
        
        foreach (GameObject collector in playerCollectors)
        {
            if (collector != null) Destroy(collector);
        }
        
        polygons.Clear();
        playerCollectors.Clear();
    }
    
    void OnDestroy()
    {
        CleanupPolygonGame();
    }
}

// Helper class for polygon rotation animation
public class PolygonRotator : MonoBehaviour
{
    private float rotationSpeed;
    
    public void Initialize()
    {
        rotationSpeed = Random.Range(30f, 120f);
    }
    
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}

// Helper class for polygon collection
public class PolygonCollector : MonoBehaviour
{
    private int polygonIndex;
    private int polygonType;
    private PolygonGame polygonGame;
    
    public void Initialize(int index, int type, PolygonGame game)
    {
        polygonIndex = index;
        polygonType = type;
        polygonGame = game;
    }
    
    void OnTriggerEnter(Collider other)
    {
        PolygonPlayerController controller = other.GetComponent<PolygonPlayerController>();
        if (controller != null)
        {
            polygonGame.CollectPolygon(polygonIndex, controller.playerIndex, polygonType);
        }
    }
}

// Helper class for player movement in polygon game
public class PolygonPlayerController : MonoBehaviour
{
    public int playerIndex;
    private Rigidbody rb;
    
    public void Initialize(int index)
    {
        playerIndex = index;
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (playerIndex == 0) // Player 1 controls
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 movement = new Vector3(horizontal, 0, vertical) * 8f;
            rb.AddForce(movement);
        }
        else // AI movement
        {
            // Simple AI - move towards nearest polygon
            if (Random.Range(0f, 1f) < 0.05f)
            {
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0;
                rb.AddForce(randomDirection * 5f);
            }
        }
    }
}