using ChatAI_Assistant.Server.Commons;

namespace ChatAI_Assistant.Server.Data.Entities
{
    public class ConversationContext : BaseEntity
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string ContextData { get; set; } = string.Empty; // JSON serialized context
        public int MessageCount { get; set; } = 0;
        public DateTime? ExpiresAt { get; set; }

        // Navigation Properties
        public ChatSession Session { get; set; } = null!;
    }
}
