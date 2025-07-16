using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MemoryGame : MiniGameBase
{
    [Header("Memory Game Settings")]
    public int gridSize = 4;
    public float cardSpacing = 1.5f;
    public float showTime = 1f;
    
    [Header("Memory Objects")]
    public List<GameObject> cards = new List<GameObject>();
    public List<int> cardValues = new List<int>();
    public List<bool> cardRevealed = new List<bool>();
    
    private List<GameObject> flippedCards = new List<GameObject>();
    private List<int> flippedIndices = new List<int>();
    private bool canFlip = true;
    private int currentPlayer = 0;
    
    protected override void SetupGame()
    {
        gameName = "Memory Match";
        CreateMemoryGrid();
    }
    
    void CreateMemoryGrid()
    {
        cards.Clear();
        cardValues.Clear();
        cardRevealed.Clear();
        
        int totalCards = gridSize * gridSize;
        
        // Create pairs of values
        List<int> values = new List<int>();
        for (int i = 0; i < totalCards / 2; i++)
        {
            values.Add(i);
            values.Add(i);
        }
        
        // Shuffle the values
        for (int i = values.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = values[i];
            values[i] = values[randomIndex];
            values[randomIndex] = temp;
        }
        
        // Create card objects
        for (int i = 0; i < totalCards; i++)
        {
            int row = i / gridSize;
            int col = i % gridSize;
            
            Vector3 position = new Vector3(
                (col - (gridSize - 1) / 2f) * cardSpacing,
                0.5f,
                (row - (gridSize - 1) / 2f) * cardSpacing
            );
            
            GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube);
            card.transform.position = position;
            card.name = $"Card_{i}";
            
            // Add click detection
            MemoryCard memoryCard = card.AddComponent<MemoryCard>();
            memoryCard.Initialize(i, values[i], this);
            
            cards.Add(card);
            cardValues.Add(values[i]);
            cardRevealed.Add(false);
        }
    }
    
    protected override void OnGameStart()
    {
        StartCoroutine(ShowAllCards());
    }
    
    IEnumerator ShowAllCards()
    {
        // Show all cards briefly
        canFlip = false;
        
        for (int i = 0; i < cards.Count; i++)
        {
            RevealCard(i, true);
        }
        
        yield return new WaitForSeconds(showTime);
        
        // Hide all cards
        for (int i = 0; i < cards.Count; i++)
        {
            RevealCard(i, false);
        }
        
        canFlip = true;
    }
    
    void RevealCard(int index, bool reveal)
    {
        if (index < 0 || index >= cards.Count) return;
        
        GameObject card = cards[index];
        MeshRenderer renderer = card.GetComponent<MeshRenderer>();
        
        if (reveal)
        {
            // Show card value with color
            Color cardColor = GetCardColor(cardValues[index]);
            renderer.material.color = cardColor;
        }
        else
        {
            // Hide card (gray)
            renderer.material.color = Color.gray;
        }
        
        cardRevealed[index] = reveal;
    }
    
    Color GetCardColor(int value)
    {
        Color[] colors = {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.magenta, Color.cyan, Color.white, Color.black
        };
        
        return colors[value % colors.Length];
    }
    
    public void OnCardClicked(int cardIndex)
    {
        if (!canFlip || !isPlaying) return;
        if (cardRevealed[cardIndex]) return;
        if (flippedCards.Count >= 2) return;
        
        // Flip the card
        RevealCard(cardIndex, true);
        flippedCards.Add(cards[cardIndex]);
        flippedIndices.Add(cardIndex);
        
        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckMatch());
        }
    }
    
    IEnumerator CheckMatch()
    {
        canFlip = false;
        yield return new WaitForSeconds(1f);
        
        if (cardValues[flippedIndices[0]] == cardValues[flippedIndices[1]])
        {
            // Match found
            AddScore(currentPlayer, 10);
            Debug.Log($"{participants[currentPlayer].playerName} found a match!");
        }
        else
        {
            // No match, flip back
            RevealCard(flippedIndices[0], false);
            RevealCard(flippedIndices[1], false);
            
            // Next player's turn
            currentPlayer = (currentPlayer + 1) % participants.Count;
        }
        
        flippedCards.Clear();
        flippedIndices.Clear();
        canFlip = true;
    }
    
    protected override void HandleGameLogic()
    {
        // Handle AI players
        if (currentPlayer > 0 && canFlip && flippedCards.Count == 0)
        {
            StartCoroutine(AITurn());
        }
    }
    
    IEnumerator AITurn()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        
        // AI randomly selects cards
        List<int> availableCards = new List<int>();
        for (int i = 0; i < cards.Count; i++)
        {
            if (!cardRevealed[i])
            {
                availableCards.Add(i);
            }
        }
        
        if (availableCards.Count >= 2)
        {
            int card1 = availableCards[Random.Range(0, availableCards.Count)];
            OnCardClicked(card1);
            
            yield return new WaitForSeconds(0.5f);
            
            availableCards.Remove(card1);
            int card2 = availableCards[Random.Range(0, availableCards.Count)];
            OnCardClicked(card2);
        }
    }
    
    protected override void CheckWinConditions()
    {
        // Check if all cards are matched
        bool allMatched = true;
        for (int i = 0; i < cardRevealed.Count; i++)
        {
            if (!cardRevealed[i])
            {
                allMatched = false;
                break;
            }
        }
        
        if (allMatched)
        {
            // Find winner based on score
            int highestScore = -1;
            int winner = -1;
            
            for (int i = 0; i < scores.Count; i++)
            {
                if (scores[i] > highestScore)
                {
                    highestScore = scores[i];
                    winner = i;
                }
            }
            
            EndGame(winner);
        }
    }
    
    protected override void OnGameEnd()
    {
        if (winnerIndex >= 0)
        {
            Debug.Log($"{participants[winnerIndex].playerName} won the memory game!");
        }
        
        CleanupMemoryGame();
    }
    
    void CleanupMemoryGame()
    {
        foreach (GameObject card in cards)
        {
            if (card != null) Destroy(card);
        }
        cards.Clear();
    }
    
    void OnDestroy()
    {
        CleanupMemoryGame();
    }
}

// Helper class for card interaction
public class MemoryCard : MonoBehaviour
{
    private int cardIndex;
    private int cardValue;
    private MemoryGame memoryGame;
    
    public void Initialize(int index, int value, MemoryGame game)
    {
        cardIndex = index;
        cardValue = value;
        memoryGame = game;
    }
    
    void OnMouseDown()
    {
        memoryGame?.OnCardClicked(cardIndex);
    }
}