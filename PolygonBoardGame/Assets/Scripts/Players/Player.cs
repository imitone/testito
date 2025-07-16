using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Info")]
    public int playerId;
    public string playerName;
    public Color playerColor;
    public int money;
    public int currentSpaceIndex;
    public PlayerType playerType = PlayerType.Human;
    
    [Header("Player Properties")]
    public List<BoardSpace> ownedProperties = new List<BoardSpace>();
    public List<PowerUp> activePowerUps = new List<PowerUp>();
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float bounceHeight = 0.5f;
    public float rotationSpeed = 10f;
    
    [Header("Visual Components")]
    public MeshRenderer playerRenderer;
    public TextMesh playerNameText;
    public TextMesh moneyText;
    public GameObject playerModel;
    public GameObject selectionIndicator;
    public ParticleSystem moneyParticles;
    public ParticleSystem moveParticles;
    
    [Header("Audio")]
    public AudioClip moveSound;
    public AudioClip moneySound;
    public AudioClip purchaseSound;
    
    [Header("AI Settings")]
    public float aiDecisionTime = 2f;
    public float aiAggressiveness = 0.5f;
    public float aiRiskTolerance = 0.6f;
    
    private BoardManager boardManager;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isSelected = false;
    private Vector3 originalScale;
    private Coroutine moveCoroutine;
    
    // Statistics
    private int propertiesBought = 0;
    private int propertiesSold = 0;
    private int miniGamesWon = 0;
    private int miniGamesPlayed = 0;
    private int moneyEarned = 0;
    private int moneySpent = 0;
    
    public enum PlayerType
    {
        Human,
        AI_Easy,
        AI_Medium,
        AI_Hard
    }
    
    public enum PlayerState
    {
        Idle,
        Moving,
        DecisionMaking,
        InMiniGame,
        Bankrupt
    }
    
    public PlayerState currentState = PlayerState.Idle;
    
    // Events
    public System.Action<Player> OnPlayerMoneyChanged;
    public System.Action<Player, BoardSpace> OnPropertyPurchased;
    public System.Action<Player, BoardSpace> OnPropertySold;
    public System.Action<Player> OnPlayerBankrupt;
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        originalScale = transform.localScale;
        
        if (playerRenderer == null)
            playerRenderer = GetComponent<MeshRenderer>();
        
        // Position player at start
        if (boardManager != null)
        {
            transform.position = boardManager.GetSpacePosition(0);
        }
        
        SetupVisualComponents();
        UpdateVisuals();
    }
    
    void SetupVisualComponents()
    {
        // Create selection indicator if it doesn't exist
        if (selectionIndicator == null)
        {
            selectionIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionIndicator.transform.SetParent(transform);
            selectionIndicator.transform.localPosition = new Vector3(0, -0.5f, 0);
            selectionIndicator.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
            selectionIndicator.GetComponent<MeshRenderer>().material.color = playerColor;
            selectionIndicator.SetActive(false);
        }
        
        // Setup particles
        if (moneyParticles == null)
        {
            GameObject particleObj = new GameObject("MoneyParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.up;
            moneyParticles = particleObj.AddComponent<ParticleSystem>();
            SetupMoneyParticles();
        }
        
        if (moveParticles == null)
        {
            GameObject particleObj = new GameObject("MoveParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;
            moveParticles = particleObj.AddComponent<ParticleSystem>();
            SetupMoveParticles();
        }
    }
    
    void SetupMoneyParticles()
    {
        var main = moneyParticles.main;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        main.startColor = Color.yellow;
        main.startSize = 0.1f;
        main.maxParticles = 50;
        
        var emission = moneyParticles.emission;
        emission.enabled = false;
        
        var shape = moneyParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
    }
    
    void SetupMoveParticles()
    {
        var main = moveParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startColor = playerColor;
        main.startSize = 0.05f;
        main.maxParticles = 20;
        
        var emission = moveParticles.emission;
        emission.enabled = false;
        
        var shape = moveParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
    }
    
    public void Initialize(int id, int startMoney, Color color, string name)
    {
        playerId = id;
        playerName = name;
        money = startMoney;
        playerColor = color;
        currentSpaceIndex = 0;
        
        // Set player type based on ID (first player is human, others are AI)
        if (id == 0)
        {
            playerType = PlayerType.Human;
        }
        else
        {
            playerType = (PlayerType)(Random.Range(1, 4)); // Random AI difficulty
        }
        
        // Apply color to player model
        if (playerRenderer != null)
        {
            Material playerMaterial = new Material(playerRenderer.material);
            playerMaterial.color = playerColor;
            playerRenderer.material = playerMaterial;
        }
        
        UpdateVisuals();
        
        Debug.Log($"Player {playerName} initialized with ${money} - Type: {playerType}");
    }
    
    void UpdateVisuals()
    {
        if (playerNameText != null)
        {
            playerNameText.text = playerName;
            playerNameText.color = playerColor;
        }
        
        if (moneyText != null)
        {
            moneyText.text = $"${money}";
            
            // Change color based on money amount
            if (money > 2000)
                moneyText.color = Color.green;
            else if (money > 500)
                moneyText.color = Color.white;
            else
                moneyText.color = Color.red;
        }
        
        // Update selection indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
        
        if (selected)
        {
            StartCoroutine(SelectionPulse());
        }
    }
    
    IEnumerator SelectionPulse()
    {
        float time = 0f;
        while (isSelected)
        {
            time += Time.deltaTime;
            float scale = 1f + Mathf.Sin(time * 3f) * 0.1f;
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    public void MoveToNextSpace()
    {
        if (isMoving || boardManager == null) return;
        
        int nextIndex = boardManager.GetNextSpaceIndex(currentSpaceIndex);
        MoveToSpace(nextIndex);
    }
    
    public void MoveToSpace(int spaceIndex)
    {
        if (isMoving || boardManager == null) return;
        
        currentSpaceIndex = spaceIndex;
        Vector3 newPosition = boardManager.GetSpacePosition(spaceIndex);
        
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        
        moveCoroutine = StartCoroutine(MoveToPosition(newPosition));
    }
    
    IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;
        currentState = PlayerState.Moving;
        
        Vector3 startPos = transform.position;
        float journeyTime = 0;
        float journeyLength = Vector3.Distance(startPos, targetPos);
        
        // Play move particles
        if (moveParticles != null)
        {
            var emission = moveParticles.emission;
            emission.enabled = true;
        }
        
        // Face movement direction
        Vector3 direction = (targetPos - startPos).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
        
        while (journeyTime <= 1f)
        {
            journeyTime += Time.deltaTime * moveSpeed / journeyLength;
            float curveValue = moveCurve.Evaluate(journeyTime);
            
            // Move with bounce
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, curveValue);
            currentPos.y += Mathf.Sin(journeyTime * Mathf.PI) * bounceHeight;
            transform.position = currentPos;
            
            yield return null;
        }
        
        // Stop particles
        if (moveParticles != null)
        {
            var emission = moveParticles.emission;
            emission.enabled = false;
        }
        
        transform.position = targetPos;
        isMoving = false;
        currentState = PlayerState.Idle;
        
        // Play audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMoveSound();
        }
        
        moveCoroutine = null;
    }
    
    public void AddMoney(int amount)
    {
        money += amount;
        moneyEarned += amount;
        
        // Play money particles
        if (moneyParticles != null && amount > 0)
        {
            var emission = moneyParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, (short)Mathf.Min(amount / 10, 50))
            });
            moneyParticles.Play();
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMoneySound();
        }
        
        UpdateVisuals();
        OnPlayerMoneyChanged?.Invoke(this);
    }
    
    public void SpendMoney(int amount)
    {
        money = Mathf.Max(0, money - amount);
        moneySpent += amount;
        
        UpdateVisuals();
        OnPlayerMoneyChanged?.Invoke(this);
        
        // Check for bankruptcy
        if (money <= 0 && ownedProperties.Count == 0)
        {
            SetBankrupt();
        }
    }
    
    public bool CanAfford(int amount)
    {
        return money >= amount;
    }
    
    public void AddProperty(BoardSpace property)
    {
        if (!ownedProperties.Contains(property))
        {
            ownedProperties.Add(property);
            property.SetOwner(this);
            propertiesBought++;
            
            OnPropertyPurchased?.Invoke(this, property);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPurchaseSound();
            }
        }
    }
    
    public void RemoveProperty(BoardSpace property)
    {
        if (ownedProperties.Contains(property))
        {
            ownedProperties.Remove(property);
            property.RemoveOwner();
            propertiesSold++;
            
            OnPropertySold?.Invoke(this, property);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySellSound();
            }
        }
    }
    
    public int GetNetWorth()
    {
        int propertyValue = 0;
        foreach (BoardSpace property in ownedProperties)
        {
            propertyValue += property.price;
        }
        return money + propertyValue;
    }
    
    public int GetPropertyValue()
    {
        int value = 0;
        foreach (BoardSpace property in ownedProperties)
        {
            value += property.price;
        }
        return value;
    }
    
    public void SetBankrupt()
    {
        currentState = PlayerState.Bankrupt;
        
        // Transfer all properties back to bank
        foreach (var property in ownedProperties.ToArray())
        {
            RemoveProperty(property);
        }
        
        OnPlayerBankrupt?.Invoke(this);
        
        // Visual feedback
        StartCoroutine(BankruptcyAnimation());
    }
    
    IEnumerator BankruptcyAnimation()
    {
        // Flash red
        Material originalMaterial = playerRenderer.material;
        Material flashMaterial = new Material(originalMaterial);
        flashMaterial.color = Color.red;
        
        for (int i = 0; i < 5; i++)
        {
            playerRenderer.material = flashMaterial;
            yield return new WaitForSeconds(0.2f);
            playerRenderer.material = originalMaterial;
            yield return new WaitForSeconds(0.2f);
        }
        
        // Fade out
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 2f;
            Color color = originalMaterial.color;
            color.a = alpha;
            originalMaterial.color = color;
            yield return null;
        }
        
        gameObject.SetActive(false);
    }
    
    // AI Decision Making
    public void MakeAIDecision(BoardSpace currentSpace)
    {
        if (playerType == PlayerType.Human) return;
        
        StartCoroutine(AIDecisionProcess(currentSpace));
    }
    
    IEnumerator AIDecisionProcess(BoardSpace space)
    {
        currentState = PlayerState.DecisionMaking;
        
        yield return new WaitForSeconds(aiDecisionTime);
        
        string decision = GetAIDecision(space);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPropertyDecision(decision);
        }
        
        currentState = PlayerState.Idle;
    }
    
    string GetAIDecision(BoardSpace space)
    {
        if (space.owner == null && CanAfford(space.price))
        {
            // Decide whether to buy based on AI personality
            float buyChance = GetBuyChance(space);
            
            if (Random.Range(0f, 1f) < buyChance)
            {
                return "buy";
            }
        }
        
        // Consider selling properties if low on money
        if (money < 200 && ownedProperties.Count > 0)
        {
            float sellChance = GetSellChance();
            
            if (Random.Range(0f, 1f) < sellChance)
            {
                return "sell";
            }
        }
        
        // Consider mini-game challenge
        float challengeChance = GetChallengeChance();
        
        if (Random.Range(0f, 1f) < challengeChance)
        {
            return "challenge";
        }
        
        return "skip";
    }
    
    float GetBuyChance(BoardSpace space)
    {
        float chance = 0.5f;
        
        // Adjust based on AI difficulty
        switch (playerType)
        {
            case PlayerType.AI_Easy:
                chance = 0.3f;
                break;
            case PlayerType.AI_Medium:
                chance = 0.5f;
                break;
            case PlayerType.AI_Hard:
                chance = 0.7f;
                break;
        }
        
        // Adjust based on money
        if (money > space.price * 3)
        {
            chance += 0.2f;
        }
        else if (money < space.price * 1.5f)
        {
            chance -= 0.3f;
        }
        
        // Adjust based on property value
        if (space.price < 200)
        {
            chance += 0.1f;
        }
        
        return Mathf.Clamp01(chance);
    }
    
    float GetSellChance()
    {
        float chance = 0.3f;
        
        // More likely to sell if desperate
        if (money < 100)
        {
            chance = 0.8f;
        }
        else if (money < 200)
        {
            chance = 0.5f;
        }
        
        return Mathf.Clamp01(chance);
    }
    
    float GetChallengeChance()
    {
        float chance = aiAggressiveness * 0.4f;
        
        // Adjust based on money situation
        if (money < 500)
        {
            chance += 0.2f; // More desperate, more likely to challenge
        }
        
        return Mathf.Clamp01(chance);
    }
    
    // Power-ups
    public void AddPowerUp(PowerUp powerUp)
    {
        activePowerUps.Add(powerUp);
        powerUp.Activate(this);
    }
    
    public void RemovePowerUp(PowerUp powerUp)
    {
        if (activePowerUps.Contains(powerUp))
        {
            activePowerUps.Remove(powerUp);
            powerUp.Deactivate(this);
        }
    }
    
    // Statistics
    public void RecordMiniGameResult(bool won)
    {
        miniGamesPlayed++;
        if (won)
        {
            miniGamesWon++;
        }
    }
    
    public float GetMiniGameWinRate()
    {
        return miniGamesPlayed > 0 ? (float)miniGamesWon / miniGamesPlayed : 0f;
    }
    
    public PlayerStatistics GetStatistics()
    {
        return new PlayerStatistics
        {
            playerName = this.playerName,
            propertiesBought = this.propertiesBought,
            propertiesSold = this.propertiesSold,
            miniGamesWon = this.miniGamesWon,
            miniGamesPlayed = this.miniGamesPlayed,
            moneyEarned = this.moneyEarned,
            moneySpent = this.moneySpent,
            netWorth = GetNetWorth(),
            propertyValue = GetPropertyValue()
        };
    }
    
    void Update()
    {
        UpdateVisuals();
        
        // Update power-ups
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            if (activePowerUps[i].IsExpired())
            {
                RemovePowerUp(activePowerUps[i]);
            }
        }
    }
    
    void OnDestroy()
    {
        OnPlayerMoneyChanged = null;
        OnPropertyPurchased = null;
        OnPropertySold = null;
        OnPlayerBankrupt = null;
    }
}

[System.Serializable]
public class PlayerStatistics
{
    public string playerName;
    public int propertiesBought;
    public int propertiesSold;
    public int miniGamesWon;
    public int miniGamesPlayed;
    public int moneyEarned;
    public int moneySpent;
    public int netWorth;
    public int propertyValue;
}

[System.Serializable]
public class PowerUp
{
    public string name;
    public float duration;
    public float timeRemaining;
    
    public virtual void Activate(Player player) 
    {
        timeRemaining = duration;
    }
    
    public virtual void Deactivate(Player player) { }
    
    public bool IsExpired()
    {
        return timeRemaining <= 0;
    }
    
    public void UpdateTimer()
    {
        timeRemaining -= Time.deltaTime;
    }
}