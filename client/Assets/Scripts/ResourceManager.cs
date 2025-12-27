using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public int gold = 10;
    public int wood = 10;
    public int food = 15;
    public int stone = 5;
    public int population = 1;
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
                if (VisualEffects.Instance != null && PlayerController.Instance != null)
                {
                    Color goldColor = new Color(1f, 0.84f, 0f); // Gold color
                    VisualEffects.Instance.ShowResourcePopup(PlayerController.Instance.transform.position, "Gold", amount, goldColor);
                }
                break;
            case "wood":
                wood += amount;
                Debug.Log($"Gained {amount} Wood! Total: {wood}");
                if (VisualEffects.Instance != null && PlayerController.Instance != null)
                {
                    Color woodColor = new Color(0.55f, 0.27f, 0.07f); // Brown color
                    VisualEffects.Instance.ShowResourcePopup(PlayerController.Instance.transform.position, "Wood", amount, woodColor);
                }
                break;
            case "food":
                food += amount;
                Debug.Log($"Gained {amount} Food! Total: {food}");
                if (VisualEffects.Instance != null && PlayerController.Instance != null)
                {
                    Color foodColor = Color.green;
                    VisualEffects.Instance.ShowResourcePopup(PlayerController.Instance.transform.position, "Food", amount, foodColor);
                }
                break;
            case "stone":
                stone += amount;
                Debug.Log($"Gained {amount} Stone! Total: {stone}");
                if (VisualEffects.Instance != null && PlayerController.Instance != null)
                {
                    Color stoneColor = Color.gray;
                    VisualEffects.Instance.ShowResourcePopup(PlayerController.Instance.transform.position, "Stone", amount, stoneColor);
                }
                break;
            case "population":
                population += amount;
                Debug.Log($"Population increased by {amount}! Total: {population}");
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
                    GameLogger.Instance.RecordResourceManagement("Spent", "gold", amount, gold);
                    return true;
                }
                break;
            case "wood":
                if (wood >= amount)
                {
                    wood -= amount;
                    Debug.Log($"Spent {amount} Wood! Remaining: {wood}");
                    GameLogger.Instance.RecordResourceManagement("Spent", "wood", amount, wood);
                    return true;
                }
                break;
            case "food":
                if (food >= amount)
                {
                    food -= amount;
                    Debug.Log($"Spent {amount} Food! Remaining: {food}");
                    GameLogger.Instance.RecordResourceManagement("Spent", "food", amount, food);
                    return true;
                }
                break;
            case "stone":
                if (stone >= amount)
                {
                    stone -= amount;
                    Debug.Log($"Spent {amount} Stone! Remaining: {stone}");
                    GameLogger.Instance.RecordResourceManagement("Spent", "stone", amount, stone);
                    return true;
                }
                break;
        }
        GameLogger.Instance.RecordResourceManagement("FailedSpend", type, amount, GetResource(type));
        return false;
    }

    public bool CanAffordMove()
    {
        return gold >= movementCost || wood >= movementCost || food >= movementCost;
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
        else if (food >= movementCost)
        {
            SpendResource("food", movementCost);
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
            case "food":
                return food;
            case "stone":
                return stone;
            case "population":
                return population;
            default:
                return 0;
        }
    }

    public void SetResource(string type, int amount)
    {
        switch (type.ToLower())
        {
            case "gold":
                gold = amount;
                Debug.Log($"Gold set to {gold}");
                break;
            case "wood":
                wood = amount;
                Debug.Log($"Wood set to {wood}");
                break;
            case "food":
                food = amount;
                Debug.Log($"Food set to {food}");
                break;
            case "stone":
                stone = amount;
                Debug.Log($"Stone set to {stone}");
                break;
            case "population":
                population = amount;
                Debug.Log($"Population set to {population}");
                break;
        }
    }
}