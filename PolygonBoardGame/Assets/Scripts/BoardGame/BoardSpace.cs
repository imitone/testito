using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    [Header("Space Properties")]
    public SpaceType spaceType;
    public string propertyName;
    public int price;
    public int rent;
    public Player owner;
    public int spaceIndex;
    
    [Header("Visual Components")]
    public MeshRenderer spaceRenderer;
    public TextMesh nameText;
    public TextMesh priceText;
    public GameObject ownerIndicator;
    
    [Header("Materials")]
    public Material defaultMaterial;
    public Material ownedMaterial;
    public Material specialMaterial;
    
    public enum SpaceType
    {
        Start,
        Property,
        Corner,
        Special,
        Chance
    }
    
    void Start()
    {
        if (spaceRenderer == null)
            spaceRenderer = GetComponent<MeshRenderer>();
        
        UpdateVisuals();
    }
    
    public void ConfigureAsStart()
    {
        spaceType = SpaceType.Start;
        propertyName = "START";
        price = 0;
        rent = 0;
        UpdateVisuals();
    }
    
    public void ConfigureAsProperty(string name, int propertyPrice)
    {
        spaceType = SpaceType.Property;
        propertyName = name;
        price = propertyPrice;
        rent = propertyPrice / 10;
        UpdateVisuals();
    }
    
    public void ConfigureAsCorner()
    {
        spaceType = SpaceType.Corner;
        propertyName = "CORNER";
        price = 0;
        rent = 0;
        UpdateVisuals();
    }
    
    public void ConfigureAsSpecial()
    {
        spaceType = SpaceType.Special;
        propertyName = "SPECIAL";
        price = 0;
        rent = 0;
        UpdateVisuals();
    }
    
    void UpdateVisuals()
    {
        // Update material based on ownership
        if (owner != null)
        {
            spaceRenderer.material = ownedMaterial;
            ownerIndicator?.SetActive(true);
            if (ownerIndicator != null)
                ownerIndicator.GetComponent<MeshRenderer>().material.color = owner.playerColor;
        }
        else if (spaceType == SpaceType.Special || spaceType == SpaceType.Corner)
        {
            spaceRenderer.material = specialMaterial;
            ownerIndicator?.SetActive(false);
        }
        else
        {
            spaceRenderer.material = defaultMaterial;
            ownerIndicator?.SetActive(false);
        }
        
        // Update text displays
        if (nameText != null)
        {
            nameText.text = propertyName;
        }
        
        if (priceText != null && spaceType == SpaceType.Property)
        {
            priceText.text = $"${price}";
        }
        else if (priceText != null)
        {
            priceText.text = "";
        }
    }
    
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        UpdateVisuals();
    }
    
    public void RemoveOwner()
    {
        owner = null;
        UpdateVisuals();
    }
    
    public bool CanBeBought()
    {
        return spaceType == SpaceType.Property && owner == null;
    }
    
    public bool CanBeSold(Player player)
    {
        return spaceType == SpaceType.Property && owner == player;
    }
    
    void OnMouseDown()
    {
        if (GameManager.Instance.currentState == GameManager.GameState.PropertyDecision)
        {
            GameManager.Instance.OnPropertyDecision("buy");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            OnPlayerLanded(player);
        }
    }
    
    void OnPlayerLanded(Player player)
    {
        switch (spaceType)
        {
            case SpaceType.Start:
                player.money += GameManager.Instance.passStartBonus;
                break;
            case SpaceType.Property:
                if (owner != null && owner != player)
                {
                    // Pay rent
                    int rentAmount = Mathf.Min(rent, player.money);
                    player.money -= rentAmount;
                    owner.money += rentAmount;
                }
                break;
            case SpaceType.Special:
                // Trigger special event
                break;
        }
    }
}