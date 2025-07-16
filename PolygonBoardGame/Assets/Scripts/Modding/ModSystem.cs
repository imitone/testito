using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

public class ModSystem : MonoBehaviour
{
    public static ModSystem Instance { get; private set; }
    
    [Header("Mod Settings")]
    public bool enableModSupport = true;
    public string modDirectory = "Mods";
    public string customBoardsPath = "Boards";
    public string customMiniGamesPath = "MiniGames";
    public string customAssetsPath = "Assets";
    
    [Header("Mod Loading")]
    public bool loadOnStartup = true;
    public bool enableHotReloading = true;
    public float hotReloadCheckInterval = 2f;
    
    [Header("Security")]
    public bool enableSandboxMode = true;
    public List<string> allowedNamespaces = new List<string> { "UnityEngine", "System", "PolygonBoardGame" };
    public List<string> blockedMethods = new List<string> { "File", "Directory", "Process", "Registry" };
    
    [Header("Events")]
    public System.Action<ModInfo> OnModLoaded;
    public System.Action<ModInfo> OnModUnloaded;
    public System.Action<string> OnModError;
    
    private Dictionary<string, ModInfo> loadedMods = new Dictionary<string, ModInfo>();
    private Dictionary<string, CustomMiniGame> customMiniGames = new Dictionary<string, CustomMiniGame>();
    private Dictionary<string, CustomBoard> customBoards = new Dictionary<string, CustomBoard>();
    private FileSystemWatcher modWatcher;
    private float lastHotReloadCheck = 0f;
    
    [System.Serializable]
    public class ModInfo
    {
        public string id;
        public string name;
        public string version;
        public string author;
        public string description;
        public string unityVersion;
        public bool isEnabled;
        public bool isValid;
        public DateTime loadTime;
        public string filePath;
        public List<string> dependencies = new List<string>();
        public List<string> miniGames = new List<string>();
        public List<string> boards = new List<string>();
        public List<string> assets = new List<string>();
        public Dictionary<string, object> customData = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class CustomMiniGame
    {
        public string id;
        public string name;
        public string description;
        public Sprite icon;
        public GameObject gamePrefab;
        public Type gameScript;
        public MiniGameSettings settings;
        public bool isEnabled;
        public string modId;
        
        [System.Serializable]
        public class MiniGameSettings
        {
            public float timeLimit = 60f;
            public int minPlayers = 2;
            public int maxPlayers = 4;
            public bool allowsPowerUps = true;
            public bool isTimedGame = true;
            public bool allowsSpectators = true;
            public Dictionary<string, object> customSettings = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class CustomBoard
    {
        public string id;
        public string name;
        public string description;
        public Sprite thumbnail;
        public GameObject boardPrefab;
        public BoardSettings settings;
        public bool isEnabled;
        public string modId;
        
        [System.Serializable]
        public class BoardSettings
        {
            public int spaceCount = 40;
            public Vector3 boardSize = new Vector3(20, 0, 20);
            public BoardTheme theme = BoardTheme.Classic;
            public List<SpaceConfiguration> spaces = new List<SpaceConfiguration>();
            public Dictionary<string, object> customSettings = new Dictionary<string, object>();
        }
        
        [System.Serializable]
        public class SpaceConfiguration
        {
            public int index;
            public SpaceType type;
            public string name;
            public int value;
            public Vector3 position;
            public Vector3 rotation;
            public Color color;
            public Sprite icon;
        }
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeModSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (loadOnStartup)
        {
            LoadAllMods();
        }
        
        SetupHotReloading();
    }
    
    void Update()
    {
        if (enableHotReloading && Time.time - lastHotReloadCheck > hotReloadCheckInterval)
        {
            CheckForModChanges();
            lastHotReloadCheck = Time.time;
        }
    }
    
    void InitializeModSystem()
    {
        if (!enableModSupport) return;
        
        // Create mod directories
        CreateModDirectories();
        
        // Initialize mod watching
        if (enableHotReloading)
        {
            SetupFileWatcher();
        }
        
        Debug.Log("Mod System initialized successfully!");
    }
    
    void CreateModDirectories()
    {
        string basePath = Path.Combine(Application.persistentDataPath, modDirectory);
        
        try
        {
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, customBoardsPath));
            Directory.CreateDirectory(Path.Combine(basePath, customMiniGamesPath));
            Directory.CreateDirectory(Path.Combine(basePath, customAssetsPath));
            
            Debug.Log($"Mod directories created at: {basePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create mod directories: {e.Message}");
        }
    }
    
    void SetupFileWatcher()
    {
        try
        {
            string watchPath = Path.Combine(Application.persistentDataPath, modDirectory);
            
            modWatcher = new FileSystemWatcher(watchPath)
            {
                IncludeSubdirectories = true,
                Filter = "*.dll",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            
            modWatcher.Changed += OnModFileChanged;
            modWatcher.Created += OnModFileChanged;
            modWatcher.Deleted += OnModFileChanged;
            modWatcher.Renamed += OnModFileRenamed;
            
            modWatcher.EnableRaisingEvents = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to setup file watcher: {e.Message}");
        }
    }
    
    void SetupHotReloading()
    {
        // Additional setup for hot reloading
    }
    
    #region Mod Loading
    
    public void LoadAllMods()
    {
        if (!enableModSupport) return;
        
        string modsPath = Path.Combine(Application.persistentDataPath, modDirectory);
        
        if (!Directory.Exists(modsPath))
        {
            Debug.LogWarning($"Mods directory not found: {modsPath}");
            return;
        }
        
        // Load mods from directories
        string[] modDirectories = Directory.GetDirectories(modsPath);
        foreach (string modDir in modDirectories)
        {
            LoadMod(modDir);
        }
        
        // Load standalone mod files
        string[] modFiles = Directory.GetFiles(modsPath, "*.dll");
        foreach (string modFile in modFiles)
        {
            LoadModFromFile(modFile);
        }
        
        Debug.Log($"Loaded {loadedMods.Count} mods successfully!");
    }
    
    public bool LoadMod(string modPath)
    {
        try
        {
            ModInfo modInfo = ReadModInfo(modPath);
            if (modInfo == null) return false;
            
            if (loadedMods.ContainsKey(modInfo.id))
            {
                Debug.LogWarning($"Mod {modInfo.id} is already loaded!");
                return false;
            }
            
            // Check dependencies
            if (!CheckModDependencies(modInfo))
            {
                Debug.LogError($"Mod {modInfo.id} has missing dependencies!");
                return false;
            }
            
            // Load mod assembly
            Assembly modAssembly = LoadModAssembly(modPath);
            if (modAssembly == null) return false;
            
            // Load custom mini-games
            LoadCustomMiniGames(modAssembly, modInfo);
            
            // Load custom boards
            LoadCustomBoards(modAssembly, modInfo);
            
            // Load custom assets
            LoadCustomAssets(modPath, modInfo);
            
            modInfo.isValid = true;
            modInfo.loadTime = DateTime.Now;
            loadedMods[modInfo.id] = modInfo;
            
            OnModLoaded?.Invoke(modInfo);
            
            Debug.Log($"Successfully loaded mod: {modInfo.name} v{modInfo.version}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load mod from {modPath}: {e.Message}");
            OnModError?.Invoke(e.Message);
            return false;
        }
    }
    
    public bool LoadModFromFile(string filePath)
    {
        try
        {
            Assembly assembly = Assembly.LoadFrom(filePath);
            
            // Create temporary mod info
            ModInfo modInfo = new ModInfo
            {
                id = Path.GetFileNameWithoutExtension(filePath),
                name = Path.GetFileNameWithoutExtension(filePath),
                version = "1.0.0",
                author = "Unknown",
                description = "Loaded from file",
                filePath = filePath,
                isEnabled = true
            };
            
            // Load components from assembly
            LoadCustomMiniGames(assembly, modInfo);
            LoadCustomBoards(assembly, modInfo);
            
            modInfo.isValid = true;
            modInfo.loadTime = DateTime.Now;
            loadedMods[modInfo.id] = modInfo;
            
            OnModLoaded?.Invoke(modInfo);
            
            Debug.Log($"Successfully loaded mod from file: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load mod from file {filePath}: {e.Message}");
            OnModError?.Invoke(e.Message);
            return false;
        }
    }
    
    public void UnloadMod(string modId)
    {
        if (!loadedMods.ContainsKey(modId)) return;
        
        ModInfo modInfo = loadedMods[modId];
        
        // Remove custom mini-games
        foreach (string gameId in modInfo.miniGames)
        {
            customMiniGames.Remove(gameId);
        }
        
        // Remove custom boards
        foreach (string boardId in modInfo.boards)
        {
            customBoards.Remove(boardId);
        }
        
        loadedMods.Remove(modId);
        OnModUnloaded?.Invoke(modInfo);
        
        Debug.Log($"Unloaded mod: {modInfo.name}");
    }
    
    public void ReloadMod(string modId)
    {
        if (!loadedMods.ContainsKey(modId)) return;
        
        ModInfo modInfo = loadedMods[modId];
        string modPath = Path.GetDirectoryName(modInfo.filePath);
        
        UnloadMod(modId);
        LoadMod(modPath);
    }
    
    #endregion
    
    #region Mod Info Reading
    
    ModInfo ReadModInfo(string modPath)
    {
        string infoPath = Path.Combine(modPath, "mod.json");
        
        if (!File.Exists(infoPath))
        {
            Debug.LogWarning($"mod.json not found in {modPath}");
            return null;
        }
        
        try
        {
            string json = File.ReadAllText(infoPath);
            ModInfo modInfo = JsonUtility.FromJson<ModInfo>(json);
            modInfo.filePath = modPath;
            return modInfo;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read mod info from {infoPath}: {e.Message}");
            return null;
        }
    }
    
    bool CheckModDependencies(ModInfo modInfo)
    {
        foreach (string dependency in modInfo.dependencies)
        {
            if (!loadedMods.ContainsKey(dependency))
            {
                Debug.LogError($"Missing dependency: {dependency}");
                return false;
            }
        }
        return true;
    }
    
    Assembly LoadModAssembly(string modPath)
    {
        string[] dllFiles = Directory.GetFiles(modPath, "*.dll");
        
        if (dllFiles.Length == 0)
        {
            Debug.LogWarning($"No DLL files found in {modPath}");
            return null;
        }
        
        try
        {
            // Load the main assembly (assuming first DLL is the main one)
            Assembly assembly = Assembly.LoadFrom(dllFiles[0]);
            
            if (enableSandboxMode)
            {
                ValidateAssembly(assembly);
            }
            
            return assembly;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load assembly from {modPath}: {e.Message}");
            return null;
        }
    }
    
    void ValidateAssembly(Assembly assembly)
    {
        // Basic security validation
        Type[] types = assembly.GetTypes();
        
        foreach (Type type in types)
        {
            // Check if type is in allowed namespaces
            if (!IsNamespaceAllowed(type.Namespace))
            {
                throw new SecurityException($"Type {type.FullName} is not in allowed namespace");
            }
            
            // Check for blocked methods
            MethodInfo[] methods = type.GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (IsMethodBlocked(method))
                {
                    throw new SecurityException($"Method {method.Name} is blocked");
                }
            }
        }
    }
    
    bool IsNamespaceAllowed(string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName)) return true;
        
        return allowedNamespaces.Any(allowed => namespaceName.StartsWith(allowed));
    }
    
    bool IsMethodBlocked(MethodInfo method)
    {
        return blockedMethods.Any(blocked => method.Name.Contains(blocked));
    }
    
    #endregion
    
    #region Custom Mini-Games
    
    void LoadCustomMiniGames(Assembly assembly, ModInfo modInfo)
    {
        Type[] types = assembly.GetTypes();
        
        foreach (Type type in types)
        {
            if (type.IsSubclassOf(typeof(MiniGameBase)))
            {
                try
                {
                    CustomMiniGame customGame = CreateCustomMiniGame(type, modInfo);
                    if (customGame != null)
                    {
                        customMiniGames[customGame.id] = customGame;
                        modInfo.miniGames.Add(customGame.id);
                        
                        Debug.Log($"Loaded custom mini-game: {customGame.name}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load custom mini-game {type.Name}: {e.Message}");
                }
            }
        }
    }
    
    CustomMiniGame CreateCustomMiniGame(Type gameType, ModInfo modInfo)
    {
        // Get custom attributes for mini-game info
        var gameAttribute = gameType.GetCustomAttribute<CustomMiniGameAttribute>();
        if (gameAttribute == null) return null;
        
        CustomMiniGame customGame = new CustomMiniGame
        {
            id = gameAttribute.id,
            name = gameAttribute.name,
            description = gameAttribute.description,
            gameScript = gameType,
            modId = modInfo.id,
            isEnabled = true,
            settings = new CustomMiniGame.MiniGameSettings
            {
                timeLimit = gameAttribute.timeLimit,
                minPlayers = gameAttribute.minPlayers,
                maxPlayers = gameAttribute.maxPlayers,
                allowsPowerUps = gameAttribute.allowsPowerUps,
                isTimedGame = gameAttribute.isTimedGame
            }
        };
        
        return customGame;
    }
    
    public List<CustomMiniGame> GetCustomMiniGames()
    {
        return customMiniGames.Values.Where(g => g.isEnabled).ToList();
    }
    
    public CustomMiniGame GetCustomMiniGame(string gameId)
    {
        return customMiniGames.ContainsKey(gameId) ? customMiniGames[gameId] : null;
    }
    
    #endregion
    
    #region Custom Boards
    
    void LoadCustomBoards(Assembly assembly, ModInfo modInfo)
    {
        Type[] types = assembly.GetTypes();
        
        foreach (Type type in types)
        {
            if (type.IsSubclassOf(typeof(MonoBehaviour)) && type.Name.EndsWith("Board"))
            {
                try
                {
                    CustomBoard customBoard = CreateCustomBoard(type, modInfo);
                    if (customBoard != null)
                    {
                        customBoards[customBoard.id] = customBoard;
                        modInfo.boards.Add(customBoard.id);
                        
                        Debug.Log($"Loaded custom board: {customBoard.name}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load custom board {type.Name}: {e.Message}");
                }
            }
        }
    }
    
    CustomBoard CreateCustomBoard(Type boardType, ModInfo modInfo)
    {
        // Get custom attributes for board info
        var boardAttribute = boardType.GetCustomAttribute<CustomBoardAttribute>();
        if (boardAttribute == null) return null;
        
        CustomBoard customBoard = new CustomBoard
        {
            id = boardAttribute.id,
            name = boardAttribute.name,
            description = boardAttribute.description,
            modId = modInfo.id,
            isEnabled = true,
            settings = new CustomBoard.BoardSettings
            {
                spaceCount = boardAttribute.spaceCount,
                boardSize = boardAttribute.boardSize,
                theme = boardAttribute.theme
            }
        };
        
        return customBoard;
    }
    
    public List<CustomBoard> GetCustomBoards()
    {
        return customBoards.Values.Where(b => b.isEnabled).ToList();
    }
    
    public CustomBoard GetCustomBoard(string boardId)
    {
        return customBoards.ContainsKey(boardId) ? customBoards[boardId] : null;
    }
    
    #endregion
    
    #region Custom Assets
    
    void LoadCustomAssets(string modPath, ModInfo modInfo)
    {
        string assetsPath = Path.Combine(modPath, customAssetsPath);
        
        if (!Directory.Exists(assetsPath)) return;
        
        // Load textures
        LoadCustomTextures(assetsPath, modInfo);
        
        // Load audio clips
        LoadCustomAudioClips(assetsPath, modInfo);
        
        // Load prefabs (if any)
        LoadCustomPrefabs(assetsPath, modInfo);
    }
    
    void LoadCustomTextures(string assetsPath, ModInfo modInfo)
    {
        string[] imageFiles = Directory.GetFiles(assetsPath, "*.png")
            .Concat(Directory.GetFiles(assetsPath, "*.jpg"))
            .Concat(Directory.GetFiles(assetsPath, "*.jpeg"))
            .ToArray();
        
        foreach (string imageFile in imageFiles)
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(imageFile);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                
                string assetId = Path.GetFileNameWithoutExtension(imageFile);
                modInfo.assets.Add(assetId);
                
                Debug.Log($"Loaded custom texture: {assetId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load texture {imageFile}: {e.Message}");
            }
        }
    }
    
    void LoadCustomAudioClips(string assetsPath, ModInfo modInfo)
    {
        string[] audioFiles = Directory.GetFiles(assetsPath, "*.wav")
            .Concat(Directory.GetFiles(assetsPath, "*.mp3"))
            .Concat(Directory.GetFiles(assetsPath, "*.ogg"))
            .ToArray();
        
        foreach (string audioFile in audioFiles)
        {
            try
            {
                // Note: Loading audio files at runtime requires special handling
                // This is a simplified example
                string assetId = Path.GetFileNameWithoutExtension(audioFile);
                modInfo.assets.Add(assetId);
                
                Debug.Log($"Loaded custom audio: {assetId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load audio {audioFile}: {e.Message}");
            }
        }
    }
    
    void LoadCustomPrefabs(string assetsPath, ModInfo modInfo)
    {
        // Loading prefabs at runtime is complex and would require AssetBundles
        // This is a placeholder for the concept
    }
    
    #endregion
    
    #region Hot Reloading
    
    void CheckForModChanges()
    {
        // Check for modified mods and reload them
        foreach (var mod in loadedMods.Values.ToList())
        {
            if (File.Exists(mod.filePath))
            {
                DateTime lastWrite = File.GetLastWriteTime(mod.filePath);
                if (lastWrite > mod.loadTime)
                {
                    Debug.Log($"Mod {mod.name} has been modified, reloading...");
                    ReloadMod(mod.id);
                }
            }
        }
    }
    
    void OnModFileChanged(object sender, FileSystemEventArgs e)
    {
        // Handle file changes for hot reloading
        Debug.Log($"Mod file changed: {e.FullPath}");
    }
    
    void OnModFileRenamed(object sender, RenamedEventArgs e)
    {
        // Handle file renames
        Debug.Log($"Mod file renamed: {e.OldFullPath} -> {e.FullPath}");
    }
    
    #endregion
    
    #region Public API
    
    public List<ModInfo> GetLoadedMods()
    {
        return loadedMods.Values.ToList();
    }
    
    public ModInfo GetModInfo(string modId)
    {
        return loadedMods.ContainsKey(modId) ? loadedMods[modId] : null;
    }
    
    public bool IsModLoaded(string modId)
    {
        return loadedMods.ContainsKey(modId);
    }
    
    public void EnableMod(string modId)
    {
        if (loadedMods.ContainsKey(modId))
        {
            loadedMods[modId].isEnabled = true;
        }
    }
    
    public void DisableMod(string modId)
    {
        if (loadedMods.ContainsKey(modId))
        {
            loadedMods[modId].isEnabled = false;
        }
    }
    
    public void RefreshMods()
    {
        // Unload all mods
        foreach (string modId in loadedMods.Keys.ToList())
        {
            UnloadMod(modId);
        }
        
        // Reload all mods
        LoadAllMods();
    }
    
    #endregion
    
    void OnDestroy()
    {
        if (modWatcher != null)
        {
            modWatcher.Dispose();
        }
    }
}

// Attributes for mod developers
[AttributeUsage(AttributeTargets.Class)]
public class CustomMiniGameAttribute : Attribute
{
    public string id;
    public string name;
    public string description;
    public float timeLimit = 60f;
    public int minPlayers = 2;
    public int maxPlayers = 4;
    public bool allowsPowerUps = true;
    public bool isTimedGame = true;
    
    public CustomMiniGameAttribute(string id, string name, string description)
    {
        this.id = id;
        this.name = name;
        this.description = description;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class CustomBoardAttribute : Attribute
{
    public string id;
    public string name;
    public string description;
    public int spaceCount = 40;
    public Vector3 boardSize = new Vector3(20, 0, 20);
    public BoardTheme theme = BoardTheme.Classic;
    
    public CustomBoardAttribute(string id, string name, string description)
    {
        this.id = id;
        this.name = name;
        this.description = description;
    }
}

public enum BoardTheme
{
    Classic,
    Modern,
    Fantasy,
    SciFi,
    Horror,
    Comedy,
    Custom
}

public enum SpaceType
{
    Property,
    Start,
    Chance,
    Community,
    Tax,
    Utility,
    Railroad,
    Jail,
    FreeParking,
    GoToJail,
    Custom
}