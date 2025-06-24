namespace ChatAI_Assistant.Server.Configurations
{
    public class MockAISettings
    {
        public string Model { get; set; } = "simple-bot-v1";
        public int MaxTokens { get; set; } = 150;
        public double Temperature { get; set; } = 0.7;
        public bool Enabled { get; set; } = true;
    }
}
