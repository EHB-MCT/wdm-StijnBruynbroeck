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
    }

    void InitializeDefaultBuildings()
    {
        buildingTypes.Add(new BuildingType
        {
            name = "Village",
            goldCost = 5,
            woodCost = 8,
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
        if (!CanBuildBuilding("Village")) return false;

        Vector2Int position = new Vector2Int(gridX, gridY);
        if (placedBuildings.ContainsKey(position))
        {
            Debug.Log("There's already a building at this position!");
            return false;
        }

        BuildingType villageBuilding = buildingTypes.Find(b => b.name == "Village");
        
        if (ResourceManager.Instance.SpendResource("gold", villageBuilding.goldCost) &&
            ResourceManager.Instance.SpendResource("wood", villageBuilding.woodCost))
        {
            GameObject villageMarker = CreateVillageMarker(gridX, gridY);
            placedBuildings[position] = villageMarker;
            
            Debug.Log($"Village built at ({gridX}, {gridY})!");
            GameLogger.Instance.RecordEvent("Built Village", gridX, gridY);
            
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
}