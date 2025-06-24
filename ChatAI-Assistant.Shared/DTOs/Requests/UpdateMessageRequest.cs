using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class UpdateMessageRequest
    {
        [Required]
        public Guid MessageId { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}
