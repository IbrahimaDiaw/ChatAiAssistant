using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class SendMessageRequest
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public AIProvider? PreferredAIProvider { get; set; }
    }
}
