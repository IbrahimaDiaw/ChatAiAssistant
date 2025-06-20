using System.ComponentModel.DataAnnotations;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Shared.DTOs.AI;

public class AIRequestDto
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(4000, ErrorMessage = "Message cannot exceed 4000 characters")]
    public string Message { get; set; } = string.Empty;

    [Required]
    public Guid MessageId { get; set; } = Guid.NewGuid();

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Context for AI processing
    public List<AIMessageContext> ConversationHistory { get; set; } = new();

    public Dictionary<string, string> UserMetadata { get; set; } = new();

    // AI processing preferences
    public AIProcessingOptions ProcessingOptions { get; set; } = new();

    // Optional parent message for threaded conversations
    public Guid? ParentMessageId { get; set; }

    // Session context
    public string? SessionContext { get; set; }

    // User preferences
    public string? PreferredLanguage { get; set; }
    public string? UserTimezone { get; set; }
}