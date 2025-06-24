using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class LoginRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Username { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;
    }
}
