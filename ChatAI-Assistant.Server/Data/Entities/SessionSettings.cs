using ChatAI_Assistant.Server.Commons;

namespace ChatAI_Assistant.Server.Data.Entities
{
    public class SessionSettings : BaseEntity
    {
        public Guid SessionId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation Properties
        public ChatSession Session { get; set; } = null!;
    }
}
