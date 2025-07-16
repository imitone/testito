using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;
    
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    public AudioClip menuMusic;
    public AudioClip gameOverMusic;
    
    [Header("Game SFX")]
    public AudioClip diceRollSound;
    public AudioClip moveSound;
    public AudioClip purchaseSound;
    public AudioClip sellSound;
    public AudioClip rentSound;
    public AudioClip moneySound;
    public AudioClip errorSound;
    public AudioClip victorySound;
    public AudioClip gameOverSound;
    
    [Header("UI SFX")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    public AudioClip notificationSound;
    public AudioClip countdownSound;
    
    [Header("Mini-Game SFX")]
    public AudioClip miniGameStartSound;
    public AudioClip collectSound;
    public AudioClip jumpSound;
    public AudioClip matchSound;
    public AudioClip raceStartSound;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0f, 1f)]
    public float uiVolume = 0.6f;
    
    private Dictionary<string, AudioClip> audioClips;
    private bool isMuted = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            InitializeAudioClips();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetupAudioSources()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music Source");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UI Source");
            uiObj.transform.SetParent(transform);
            uiSource = uiObj.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
        }
    }
    
    void InitializeAudioClips()
    {
        audioClips = new Dictionary<string, AudioClip>();
        
        // Add all audio clips to dictionary for easy access
        if (backgroundMusic != null) audioClips["backgroundMusic"] = backgroundMusic;
        if (menuMusic != null) audioClips["menuMusic"] = menuMusic;
        if (gameOverMusic != null) audioClips["gameOverMusic"] = gameOverMusic;
        
        if (diceRollSound != null) audioClips["diceRoll"] = diceRollSound;
        if (moveSound != null) audioClips["move"] = moveSound;
        if (purchaseSound != null) audioClips["purchase"] = purchaseSound;
        if (sellSound != null) audioClips["sell"] = sellSound;
        if (rentSound != null) audioClips["rent"] = rentSound;
        if (moneySound != null) audioClips["money"] = moneySound;
        if (errorSound != null) audioClips["error"] = errorSound;
        if (victorySound != null) audioClips["victory"] = victorySound;
        if (gameOverSound != null) audioClips["gameOver"] = gameOverSound;
        
        if (buttonClickSound != null) audioClips["buttonClick"] = buttonClickSound;
        if (buttonHoverSound != null) audioClips["buttonHover"] = buttonHoverSound;
        if (notificationSound != null) audioClips["notification"] = notificationSound;
        if (countdownSound != null) audioClips["countdown"] = countdownSound;
        
        if (miniGameStartSound != null) audioClips["miniGameStart"] = miniGameStartSound;
        if (collectSound != null) audioClips["collect"] = collectSound;
        if (jumpSound != null) audioClips["jump"] = jumpSound;
        if (matchSound != null) audioClips["match"] = matchSound;
        if (raceStartSound != null) audioClips["raceStart"] = raceStartSound;
    }
    
    void Update()
    {
        UpdateVolumes();
    }
    
    void UpdateVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume * (isMuted ? 0f : 1f);
        }
        
        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume * (isMuted ? 0f : 1f);
        }
        
        if (uiSource != null)
        {
            uiSource.volume = masterVolume * uiVolume * (isMuted ? 0f : 1f);
        }
    }
    
    // Music Methods
    public void PlayBackgroundMusic()
    {
        PlayMusic("backgroundMusic");
    }
    
    public void PlayMenuMusic()
    {
        PlayMusic("menuMusic");
    }
    
    public void PlayGameOverMusic()
    {
        PlayMusic("gameOverMusic");
    }
    
    void PlayMusic(string clipName)
    {
        if (audioClips.ContainsKey(clipName) && musicSource != null)
        {
            if (musicSource.clip != audioClips[clipName])
            {
                musicSource.clip = audioClips[clipName];
                musicSource.Play();
            }
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void FadeOutMusic(float duration = 1f)
    {
        StartCoroutine(FadeOutMusicCoroutine(duration));
    }
    
    IEnumerator FadeOutMusicCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        musicSource.Stop();
        UpdateVolumes(); // Restore volume
    }
    
    // Game SFX Methods
    public void PlayDiceRoll()
    {
        PlaySFX("diceRoll");
    }
    
    public void PlayMoveSound()
    {
        PlaySFX("move");
    }
    
    public void PlayPurchaseSound()
    {
        PlaySFX("purchase");
    }
    
    public void PlaySellSound()
    {
        PlaySFX("sell");
    }
    
    public void PlayRentSound()
    {
        PlaySFX("rent");
    }
    
    public void PlayMoneySound()
    {
        PlaySFX("money");
    }
    
    public void PlayErrorSound()
    {
        PlaySFX("error");
    }
    
    public void PlayVictorySound()
    {
        PlaySFX("victory");
    }
    
    public void PlayGameOverSound()
    {
        PlaySFX("gameOver");
    }
    
    // UI SFX Methods
    public void PlayButtonClick()
    {
        PlayUI("buttonClick");
    }
    
    public void PlayButtonHover()
    {
        PlayUI("buttonHover");
    }
    
    public void PlayNotification()
    {
        PlayUI("notification");
    }
    
    public void PlayCountdown()
    {
        PlayUI("countdown");
    }
    
    // Mini-Game SFX Methods
    public void PlayMiniGameStart()
    {
        PlaySFX("miniGameStart");
    }
    
    public void PlayCollectSound()
    {
        PlaySFX("collect");
    }
    
    public void PlayJumpSound()
    {
        PlaySFX("jump");
    }
    
    public void PlayMatchSound()
    {
        PlaySFX("match");
    }
    
    public void PlayRaceStartSound()
    {
        PlaySFX("raceStart");
    }
    
    // Generic play methods
    void PlaySFX(string clipName)
    {
        if (audioClips.ContainsKey(clipName) && sfxSource != null)
        {
            sfxSource.PlayOneShot(audioClips[clipName]);
        }
    }
    
    void PlayUI(string clipName)
    {
        if (audioClips.ContainsKey(clipName) && uiSource != null)
        {
            uiSource.PlayOneShot(audioClips[clipName]);
        }
    }
    
    public void PlayStateChangeSound(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.PlayerTurn:
                PlayNotification();
                break;
            case GameManager.GameState.MiniGame:
                PlayMiniGameStart();
                break;
            case GameManager.GameState.GameOver:
                PlayGameOverSound();
                break;
        }
    }
    
    // Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
    }
    
    public void ToggleMute()
    {
        isMuted = !isMuted;
    }
    
    public bool IsMuted()
    {
        return isMuted;
    }
    
    // Utility Methods
    public void PlayRandomSFX(string[] clipNames)
    {
        if (clipNames.Length > 0)
        {
            string randomClip = clipNames[Random.Range(0, clipNames.Length)];
            PlaySFX(randomClip);
        }
    }
    
    public void PlaySFXWithPitch(string clipName, float pitch)
    {
        if (audioClips.ContainsKey(clipName) && sfxSource != null)
        {
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(audioClips[clipName]);
            sfxSource.pitch = 1f; // Reset pitch
        }
    }
    
    public void PlaySFXWithDelay(string clipName, float delay)
    {
        StartCoroutine(PlaySFXDelayed(clipName, delay));
    }
    
    IEnumerator PlaySFXDelayed(string clipName, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(clipName);
    }
    
    // Save/Load Settings
    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 0.6f);
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAudioSettings();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAudioSettings();
        }
    }
    
    // Additional audio methods for GameManager compatibility
    public void PlayStateChangeSound(GameManager.GameState newState)
    {
        // Play appropriate sound based on state
        switch (newState)
        {
            case GameManager.GameState.PlayerTurn:
                PlaySFX("buttonClickSound");
                break;
            case GameManager.GameState.MiniGame:
                PlaySFX("miniGameStartSound");
                break;
            case GameManager.GameState.GameOver:
                PlaySFX("gameOverSound");
                break;
        }
    }
    
    public void PlayDiceRoll()
    {
        PlaySFX("diceRollSound");
    }
    
    public void PlayGameOverSound()
    {
        PlayGameOverMusic();
        PlaySFX("gameOverSound");
    }
    
    // Additional methods needed by GameManager
    public void PlayMoveSound()
    {
        PlaySFX("moveSound");
    }
    
    public void PlayMoneySound()
    {
        PlaySFX("moneySound");
    }
    
    public void PlayPurchaseSound()
    {
        PlaySFX("purchaseSound");
    }
    
    public void PlayErrorSound()
    {
        PlaySFX("errorSound");
    }
    
    public void PlaySellSound()
    {
        PlaySFX("sellSound");
    }
    
    public void PlayRentSound()
    {
        PlaySFX("rentSound");
    }
    
    public void PlayVictorySound()
    {
        PlaySFX("victorySound");
    }
    
    void OnDestroy()
    {
        SaveAudioSettings();
    }
}