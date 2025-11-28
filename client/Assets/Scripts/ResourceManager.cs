using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public int gold = 10;
    public int wood = 10;
    public int movementCost = 1;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject resourceManager = new GameObject("ResourceManager");
            resourceManager.AddComponent<ResourceManager>();
        }
    }

    void Start()
    {
        Debug.Log($"Resources - Gold: {gold}, Wood: {wood}");
    }

    public void AddResource(string type, int amount)
    {
        switch (type.ToLower())
        {
            case "gold":
                gold += amount;
                Debug.Log($"Gained {amount} Gold! Total: {gold}");
                break;
            case "wood":
                wood += amount;
                Debug.Log($"Gained {amount} Wood! Total: {wood}");
                break;
        }
    }

    public bool SpendResource(string type, int amount)
    {
        switch (type.ToLower())
        {
            case "gold":
                if (gold >= amount)
                {
                    gold -= amount;
                    Debug.Log($"Spent {amount} Gold! Remaining: {gold}");
                    return true;
                }
                break;
            case "wood":
                if (wood >= amount)
                {
                    wood -= amount;
                    Debug.Log($"Spent {amount} Wood! Remaining: {wood}");
                    return true;
                }
                break;
        }
        return false;
    }

    public bool CanAffordMove()
    {
        return gold >= movementCost || wood >= movementCost;
    }

    public void SpendMovementCost()
    {
        if (gold >= movementCost)
        {
            SpendResource("gold", movementCost);
        }
        else if (wood >= movementCost)
        {
            SpendResource("wood", movementCost);
        }
    }

    public int GetResource(string type)
    {
        switch (type.ToLower())
        {
            case "gold":
                return gold;
            case "wood":
                return wood;
            default:
                return 0;
        }
    }
}