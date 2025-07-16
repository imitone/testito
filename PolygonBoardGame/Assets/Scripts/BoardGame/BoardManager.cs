using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int boardSize = 40;
    public float spaceDistance = 2f;
    public GameObject spacePrefab;
    
    [Header("Board Layout")]
    public List<BoardSpace> boardSpaces = new List<BoardSpace>();
    public Transform[] cornerPositions = new Transform[4];
    
    void Start()
    {
        GenerateBoard();
    }
    
    void GenerateBoard()
    {
        // Create a square board layout similar to Monopoly
        Vector3[] corners = {
            new Vector3(-10, 0, -10), // Bottom-left
            new Vector3(10, 0, -10),  // Bottom-right
            new Vector3(10, 0, 10),   // Top-right
            new Vector3(-10, 0, 10)   // Top-left
        };
        
        int spacesPerSide = boardSize / 4;
        
        for (int i = 0; i < boardSize; i++)
        {
            Vector3 position = GetSpacePosition(i, corners, spacesPerSide);
            GameObject spaceObj = Instantiate(spacePrefab, position, Quaternion.identity, transform);
            BoardSpace space = spaceObj.GetComponent<BoardSpace>();
            
            // Configure space based on position
            ConfigureSpace(space, i);
            boardSpaces.Add(space);
        }
    }
    
    Vector3 GetSpacePosition(int index, Vector3[] corners, int spacesPerSide)
    {
        int side = index / spacesPerSide;
        int positionOnSide = index % spacesPerSide;
        float t = (float)positionOnSide / (spacesPerSide - 1);
        
        switch (side)
        {
            case 0: // Bottom side
                return Vector3.Lerp(corners[0], corners[1], t);
            case 1: // Right side
                return Vector3.Lerp(corners[1], corners[2], t);
            case 2: // Top side
                return Vector3.Lerp(corners[2], corners[3], t);
            case 3: // Left side
                return Vector3.Lerp(corners[3], corners[0], t);
            default:
                return Vector3.zero;
        }
    }
    
    void ConfigureSpace(BoardSpace space, int index)
    {
        space.spaceIndex = index;
        
        // Configure different space types
        if (index == 0)
        {
            space.ConfigureAsStart();
        }
        else if (index % 10 == 0)
        {
            space.ConfigureAsCorner();
        }
        else if (index % 7 == 0)
        {
            space.ConfigureAsSpecial();
        }
        else
        {
            space.ConfigureAsProperty(GetRandomCityName(), Random.Range(100, 500));
        }
    }
    
    string GetRandomCityName()
    {
        string[] cityNames = {
            "New Tokyo", "Polygon City", "Crystal Bay", "Neon Heights", "Vertex Village",
            "Prism Point", "Geometric Gardens", "Angular Avenue", "Faceted Falls", "Triangular Town",
            "Cubic Coast", "Hexagonal Hills", "Octagonal Oasis", "Rectangular Ridge", "Spherical Springs"
        };
        
        return cityNames[Random.Range(0, cityNames.Length)];
    }
    
    public BoardSpace GetSpaceAt(int index)
    {
        if (index >= 0 && index < boardSpaces.Count)
        {
            return boardSpaces[index];
        }
        return null;
    }
    
    public Vector3 GetSpacePosition(int index)
    {
        BoardSpace space = GetSpaceAt(index);
        return space != null ? space.transform.position : Vector3.zero;
    }
    
    public int GetNextSpaceIndex(int currentIndex)
    {
        return (currentIndex + 1) % boardSpaces.Count;
    }
}