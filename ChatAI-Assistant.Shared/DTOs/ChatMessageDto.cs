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
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(4000, ErrorMessage = "Message cannot exceed 4000 characters")]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public MessageType Type { get; set; } = MessageType.User;

        public bool IsFromAI { get; set; } = false;

        public string? ConversationContext { get; set; }

        public Guid? ParentMessageId { get; set; }

        // Audit fields from BaseEntity
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Additional display properties
        public string? Username { get; set; }
        public string? UserAvatar { get; set; }
        public List<ChatMessageDto> Replies { get; set; } = new();
    }
}
