namespace ChatAI_Assistant.Server.Configurations
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public string Model { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public string SystemPrompt { get; set; } = "You are a helpful AI assistant.";
        public bool Enabled { get; set; } = true;
    }
}
