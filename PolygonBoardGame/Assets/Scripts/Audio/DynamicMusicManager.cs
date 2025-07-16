using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DynamicMusicManager : MonoBehaviour
{
    public static DynamicMusicManager Instance { get; private set; }
    
    [Header("Music Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 0.8f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    public float crossfadeTime = 2f;
    public bool enableDynamicMusic = true;
    
    [Header("Audio Sources")]
    public AudioSource primarySource;
    public AudioSource secondarySource;
    public AudioSource ambienceSource;
    public AudioSource stingerSource;
    
    [Header("Music Tracks")]
    public MusicTrack[] musicTracks;
    
    [Header("Adaptive Music")]
    public bool enableAdaptiveMusic = true;
    public float adaptiveUpdateInterval = 0.5f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Custom Playlist")]
    public List<AudioClip> customTracks = new List<AudioClip>();
    public bool enableCustomPlaylist = false;
    public bool shuffleCustomPlaylist = true;
    
    [Header("Music Events")]
    public System.Action<string> OnTrackChanged;
    public System.Action<float> OnIntensityChanged;
    public System.Action<MusicState> OnMusicStateChanged;
    
    private AudioSource currentPrimarySource;
    private AudioSource currentSecondarySource;
    private MusicState currentMusicState = MusicState.Menu;
    private string currentTrackName = "";
    private float currentIntensity = 0f;
    private float targetIntensity = 0f;
    private bool isTransitioning = false;
    private Coroutine currentTransition;
    private Coroutine adaptiveCoroutine;
    private Dictionary<string, MusicTrack> trackDictionary = new Dictionary<string, MusicTrack>();
    
    // Custom playlist variables
    private int currentCustomTrackIndex = 0;
    private List<int> shuffledIndices = new List<int>();
    private bool isPlayingCustomPlaylist = false;
    
    [System.Serializable]
    public class MusicTrack
    {
        public string trackName;
        public AudioClip mainTrack;
        public AudioClip[] intensityLayers;
        public AudioClip[] harmonicLayers;
        public AudioClip[] rhythmicLayers;
        public MusicState associatedState;
        public float baseIntensity = 0.5f;
        public bool isLooping = true;
        public bool enableLayers = true;
        public float fadeInTime = 2f;
        public float fadeOutTime = 2f;
        [Range(0f, 1f)]
        public float trackVolume = 1f;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMusicSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Start with menu music
        PlayMusicForState(MusicState.Menu);
        
        // Subscribe to game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnPlayerTurnChanged += OnPlayerTurnChanged;
            GameManager.Instance.OnMiniGameStarted += OnMiniGameStarted;
            GameManager.Instance.OnMiniGameEnded += OnMiniGameEnded;
        }
    }
    
    void InitializeMusicSystem()
    {
        // Initialize audio sources
        if (primarySource == null)
            primarySource = gameObject.AddComponent<AudioSource>();
        if (secondarySource == null)
            secondarySource = gameObject.AddComponent<AudioSource>();
        if (ambienceSource == null)
            ambienceSource = gameObject.AddComponent<AudioSource>();
        if (stingerSource == null)
            stingerSource = gameObject.AddComponent<AudioSource>();
        
        // Configure audio sources
        ConfigureAudioSource(primarySource);
        ConfigureAudioSource(secondarySource);
        ConfigureAudioSource(ambienceSource);
        ConfigureAudioSource(stingerSource);
        
        currentPrimarySource = primarySource;
        currentSecondarySource = secondarySource;
        
        // Build track dictionary
        BuildTrackDictionary();
        
        // Initialize custom playlist
        if (enableCustomPlaylist)
        {
            InitializeCustomPlaylist();
        }
        
        // Start adaptive music system
        if (enableAdaptiveMusic)
        {
            StartAdaptiveMusic();
        }
    }
    
    void ConfigureAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.volume = 0f;
        source.spatialBlend = 0f; // 2D audio
    }
    
    void BuildTrackDictionary()
    {
        trackDictionary.Clear();
        
        foreach (var track in musicTracks)
        {
            if (!string.IsNullOrEmpty(track.trackName))
            {
                trackDictionary[track.trackName] = track;
            }
        }
    }
    
    void InitializeCustomPlaylist()
    {
        if (customTracks.Count > 0)
        {
            shuffledIndices.Clear();
            for (int i = 0; i < customTracks.Count; i++)
            {
                shuffledIndices.Add(i);
            }
            
            if (shuffleCustomPlaylist)
            {
                ShufflePlaylist();
            }
        }
    }
    
    void ShufflePlaylist()
    {
        for (int i = 0; i < shuffledIndices.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledIndices.Count);
            int temp = shuffledIndices[i];
            shuffledIndices[i] = shuffledIndices[randomIndex];
            shuffledIndices[randomIndex] = temp;
        }
    }
    
    #region Public Methods
    
    public void PlayMusicForState(MusicState state)
    {
        if (currentMusicState == state && !isPlayingCustomPlaylist) return;
        
        currentMusicState = state;
        OnMusicStateChanged?.Invoke(state);
        
        MusicTrack track = FindTrackForState(state);
        if (track != null)
        {
            PlayTrack(track);
        }
    }
    
    public void PlayTrack(string trackName)
    {
        if (trackDictionary.ContainsKey(trackName))
        {
            PlayTrack(trackDictionary[trackName]);
        }
        else
        {
            Debug.LogWarning($"Track '{trackName}' not found in music tracks!");
        }
    }
    
    public void PlayTrack(MusicTrack track)
    {
        if (track == null || track.mainTrack == null) return;
        
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        
        currentTransition = StartCoroutine(TransitionToTrack(track));
    }
    
    public void SetIntensity(float intensity)
    {
        targetIntensity = Mathf.Clamp01(intensity);
        OnIntensityChanged?.Invoke(targetIntensity);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumeForAllSources();
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumeForAllSources();
    }
    
    public void PlayStinger(AudioClip stinger, float volume = 1f)
    {
        if (stinger != null && stingerSource != null)
        {
            stingerSource.clip = stinger;
            stingerSource.volume = volume * musicVolume * masterVolume;
            stingerSource.Play();
        }
    }
    
    public void FadeOut(float duration = 2f)
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        
        currentTransition = StartCoroutine(FadeOutCoroutine(duration));
    }
    
    public void FadeIn(float duration = 2f)
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        
        currentTransition = StartCoroutine(FadeInCoroutine(duration));
    }
    
    public void PauseMusic()
    {
        currentPrimarySource.Pause();
        currentSecondarySource.Pause();
    }
    
    public void ResumeMusic()
    {
        currentPrimarySource.UnPause();
        currentSecondarySource.UnPause();
    }
    
    public void StopMusic()
    {
        currentPrimarySource.Stop();
        currentSecondarySource.Stop();
        currentTrackName = "";
    }
    
    public void EnableCustomPlaylist(bool enable)
    {
        enableCustomPlaylist = enable;
        isPlayingCustomPlaylist = enable;
        
        if (enable && customTracks.Count > 0)
        {
            PlayNextCustomTrack();
        }
        else
        {
            PlayMusicForState(currentMusicState);
        }
    }
    
    public void AddCustomTrack(AudioClip track)
    {
        if (track != null)
        {
            customTracks.Add(track);
            if (enableCustomPlaylist)
            {
                InitializeCustomPlaylist();
            }
        }
    }
    
    public void RemoveCustomTrack(AudioClip track)
    {
        customTracks.Remove(track);
        if (enableCustomPlaylist)
        {
            InitializeCustomPlaylist();
        }
    }
    
    public void PlayNextCustomTrack()
    {
        if (!enableCustomPlaylist || customTracks.Count == 0) return;
        
        currentCustomTrackIndex = (currentCustomTrackIndex + 1) % shuffledIndices.Count;
        
        if (currentCustomTrackIndex == 0 && shuffleCustomPlaylist)
        {
            ShufflePlaylist();
        }
        
        AudioClip nextTrack = customTracks[shuffledIndices[currentCustomTrackIndex]];
        PlayCustomTrack(nextTrack);
    }
    
    public void PlayPreviousCustomTrack()
    {
        if (!enableCustomPlaylist || customTracks.Count == 0) return;
        
        currentCustomTrackIndex = (currentCustomTrackIndex - 1 + shuffledIndices.Count) % shuffledIndices.Count;
        AudioClip previousTrack = customTracks[shuffledIndices[currentCustomTrackIndex]];
        PlayCustomTrack(previousTrack);
    }
    
    #endregion
    
    #region Private Methods
    
    private void PlayCustomTrack(AudioClip track)
    {
        if (track == null) return;
        
        MusicTrack customTrack = new MusicTrack
        {
            trackName = track.name,
            mainTrack = track,
            isLooping = true,
            trackVolume = 1f,
            fadeInTime = crossfadeTime,
            fadeOutTime = crossfadeTime
        };
        
        PlayTrack(customTrack);
        isPlayingCustomPlaylist = true;
    }
    
    private MusicTrack FindTrackForState(MusicState state)
    {
        foreach (var track in musicTracks)
        {
            if (track.associatedState == state)
            {
                return track;
            }
        }
        
        // Fallback to first track if no match found
        return musicTracks.Length > 0 ? musicTracks[0] : null;
    }
    
    private IEnumerator TransitionToTrack(MusicTrack newTrack)
    {
        isTransitioning = true;
        
        // Setup new track on secondary source
        AudioSource newSource = currentSecondarySource;
        newSource.clip = newTrack.mainTrack;
        newSource.loop = newTrack.isLooping;
        newSource.volume = 0f;
        newSource.Play();
        
        // Crossfade
        float elapsed = 0f;
        float oldVolume = currentPrimarySource.volume;
        float newVolume = newTrack.trackVolume * musicVolume * masterVolume;
        
        while (elapsed < crossfadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeTime;
            
            currentPrimarySource.volume = Mathf.Lerp(oldVolume, 0f, t);
            newSource.volume = Mathf.Lerp(0f, newVolume, t);
            
            yield return null;
        }
        
        // Finish transition
        currentPrimarySource.Stop();
        currentPrimarySource.volume = 0f;
        
        // Swap sources
        AudioSource temp = currentPrimarySource;
        currentPrimarySource = currentSecondarySource;
        currentSecondarySource = temp;
        
        currentTrackName = newTrack.trackName;
        OnTrackChanged?.Invoke(currentTrackName);
        
        isTransitioning = false;
        
        // Handle track ending for custom playlist
        if (isPlayingCustomPlaylist && enableCustomPlaylist)
        {
            StartCoroutine(WaitForTrackEnd(newTrack));
        }
    }
    
    private IEnumerator WaitForTrackEnd(MusicTrack track)
    {
        if (track.isLooping) yield break;
        
        float trackLength = track.mainTrack.length;
        yield return new WaitForSeconds(trackLength - crossfadeTime);
        
        if (isPlayingCustomPlaylist)
        {
            PlayNextCustomTrack();
        }
    }
    
    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = currentPrimarySource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentPrimarySource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        
        currentPrimarySource.volume = 0f;
        currentPrimarySource.Stop();
    }
    
    private IEnumerator FadeInCoroutine(float duration)
    {
        float targetVolume = musicVolume * masterVolume;
        float elapsed = 0f;
        
        currentPrimarySource.volume = 0f;
        if (!currentPrimarySource.isPlaying)
        {
            currentPrimarySource.Play();
        }
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentPrimarySource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }
        
        currentPrimarySource.volume = targetVolume;
    }
    
    private void StartAdaptiveMusic()
    {
        if (adaptiveCoroutine != null)
        {
            StopCoroutine(adaptiveCoroutine);
        }
        
        adaptiveCoroutine = StartCoroutine(AdaptiveMusicCoroutine());
    }
    
    private IEnumerator AdaptiveMusicCoroutine()
    {
        while (enableAdaptiveMusic)
        {
            UpdateAdaptiveMusic();
            yield return new WaitForSeconds(adaptiveUpdateInterval);
        }
    }
    
    private void UpdateAdaptiveMusic()
    {
        // Smooth intensity transition
        if (Mathf.Abs(currentIntensity - targetIntensity) > 0.01f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 2f);
            
            // Apply intensity to current track
            float intensityVolume = intensityCurve.Evaluate(currentIntensity);
            UpdateVolumeForAllSources();
        }
    }
    
    private void UpdateVolumeForAllSources()
    {
        float finalVolume = musicVolume * masterVolume;
        
        if (currentPrimarySource != null)
        {
            currentPrimarySource.volume = finalVolume * intensityCurve.Evaluate(currentIntensity);
        }
        
        if (currentSecondarySource != null && isTransitioning)
        {
            currentSecondarySource.volume = finalVolume * intensityCurve.Evaluate(currentIntensity);
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnGameStateChanged(GameState newState)
    {
        MusicState musicState = ConvertGameStateToMusicState(newState);
        PlayMusicForState(musicState);
        
        // Adjust intensity based on game state
        switch (newState)
        {
            case GameState.Playing:
                SetIntensity(0.6f);
                break;
            case GameState.MiniGame:
                SetIntensity(0.9f);
                break;
            case GameState.GameOver:
                SetIntensity(0.3f);
                break;
            default:
                SetIntensity(0.5f);
                break;
        }
    }
    
    private void OnPlayerTurnChanged(int playerIndex)
    {
        // Slightly increase intensity during player turns
        SetIntensity(0.7f);
    }
    
    private void OnMiniGameStarted(MiniGameType gameType)
    {
        PlayMusicForState(MusicState.MiniGame);
        SetIntensity(1f);
    }
    
    private void OnMiniGameEnded(MiniGameType gameType, int winnerId)
    {
        PlayMusicForState(MusicState.Playing);
        SetIntensity(0.6f);
    }
    
    private MusicState ConvertGameStateToMusicState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Menu:
                return MusicState.Menu;
            case GameState.Playing:
                return MusicState.Gameplay;
            case GameState.MiniGame:
                return MusicState.MiniGame;
            case GameState.GameOver:
                return MusicState.Victory;
            default:
                return MusicState.Menu;
        }
    }
    
    #endregion
    
    #region Public Properties
    
    public bool IsPlaying => currentPrimarySource != null && currentPrimarySource.isPlaying;
    public string CurrentTrackName => currentTrackName;
    public float CurrentIntensity => currentIntensity;
    public MusicState CurrentMusicState => currentMusicState;
    public bool IsTransitioning => isTransitioning;
    public bool IsCustomPlaylistEnabled => enableCustomPlaylist;
    public int CustomTrackCount => customTracks.Count;
    public int CurrentCustomTrackIndex => currentCustomTrackIndex;
    
    #endregion
    
    void OnDestroy()
    {
        if (adaptiveCoroutine != null)
        {
            StopCoroutine(adaptiveCoroutine);
        }
        
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
    }
}

public enum MusicState
{
    Menu,
    Gameplay,
    MiniGame,
    Victory,
    Tension,
    Ambient
}