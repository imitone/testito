using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool autoSetup = true;
    public bool createDefaultMaterials = true;
    public bool createDefaultPrefabs = true;
    
    [Header("Default Colors")]
    public Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };
    public Color defaultBoardColor = Color.white;
    public Color ownedBoardColor = Color.cyan;
    public Color specialBoardColor = Color.magenta;
    
    [Header("Generated Objects")]
    public GameObject boardSpacePrefab;
    public GameObject playerPrefab;
    public Material[] boardMaterials;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupGame();
        }
    }
    
    public void SetupGame()
    {
        Debug.Log("Setting up Polygon Board Game...");
        
        if (createDefaultMaterials)
        {
            CreateDefaultMaterials();
        }
        
        if (createDefaultPrefabs)
        {
            CreateDefaultPrefabs();
        }
        
        SetupManagers();
        
        Debug.Log("Game setup complete!");
    }
    
    void CreateDefaultMaterials()
    {
        Debug.Log("Creating default materials...");
        
        // Create board materials
        boardMaterials = new Material[3];
        
        // Default board material
        boardMaterials[0] = new Material(Shader.Find("Standard"));
        boardMaterials[0].color = defaultBoardColor;
        boardMaterials[0].name = "BoardDefault";
        
        // Owned board material
        boardMaterials[1] = new Material(Shader.Find("Standard"));
        boardMaterials[1].color = ownedBoardColor;
        boardMaterials[1].name = "BoardOwned";
        
        // Special board material
        boardMaterials[2] = new Material(Shader.Find("Standard"));
        boardMaterials[2].color = specialBoardColor;
        boardMaterials[2].name = "BoardSpecial";
    }
    
    void CreateDefaultPrefabs()
    {
        Debug.Log("Creating default prefabs...");
        
        CreateBoardSpacePrefab();
        CreatePlayerPrefab();
    }
    
    void CreateBoardSpacePrefab()
    {
        // Create board space prefab
        GameObject spaceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spaceObj.name = "BoardSpacePrefab";
        spaceObj.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
        
        // Add BoardSpace script
        BoardSpace boardSpace = spaceObj.AddComponent<BoardSpace>();
        
        // Set materials
        if (boardMaterials != null && boardMaterials.Length > 0)
        {
            boardSpace.defaultMaterial = boardMaterials[0];
            boardSpace.ownedMaterial = boardMaterials[1];
            boardSpace.specialMaterial = boardMaterials[2];
        }
        
        // Add text components
        CreateSpaceTexts(spaceObj);
        
        // Add owner indicator
        CreateOwnerIndicator(spaceObj);
        
        // Add collider
        BoxCollider collider = spaceObj.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        boardSpacePrefab = spaceObj;
        
        // Make it a prefab (in editor, you would save this as a prefab)
        spaceObj.SetActive(false);
    }
    
    void CreateSpaceTexts(GameObject spaceObj)
    {
        // Create name text
        GameObject nameTextObj = new GameObject("NameText");
        nameTextObj.transform.SetParent(spaceObj.transform);
        nameTextObj.transform.localPosition = new Vector3(0, 0.1f, 0);
        nameTextObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        TextMesh nameText = nameTextObj.AddComponent<TextMesh>();
        nameText.text = "Property";
        nameText.fontSize = 10;
        nameText.color = Color.black;
        nameText.alignment = TextAlignment.Center;
        nameText.anchor = TextAnchor.MiddleCenter;
        
        // Create price text
        GameObject priceTextObj = new GameObject("PriceText");
        priceTextObj.transform.SetParent(spaceObj.transform);
        priceTextObj.transform.localPosition = new Vector3(0, 0.1f, 0.5f);
        priceTextObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        TextMesh priceText = priceTextObj.AddComponent<TextMesh>();
        priceText.text = "$100";
        priceText.fontSize = 8;
        priceText.color = Color.black;
        priceText.alignment = TextAlignment.Center;
        priceText.anchor = TextAnchor.MiddleCenter;
        
        // Link to BoardSpace
        BoardSpace boardSpace = spaceObj.GetComponent<BoardSpace>();
        if (boardSpace != null)
        {
            boardSpace.nameText = nameText;
            boardSpace.priceText = priceText;
        }
    }
    
    void CreateOwnerIndicator(GameObject spaceObj)
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "OwnerIndicator";
        indicator.transform.SetParent(spaceObj.transform);
        indicator.transform.localPosition = new Vector3(0, 0.2f, 0);
        indicator.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        indicator.SetActive(false);
        
        // Link to BoardSpace
        BoardSpace boardSpace = spaceObj.GetComponent<BoardSpace>();
        if (boardSpace != null)
        {
            boardSpace.ownerIndicator = indicator;
        }
    }
    
    void CreatePlayerPrefab()
    {
        // Create player prefab
        GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerObj.name = "PlayerPrefab";
        playerObj.transform.localScale = Vector3.one * 0.8f;
        
        // Add Player script
        Player player = playerObj.AddComponent<Player>();
        
        // Add rigidbody
        Rigidbody rb = playerObj.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.drag = 5f;
        rb.freezeRotation = true;
        
        // Create player name text
        GameObject nameTextObj = new GameObject("PlayerNameText");
        nameTextObj.transform.SetParent(playerObj.transform);
        nameTextObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        TextMesh nameText = nameTextObj.AddComponent<TextMesh>();
        nameText.text = "Player";
        nameText.fontSize = 10;
        nameText.color = Color.white;
        nameText.alignment = TextAlignment.Center;
        nameText.anchor = TextAnchor.MiddleCenter;
        
        // Create money text
        GameObject moneyTextObj = new GameObject("MoneyText");
        moneyTextObj.transform.SetParent(playerObj.transform);
        moneyTextObj.transform.localPosition = new Vector3(0, 1.2f, 0);
        
        TextMesh moneyText = moneyTextObj.AddComponent<TextMesh>();
        moneyText.text = "$1500";
        moneyText.fontSize = 8;
        moneyText.color = Color.white;
        moneyText.alignment = TextAlignment.Center;
        moneyText.anchor = TextAnchor.MiddleCenter;
        
        // Link to Player
        player.playerNameText = nameText;
        player.moneyText = moneyText;
        player.playerRenderer = playerObj.GetComponent<MeshRenderer>();
        player.playerModel = playerObj;
        
        playerPrefab = playerObj;
        
        // Make it a prefab (in editor, you would save this as a prefab)
        playerObj.SetActive(false);
    }
    
    void SetupManagers()
    {
        Debug.Log("Setting up game managers...");
        
        // Find or create GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gameManager = gmObj.AddComponent<GameManager>();
        }
        
        // Find or create BoardManager
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        if (boardManager == null)
        {
            GameObject bmObj = new GameObject("BoardManager");
            boardManager = bmObj.AddComponent<BoardManager>();
        }
        
        // Find or create MiniGameManager
        MiniGameManager miniGameManager = FindObjectOfType<MiniGameManager>();
        if (miniGameManager == null)
        {
            GameObject mgmObj = new GameObject("MiniGameManager");
            miniGameManager = mgmObj.AddComponent<MiniGameManager>();
        }
        
        // Find or create UI_GameManager
        UI_GameManager uiManager = FindObjectOfType<UI_GameManager>();
        if (uiManager == null)
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("UI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Add UI Manager
            uiManager = canvasObj.AddComponent<UI_GameManager>();
            
            // Create basic UI structure
            CreateBasicUI(canvasObj);
        }
        
        // Link references
        if (gameManager != null)
        {
            gameManager.playerPrefab = playerPrefab;
            gameManager.uiManager = uiManager;
            gameManager.miniGameManager = miniGameManager;
        }
        
        if (boardManager != null)
        {
            boardManager.spacePrefab = boardSpacePrefab;
        }
    }
    
    void CreateBasicUI(GameObject canvasObj)
    {
        // Create main panels
        CreatePanel(canvasObj, "MainMenuPanel", new Vector2(0, 0), new Vector2(400, 300));
        CreatePanel(canvasObj, "GameHUD", new Vector2(0, 0), new Vector2(800, 600));
        CreatePanel(canvasObj, "PropertyDecisionPanel", new Vector2(0, 0), new Vector2(300, 200));
        CreatePanel(canvasObj, "MessagePanel", new Vector2(0, 0), new Vector2(350, 150));
        
        Debug.Log("Basic UI structure created. Please configure UI elements in the inspector.");
    }
    
    GameObject CreatePanel(GameObject parent, string name, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.5f);
        
        return panel;
    }
    
    [ContextMenu("Setup Game")]
    public void SetupGameManual()
    {
        SetupGame();
    }
    
    [ContextMenu("Reset Setup")]
    public void ResetSetup()
    {
        // Clean up existing objects
        if (boardSpacePrefab != null)
        {
            DestroyImmediate(boardSpacePrefab);
        }
        
        if (playerPrefab != null)
        {
            DestroyImmediate(playerPrefab);
        }
        
        Debug.Log("Setup reset. Run setup again to recreate objects.");
    }
}