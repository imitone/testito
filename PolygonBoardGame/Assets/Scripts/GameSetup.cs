using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetup = true;
    public bool createDefaultMaterials = true;
    public bool createDefaultPrefabs = true;
    
    [Header("Game Configuration")]
    public Color[] playerColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };
    
    public Color defaultBoardColor = Color.white;
    public Color ownedBoardColor = Color.cyan;
    public Color specialBoardColor = Color.magenta;
    
    [Header("Prefab References")]
    public GameObject boardSpacePrefab;
    public GameObject playerPrefab;
    public Material[] boardMaterials;
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupGame();
        }
    }
    
    public void SetupGame()
    {
        Debug.Log("Setting up Polygon Board Game...");
        
        // Create necessary game objects
        CreateGameManagers();
        CreateUICanvas();
        CreateBoard();
        CreatePlayers();
        CreateCamera();
        CreateLighting();
        
        // Initialize game systems
        InitializeGameSystems();
        
        Debug.Log("Game setup complete! Ready to play.");
    }
    
    private void CreateGameManagers()
    {
        // Create GameManager if it doesn't exist
        if (GameManager.Instance == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
            gameManagerObj.AddComponent<BoardManager>();
            gameManagerObj.AddComponent<AudioManager>();
            gameManagerObj.AddComponent<AchievementSystem>();
            gameManagerObj.AddComponent<DynamicMusicManager>();
            gameManagerObj.AddComponent<ModSystem>();
            gameManagerObj.AddComponent<NetworkManager>();
            DontDestroyOnLoad(gameManagerObj);
        }
    }
    
    private void CreateUICanvas()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create UI_GameManager
            GameObject uiManagerObj = new GameObject("UI_GameManager");
            uiManagerObj.transform.SetParent(canvasObj.transform);
            uiManagerObj.AddComponent<UI_GameManager>();
        }
    }
    
    private void CreateBoard()
    {
        GameObject boardObj = GameObject.Find("Board");
        if (boardObj == null)
        {
            boardObj = new GameObject("Board");
            boardObj.transform.position = Vector3.zero;
            
            // Create board spaces in a polygon shape
            CreatePolygonBoard(boardObj);
        }
    }
    
    private void CreatePolygonBoard(GameObject parent)
    {
        int numSpaces = 20;
        float radius = 8f;
        
        for (int i = 0; i < numSpaces; i++)
        {
            float angle = (float)i / numSpaces * 360f * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            
            GameObject spaceObj = CreateBoardSpace(i, position);
            spaceObj.transform.SetParent(parent.transform);
        }
    }
    
    private GameObject CreateBoardSpace(int index, Vector3 position)
    {
        GameObject spaceObj;
        
        if (boardSpacePrefab != null)
        {
            spaceObj = Instantiate(boardSpacePrefab, position, Quaternion.identity);
        }
        else
        {
            // Create default board space
            spaceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spaceObj.transform.position = position;
            spaceObj.transform.localScale = new Vector3(1.5f, 0.2f, 1.5f);
            
            // Add colors
            Renderer renderer = spaceObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = GetBoardSpaceColor(index);
                renderer.material = mat;
            }
        }
        
        spaceObj.name = $"BoardSpace_{index}";
        
        // Add BoardSpace component
        BoardSpace boardSpace = spaceObj.AddComponent<BoardSpace>();
        boardSpace.spaceIndex = index;
        boardSpace.spaceType = GetBoardSpaceType(index);
        boardSpace.price = GetBoardSpacePrice(index);
        boardSpace.rent = GetBoardSpaceRent(index);
        boardSpace.spaceName = GetBoardSpaceName(index);
        
        return spaceObj;
    }
    
    private Color GetBoardSpaceColor(int index)
    {
        // Special spaces (start, mini-game, etc.)
        if (index == 0 || index == 5 || index == 10 || index == 15)
            return specialBoardColor;
        
        // Regular property spaces
        return defaultBoardColor;
    }
    
    private BoardSpace.SpaceType GetBoardSpaceType(int index)
    {
        if (index == 0) return BoardSpace.SpaceType.Start;
        if (index == 5 || index == 10 || index == 15) return BoardSpace.SpaceType.MiniGame;
        return BoardSpace.SpaceType.Property;
    }
    
    private int GetBoardSpacePrice(int index)
    {
        if (GetBoardSpaceType(index) == BoardSpace.SpaceType.Property)
        {
            return 100 + (index * 50);
        }
        return 0;
    }
    
    private int GetBoardSpaceRent(int index)
    {
        if (GetBoardSpaceType(index) == BoardSpace.SpaceType.Property)
        {
            return 20 + (index * 10);
        }
        return 0;
    }
    
    private string GetBoardSpaceName(int index)
    {
        if (index == 0) return "Start";
        if (index == 5) return "Mini-Game Arena";
        if (index == 10) return "Challenge Zone";
        if (index == 15) return "Battle Ground";
        return $"Property {index}";
    }
    
    private void CreatePlayers()
    {
        GameObject playersObj = GameObject.Find("Players");
        if (playersObj == null)
        {
            playersObj = new GameObject("Players");
        }
        
        // Create 4 players (1 human, 3 AI)
        for (int i = 0; i < 4; i++)
        {
            GameObject playerObj = CreatePlayer(i);
            playerObj.transform.SetParent(playersObj.transform);
        }
    }
    
    private GameObject CreatePlayer(int playerIndex)
    {
        GameObject playerObj;
        
        if (playerPrefab != null)
        {
            playerObj = Instantiate(playerPrefab);
        }
        else
        {
            // Create default player
            playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // Add color
            Renderer renderer = playerObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = playerColors[playerIndex % playerColors.Length];
                renderer.material = mat;
            }
        }
        
        playerObj.name = $"Player_{playerIndex}";
        
        // Add Player component
        Player player = playerObj.AddComponent<Player>();
        player.playerId = playerIndex;
        player.playerName = playerIndex == 0 ? "Human Player" : $"AI Player {playerIndex}";
        player.isHuman = playerIndex == 0;
        player.money = 1500; // Starting money
        player.currentPosition = 0;
        
        // Set initial position
        Vector3 startPos = GetBoardSpacePosition(0);
        startPos.y = 1f; // Above the board
        playerObj.transform.position = startPos;
        
        return playerObj;
    }
    
    private Vector3 GetBoardSpacePosition(int spaceIndex)
    {
        float angle = (float)spaceIndex / 20f * 360f * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Cos(angle) * 8f,
            0,
            Mathf.Sin(angle) * 8f
        );
    }
    
    private void CreateCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            
            // Position camera to view the board
            cameraObj.transform.position = new Vector3(0, 15, -10);
            cameraObj.transform.rotation = Quaternion.Euler(45, 0, 0);
            
            // Add camera controller
            cameraObj.AddComponent<CameraController>();
        }
    }
    
    private void CreateLighting()
    {
        Light[] lights = FindObjectsOfType<Light>();
        if (lights.Length == 0)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
    
    private void InitializeGameSystems()
    {
        // Wait a frame for all objects to be created
        StartCoroutine(InitializeAfterFrame());
    }
    
    private System.Collections.IEnumerator InitializeAfterFrame()
    {
        yield return null;
        
        // Initialize game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        
        // Initialize board manager
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.InitializeBoard();
        }
        
        // Initialize audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic();
        }
        
        Debug.Log("All game systems initialized!");
    }
}