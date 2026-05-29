namespace CyberbotGUI.Models
{
    public class ChatMessage
    {
        public bool IsBot { get; set; }
        public string Text { get; set; } = "";
        public string Timestamp { get; set; } = "";
    }
}
