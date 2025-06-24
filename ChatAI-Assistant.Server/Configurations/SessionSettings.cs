namespace ChatAI_Assistant.Server.Configurations
{
    public class SessionSettings
    {
        public int InactiveSessionTimeoutMinutes { get; set; } = 30;
        public int OldSessionThresholdDays { get; set; } = 90;
        public int MaxParticipantsPerSession { get; set; } = 100;
        public int MaxSessionsPerUser { get; set; } = 50;
    }
}
