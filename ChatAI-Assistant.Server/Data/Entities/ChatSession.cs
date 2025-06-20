using ChatAI_Assistant.Server.Commons;

namespace ChatAI_Assistant.Server.Data.Entities
{
    public class ChatSession : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public int MaxParticipants { get; set; } = 10;
        public string CreatedBy { get; set; } = string.Empty;

        // Computed Properties (not mapped to DB)
        public int ActiveParticipantCount => Participants.Count(p => p.IsActive);
        public int TotalMessageCount => Messages.Count(m => !m.IsDeleted);
        public DateTime? LastMessageAt => Messages
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefault()?.Timestamp;

        // Navigation Properties
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
        public ICollection<SessionSettings> Settings { get; set; } = new List<SessionSettings>();
    }
}
