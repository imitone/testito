using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_MiniGameAnnouncement : MonoBehaviour
{
    [Header("Announcement Elements")]
    public Text gameNameText;
    public Text gameDescriptionText;
    public Image gameIcon;
    public GameObject countdownPanel;
    public Text countdownText;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    void Start()
    {
        // Initialize as hidden
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
    
    public void ShowAnnouncement(string gameName, string description)
    {
        gameObject.SetActive(true);
        
        // Set announcement text
        if (gameNameText != null)
            gameNameText.text = gameName;
        
        if (gameDescriptionText != null)
            gameDescriptionText.text = description;
        
        // Start announcement animation
        StartCoroutine(ShowAnnouncementAnimation());
    }
    
    IEnumerator ShowAnnouncementAnimation()
    {
        // Fade in and scale up
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            canvasGroup.alpha = t;
            float scaleValue = scaleCurve.Evaluate(t);
            transform.localScale = Vector3.one * scaleValue;
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
        
        // Show countdown
        if (countdownPanel != null)
        {
            yield return StartCoroutine(ShowCountdown());
        }
        else
        {
            // Wait for display time
            yield return new WaitForSeconds(2f);
        }
        
        // Hide announcement
        yield return StartCoroutine(HideAnnouncementAnimation());
    }
    
    IEnumerator ShowCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
            
            for (int i = 3; i > 0; i--)
            {
                if (countdownText != null)
                {
                    countdownText.text = i.ToString();
                    
                    // Animate countdown number
                    StartCoroutine(AnimateCountdownNumber());
                }
                
                yield return new WaitForSeconds(1f);
            }
            
            if (countdownText != null)
            {
                countdownText.text = "GO!";
                StartCoroutine(AnimateCountdownNumber());
            }
            
            yield return new WaitForSeconds(0.5f);
            
            countdownPanel.SetActive(false);
        }
    }
    
    IEnumerator AnimateCountdownNumber()
    {
        if (countdownText != null)
        {
            Vector3 originalScale = countdownText.transform.localScale;
            
            // Scale up
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.2f;
                countdownText.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.5f, t);
                yield return null;
            }
            
            // Scale back down
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                countdownText.transform.localScale = Vector3.Lerp(originalScale * 1.5f, originalScale, t);
                yield return null;
            }
            
            countdownText.transform.localScale = originalScale;
        }
    }
    
    IEnumerator HideAnnouncementAnimation()
    {
        // Fade out and scale down
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            canvasGroup.alpha = 1f - t;
            float scaleValue = scaleCurve.Evaluate(1f - t);
            transform.localScale = Vector3.one * scaleValue;
            
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
    
    public void SetGameIcon(Sprite icon)
    {
        if (gameIcon != null)
        {
            gameIcon.sprite = icon;
        }
    }
    
    public void HideImmediate()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}