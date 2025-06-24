using ChatAI_Assistant.Shared.Enums;
using System.Text.Json;

namespace ChatAI_Assistant.Server.Configurations
{
    public class AISettings
    {
        public AIProvider DefaultProvider { get; set; } = AIProvider.OpenAI;
        public ProvidersSettings Providers { get; set; } = new();
        public int TimeoutSeconds { get; set; } = 30;
        public bool EnableContextMemory { get; set; } = true;
        public int MaxContextMessages { get; set; } = 10;
        public int RetryAttempts { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
    }
    public class ProvidersSettings
    {
        public OpenAISettings OpenAI { get; set; } = new();
        public AzureOpenAISettings AzureOpenAI { get; set; } = new();
        public MockAISettings Mock { get; set; } = new();
        public ClaudeSettings Claude { get; set; } = new();
    }

}
