using ChatAI_Assistant.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class QuickChatResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public ChatSessionDto Session { get; set; } = null!;
        public ChatMessageDto? BotResponse { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
