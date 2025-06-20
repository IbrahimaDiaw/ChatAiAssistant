using ChatAI_Assistant.Server.Commons;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Server.Data.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public MessageType Type { get; set; } = MessageType.User;
        public bool IsFromAI { get; set; } = false;
        public string? ConversationContext { get; set; }
        public Guid? ParentMessageId { get; set; }

        // Navigation Properties
        public ChatSession Session { get; set; } = null!;
        public ChatMessage? ParentMessage { get; set; }
        public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
    }
}
