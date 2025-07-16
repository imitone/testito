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
    
    [Header("Player Properties")]
    public List<BoardSpace> ownedProperties = new List<BoardSpace>();
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Components")]
    public MeshRenderer playerRenderer;
    public TextMesh playerNameText;
    public TextMesh moneyText;
    public GameObject playerModel;
    
    private BoardManager boardManager;
    private Vector3 targetPosition;
    private bool isMoving = false;
    
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        if (playerRenderer == null)
            playerRenderer = GetComponent<MeshRenderer>();
        
        // Position player at start
        if (boardManager != null)
        {
            transform.position = boardManager.GetSpacePosition(0);
        }
        
        UpdateVisuals();
    }
    
    public void Initialize(int id, int startMoney, Color color)
    {
        playerId = id;
        playerName = $"Player {id + 1}";
        money = startMoney;
        playerColor = color;
        currentSpaceIndex = 0;
        
        // Apply color to player model
        if (playerRenderer != null)
        {
            playerRenderer.material.color = playerColor;
        }
        
        UpdateVisuals();
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
        }
    }
    
    public void MoveToNextSpace()
    {
        if (isMoving || boardManager == null) return;
        
        currentSpaceIndex = boardManager.GetNextSpaceIndex(currentSpaceIndex);
        Vector3 newPosition = boardManager.GetSpacePosition(currentSpaceIndex);
        
        StartCoroutine(MoveToPosition(newPosition));
    }
    
    public void MoveToSpace(int spaceIndex)
    {
        if (isMoving || boardManager == null) return;
        
        currentSpaceIndex = spaceIndex;
        Vector3 newPosition = boardManager.GetSpacePosition(spaceIndex);
        
        StartCoroutine(MoveToPosition(newPosition));
    }
    
    IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float journeyTime = 0;
        float journeyLength = Vector3.Distance(startPos, targetPos);
        
        while (journeyTime <= 1f)
        {
            journeyTime += Time.deltaTime * moveSpeed / journeyLength;
            float curveValue = moveCurve.Evaluate(journeyTime);
            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            
            // Add a small bounce effect
            Vector3 pos = transform.position;
            pos.y = Mathf.Sin(journeyTime * Mathf.PI) * 0.5f + 0.5f;
            transform.position = pos;
            
            yield return null;
        }
        
        transform.position = targetPos;
        isMoving = false;
    }
    
    public void AddMoney(int amount)
    {
        money += amount;
        UpdateVisuals();
    }
    
    public void SpendMoney(int amount)
    {
        money = Mathf.Max(0, money - amount);
        UpdateVisuals();
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
        }
    }
    
    public void RemoveProperty(BoardSpace property)
    {
        if (ownedProperties.Contains(property))
        {
            ownedProperties.Remove(property);
            property.RemoveOwner();
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
    
    public void RotateToFaceDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    void Update()
    {
        UpdateVisuals();
    }
}