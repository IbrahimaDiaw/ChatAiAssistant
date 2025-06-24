using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Shared.DTOs.AI;

public class AIResponseDto
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public AIErrorCode? ErrorCode { get; set; }

    // Timing and performance metrics
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public int TokensUsed { get; set; } = 0;
    public decimal? EstimatedCost { get; set; }

    // AI model information
    public string Model { get; set; } = string.Empty;
    public AIProvider Provider { get; set; } = AIProvider.OpenAI;
    public double? Confidence { get; set; }

    // Context and conversation management
    public Guid? ConversationId { get; set; }
    public List<AIMessageContext> UpdatedContext { get; set; } = new();
    public bool ShouldUpdateContext { get; set; } = true;

    // Response metadata
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Follow-up suggestions
    public List<string> SuggestedResponses { get; set; } = new();

    // Content analysis
    public AIContentAnalysis? ContentAnalysis { get; set; }

    // Response type classification
    public AIResponseType ResponseType { get; set; } = AIResponseType.Standard;

    // Convert to ChatMessageDto
    public ChatMessageDto ToChatMessageDto()
    {
        return new ChatMessageDto
        {
            Id = MessageId,
            SessionId = SessionId,
            UserId = Guid.Empty, // AI user
            Content = Content,
            Timestamp = GeneratedAt,
            Type = MessageType.AI,
            IsFromAI = true,
        };
    }
}