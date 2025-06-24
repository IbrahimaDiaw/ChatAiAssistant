using ChatAI_Assistant.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class UpdateSessionRequest
    {
        [Required]
        public Guid SessionId { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsPrivate { get; set; }

        public AIProvider? SessionAIProvider { get; set; }

        [StringLength(50)]
        public string? SessionAIModel { get; set; }
    }
}
