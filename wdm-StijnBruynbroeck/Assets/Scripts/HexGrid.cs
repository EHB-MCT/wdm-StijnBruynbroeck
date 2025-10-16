using UnityEngine;

public class HexGrid : MonoBehaviour
{
    
    public GameObject hexPrefab;   
    public int width = 10;
    public int height = 10;
    public float hexSize = 1f;

    [HideInInspector] 
    public GameObject[,] grid; 

    void Awake()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new GameObject[width, height];

        float xOffset = hexSize * 1.1f;
        float yOffset = hexSize * 0.78f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xPos = x * xOffset;
                if (y % 2 == 1) xPos += xOffset / 2; 

                Vector3 pos = new Vector3(xPos, y * yOffset, 0); 
                GameObject hexGO = Instantiate(hexPrefab, pos, Quaternion.identity, this.transform);
                hexGO.name = $"Hex_{x}_{y}";

                // fog of war
                SpriteRenderer sr = hexGO.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = Color.black;

                grid[x, y] = hexGO;
            }
        }
    }

    public GameObject GetTile(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return grid[x, y];
    }

    public void RevealTile(int x, int y)
    {
        GameObject tile = GetTile(x, y);
        if (tile != null)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;

        }

    }
     public void HighlightReachable(int centerX, int centerY)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                GameObject tile = GetTile(centerX + dx, centerY + dy);
                if (tile != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.color != Color.white)
                        sr.color = Color.gray;
                }
            }
        }
    }
}
