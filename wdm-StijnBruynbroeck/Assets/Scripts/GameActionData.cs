using System;

[Serializable]
public class GameActionData
{
    public string PlayerId; 
    public string ActionType; 
    public float TimeInGame; 
    public int HexX; 
    public int HexY; 
    public string Details; 
}