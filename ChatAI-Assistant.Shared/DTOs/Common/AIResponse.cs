using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Common
{
    public class AIResponse
    {
        public string Content { get; set; } = string.Empty;
        public AIProvider Provider { get; set; }
        public string Model { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public double Temperature { get; set; }
        public string? Context { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; } = null;
    }
}
