using ChatAI_Assistant.Shared.DTOs.AI;

namespace ChatAI_Assistant.Shared.DTOs;

public class ConversationContextDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public string ContextData { get; set; } = string.Empty;

    public int MessageCount { get; set; } = 0;

    public DateTime? ExpiresAt { get; set; }

    // Audit fields from BaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Parsed context data
    public List<AIMessageContext> Messages { get; set; } = new();
    public Dictionary<string, object> UserProfile { get; set; } = new();
    public Dictionary<string, string> SessionVariables { get; set; } = new();

    // Context management
    public int MaxMessages { get; set; } = 10;
    public int CurrentTokenCount { get; set; } = 0;
    public int MaxTokens { get; set; } = 4000;

    // Statistics
    public TimeSpan ConversationDuration => (UpdatedAt ?? DateTime.UtcNow) - CreatedAt;
    public List<string> ConversationTopics { get; set; } = new();
}