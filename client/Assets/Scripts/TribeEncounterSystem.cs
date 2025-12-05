using UnityEngine;

public class TribeEncounterSystem : MonoBehaviour
{
    public static TribeEncounterSystem Instance { get; private set; }

    [System.Serializable]
    public class TribeEncounter
    {
        public string tribeName;
        public string description;
        public int diplomacyReward;
        public int aggressionReward;
        public int diplomacyCost;
        public int aggressionCost;
    }

    public TribeEncounter[] possibleEncounters;

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
            GameObject tribeSystem = new GameObject("TribeEncounterSystem");
            tribeSystem.AddComponent<TribeEncounterSystem>();
        }
    }

    void Start()
    {
        InitializeDefaultEncounters();
    }

    void InitializeDefaultEncounters()
    {
        possibleEncounters = new TribeEncounter[]
        {
            new TribeEncounter
            {
                tribeName = "Peaceful Traders",
                description = "A group of friendly traders offers you a deal.",
                diplomacyReward = 8,
                aggressionReward = 2,
                diplomacyCost = 0,
                aggressionCost = 3
            },
            new TribeEncounter
            {
                tribeName = "Warrior Clan",
                description = "A warrior clan blocks your path. They demand tribute.",
                diplomacyReward = 3,
                aggressionReward = 12,
                diplomacyCost = 5,
                aggressionCost = 8
            },
            new TribeEncounter
            {
                tribeName = "Mystic Shamans",
                description = "Mysterious shamans offer you ancient knowledge.",
                diplomacyReward = 6,
                aggressionReward = 4,
                diplomacyCost = 2,
                aggressionCost = 6
            }
        };
    }

    public void TriggerTribeEncounter(int gridX, int gridY)
    {
        if (possibleEncounters == null || possibleEncounters.Length == 0)
        {
            InitializeDefaultEncounters();
        }

        TribeEncounter encounter = possibleEncounters[Random.Range(0, possibleEncounters.Length)];
        
        Debug.Log($"=== TRIBE ENCOUNTER ===");
        Debug.Log($"Location: ({gridX}, {gridY})");
        Debug.Log($"Tribe: {encounter.tribeName}");
        Debug.Log($"Description: {encounter.description}");
        Debug.Log($"Choose your action:");
        Debug.Log($"1. Diplomatic (Cost: {encounter.diplomacyCost} gold, Reward: {encounter.diplomacyReward} gold)");
        Debug.Log($"2. Aggressive (Cost: {encounter.aggressionCost} gold, Reward: {encounter.aggressionReward} gold)");
        Debug.Log($"3. Ignore (No cost, no reward)");
        Debug.Log($"Press 1, 2, or 3 to choose");

        GameLogger.Instance.RecordEvent($"Tribe Encounter: {encounter.tribeName}", gridX, gridY);

        // In a real game, you'd show a UI dialog here
        // For now, we'll use keyboard input in Update
        StartCoroutine(WaitForPlayerChoice(encounter, gridX, gridY));
    }

    private System.Collections.IEnumerator WaitForPlayerChoice(TribeEncounter encounter, int gridX, int gridY)
    {
        bool choiceMade = false;
        
        while (!choiceMade)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                HandleDiplomaticChoice(encounter, gridX, gridY);
                choiceMade = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                HandleAggressiveChoice(encounter, gridX, gridY);
                choiceMade = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("You chose to ignore the tribe and moved on.");
                GameLogger.Instance.RecordEvent("Ignored Tribe", gridX, gridY);
                choiceMade = true;
            }
            
            yield return null;
        }
    }

    private void HandleDiplomaticChoice(TribeEncounter encounter, int gridX, int gridY)
    {
        if (ResourceManager.Instance.SpendResource("gold", encounter.diplomacyCost))
        {
            ResourceManager.Instance.AddResource("gold", encounter.diplomacyReward);
            Debug.Log($"Diplomatic approach successful! Gained {encounter.diplomacyReward} gold.");
            GameLogger.Instance.RecordEvent($"Diplomatic with {encounter.tribeName}", gridX, gridY);
        }
        else
        {
            Debug.Log("Not enough gold for diplomatic approach!");
            GameLogger.Instance.RecordEvent($"Diplomatic Failed (No Gold)", gridX, gridY);
        }
    }

    private void HandleAggressiveChoice(TribeEncounter encounter, int gridX, int gridY)
    {
        if (ResourceManager.Instance.SpendResource("gold", encounter.aggressionCost))
        {
            ResourceManager.Instance.AddResource("gold", encounter.aggressionReward);
            Debug.Log($"Aggressive approach successful! Gained {encounter.aggressionReward} gold.");
            GameLogger.Instance.RecordEvent($"Aggressive with {encounter.tribeName}", gridX, gridY);
        }
        else
        {
            Debug.Log("Not enough gold for aggressive approach!");
            GameLogger.Instance.RecordEvent($"Aggressive Failed (No Gold)", gridX, gridY);
        }
    }

    public bool ShouldTriggerEncounter()
    {
        return Random.value < 0.3f; // 30% chance
    }
}