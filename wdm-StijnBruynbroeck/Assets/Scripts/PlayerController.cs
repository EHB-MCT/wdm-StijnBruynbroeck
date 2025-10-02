using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public HexGrid grid;

    public int gridX = 0;
    public int gridY = 0;

    private Vector3 targetPos;

    void Start()
    {
        if (grid == null)
            grid = FindObjectOfType<HexGrid>();

       
        targetPos = grid.GetTile(gridX, gridY).transform.position + Vector3.up * 0.5f;
        transform.position = targetPos;

        RevealTiles();
    }

    void Update()
    {
        
        if (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            return;
        }

        // klikken om te bewegen
        if (Input.GetMouseButtonDown(0)) // linkermuisknop
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        // ray vanuit muispositie naar wereld
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // 2D wereld

        
        float bestDist = Mathf.Infinity;
        int bestX = -1, bestY = -1;

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                GameObject tile = grid.GetTile(x, y);
                if (tile == null) continue;

                float dist = Vector2.Distance(mouseWorldPos, tile.transform.position);
                if (dist < bestDist && dist < 0.5f) 
                {
                    bestDist = dist;
                    bestX = x;
                    bestY = y;
                }
            }
        }

        if (bestX != -1 && bestY != -1)
        {
            MoveTo(bestX, bestY);
        }
    }

    void MoveTo(int x, int y)
    {
        gridX = x;
        gridY = y;
        targetPos = grid.GetTile(gridX, gridY).transform.position + Vector3.up * 0.5f;
        Debug.Log($"Player moved to tile: ({gridX},{gridY})");

        RevealTiles();
    }

    void RevealTiles()
    {
        //reveal tiles
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                GameObject tile = grid.GetTile(gridX + dx, gridY + dy);
                if (tile != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.color = Color.white; 
                }
            }
        }
    }
}
