using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public HexGrid grid;

    public int gridX = 0;
    public int gridY = 0;

    private Vector3 targetPos;
    private bool isMoving = false;

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
        
        if (isMoving)
        {
            isMoving = false;
            RevealTiles();
        }

        // klikken om te bewegen
        if (Input.GetMouseButtonDown(0)) // linkermuisknop
        {
            HandleClick();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameLogger.Instance.PrintSummary();
        }
        
    }

    void HandleClick()
    {

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
            TryMoveTo(bestX, bestY);
        }


    }
    
     void TryMoveTo(int x, int y)
    {
        
        if (Mathf.Abs(x - gridX) > 1 || Mathf.Abs(y - gridY) > 1)
        {
            Debug.Log("Te ver om in één beurt te bewegen!");
            return;
        }

        MoveTo(x, y);
    }


    void MoveTo(int x, int y)
    {
        gridX = x;
        gridY = y;
        targetPos = grid.GetTile(gridX, gridY).transform.position + Vector3.up * 0.5f;
        isMoving = true;    
        
        Debug.Log($"Player moved to tile: ({gridX},{gridY})");

        GameLogger.Instance.RecordMove(gridX, gridY);

        if (Random.value < 0.3f)
        {
            string eventType = Random.value < 0.5f ? "Friendly Tribe" : "Hostile Tribe";
            Debug.Log($"Encounter: {eventType}");
            GameLogger.Instance.RecordEvent(eventType);
            
        }
    }

    void RevealTiles()
    {
        grid.ClearHighlights();
        
                for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                grid.RevealTile(gridX + dx, gridY + dy);
            }
        }
        grid.HighlightReachable(gridX, gridY);
    }
}
