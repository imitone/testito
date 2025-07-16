using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ColorGame : MiniGameBase
{
    [Header("Color Game Settings")]
    public float roundDuration = 5f;
    public int numberOfColors = 4;
    
    [Header("Color Objects")]
    public List<GameObject> colorZones = new List<GameObject>();
    public List<GameObject> playerMarkers = new List<GameObject>();
    public GameObject announcer;
    
    private List<Color> availableColors = new List<Color>();
    private Color currentTargetColor;
    private List<int> playerZones = new List<int>();
    private bool roundActive = false;
    private int roundNumber = 0;
    
    protected override void SetupGame()
    {
        gameName = "Color Clash";
        SetupColors();
        CreateColorZones();
        CreatePlayerMarkers();
    }
    
    void SetupColors()
    {
        availableColors.Clear();
        availableColors.Add(Color.red);
        availableColors.Add(Color.blue);
        availableColors.Add(Color.green);
        availableColors.Add(Color.yellow);
        availableColors.Add(Color.magenta);
        availableColors.Add(Color.cyan);
    }
    
    void CreateColorZones()
    {
        colorZones.Clear();
        
        for (int i = 0; i < numberOfColors; i++)
        {
            float angle = (i / (float)numberOfColors) * 360f * Mathf.Deg2Rad;
            float radius = 8f;
            
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
            
            GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            zone.transform.position = position;
            zone.transform.localScale = new Vector3(3f, 0.1f, 3f);
            zone.GetComponent<MeshRenderer>().material.color = availableColors[i];
            zone.name = $"ColorZone_{i}";
            
            // Add zone detector
            ColorZoneDetector detector = zone.AddComponent<ColorZoneDetector>();
            detector.Initialize(i, this);
            
            colorZones.Add(zone);
        }
    }
    
    void CreatePlayerMarkers()
    {
        playerMarkers.Clear();
        playerZones.Clear();
        
        for (int i = 0; i < participants.Count; i++)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            marker.transform.position = Vector3.zero;
            marker.GetComponent<MeshRenderer>().material.color = participants[i].playerColor;
            marker.name = $"Player_{participants[i].playerName}";
            
            // Add rigidbody for movement
            Rigidbody rb = marker.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 5f;
            
            // Add player controller
            ColorPlayerController controller = marker.AddComponent<ColorPlayerController>();
            controller.Initialize(i, this);
            
            playerMarkers.Add(marker);
            playerZones.Add(-1); // -1 means not in any zone
        }
    }
    
    protected override void OnGameStart()
    {
        StartCoroutine(GameLoop());
    }
    
    IEnumerator GameLoop()
    {
        while (isPlaying)
        {
            yield return StartCoroutine(PlayRound());
            roundNumber++;
            
            if (roundNumber >= 5) // Play 5 rounds
            {
                break;
            }
        }
    }
    
    IEnumerator PlayRound()
    {
        // Select target color
        currentTargetColor = availableColors[Random.Range(0, availableColors.Count)];
        
        // Announce target color
        Debug.Log($"Round {roundNumber + 1}: Get to {GetColorName(currentTargetColor)}!");
        
        // Give players time to move
        roundActive = true;
        yield return new WaitForSeconds(roundDuration);
        roundActive = false;
        
        // Check results
        CheckRoundResults();
        
        // Brief pause between rounds
        yield return new WaitForSeconds(2f);
    }
    
    void CheckRoundResults()
    {
        int targetZoneIndex = GetColorZoneIndex(currentTargetColor);
        
        for (int i = 0; i < playerZones.Count; i++)
        {
            if (playerZones[i] == targetZoneIndex)
            {
                AddScore(i, 100);
                Debug.Log($"{participants[i].playerName} got the correct color!");
            }
            else
            {
                Debug.Log($"{participants[i].playerName} got the wrong color!");
            }
        }
    }
    
    int GetColorZoneIndex(Color color)
    {
        for (int i = 0; i < availableColors.Count; i++)
        {
            if (ColorEquals(availableColors[i], color))
            {
                return i;
            }
        }
        return -1;
    }
    
    bool ColorEquals(Color a, Color b)
    {
        return Mathf.Approximately(a.r, b.r) && 
               Mathf.Approximately(a.g, b.g) && 
               Mathf.Approximately(a.b, b.b);
    }
    
    string GetColorName(Color color)
    {
        if (ColorEquals(color, Color.red)) return "RED";
        if (ColorEquals(color, Color.blue)) return "BLUE";
        if (ColorEquals(color, Color.green)) return "GREEN";
        if (ColorEquals(color, Color.yellow)) return "YELLOW";
        if (ColorEquals(color, Color.magenta)) return "MAGENTA";
        if (ColorEquals(color, Color.cyan)) return "CYAN";
        return "UNKNOWN";
    }
    
    public void SetPlayerZone(int playerIndex, int zoneIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerZones.Count)
        {
            playerZones[playerIndex] = zoneIndex;
        }
    }
    
    protected override void HandleGameLogic()
    {
        // Game logic is handled in the coroutine
    }
    
    protected override void CheckWinConditions()
    {
        // Win condition is checked at the end of all rounds
    }
    
    protected override void OnGameEnd()
    {
        if (winnerIndex >= 0)
        {
            Debug.Log($"{participants[winnerIndex].playerName} won the color game!");
        }
        else
        {
            Debug.Log("Color game ended!");
        }
        
        CleanupColorGame();
    }
    
    void CleanupColorGame()
    {
        foreach (GameObject zone in colorZones)
        {
            if (zone != null) Destroy(zone);
        }
        
        foreach (GameObject marker in playerMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        
        colorZones.Clear();
        playerMarkers.Clear();
    }
    
    void OnDestroy()
    {
        CleanupColorGame();
    }
}

// Helper class for color zone detection
public class ColorZoneDetector : MonoBehaviour
{
    private int zoneIndex;
    private ColorGame colorGame;
    
    public void Initialize(int index, ColorGame game)
    {
        zoneIndex = index;
        colorGame = game;
    }
    
    void OnTriggerEnter(Collider other)
    {
        ColorPlayerController controller = other.GetComponent<ColorPlayerController>();
        if (controller != null)
        {
            colorGame.SetPlayerZone(controller.playerIndex, zoneIndex);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        ColorPlayerController controller = other.GetComponent<ColorPlayerController>();
        if (controller != null)
        {
            colorGame.SetPlayerZone(controller.playerIndex, -1);
        }
    }
}

// Helper class for player movement in color game
public class ColorPlayerController : MonoBehaviour
{
    public int playerIndex;
    private ColorGame colorGame;
    private Rigidbody rb;
    
    public void Initialize(int index, ColorGame game)
    {
        playerIndex = index;
        colorGame = game;
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (playerIndex == 0) // Player 1 controls
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 movement = new Vector3(horizontal, 0, vertical) * 5f;
            rb.AddForce(movement);
        }
        else // AI movement
        {
            // Simple AI - move towards random color zone
            if (Random.Range(0f, 1f) < 0.02f)
            {
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0;
                rb.AddForce(randomDirection * 3f);
            }
        }
    }
}