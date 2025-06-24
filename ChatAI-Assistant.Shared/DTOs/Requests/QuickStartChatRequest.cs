using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class QuickStartChatRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Username { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? InitialMessage { get; set; }

        public AIProvider? PreferredAIProvider { get; set; } = AIProvider.OpenAI;
    }
}
