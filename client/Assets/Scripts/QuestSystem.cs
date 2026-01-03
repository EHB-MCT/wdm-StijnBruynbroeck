using UnityEngine;
using System.Collections.Generic;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem Instance { get; private set; }

    [System.Serializable]
    public class Quest
    {
        public string questName;
        public string description;
        public int goldReward;
        public int woodReward;
        public int foodReward;
        public int experienceReward;
        public bool isCompleted;
        public bool isActive;
    }

    [Header("Quest Settings")]
    public Quest[] availableQuests;
    public float questCheckInterval = 20f;
    private float lastQuestCheck;

    private Queue<Quest> activeQuests = new Queue<Quest>();

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
            GameObject questSystem = new GameObject("QuestSystem");
            questSystem.AddComponent<QuestSystem>();
        }
    }

    void Start()
    {
        lastQuestCheck = Time.time;
        InitializeDefaultQuests();
        
        // Start with one quest
        if (availableQuests.Length > 0)
        {
            ActivateQuest(0);
        }
    }

    void InitializeDefaultQuests()
    {
        availableQuests = new Quest[]
        {
            new Quest
            {
                questName = "First Settlement",
                description = "Build your first village to establish your presence.",
                goldReward = 20,
                woodReward = 10,
                foodReward = 5,
                experienceReward = 30,
                isCompleted = false,
                isActive = false
            },
            new Quest
            {
                questName = "Resource Collector",
                description = "Accumulate 50 gold to show your resource management skills.",
                goldReward = 15,
                woodReward = 15,
                foodReward = 10,
                experienceReward = 25,
                isCompleted = false,
                isActive = false
            },
            new Quest
            {
                questName = "Expanding Empire",
                description = "Build 3 villages to expand your territory.",
                goldReward = 30,
                woodReward = 20,
                foodReward = 15,
                experienceReward = 50,
                isCompleted = false,
                isActive = false
            },
            new Quest
            {
                questName = "Survivor",
                description = "Survive for 2 minutes without running out of resources.",
                goldReward = 25,
                woodReward = 25,
                foodReward = 20,
                experienceReward = 40,
                isCompleted = false,
                isActive = false
            },
            new Quest
            {
                questName = "Threat Neutralizer",
                description = "Successfully handle 3 enemy threats (pay off or defeat).",
                goldReward = 40,
                woodReward = 30,
                foodReward = 25,
                experienceReward = 60,
                isCompleted = false,
                isActive = false
            }
        };
    }

    void Update()
    {
        CheckQuestCompletion();
        
        if (Time.time - lastQuestCheck >= questCheckInterval)
        {
            OfferNewQuest();
            lastQuestCheck = Time.time;
        }
    }

    void CheckQuestCompletion()
    {
        if (BuildingSystem.Instance == null || ResourceManager.Instance == null || GameManager.Instance == null) return;

        foreach (Quest quest in availableQuests)
        {
            if (quest.isCompleted || !quest.isActive) continue;

            bool completed = false;

            switch (quest.questName)
            {
                case "First Settlement":
                    completed = BuildingSystem.Instance.GetBuildingCount("Village") >= 1;
                    break;
                    
                case "Resource Collector":
                    completed = ResourceManager.Instance.GetResource("gold") >= 50;
                    break;
                    
                case "Expanding Empire":
                    completed = BuildingSystem.Instance.GetBuildingCount("Village") >= 3;
                    break;
                    
                case "Survivor":
                    completed = GameManager.Instance.GetGameTime() >= 120f; // 2 minutes
                    break;
                    
                case "Threat Neutralizer":
                    // This would need threat counter - for now use village count as proxy
                    completed = BuildingSystem.Instance.GetBuildingCount("Village") >= 2;
                    break;
            }

            if (completed && !quest.isCompleted)
            {
                CompleteQuest(quest);
            }
        }
    }

    void CompleteQuest(Quest quest)
    {
        quest.isCompleted = true;
        quest.isActive = false;

        float completionTime = Time.time;
        float gameTime = GameManager.Instance != null ? GameManager.Instance.GetGameTime() : Time.time;

        Debug.Log($"QUEST COMPLETED: {quest.questName}!");
        Debug.Log($"Rewards: {quest.goldReward}G, {quest.woodReward}W, {quest.foodReward}F, {quest.experienceReward} XP");

        // Enhanced behavioral tracking
        if (GameLogger.Instance != null)
        {
            // Track quest completion with detailed context
            GameLogger.Instance.RecordEvent($"Completed Quest: {quest.questName}", 0, 0);
            
            // Track quest decision patterns
            GameLogger.Instance.RecordQuestDecision(quest.questName, true, "Completed Successfully");
            
            // Track quest completion timing
            GameLogger.Instance.RecordDecisionTiming("QuestCompletion", gameTime, $"Quest: {quest.questName}");
            
            // Track player state at quest completion
            int currentGold = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("gold") : 0;
            int currentWood = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("wood") : 0;
            int villages = BuildingSystem.Instance != null ? BuildingSystem.Instance.GetBuildingCount("Village") : 0;
            
            GameLogger.Instance.RecordStrategicChoice("QuestCompletionState", 
                $"Gold:{currentGold}_Wood:{currentWood}_Villages:{villages}_Quest:{quest.questName}", gameTime);
        }

        // Give rewards
        ResourceManager.Instance.AddResource("gold", quest.goldReward);
        ResourceManager.Instance.AddResource("wood", quest.woodReward);
        ResourceManager.Instance.AddResource("food", quest.foodReward);
        
        if (ProgressionSystem.Instance != null)
        {
            ProgressionSystem.Instance.AddExperience(quest.experienceReward, $"Quest: {quest.questName}");
        }

        // Show completion effect
        if (VisualEffects.Instance != null && PlayerController.Instance != null)
        {
            VisualEffects.Instance.ShowResourcePopup(
                PlayerController.Instance.transform.position,
                "QUEST COMPLETE!",
                quest.experienceReward,
                Color.cyan
            );
        }

        // Notify UI
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ShowQuestCompletion(quest);
        }
    }

    void OfferNewQuest()
    {
        // Find an inactive, uncompleted quest
        foreach (Quest quest in availableQuests)
        {
            if (!quest.isCompleted && !quest.isActive)
            {
                ActivateQuest(quest);
                break;
            }
        }
    }

    void ActivateQuest(int questIndex)
    {
        if (questIndex >= 0 && questIndex < availableQuests.Length)
        {
            Quest quest = availableQuests[questIndex];
            quest.isActive = true;
            
            Debug.Log($"NEW QUEST: {quest.questName}");
            Debug.Log($"Description: {quest.description}");
            
            GameLogger.Instance.RecordEvent($"Started Quest: {quest.questName}", 0, 0);
            
            // Notify UI
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowNewQuest(quest);
            }
        }
    }

    void ActivateQuest(Quest quest)
    {
        quest.isActive = true;
        float gameTime = GameManager.Instance != null ? GameManager.Instance.GetGameTime() : Time.time;
        
        Debug.Log($"NEW QUEST: {quest.questName}");
        Debug.Log($"Description: {quest.description}");
        
        if (GameLogger.Instance != null)
        {
            // Track quest activation with timing
            GameLogger.Instance.RecordEvent($"Started Quest: {quest.questName}", 0, 0);
            
            // Track quest decision patterns (acceptance)
            GameLogger.Instance.RecordQuestDecision(quest.questName, true, "Quest Accepted");
            
            // Track quest acceptance timing
            GameLogger.Instance.RecordDecisionTiming("QuestAcceptance", gameTime, $"Quest: {quest.questName}");
            
            // Track player state at quest acceptance
            int currentGold = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("gold") : 0;
            int currentWood = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("wood") : 0;
            int villages = BuildingSystem.Instance != null ? BuildingSystem.Instance.GetBuildingCount("Village") : 0;
            
            GameLogger.Instance.RecordStrategicChoice("QuestAcceptanceState", 
                $"Gold:{currentGold}_Wood:{currentWood}_Villages:{villages}_Quest:{quest.questName}", gameTime);
        }
        
        // Notify UI
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ShowNewQuest(quest);
        }
    }

    public Quest[] GetActiveQuests()
    {
        List<Quest> active = new List<Quest>();
        foreach (Quest quest in availableQuests)
        {
            if (quest.isActive && !quest.isCompleted)
                active.Add(quest);
        }
        return active.ToArray();
    }

    public Quest[] GetCompletedQuests()
    {
        List<Quest> completed = new List<Quest>();
        foreach (Quest quest in availableQuests)
        {
            if (quest.isCompleted)
                completed.Add(quest);
        }
        return completed.ToArray();
    }
}