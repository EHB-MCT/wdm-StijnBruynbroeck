using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("Resource Display")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;

    [Header("Build UI")]
    public Button buildVillageButton;
    public TextMeshProUGUI buildCostText;

    [Header("Instructions")]
    public TextMeshProUGUI instructionsText;

    private bool buildMode = false;

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

        UpdateInstructions();
    }

    void Update()
    {
        UpdateResourceDisplay();
        UpdateBuildButton();
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

    void Update()
    {
        UpdateResourceDisplay();
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

    public bool IsInBuildMode()
    {
        return buildMode;
    }

    public void ExitBuildMode()
    {
        buildMode = false;
        UpdateInstructions();
    }
}