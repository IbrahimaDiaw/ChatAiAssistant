namespace ChatAI_Assistant.Server.Data.Entities
{
    public class SessionParticipant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        public bool CanSendMessages { get; set; } = true;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ChatSession Session { get; set; } = null!;

        // Computed Properties
        public TimeSpan SessionDuration => (LeftAt ?? DateTime.UtcNow) - JoinedAt;
        public bool IsOnline => IsActive && (DateTime.UtcNow - LastSeen).TotalMinutes < 5;
    }
}
