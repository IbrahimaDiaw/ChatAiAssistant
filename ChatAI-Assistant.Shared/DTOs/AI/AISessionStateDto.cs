namespace ChatAI_Assistant.Shared.DTOs.AI;

public class AISessionStateDto
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public bool IsAIEnabled { get; set; } = true;
    public bool IsProcessing { get; set; } = false;

    // AI preferences for this session
    public AIProcessingOptions ProcessingOptions { get; set; } = new();
    public string? CustomSystemPrompt { get; set; }
    public string PreferredModel { get; set; } = "gpt-3.5-turbo";
    public AIProvider PreferredProvider { get; set; } = AIProvider.OpenAI;

    // Session statistics
    public int TotalAIMessages { get; set; } = 0;
    public int TotalTokensUsed { get; set; } = 0;
    public decimal TotalCost { get; set; } = 0.0m;
    public TimeSpan TotalProcessingTime { get; set; } = TimeSpan.Zero;

    // Rate limiting
    public int MessagesInCurrentHour { get; set; } = 0;
    public int TokensInCurrentHour { get; set; } = 0;
    public DateTime LastResetTime { get; set; } = DateTime.UtcNow;

    // Error tracking
    public int ConsecutiveErrors { get; set; } = 0;
    public DateTime? LastErrorTime { get; set; }
    public List<string> RecentErrors { get; set; } = new();

    // Feature flags
    public Dictionary<string, bool> FeatureFlags { get; set; } = new()
    {
        ["EnableContextMemory"] = true,
        ["EnableContentFiltering"] = true,
        ["EnableSentimentAnalysis"] = false,
        ["EnableSuggestedResponses"] = true
    };

    // Updated timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Rate limiting checks
    public bool CanSendMessage()
    {
        ResetCountersIfNeeded();
        return MessagesInCurrentHour < 100; // Max messages per hour
    }

    public bool CanUseTokens(int requestedTokens)
    {
        ResetCountersIfNeeded();
        return (TokensInCurrentHour + requestedTokens) < 10000; // Max tokens per hour
    }

    private void ResetCountersIfNeeded()
    {
        if (DateTime.UtcNow - LastResetTime > TimeSpan.FromHours(1))
        {
            MessagesInCurrentHour = 0;
            TokensInCurrentHour = 0;
            LastResetTime = DateTime.UtcNow;
        }
    }
}