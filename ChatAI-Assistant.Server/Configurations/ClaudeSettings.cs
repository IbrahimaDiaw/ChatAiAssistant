namespace ChatAI_Assistant.Server.Configurations
{
    public class ClaudeSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.anthropic.com/v1/";
        public string Model { get; set; } = "claude-3-sonnet-20240229";
        public int MaxTokens { get; set; } = 4000;
        public double Temperature { get; set; } = 0.7;
        public string? SystemPrompt { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
