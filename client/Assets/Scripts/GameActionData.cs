using System;

/// <summary>
/// Represents a game action event with player behavioral data.
/// Based on Unity best practices for data structures and Microsoft coding conventions.
/// Source: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
/// </summary>
[System.Serializable]
public class GameActionData
{
    /// <summary>
    /// Unique identifier for the player performing the action.
    /// </summary>
    public string PlayerId;

    /// <summary>
    /// Type of action being performed (e.g., "Move", "Build", "ThreatResponse").
    /// </summary>
    public string ActionType;

    /// <summary>
    /// Time elapsed in the game when this action occurred.
    /// </summary>
    public float TimeInGame;

    /// <summary>
    /// X coordinate of the hex grid where the action occurred.
    /// </summary>
    public int HexX;

    /// <summary>
    /// Y coordinate of the hex grid where the action occurred.
    /// </summary>
    public int HexY;

    /// <summary>
    /// Additional details about the action in a serialized format.
    /// </summary>
    public string Details;
}