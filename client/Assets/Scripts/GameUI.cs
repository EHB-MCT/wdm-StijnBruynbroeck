using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("Resource Display")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;

    [Header("Progress Display")]
    public TextMeshProUGUI villagesText;
    public TextMeshProUGUI winConditionText;

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

    private bool buildMode = false;
    private TribeEncounterSystem.TribeEncounter currentEncounter;
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

        // Hide encounter panel initially
        if (encounterPanel != null)
            encounterPanel.SetActive(false);

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

    void UpdateResourceDisplay()
    {
        if (ResourceManager.Instance != null)
        {
            if (goldText != null)
                goldText.text = $"Gold: {ResourceManager.Instance.GetResource("gold")}";
            
            if (woodText != null)
                woodText.text = $"Wood: {ResourceManager.Instance.GetResource("wood")}";
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
                buildCostText.text = canBuild ? "Build Village (5G, 8W)" : "Need: 5 Gold, 8 Wood";
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
}