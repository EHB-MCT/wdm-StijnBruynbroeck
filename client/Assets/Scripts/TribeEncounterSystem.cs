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
        
        Debug.Log($"Tribe Encounter: {encounter.tribeName} at ({gridX}, {gridY})");
        GameLogger.Instance.RecordEvent($"Tribe Encounter: {encounter.tribeName}", gridX, gridY);

        // Show the UI popup
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ShowTribeEncounter(encounter, gridX, gridY);
        }
    }



    public bool ShouldTriggerEncounter()
    {
        return Random.value < 0.3f; // 30% chance
    }
}