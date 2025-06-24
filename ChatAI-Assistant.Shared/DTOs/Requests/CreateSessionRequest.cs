using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class CreateSessionRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        public bool IsPrivate { get; set; } = false;

        public AIProvider? PreferredAIProvider { get; set; }

        [StringLength(50)]
        public string? PreferredAIModel { get; set; }

        [StringLength(1000)]
        public string? InitialContext { get; set; }
    }
}
