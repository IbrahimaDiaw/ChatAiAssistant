using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class GetMessagesRequest
    {
        [Required]
        public Guid SessionId { get; set; }

        [Range(1, 100)]
        public int Limit { get; set; } = 50;

        public DateTime? Before { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
    }
}
