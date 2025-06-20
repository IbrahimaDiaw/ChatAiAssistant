namespace ChatAI_Assistant.Shared.DTOs.AI;

public class AIProcessingOptions
{
    // Model parameters
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 150;
    public double TopP { get; set; } = 1.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double PresencePenalty { get; set; } = 0.0;

    // Response preferences
    public string? SystemPrompt { get; set; }
    public List<string> StopSequences { get; set; } = new();

    // Context management
    public int MaxContextMessages { get; set; } = 10;
    public bool EnableContextPruning { get; set; } = true;

    // Content filtering
    public bool EnableContentFiltering { get; set; } = true;
    public bool EnableSafetyCheck { get; set; } = true;

    // Performance settings
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 3;
}