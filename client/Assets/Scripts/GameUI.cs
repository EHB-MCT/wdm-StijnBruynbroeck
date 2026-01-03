using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all user interface elements including menus, HUD, and interactive panels.
/// Handles display of resources, quests, threats, and game state.
/// Source: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
/// </summary>
public class GameUI : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static GameUI Instance { get; private set; }

    [Header("Resource Display")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI populationText;

    [Header("Progress Display")]
    public TextMeshProUGUI villagesText;
    public TextMeshProUGUI winConditionText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceText;

    [Header("Build UI")]
    public Button buildVillageButton;
    public TextMeshProUGUI buildCostText;

    [Header("Instructions")]
    public TextMeshProUGUI instructionsText;

    [Header("Tribe Encounter UI")]
    public GameObject encounterPanel;
    public TextMeshProUGUI tribeNameText;
    public TextMeshProUGUI tribeDescriptionText;
    public Button diplomaticButton;
    public Button aggressiveButton;
    public Button ignoreButton;
    public TextMeshProUGUI diplomaticCostText;
    public TextMeshProUGUI aggressiveCostText;

    [Header("Threat UI")]
    public GameObject threatPanel;
    public TextMeshProUGUI threatNameText;
    public TextMeshProUGUI threatDescriptionText;
    public Button payOffButton;
    public Button fightButton;
    public TextMeshProUGUI payOffCostText;
    public TextMeshProUGUI fightRewardText;

    [Header("Quest UI")]
    public GameObject questNotificationPanel;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questRewardText;
    public Button questAcceptButton;

    private bool buildMode = false;
    private TribeEncounterSystem.TribeEncounter currentEncounter;
    private EnemyThreatSystem.ThreatType currentThreat;
    private int currentEncounterX, currentEncounterY;

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
        SetupUI();
        UpdateResourceDisplay();
        UpdateBuildButton();
    }

    void SetupUI()
    {
        if (buildVillageButton != null)
        {
            buildVillageButton.onClick.AddListener(ToggleBuildMode);
        }

        // Setup tribe encounter buttons
        if (diplomaticButton != null)
            diplomaticButton.onClick.AddListener(OnDiplomaticChoice);
        
        if (aggressiveButton != null)
            aggressiveButton.onClick.AddListener(OnAggressiveChoice);
        
        if (ignoreButton != null)
            ignoreButton.onClick.AddListener(OnIgnoreChoice);

        // Setup threat buttons
        if (payOffButton != null)
            payOffButton.onClick.AddListener(OnPayOffChoice);
        
        if (fightButton != null)
            fightButton.onClick.AddListener(OnFightChoice);

        // Setup quest button
        if (questAcceptButton != null)
            questAcceptButton.onClick.AddListener(OnQuestAccept);

        // Hide panels initially
        if (encounterPanel != null)
            encounterPanel.SetActive(false);
        
        if (threatPanel != null)
            threatPanel.SetActive(false);
            
        if (questNotificationPanel != null)
            questNotificationPanel.SetActive(false);

        UpdateInstructions();
    }
    void Update()
    {
        UpdateResourceDisplay();
        UpdateProgressDisplay();
        UpdateBuildButton();

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && buildMode)
        {
            buildMode = false;
            UpdateInstructions();
            Debug.Log("Build mode cancelled");
        }
    }

    public void UpdateResourceDisplay()
    {
        if (ResourceManager.Instance != null)
        {
            if (goldText != null)
                goldText.text = $"Gold: {ResourceManager.Instance.GetResource("gold")}";
            
            if (woodText != null)
                woodText.text = $"Wood: {ResourceManager.Instance.GetResource("wood")}";
            
            if (foodText != null)
                foodText.text = $"Food: {ResourceManager.Instance.GetResource("food")}";
            
            if (stoneText != null)
                stoneText.text = $"Stone: {ResourceManager.Instance.GetResource("stone")}";
            
            if (populationText != null)
                populationText.text = $"Population: {ResourceManager.Instance.GetResource("population")}";
        }
    }

    void UpdateProgressDisplay()
    {
        if (GameManager.Instance != null)
        {
            if (villagesText != null)
            {
                int villages = GameManager.Instance.GetVillagesBuilt();
                villagesText.text = $"Villages: {villages}/{GameManager.Instance.villagesToWin}";
            }
            
            if (winConditionText != null)
            {
                int gold = ResourceManager.Instance?.GetResource("gold") ?? 0;
                winConditionText.text = $"Goals: {GameManager.Instance.villagesToWin} Villages OR {GameManager.Instance.goldToWin} Gold";
            }
        }
        
        // Update progression display
        if (ProgressionSystem.Instance != null)
        {
            if (levelText != null)
            {
                levelText.text = $"Level: {ProgressionSystem.Instance.currentLevel}";
            }
            
            if (experienceText != null)
            {
                int expToNext = ProgressionSystem.Instance.GetExperienceToNextLevel();
                float progress = ProgressionSystem.Instance.GetExperienceProgress();
                experienceText.text = expToNext > 0 
                    ? $"XP: {ProgressionSystem.Instance.currentExperience}/{expToNext} ({progress:P0})"
                    : $"XP: {ProgressionSystem.Instance.currentExperience} (MAX)";
            }
        }
    }

    void UpdateBuildButton()
    {
        if (buildVillageButton != null && BuildingSystem.Instance != null)
        {
            bool canBuild = BuildingSystem.Instance.CanBuildBuilding("Village");
            buildVillageButton.interactable = canBuild;
            
            if (buildCostText != null)
            {
                string baseText = canBuild ? "Build Village (5G, 8W)" : "Need: 5 Gold, 8 Wood";
                
                // Apply influence mechanisms to building text
                if (InfluenceManager.Instance != null && canBuild)
                {
                    baseText = InfluenceManager.Instance.ApplyFramingEffect("building", "Build Village (5G, 8W)");
                    baseText = InfluenceManager.Instance.ApplyScarcityEffect("building", baseText);
                    baseText = InfluenceManager.Instance.ApplySocialProof("building", baseText);
                    baseText = InfluenceManager.Instance.ApplyAnchoringEffect("building_cost", baseText, 13); // 5+8 total cost
                }
                
                buildCostText.text = baseText;
                buildCostText.color = canBuild ? Color.white : Color.red;
            }
        }
    }

    void UpdateInstructions()
    {
        if (instructionsText != null)
        {
            instructionsText.text = buildMode 
                ? "Click a nearby tile to build a village\nPress ESC to cancel"
                : "Click to move • Shift+Click to build\nPress B for build mode";
        }
    }

    void ToggleBuildMode()
    {
        buildMode = !buildMode;
        UpdateInstructions();
        
        if (buildMode)
        {
            Debug.Log("Build mode activated - Click a nearby tile to build a village");
        }
        else
        {
            Debug.Log("Build mode deactivated");
        }
    }

    public bool IsInBuildMode()
    {
        return buildMode;
    }

    public void ExitBuildMode()
    {
        buildMode = false;
        UpdateInstructions();
    }

    public void ShowTribeEncounter(TribeEncounterSystem.TribeEncounter encounter, int gridX, int gridY)
    {
        currentEncounter = encounter;
        currentEncounterX = gridX;
        currentEncounterY = gridY;

        if (encounterPanel != null)
        {
            encounterPanel.SetActive(true);
            
            if (tribeNameText != null)
                tribeNameText.text = encounter.tribeName;
            
            if (tribeDescriptionText != null)
                tribeDescriptionText.text = encounter.description;
            
            if (diplomaticCostText != null)
                diplomaticCostText.text = $"Diplomatic (Cost: {encounter.diplomacyCost} Gold, Reward: {encounter.diplomacyReward} Gold)";
            
            if (aggressiveCostText != null)
                aggressiveCostText.text = $"Aggressive (Cost: {encounter.aggressionCost} Gold, Reward: {encounter.aggressionReward} Gold)";
            
            // Update button states based on resources
            UpdateEncounterButtons();
        }
    }

    void UpdateEncounterButtons()
    {
        if (ResourceManager.Instance == null) return;

        bool canDiplomatic = ResourceManager.Instance.GetResource("gold") >= currentEncounter.diplomacyCost;
        bool canAggressive = ResourceManager.Instance.GetResource("gold") >= currentEncounter.aggressionCost;

        if (diplomaticButton != null)
            diplomaticButton.interactable = canDiplomatic;
        
        if (aggressiveButton != null)
            aggressiveButton.interactable = canAggressive;
    }

    void OnDiplomaticChoice()
    {
        if (currentEncounter != null && ResourceManager.Instance != null)
        {
            if (ResourceManager.Instance.SpendResource("gold", currentEncounter.diplomacyCost))
            {
                ResourceManager.Instance.AddResource("gold", currentEncounter.diplomacyReward);
                GameLogger.Instance.RecordEvent($"Diplomatic with {currentEncounter.tribeName}", currentEncounterX, currentEncounterY);
                Debug.Log($"Diplomatic approach successful! Gained {currentEncounter.diplomacyReward} gold.");
            }
            else
            {
                GameLogger.Instance.RecordEvent($"Diplomatic Failed (No Gold)", currentEncounterX, currentEncounterY);
                Debug.Log("Not enough gold for diplomatic approach!");
            }
            
            HideEncounterPanel();
        }
    }

    void OnAggressiveChoice()
    {
        if (currentEncounter != null && ResourceManager.Instance != null)
        {
            if (ResourceManager.Instance.SpendResource("gold", currentEncounter.aggressionCost))
            {
                ResourceManager.Instance.AddResource("gold", currentEncounter.aggressionReward);
                GameLogger.Instance.RecordEvent($"Aggressive with {currentEncounter.tribeName}", currentEncounterX, currentEncounterY);
                Debug.Log($"Aggressive approach successful! Gained {currentEncounter.aggressionReward} gold.");
            }
            else
            {
                GameLogger.Instance.RecordEvent($"Aggressive Failed (No Gold)", currentEncounterX, currentEncounterY);
                Debug.Log("Not enough gold for aggressive approach!");
            }
            
            HideEncounterPanel();
        }
    }

    void OnIgnoreChoice()
    {
        if (currentEncounter != null)
        {
            GameLogger.Instance.RecordEvent("Ignored Tribe", currentEncounterX, currentEncounterY);
            Debug.Log("You chose to ignore the tribe and moved on.");
            HideEncounterPanel();
        }
    }

    void HideEncounterPanel()
    {
        if (encounterPanel != null)
            encounterPanel.SetActive(false);
        
        currentEncounter = null;
    }

    public void ShowThreatEncounter(EnemyThreatSystem.ThreatType threat)
    {
        currentThreat = threat;

        if (threatPanel != null)
        {
            threatPanel.SetActive(true);
            
            // Apply influence mechanisms to threat description
            string influencedDescription = threat.description;
            string influencedPayOffText = $"Pay Off ({threat.goldCost}G, {threat.woodCost}W, {threat.foodCost}F)";
            string influencedFightText = $"Fight ({threat.successChance:P0} success, {threat.goldReward}G, {threat.woodReward}W, {threat.foodReward}F reward)";
            
            // Apply influence if InfluenceManager is available
            if (InfluenceManager.Instance != null)
            {
                influencedDescription = InfluenceManager.Instance.ApplyFramingEffect("threat", threat.description);
                influencedDescription = InfluenceManager.Instance.ApplyScarcityEffect("threat", influencedDescription);
                influencedDescription = InfluenceManager.Instance.ApplySocialProof("threat", influencedDescription);
                
                influencedPayOffText = InfluenceManager.Instance.ApplyAnchoringEffect("threat_payment", influencedPayOffText, threat.goldCost + threat.woodCost + threat.foodCost);
                influencedFightText = InfluenceManager.Instance.ApplySocialProof("threat", influencedFightText);
            }
            
            if (threatNameText != null)
                threatNameText.text = threat.name;
            
            if (threatDescriptionText != null)
                threatDescriptionText.text = influencedDescription;
            
            if (payOffCostText != null)
                payOffCostText.text = influencedPayOffText;
            
            if (fightRewardText != null)
                fightRewardText.text = influencedFightText;
            
            // Update button states based on resources
            UpdateThreatButtons();
        }
    }

    void UpdateThreatButtons()
    {
        if (ResourceManager.Instance == null || currentThreat == null) return;

        bool canPayOff = ResourceManager.Instance.GetResource("gold") >= currentThreat.goldCost &&
                        ResourceManager.Instance.GetResource("wood") >= currentThreat.woodCost &&
                        ResourceManager.Instance.GetResource("food") >= currentThreat.foodCost;

        if (payOffButton != null)
            payOffButton.interactable = canPayOff;
    }

    void OnPayOffChoice()
    {
        if (currentThreat != null && EnemyThreatSystem.Instance != null)
        {
            EnemyThreatSystem.Instance.HandleThreatChoice(currentThreat, true);
            HideThreatPanel();
        }
    }

    void OnFightChoice()
    {
        if (currentThreat != null && EnemyThreatSystem.Instance != null)
        {
            EnemyThreatSystem.Instance.HandleThreatChoice(currentThreat, false);
            HideThreatPanel();
        }
    }

    void HideThreatPanel()
    {
        if (threatPanel != null)
            threatPanel.SetActive(false);
        
        currentThreat = null;
    }

    public void ShowNewQuest(QuestSystem.Quest quest)
    {
        if (questNotificationPanel != null)
        {
            questNotificationPanel.SetActive(true);
            
            // Apply influence mechanisms to quest description
            string influencedDescription = quest.description;
            string influencedRewardText = $"Rewards: {quest.goldReward}G, {quest.woodReward}W, {quest.foodReward}F, {quest.experienceReward} XP";
            
            // Apply influence if InfluenceManager is available
            if (InfluenceManager.Instance != null)
            {
                influencedDescription = InfluenceManager.Instance.ApplyFramingEffect("quest", quest.description);
                influencedDescription = InfluenceManager.Instance.ApplyScarcityEffect("quest", influencedDescription);
                influencedDescription = InfluenceManager.Instance.ApplySocialProof("quest", influencedDescription);
                
                influencedRewardText = InfluenceManager.Instance.ApplyAnchoringEffect("quest_reward", influencedRewardText, quest.goldReward + quest.woodReward + quest.foodReward);
            }
            
            if (questNameText != null)
                questNameText.text = quest.questName;
            
            if (questDescriptionText != null)
                questDescriptionText.text = influencedDescription;
            
            if (questRewardText != null)
                questRewardText.text = influencedRewardText;
        }
    }

    public void ShowQuestCompletion(QuestSystem.Quest quest)
    {
        if (questNotificationPanel != null)
        {
            // Show completion message briefly
            if (questNameText != null)
                questNameText.text = "QUEST COMPLETE!";
            
            if (questDescriptionText != null)
                questDescriptionText.text = $"{quest.questName} completed!";
            
            if (questRewardText != null)
                questRewardText.text = $"Earned: {quest.goldReward}G, {quest.woodReward}W, {quest.foodReward}F, {quest.experienceReward} XP";
            
            // Hide after 3 seconds
            Invoke("HideQuestPanel", 3f);
        }
    }

    void HideQuestPanel()
    {
        if (questNotificationPanel != null)
            questNotificationPanel.SetActive(false);
    }

    void OnQuestAccept()
    {
        HideQuestPanel();
    }
}