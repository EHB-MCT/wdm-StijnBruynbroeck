using UnityEngine;
using System.Collections;

public class EnemyThreatSystem : MonoBehaviour
{
    public static EnemyThreatSystem Instance { get; private set; }

    [Header("Threat Settings")]
    public float threatCheckInterval = 15f;
    public float baseThreatChance = 0.3f;
    public int villagesPerThreatLevel = 2;

    [Header("Threat Types")]
    public ThreatType[] threatTypes;

    [System.Serializable]
    public class ThreatType
    {
        public string name;
        public string description;
        public int goldCost;
        public int woodCost;
        public int foodCost;
        public int goldReward;
        public int woodReward;
        public int foodReward;
        public float successChance;
        public Color threatColor;
    }

    private float lastThreatCheck;

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
            GameObject threatSystem = new GameObject("EnemyThreatSystem");
            threatSystem.AddComponent<EnemyThreatSystem>();
        }
    }

    void Start()
    {
        lastThreatCheck = Time.time;
        InitializeDefaultThreats();
    }

    void InitializeDefaultThreats()
    {
        threatTypes = new ThreatType[]
        {
            new ThreatType
            {
                name = "Bandit Raid",
                description = "Bandits are attacking your villages! Pay them off or fight them off.",
                goldCost = 8,
                woodCost = 5,
                foodCost = 3,
                goldReward = 15,
                woodReward = 8,
                foodReward = 5,
                successChance = 0.6f,
                threatColor = Color.red
            },
            new ThreatType
            {
                name = "Monster Attack",
                description = "A wild monster threatens your settlement! Defend your resources.",
                goldCost = 12,
                woodCost = 8,
                foodCost = 6,
                goldReward = 25,
                woodReward = 15,
                foodReward = 10,
                successChance = 0.4f,
                threatColor = Color.magenta
            },
            new ThreatType
            {
                name = "Rival Scout",
                description = "A rival empire scout is probing your defenses. Show your strength.",
                goldCost = 5,
                woodCost = 3,
                foodCost = 2,
                goldReward = 10,
                woodReward = 6,
                foodReward = 4,
                successChance = 0.8f,
                threatColor = new Color(1f, 0.5f, 0f) 
            }
        };
    }

    void Update()
    {
        if (Time.time - lastThreatCheck >= threatCheckInterval)
        {
            CheckForThreats();
            lastThreatCheck = Time.time;
        }
    }

    void CheckForThreats()
    {
     
        if (BuildingSystem.Instance == null) return;

        int villageCount = BuildingSystem.Instance.GetBuildingCount("Village");
        if (villageCount == 0) return;

    
        float threatChance = baseThreatChance + (villageCount / (float)villagesPerThreatLevel) * 0.1f;
        threatChance = Mathf.Min(threatChance, 0.8f); 

        if (Random.value < threatChance)
        {
            TriggerThreat();
        }
    }

    void TriggerThreat()
    {
        if (threatTypes == null || threatTypes.Length == 0) return;

        ThreatType threat = threatTypes[Random.Range(0, threatTypes.Length)];
        
        Debug.Log($"=== ENEMY THREAT ===");
        Debug.Log($"Threat: {threat.name}");
        Debug.Log($"Defense Cost: {threat.goldCost} Gold, {threat.woodCost} Wood, {threat.foodCost} Food");
        
        GameLogger.Instance.RecordEvent($"Threat Encountered: {threat.name}", 0, 0);

 
        if (GameUI.Instance != null)
        {
         
            Debug.Log("UI voor threat zou hier moeten openen");
        }

       
    }

    public void HandleThreatChoice(ThreatType threat, bool payOff)
    {
        if (payOff)
        {
          
            bool hasGold = ResourceManager.Instance.GetResource("gold") >= threat.goldCost;
            bool hasWood = ResourceManager.Instance.GetResource("wood") >= threat.woodCost;
            
            bool hasFood = ResourceManager.Instance.GetResource("food") >= threat.foodCost; 

            if (hasGold && hasWood && hasFood)
            {
                
                ResourceManager.Instance.SpendResource("gold", threat.goldCost);
                ResourceManager.Instance.SpendResource("wood", threat.woodCost);
                ResourceManager.Instance.SpendResource("food", threat.foodCost);

                Debug.Log($"Successfully paid off {threat.name}!");
                GameLogger.Instance.RecordEvent($"Paid off threat: {threat.name}", 0, 0);
            }
            else
            {
                
                Debug.Log($"Not enough resources to pay off {threat.name}!");
                GameLogger.Instance.RecordEvent($"Failed payment: {threat.name}", 0, 0);
                ApplyThreatPenalty(threat);
            }
        }
        else
        {
           
            if (Random.value < threat.successChance)
            {
                
                Debug.Log($"Successfully defeated {threat.name}!");
                GameLogger.Instance.RecordEvent($"Defeated threat: {threat.name}", 0, 0);
                
                
                ResourceManager.Instance.AddResource("gold", threat.goldReward);
                ResourceManager.Instance.AddResource("wood", threat.woodReward);
                ResourceManager.Instance.AddResource("food", threat.foodReward);
            }
            else
            {
                
                Debug.Log($"Failed to defeat {threat.name}!");
                GameLogger.Instance.RecordEvent($"Lost fight: {threat.name}", 0, 0);
                ApplyThreatPenalty(threat);
            }
        }
    }

    void ApplyThreatPenalty(ThreatType threat)
    {
        // Straf: Verlies de helft van de resources die het had gekost
        int goldLoss = Mathf.Min(ResourceManager.Instance.GetResource("gold"), threat.goldCost / 2);
        int woodLoss = Mathf.Min(ResourceManager.Instance.GetResource("wood"), threat.woodCost / 2);
        
        // Let op: Food check
        int foodLoss = 0; 
        if(ResourceManager.Instance.GetResource("food") > 0) {
             foodLoss = Mathf.Min(ResourceManager.Instance.GetResource("food"), threat.foodCost / 2);
        }

        ResourceManager.Instance.SpendResource("gold", goldLoss);
        ResourceManager.Instance.SpendResource("wood", woodLoss);
        ResourceManager.Instance.SpendResource("food", foodLoss);

        Debug.Log($"Threat penalty! Lost: {goldLoss} Gold, {woodLoss} Wood, {foodLoss} Food");
    }
}