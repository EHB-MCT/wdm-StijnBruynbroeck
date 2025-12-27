using UnityEngine;
using System.Collections.Generic;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance { get; private set; }

    [System.Serializable]
    public class BuildingType
    {
        public string name;
        public int goldCost;
        public int woodCost;
        public GameObject prefab;
    }

    public List<BuildingType> buildingTypes = new List<BuildingType>();
    private Dictionary<Vector2Int, GameObject> placedBuildings = new Dictionary<Vector2Int, GameObject>();
    
    [Header("Village Benefits")]
    public int goldPerVillagePerInterval = 2;
    public int woodPerVillagePerInterval = 1;
    public float incomeInterval = 5.0f;
    private float lastIncomeTime;

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
            GameObject buildingSystem = new GameObject("BuildingSystem");
            buildingSystem.AddComponent<BuildingSystem>();
        }
    }

    void Start()
    {
        InitializeDefaultBuildings();
        lastIncomeTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastIncomeTime >= incomeInterval)
        {
            GenerateVillageIncome();
            lastIncomeTime = Time.time;
        }
    }

    void GenerateVillageIncome()
    {
        int villageCount = GetBuildingCount("Village");
        if (villageCount > 0 && ResourceManager.Instance != null)
        {
            int goldIncome = villageCount * goldPerVillagePerInterval;
            int woodIncome = villageCount * woodPerVillagePerInterval;
            
            ResourceManager.Instance.AddResource("gold", goldIncome);
            ResourceManager.Instance.AddResource("wood", woodIncome);
            
            Debug.Log($"Village income: +{goldIncome} Gold, +{woodIncome} Wood from {villageCount} villages");
            GameLogger.Instance.RecordEvent($"Village Income: {goldIncome}G, {woodIncome}W", 0, 0);
        }
    }

    void InitializeDefaultBuildings()
    {
        buildingTypes.Add(new BuildingType
        {
            name = "Village",
            goldCost = 8,
            woodCost = 12,
            prefab = null
        });
    }

    public bool CanBuildBuilding(string buildingName)
    {
        BuildingType building = buildingTypes.Find(b => b.name == buildingName);
        if (building == null) return false;

        return ResourceManager.Instance.GetResource("gold") >= building.goldCost &&
               ResourceManager.Instance.GetResource("wood") >= building.woodCost;
    }

    public bool TryBuildVillage(int gridX, int gridY)
    {
        float decisionStartTime = Time.time;
        int goldBefore = ResourceManager.Instance.GetResource("gold");
        int woodBefore = ResourceManager.Instance.GetResource("wood");
        
        if (!CanBuildBuilding("Village")) 
        {
            float decisionTime = Time.time - decisionStartTime;
            GameLogger.Instance.RecordStrategicChoice("BuildAttempt", "Village_Failed_Resources", decisionTime);
            return false;
        }

        Vector2Int position = new Vector2Int(gridX, gridY);
        if (placedBuildings.ContainsKey(position))
        {
            float decisionTime = Time.time - decisionStartTime;
            GameLogger.Instance.RecordStrategicChoice("BuildAttempt", "Village_Failed_Occupied", decisionTime);
            Debug.Log("There's already a building at this position!");
            return false;
        }

        BuildingType villageBuilding = buildingTypes.Find(b => b.name == "Village");
        
        if (ResourceManager.Instance.SpendResource("gold", villageBuilding.goldCost) &&
            ResourceManager.Instance.SpendResource("wood", villageBuilding.woodCost))
        {
            float decisionTime = Time.time - decisionStartTime;
            int goldAfter = ResourceManager.Instance.GetResource("gold");
            int woodAfter = ResourceManager.Instance.GetResource("wood");
            
            GameObject villageMarker = CreateVillageMarker(gridX, gridY);
            placedBuildings[position] = villageMarker;
            
            Debug.Log($"Village built at ({gridX}, {gridY})!");
            GameLogger.Instance.RecordEvent("Built Village", gridX, gridY);
            GameLogger.Instance.RecordStrategicChoice("BuildSuccess", "Village", decisionTime);
            GameLogger.Instance.RecordResourceManagement("Spent", "gold", villageBuilding.goldCost, goldAfter);
            GameLogger.Instance.RecordResourceManagement("Spent", "wood", villageBuilding.woodCost, woodAfter);
            
            // Grant experience for building
            if (ProgressionSystem.Instance != null)
            {
                ProgressionSystem.Instance.OnVillageBuilt();
            }
            
            return true;
        }

        return false;
    }

    private GameObject CreateVillageMarker(int gridX, int gridY)
    {
        GameObject villageMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        villageMarker.name = $"Village_{gridX}_{gridY}";
        villageMarker.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        
        HexGrid grid = FindObjectOfType<HexGrid>();
        if (grid != null)
        {
            GameObject tile = grid.GetTile(gridX, gridY);
            if (tile != null)
            {
                villageMarker.transform.position = tile.transform.position + Vector3.up * 0.2f;
            }
        }

        Renderer renderer = villageMarker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.8f, 0.6f, 0.4f);
        }

        return villageMarker;
    }

    public bool HasBuildingAt(int gridX, int gridY)
    {
        return placedBuildings.ContainsKey(new Vector2Int(gridX, gridY));
    }

    public List<Vector2Int> GetPlacedVillages()
    {
        return new List<Vector2Int>(placedBuildings.Keys);
    }

    public int GetBuildingCount(string buildingName)
    {
        int count = 0;
        foreach (var building in placedBuildings)
        {
            if (building.Value.name.StartsWith(buildingName))
                count++;
        }
        return count;
    }

    public void ClearAllBuildings()
    {
        foreach (var building in placedBuildings.Values)
        {
            if (building != null)
                Destroy(building);
        }
        placedBuildings.Clear();
    }
}