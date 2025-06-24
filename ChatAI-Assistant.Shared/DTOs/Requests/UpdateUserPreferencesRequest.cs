using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class UpdateUserPreferencesRequest
    {
        public AIProvider PreferredAIProvider { get; set; } = AIProvider.OpenAI;

        [StringLength(50)]
        public string? PreferredModel { get; set; }

        [Range(0.0, 2.0)]
        public double Temperature { get; set; } = 0.7;

        [Range(1, 4000)]
        public int MaxTokens { get; set; } = 1000;

        [StringLength(1000)]
        public string? SystemPrompt { get; set; }

        [StringLength(20)]
        public string Theme { get; set; } = "light";

        [StringLength(10)]
        public string Language { get; set; } = "fr";

        public bool EnableNotifications { get; set; } = true;
        public bool EnableSoundEffects { get; set; } = true;
        public bool ShowTypingIndicator { get; set; } = true;
    }
}
