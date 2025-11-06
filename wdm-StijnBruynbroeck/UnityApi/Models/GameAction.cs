namespace UnityApi.Models
{
    public class GameAction
    {
        public int Id { get; set; } 
        public string PlayerId { get; set; }
        public string ActionType { get; set; }
        public float TimeInGame { get; set; }
        public int HexX { get; set; }
        public int HexY { get; set; }
        public string Details { get; set; }
    }
}