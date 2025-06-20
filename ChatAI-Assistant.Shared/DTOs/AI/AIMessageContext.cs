using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Shared.DTOs.AI;

public class AIMessageContext
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public string Role { get; set; } = string.Empty; // "user", "assistant", "system"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; } = MessageType.User;
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }

    // Importance scoring for context pruning
    public double ImportanceScore { get; set; } = 1.0;

    // Emotional context
    public string? Sentiment { get; set; }
    public Dictionary<string, double> EmotionScores { get; set; } = new();

    // Topic classification
    public List<string> Topics { get; set; } = new();
    public string? Intent { get; set; }

    // Reference to entities mentioned
    public List<string> Entities { get; set; } = new();

    // Token count for context management
    public int TokenCount { get; set; } = 0;

    // Create from ChatMessageDto
    public static AIMessageContext FromChatMessage(ChatMessageDto message)
    {
        return new AIMessageContext
        {
            MessageId = message.Id,
            Role = message.IsFromAI ? "assistant" : "user",
            Content = message.Content,
            Timestamp = message.Timestamp,
            Type = message.Type,
            UserId = message.UserId,
            SessionId = message.SessionId,
            TokenCount = EstimateTokenCount(message.Content)
        };
    }

    private static int EstimateTokenCount(string content)
    {
        // Simple token estimation: ~4 characters per token
        return Math.Max(1, content.Length / 4);
    }
}