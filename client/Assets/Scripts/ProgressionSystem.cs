using UnityEngine;

public class ProgressionSystem : MonoBehaviour
{
    public static ProgressionSystem Instance { get; private set; }

    [Header("Experience Settings")]
    public int experiencePerMove = 1;
    public int experiencePerVillage = 25;
    public int experiencePerThreatDefeated = 15;
    public int experiencePerThreatPaidOff = 5;

    [Header("Upgrade Settings")]
    public int[] experienceThresholds = { 10, 25, 50, 100, 200, 500 };
    public float[] movementCostReduction = { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
    public float[] resourceBonusMultiplier = { 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f };

    [Header("Current Progress")]
    public int currentLevel = 0;
    public int currentExperience = 0;

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
            GameObject progressionSystem = new GameObject("ProgressionSystem");
            progressionSystem.AddComponent<ProgressionSystem>();
        }
    }

    void Start()
    {
        Debug.Log($"Progression System initialized - Level {currentLevel}, {currentExperience} XP");
    }

    public void AddExperience(int amount, string source)
    {
        currentExperience += amount;
        Debug.Log($"Gained {amount} XP from {source}. Total: {currentExperience}");
        GameLogger.Instance.RecordEvent($"XP +{amount} from {source}", 0, 0);

        CheckForLevelUp();
    }

    void CheckForLevelUp()
    {
        int newLevel = CalculateLevel(currentExperience);
        
        if (newLevel > currentLevel)
        {
            int levelsGained = newLevel - currentLevel;
            currentLevel = newLevel;
            
            Debug.Log($"LEVEL UP! Now level {currentLevel} (+{levelsGained} levels)");
            GameLogger.Instance.RecordEvent($"LEVELED UP to {currentLevel}", 0, 0);
            
            ApplyLevelBenefits();
            
            // Show level up effect
            if (VisualEffects.Instance != null && PlayerController.Instance != null)
            {
                VisualEffects.Instance.ShowResourcePopup(
                    PlayerController.Instance.transform.position, 
                    "LEVEL UP!", 
                    currentLevel, 
                    Color.yellow
                );
            }
        }
    }

    int CalculateLevel(int experience)
    {
        for (int i = experienceThresholds.Length - 1; i >= 0; i--)
        {
            if (experience >= experienceThresholds[i])
                return i + 1;
        }
        return 0;
    }

    void ApplyLevelBenefits()
    {
        // Update movement cost based on level
        if (ResourceManager.Instance != null)
        {
            float reduction = GetMovementCostReduction();
            ResourceManager.Instance.movementCost = Mathf.Max(1, Mathf.RoundToInt(1f * (1f - reduction)));
            Debug.Log($"Movement cost reduced to {ResourceManager.Instance.movementCost}");
        }
    }

    public float GetMovementCostReduction()
    {
        if (currentLevel >= 0 && currentLevel < movementCostReduction.Length)
            return movementCostReduction[currentLevel];
        return movementCostReduction[movementCostReduction.Length - 1];
    }

    public float GetResourceBonusMultiplier()
    {
        if (currentLevel >= 0 && currentLevel < resourceBonusMultiplier.Length)
            return resourceBonusMultiplier[currentLevel];
        return resourceBonusMultiplier[resourceBonusMultiplier.Length - 1];
    }

    public int GetExperienceToNextLevel()
    {
        if (currentLevel < experienceThresholds.Length)
            return experienceThresholds[currentLevel] - currentExperience;
        return 0; // Max level
    }

    /// <summary>
    /// Called when resources change to update progression calculations.
    /// </summary>
    /// <param name="resourceType">Type of resource that changed</param>
    /// <param name="amount">New amount value</param>
    public void OnResourceChanged(string resourceType, int amount)
    {
        // Resource changes can affect progression calculations
        // This method allows progression system to react to resource changes
        Debug.Log($"Resource changed: {resourceType} -> {amount}");
    }

    public float GetExperienceProgress()
    {
        if (currentLevel >= experienceThresholds.Length)
            return 1f; // Max level
        
        int currentThreshold = currentLevel == 0 ? 0 : experienceThresholds[currentLevel - 1];
        int nextThreshold = experienceThresholds[currentLevel];
        
        return (float)(currentExperience - currentThreshold) / (nextThreshold - currentThreshold);
    }

    // Events that grant experience
    public void OnPlayerMoved()
    {
        AddExperience(experiencePerMove, "Movement");
    }

    public void OnVillageBuilt()
    {
        AddExperience(experiencePerVillage, "Building");
    }

    public void OnThreatDefeated()
    {
        AddExperience(experiencePerThreatDefeated, "Combat");
    }

    public void OnThreatPaidOff()
    {
        AddExperience(experiencePerThreatPaidOff, "Diplomacy");
    }
}