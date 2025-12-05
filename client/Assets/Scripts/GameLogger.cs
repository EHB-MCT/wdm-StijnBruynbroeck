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

    private string PlayerIdentifier = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 4);

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

   
}
