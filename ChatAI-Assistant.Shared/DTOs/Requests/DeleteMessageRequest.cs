using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class DeleteMessageRequest
    {
        [Required]
        public Guid MessageId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

}
