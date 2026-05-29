using System.Collections.Generic;

namespace CyberbotGUI.Models
{
    public class Intent
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public List<string> Keywords { get; set; } = new();
        public List<string> Responses { get; set; } = new();
        public string DetailResponse { get; set; } = "";
    }
}
