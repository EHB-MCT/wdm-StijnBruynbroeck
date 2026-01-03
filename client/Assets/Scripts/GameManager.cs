using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the overall game state, win/lose conditions, and game flow.
/// Implements singleton pattern following Unity best practices.
/// Source: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static GameManager Instance { get; private set; }

    [Header("Win Conditions")]
    [Tooltip("Number of villages required to win the game")]
    public int villagesToWin = 5;

    [Tooltip("Amount of gold required to win the game")]
    public int goldToWin = 100;

    [Header("Game State")]
    [Tooltip("Indicates whether the game has ended")]
    public bool gameEnded = false;

    [Tooltip("Time when the game started")]
    public float gameStartTime;

    [Header("UI")]
    [Tooltip("Panel displayed when game ends")]
    public GameObject gameOverPanel;

    [Tooltip("Text component for game over title")]
    public TextMeshProUGUI gameOverTitleText;

    [Tooltip("Text component for game over message")]
    public TextMeshProUGUI gameOverMessageText;

    [Tooltip("Button to restart the game")]
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

    private void Start()
    {
        gameStartTime = Time.time;
        SetupUI();
    }

    private void SetupUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void Update()
    {
        if (!gameEnded)
        {
            CheckWinConditions();
            CheckLoseConditions();
        }
    }

    private void CheckWinConditions()
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

    private void CheckLoseConditions()
    {
        if (ResourceManager.Instance == null)
        {
            return;
        }

        // Lose if run out of both resources
        int gold = ResourceManager.Instance.GetResource("gold");
        int wood = ResourceManager.Instance.GetResource("wood");

        if (gold <= 0 && wood <= 0)
        {
            TriggerLose("Defeat", "You ran out of resources and your empire collapsed.");
        }
    }

    private void TriggerWin(string title, string message)
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
            {
                gameOverTitleText.text = title;
            }
            
            if (gameOverMessageText != null)
            {
                gameOverMessageText.text = $"{message}\n\nTime: {gameTime:F1} seconds";
            }
        }

        // Disable player movement
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
        }
    }

    private void TriggerLose(string title, string message)
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
            {
                gameOverTitleText.text = title;
            }
            
            if (gameOverMessageText != null)
            {
                gameOverMessageText.text = $"{message}\n\nTime: {gameTime:F1} seconds";
            }
        }

        // Disable player movement
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
        }
    }

    private void RestartGame()
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
        {
            gameOverPanel.SetActive(false);
        }

        // Re-enable player
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = true;
        }

        GameLogger.Instance.RecordEvent("Game Restarted", 0, 0);
    }

    /// <summary>
    /// Gets the number of villages built by the player.
    /// </summary>
    /// <returns>Number of villages built</returns>
    public int GetVillagesBuilt()
    {
        return BuildingSystem.Instance != null ? BuildingSystem.Instance.GetBuildingCount("Village") : 0;
    }

    /// <summary>
    /// Gets the current game time elapsed since start.
    /// </summary>
    /// <returns>Game time in seconds</returns>
    public float GetGameTime()
    {
        return Time.time - gameStartTime;
    }
}