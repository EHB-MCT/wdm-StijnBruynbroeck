using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float moveSpeed = 5f;
    public HexGrid grid;

    public int gridX = 0;
    public int gridY = 0;

    private Vector3 targetPos;
    private bool isMoving = false;
    private float lastMoveTime;
    private float decisionStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
            
            // Grant experience for movement
            if (ProgressionSystem.Instance != null)
            {
                ProgressionSystem.Instance.OnPlayerMoved();
            }
        }

      
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }

    
        
    }

    void HandleClick()
    {
        decisionStartTime = Time.time;
        
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            HandleBuildAction();
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; 

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

    void HandleBuildAction()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

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
            if (Mathf.Abs(bestX - gridX) <= 1 && Mathf.Abs(bestY - gridY) <= 1)
            {
                if (BuildingSystem.Instance.TryBuildVillage(bestX, bestY))
                {
                    Debug.Log("Village construction successful!");
                }
                else
                {
                    Debug.Log("Cannot build village here! Check resources or existing buildings.");
                }
            }
            else
            {
                Debug.Log("Too far away to build!");
            }
        }
    }
    
     void TryMoveTo(int x, int y)
    {
        float decisionTime = Time.time - decisionStartTime;
        
        if (Mathf.Abs(x - gridX) > 1 || Mathf.Abs(y - gridY) > 1)
        {
            Debug.Log("Te ver om in één beurt te bewegen!");
            GameLogger.Instance.RecordDecisionTiming("InvalidMove", decisionTime, $"Target:({x},{y})");
            return;
        }

        if (!ResourceManager.Instance.CanAffordMove())
        {
            Debug.Log("Not enough resources to move!");
            GameLogger.Instance.RecordDecisionTiming("MoveBlocked", decisionTime, "InsufficientResources");
            return;
        }

        MoveTo(x, y);
        GameLogger.Instance.RecordDecisionTiming("ValidMove", decisionTime, $"Target:({x},{y})");
    }


    void MoveTo(int x, int y)
    {
        lastMoveTime = Time.time;
        gridX = x;
        gridY = y;
        targetPos = grid.GetTile(gridX, gridY).transform.position + Vector3.up * 0.5f;
        isMoving = true;    
        
        // Track resources before and after move
        int goldBefore = ResourceManager.Instance.GetResource("gold");
        int woodBefore = ResourceManager.Instance.GetResource("wood");
        
        ResourceManager.Instance.SpendMovementCost();
        
        int goldAfter = ResourceManager.Instance.GetResource("gold");
        int woodAfter = ResourceManager.Instance.GetResource("wood");
        
        Debug.Log($"Player moved to tile: ({gridX},{gridY})");

        GameLogger.Instance.RecordMove(gridX, gridY);
        
        // Track resource changes
        if (goldBefore != goldAfter)
        {
            GameLogger.Instance.RecordResourceManagement("Spent", "gold", goldBefore - goldAfter, goldAfter);
        }
        if (woodBefore != woodAfter)
        {
            GameLogger.Instance.RecordResourceManagement("Spent", "wood", woodBefore - woodAfter, woodAfter);
        }

        CheckTileResources();

        if (TribeEncounterSystem.Instance.ShouldTriggerEncounter())
        {
            TribeEncounterSystem.Instance.TriggerTribeEncounter(gridX, gridY);
        }
    }

    void CheckTileResources()
    {
        if (Random.value < 0.3f)
        {
            if (Random.value < 0.5f)
            {
                int woodAmount = Random.Range(2, 6);
                ResourceManager.Instance.AddResource("wood", woodAmount);
                int currentWood = ResourceManager.Instance.GetResource("wood");
                Debug.Log($"Found a forest! Gained {woodAmount} wood. Total: {currentWood}");
                GameLogger.Instance.RecordResourceManagement("Found", "wood", woodAmount, currentWood);
                GameLogger.Instance.RecordEngagementMetrics("ResourceDiscovery", woodAmount, "Forest");
            }
            else
            {
                int goldAmount = Random.Range(1, 4);
                ResourceManager.Instance.AddResource("gold", goldAmount);
                int currentGold = ResourceManager.Instance.GetResource("gold");
                Debug.Log($"Found a mountain! Gained {goldAmount} gold. Total: {currentGold}");
                GameLogger.Instance.RecordResourceManagement("Found", "gold", goldAmount, currentGold);
                GameLogger.Instance.RecordEngagementMetrics("ResourceDiscovery", goldAmount, "Mountain");
            }
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
