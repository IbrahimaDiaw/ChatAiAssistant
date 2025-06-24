using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }
        public bool IsFromAI { get; set; }
        public Guid? ParentMessageId { get; set; }
        public AIProvider? AIProvider { get; set; }
        public string? AIModel { get; set; }
        public int? TokensUsed { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserDisplayName { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}
