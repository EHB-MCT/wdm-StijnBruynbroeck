using System.Collections.Generic;
using UnityEngine;

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance;
    private List<string> logs = new List<string>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RecordMove(int x, int y)
    {
        string entry = $"Move to ({x},{y}) at {Time.time:F1}s";
        logs.Add(entry);
        Debug.Log(entry);
    }

    public void RecordEvent(string eventType)
    {
        string entry = $"Encounter: {eventType} at {Time.time:F1}s";
        logs.Add(entry);
        Debug.Log(entry);
    }

    public void PrintSummary()
    {
        Debug.Log("=== GAME SUMMARY ===");
        Debug.Log($"Total actions: {logs.Count}");
        foreach (string log in logs)
            Debug.Log(log);
    }
}
