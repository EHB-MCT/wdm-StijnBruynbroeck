using UnityEngine;

/// <summary>
/// Manages the hexagonal grid system for the game world.
/// Handles tile generation, visibility, and highlighting.
/// Source: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
/// </summary>
public class HexGrid : MonoBehaviour
{
    [Header("Grid Configuration")]
    [Tooltip("Prefab used for hexagonal tiles")]
    public GameObject hexPrefab;   

    [Tooltip("Width of the grid in tiles")]
    public int width = 10;

    [Tooltip("Height of the grid in tiles")]
    public int height = 10;

    [Tooltip("Size of each hexagonal tile")]
    public float hexSize = 1f;

    [HideInInspector] 
    public GameObject[,] grid; 

    private void Awake()
    {
        GenerateGrid();
    }

private void GenerateGrid()
    {
        grid = new GameObject[width, height];

        float xOffset = hexSize * 1.1f;
        float yOffset = hexSize * 1f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xPos = x * xOffset;
                if (y % 2 == 1)
                {
                    xPos += xOffset / 2;
                }

                Vector3 pos = new Vector3(xPos, y * yOffset, 0);
                GameObject hexGO = Instantiate(hexPrefab, pos, Quaternion.identity, this.transform);
                hexGO.name = $"Hex_{x}_{y}";

                // fog of war
                SpriteRenderer sr = hexGO.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.black;
                }

                grid[x, y] = hexGO;
            }
        }
    }

    /// <summary>
    /// Gets a tile at the specified grid coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Tile GameObject or null if out of bounds</returns>
    public GameObject GetTile(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return null;
        }

        return grid[x, y];
    }

    /// <summary>
    /// Reveals a tile, removing fog of war.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    public void RevealTile(int x, int y)
    {
        GameObject tile = GetTile(x, y);
        if (tile != null)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Highlights reachable tiles from a center position.
    /// </summary>
    /// <param name="centerX">Center X coordinate</param>
    /// <param name="centerY">Center Y coordinate</param>
    public void HighlightReachable(int centerX, int centerY)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                GameObject tile = GetTile(centerX + dx, centerY + dy);
                if (tile != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.color != Color.white)
                    {
                        sr.color = Color.gray;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Clears all tile highlights.
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var tile in grid)
        {
            if (tile == null)
            {
                continue;
            }

            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null && sr.color == Color.gray)
            {
                sr.color = Color.black;
            }
        }
    }
}
