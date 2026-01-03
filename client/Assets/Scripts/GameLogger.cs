using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

/// <summary>
/// Comprehensive behavioral tracking system for player actions and decisions.
/// Implements singleton pattern with early initialization to ensure availability.
/// Source: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
/// </summary>
[DefaultExecutionOrder(-1000)] // Ensures GameLogger initializes before other scripts
public class GameLogger : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static GameLogger Instance;

    /// <summary>
    /// API endpoint for sending game data.
    /// </summary>
    private const string API_URL = "http://localhost:8080/api/log";

    /// <summary>
    /// Collection of logged game actions.
    /// </summary>
    private readonly List<GameActionData> logs = new List<GameActionData>();

    /// <summary>
    /// Unique identifier for the current player session.
    /// </summary>
    private string playerIdentifier;

    [System.Serializable]
    public class ApiRequestData
    {
        public string uid;
        public string type;
        public ActionData action_data;
    }

    [System.Serializable]
    public class ActionData
    {
        public float timeInGame;
        public int hexX;
        public int hexY;
        public string details;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
            
        // Initialize UID here (moved from Start to ensure availability before other scripts)
        if (PlayerPrefs.HasKey("PlayerUID"))
        {
            playerIdentifier = PlayerPrefs.GetString("PlayerUID");
            Debug.Log($"Loaded existing Player UID: {playerIdentifier}");
        }
        else
        {
            playerIdentifier = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            PlayerPrefs.SetString("PlayerUID", playerIdentifier);
            PlayerPrefs.Save();
            Debug.Log($"Created new Player UID: {playerIdentifier}");
        }
    }

    private void Start()
    {
        // Only record session start, UID already initialized in Awake
        RecordSessionStart();
    }

    /// <summary>
    /// Gets the unique player identifier.
    /// </summary>
    /// <returns>Player UID string</returns>
    public string GetPlayerUID()
    {
        return playerIdentifier;
    }

    private void SendDataToAPI(GameActionData data)
    {
        // Defensive check for UID
        if (string.IsNullOrEmpty(playerIdentifier))
        {
            Debug.LogError("playerIdentifier is null or empty - cannot send data");
            Debug.LogError("This indicates GameLogger.Awake() hasn't run yet or failed");
            
            // Emergency UID generation as fallback
            playerIdentifier = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            PlayerPrefs.SetString("PlayerUID", playerIdentifier);
            PlayerPrefs.Save();
            Debug.LogWarning($"Generated emergency UID: {playerIdentifier}");
        }
        
        var requestData = new ApiRequestData {
            uid = playerIdentifier,
            type = data.ActionType,
            action_data = new ActionData {
                timeInGame = data.TimeInGame,
                hexX = data.HexX,
                hexY = data.HexY,
                details = data.Details
            }
        };
        
        string json = JsonUtility.ToJson(requestData);
        Debug.Log($"Sending JSON: {json}");
        StartCoroutine(PostRequest(API_URL, json));
    }

    private IEnumerator PostRequest(string url, string json)
    {
        var webRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"API Error: {webRequest.error}");
            Debug.LogError($"Response Code: {webRequest.responseCode}");
            Debug.LogError($"Response: {webRequest.downloadHandler.text}");
        }
        else
        {
            Debug.Log("Action successful");
        }
    }

    public void RecordMove(int x, int y)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "Move",
            TimeInGame = Time.time,
            HexX = x,
            HexY = y,
            Details = $"Moved to ({x},{y})"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordEvent(string eventType, int x, int y)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "Encounter",
            TimeInGame = Time.time,
            HexX = x,
            HexY = y,
            Details = eventType
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordSessionStart()
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "SessionStart",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = "Game session started"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    // Comprehensive behavioral tracking methods
    public void RecordDecisionTiming(string decisionType, float timeTaken, string context)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "DecisionTiming",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{decisionType}:{timeTaken:F2}:{context}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordResourceManagement(string action, string resourceType, int amount, int currentTotal)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "ResourceManagement",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{action}:{resourceType}:{amount}:{currentTotal}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordThreatResponse(string threatType, string response, bool paidOff, int cost)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "ThreatResponse",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{threatType}:{response}:{paidOff}:{cost}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordQuestDecision(string questType, bool accepted, string decisionFactor)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "QuestDecision",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{questType}:{accepted}:{decisionFactor}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordEmotionalResponse(string trigger, string emotionType, float responseSpeed)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "EmotionalResponse",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{trigger}:{emotionType}:{responseSpeed:F2}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordMouseTracking(float mouseX, float mouseY, float hoverDuration, string targetElement)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "MouseTracking",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{mouseX:F1}:{mouseY:F1}:{hoverDuration:F2}:{targetElement}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordStrategicChoice(string choiceType, string choice, float deliberationTime)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "StrategicChoice",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{choiceType}:{choice}:{deliberationTime:F2}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    public void RecordEngagementMetrics(string metricType, float value, string additionalData)
    {
        var data = new GameActionData
        {
            PlayerId = playerIdentifier,
            ActionType = "EngagementMetric",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{metricType}:{value}:{additionalData}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

    void Update()
    {
        // Track mouse movement periodically (not every frame to avoid spam)
        if (Time.frameCount % 30 == 0) // Every 30 frames (~0.5 seconds at 60fps)
        {
            Vector3 mousePos = Input.mousePosition;
            string hoverTarget = GetCurrentHoverTarget();
            float hoverDuration = CalculateHoverDuration();
            
            if (!string.IsNullOrEmpty(hoverTarget) && hoverDuration > 0.5f)
            {
                RecordMouseTracking(mousePos.x, mousePos.y, hoverDuration, hoverTarget);
            }
        }
    }

    private string GetCurrentHoverTarget()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        
        // Check what UI element or game object the mouse is over
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        if (hit.collider != null)
        {
            return hit.collider.gameObject.name;
        }
        
        return "EmptySpace";
    }

    private Vector3 lastMousePosition;
    private float hoverStartTime;
    private string lastHoverTarget;

    private float CalculateHoverDuration()
    {
        Vector3 currentMousePos = Input.mousePosition;
        string currentHoverTarget = GetCurrentHoverTarget();
        
        if (currentMousePos == lastMousePosition && currentHoverTarget == lastHoverTarget)
        {
            return Time.time - hoverStartTime;
        }
        else
        {
            lastMousePosition = currentMousePos;
            lastHoverTarget = currentHoverTarget;
            hoverStartTime = Time.time;
            return 0f;
        }
    }
}