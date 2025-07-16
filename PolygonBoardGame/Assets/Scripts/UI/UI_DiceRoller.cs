using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_DiceRoller : MonoBehaviour
{
    [Header("Dice Elements")]
    public Image diceImage;
    public Text diceValueText;
    public Button rollButton;
    public GameObject dicePanel;
    
    [Header("Dice Sprites")]
    public Sprite[] diceFaces = new Sprite[6];
    
    [Header("Animation Settings")]
    public float rollDuration = 1f;
    public float rollSpeed = 0.1f;
    public AnimationCurve rollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isRolling = false;
    private System.Action<int> onRollComplete;
    
    void Start()
    {
        SetupDiceRoller();
    }
    
    void SetupDiceRoller()
    {
        if (rollButton != null)
        {
            rollButton.onClick.AddListener(RollDice);
        }
        
        if (dicePanel != null)
        {
            dicePanel.SetActive(false);
        }
    }
    
    public void ShowDiceRoller(System.Action<int> rollCallback)
    {
        onRollComplete = rollCallback;
        
        if (dicePanel != null)
        {
            dicePanel.SetActive(true);
        }
        
        if (rollButton != null)
        {
            rollButton.interactable = true;
        }
        
        // Show initial dice face
        if (diceImage != null && diceFaces.Length > 0)
        {
            diceImage.sprite = diceFaces[0];
        }
        
        if (diceValueText != null)
        {
            diceValueText.text = "?";
        }
    }
    
    public void RollDice()
    {
        if (isRolling) return;
        
        if (rollButton != null)
        {
            rollButton.interactable = false;
        }
        
        StartCoroutine(RollDiceAnimation());
    }
    
    IEnumerator RollDiceAnimation()
    {
        isRolling = true;
        
        // Random final result
        int finalResult = Random.Range(1, 7);
        
        // Rolling animation
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            
            // Show random dice faces during roll
            if (elapsed % rollSpeed < Time.deltaTime)
            {
                int randomFace = Random.Range(0, diceFaces.Length);
                if (diceImage != null && diceFaces.Length > 0)
                {
                    diceImage.sprite = diceFaces[randomFace];
                }
                
                if (diceValueText != null)
                {
                    diceValueText.text = (randomFace + 1).ToString();
                }
                
                // Add shake effect
                StartCoroutine(ShakeDice());
            }
            
            yield return null;
        }
        
        // Show final result
        if (diceImage != null && diceFaces.Length >= finalResult)
        {
            diceImage.sprite = diceFaces[finalResult - 1];
        }
        
        if (diceValueText != null)
        {
            diceValueText.text = finalResult.ToString();
        }
        
        // Final result animation
        StartCoroutine(FinalResultAnimation());
        
        // Wait a bit before hiding
        yield return new WaitForSeconds(1f);
        
        isRolling = false;
        HideDiceRoller();
        
        // Callback with result
        onRollComplete?.Invoke(finalResult);
    }
    
    IEnumerator ShakeDice()
    {
        if (diceImage != null)
        {
            Vector3 originalPosition = diceImage.transform.localPosition;
            float shakeIntensity = 2f;
            float shakeDuration = 0.1f;
            
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                
                Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
                randomOffset.z = 0;
                diceImage.transform.localPosition = originalPosition + randomOffset;
                
                yield return null;
            }
            
            diceImage.transform.localPosition = originalPosition;
        }
    }
    
    IEnumerator FinalResultAnimation()
    {
        if (diceImage != null)
        {
            Vector3 originalScale = diceImage.transform.localScale;
            
            // Scale up
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                diceImage.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, t);
                yield return null;
            }
            
            // Scale back down
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.3f;
                diceImage.transform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, t);
                yield return null;
            }
            
            diceImage.transform.localScale = originalScale;
        }
    }
    
    public void HideDiceRoller()
    {
        if (dicePanel != null)
        {
            dicePanel.SetActive(false);
        }
    }
    
    public void SetDiceSprites(Sprite[] sprites)
    {
        if (sprites != null && sprites.Length == 6)
        {
            diceFaces = sprites;
        }
    }
    
    public bool IsRolling()
    {
        return isRolling;
    }
}