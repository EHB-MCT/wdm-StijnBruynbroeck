using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; 
using System.Text; 
using System.Collections;
public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance;

    private const string API_URL = "http://localhost:8080/api/log";


    private List<GameActionData> logs = new List<GameActionData>();

    private string PlayerIdentifier;
    
    void Start()
    {
        // Try to load existing UID from PlayerPrefs
        if (PlayerPrefs.HasKey("PlayerUID"))
        {
            PlayerIdentifier = PlayerPrefs.GetString("PlayerUID");
            Debug.Log($"Loaded existing Player UID: {PlayerIdentifier}");
        }
        else
        {
            PlayerIdentifier = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            PlayerPrefs.SetString("PlayerUID", PlayerIdentifier);
            PlayerPrefs.Save();
            Debug.Log($"Created new Player UID: {PlayerIdentifier}");
        }
        
        // Record session start
        RecordSessionStart();
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    private float CalculateHoverDuration()
    {
        // Simple hover duration calculation
        // In a real implementation, you'd track position over time
        return Random.Range(0.1f, 2.0f); // Placeholder for actual hover calculation
    }

    private void SendDataToAPI(GameActionData data)
    {
        var requestData = new {
            uid = PlayerIdentifier,
            type = data.ActionType,
            data = new {
                timeInGame = data.TimeInGame,
                hexX = data.HexX,
                hexY = data.HexY,
                details = data.Details
            }
        };
        
        string json = JsonUtility.ToJson(requestData);
        
       
        
      
        if (json.Contains("{}") || json.Length < 20) {
            json = $"{{\"uid\":\"{PlayerIdentifier}\",\"type\":\"{data.ActionType}\",\"data\":{{\"timeInGame\":{data.TimeInGame},\"hexX\":{data.HexX},\"hexY\":{data.HexY},\"details\":\"{data.Details}\"}}}}";
           
        }
        
        StartCoroutine(PostRequest(API_URL, json));
    }

    private IEnumerator PostRequest(string url, string json)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

           
            yield return webRequest.SendWebRequest();

         

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"API Error: {webRequest.error}. Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.Log("Action successful");
            }
        }
    }

    public void RecordMove(int x, int y)
    {
        var data = new GameActionData
        {
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
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
            PlayerId = PlayerIdentifier,
            ActionType = "EngagementMetric",
            TimeInGame = Time.time,
            HexX = -1,
            HexY = -1,
            Details = $"{metricType}:{value}:{additionalData}"
        };
        logs.Add(data);
        SendDataToAPI(data);
    }

   
}
