using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs
{
    public class UserPreferencesDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AIProvider PreferredAIProvider { get; set; } = AIProvider.OpenAI;
        public string? PreferredModel { get; set; }
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1000;
        public string? SystemPrompt { get; set; }
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "fr";
        public bool EnableNotifications { get; set; } = true;
        public bool EnableSoundEffects { get; set; } = true;
        public bool ShowTypingIndicator { get; set; } = true;
        public DateTime UpdatedAt { get; set; }
    }
}
