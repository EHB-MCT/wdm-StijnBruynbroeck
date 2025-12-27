using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Win Conditions")]
    public int villagesToWin = 5;
    public int goldToWin = 100;

    [Header("Game State")]
    public bool gameEnded = false;
    public float gameStartTime;

    [Header("UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverMessageText;
    public Button restartButton;

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
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
        }
    }

    void Start()
    {
        gameStartTime = Time.time;
        SetupUI();
    }

    void SetupUI()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        if (!gameEnded)
        {
            CheckWinConditions();
            CheckLoseConditions();
        }
    }

    void CheckWinConditions()
    {
        // Win by building enough villages
        if (BuildingSystem.Instance != null && BuildingSystem.Instance.GetBuildingCount("Village") >= villagesToWin)
        {
            TriggerWin("Victory!", $"You built {villagesToWin} villages and became a great ruler!");
            return;
        }

        // Win by accumulating enough gold
        if (ResourceManager.Instance != null && ResourceManager.Instance.GetResource("gold") >= goldToWin)
        {
            TriggerWin("Victory!", $"You accumulated {goldToWin} gold and became a wealthy empire!");
            return;
        }
    }

    void CheckLoseConditions()
    {
        if (ResourceManager.Instance == null) return;

        // Lose if run out of both resources
        int gold = ResourceManager.Instance.GetResource("gold");
        int wood = ResourceManager.Instance.GetResource("wood");

        if (gold <= 0 && wood <= 0)
        {
            TriggerLose("Defeat", "You ran out of resources and your empire collapsed.");
        }
    }

    void TriggerWin(string title, string message)
    {
        gameEnded = true;
        float gameTime = Time.time - gameStartTime;
        
        Debug.Log($"GAME WON! Time: {gameTime:F1} seconds");
        GameLogger.Instance.RecordEvent($"GAME WON - {message}", 0, 0);
        GameLogger.Instance.RecordEmotionalResponse("Victory", "Positive", gameTime);
        GameLogger.Instance.RecordEngagementMetrics("SessionComplete", gameTime, "Won");
        
        // Track winning conditions and resources
        int villages = BuildingSystem.Instance != null ? BuildingSystem.Instance.GetBuildingCount("Village") : 0;
        int gold = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("gold") : 0;
        int wood = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("wood") : 0;
        
        GameLogger.Instance.RecordStrategicChoice("VictoryCondition", $"Villages:{villages}_Gold:{gold}_Wood:{wood}", gameTime);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverTitleText != null)
                gameOverTitleText.text = title;
            
            if (gameOverMessageText != null)
                gameOverMessageText.text = $"{message}\n\nTime: {gameTime:F1} seconds";
        }

        // Disable player movement
        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = false;
    }

    void TriggerLose(string title, string message)
    {
        gameEnded = true;
        float gameTime = Time.time - gameStartTime;
        
        Debug.Log($"GAME LOST! Time: {gameTime:F1} seconds");
        GameLogger.Instance.RecordEvent($"GAME LOST - {message}", 0, 0);
        GameLogger.Instance.RecordEmotionalResponse("Defeat", "Negative", gameTime);
        GameLogger.Instance.RecordEngagementMetrics("SessionComplete", gameTime, "Lost");
        
        // Track losing conditions and final resources
        int villages = BuildingSystem.Instance != null ? BuildingSystem.Instance.GetBuildingCount("Village") : 0;
        int gold = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("gold") : 0;
        int wood = ResourceManager.Instance != null ? ResourceManager.Instance.GetResource("wood") : 0;
        
        GameLogger.Instance.RecordStrategicChoice("DefeatCondition", $"Villages:{villages}_Gold:{gold}_Wood:{wood}", gameTime);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverTitleText != null)
                gameOverTitleText.text = title;
            
            if (gameOverMessageText != null)
                gameOverMessageText.text = $"{message}\n\nTime: {gameTime:F1} seconds";
        }

        // Disable player movement
        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = false;
    }

    void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // Reset resources
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.SetResource("gold", 10);
            ResourceManager.Instance.SetResource("wood", 10);
        }

        // Reset buildings
        if (BuildingSystem.Instance != null)
        {
            BuildingSystem.Instance.ClearAllBuildings();
        }

        // Reset game state
        gameEnded = false;
        gameStartTime = Time.time;

        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Re-enable player
        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = true;

        GameLogger.Instance.RecordEvent("Game Restarted", 0, 0);
    }

    public int GetVillagesBuilt()
    {
        return BuildingSystem.Instance != null ? BuildingSystem.Instance.GetBuildingCount("Village") : 0;
    }

    public float GetGameTime()
    {
        return Time.time - gameStartTime;
    }
}