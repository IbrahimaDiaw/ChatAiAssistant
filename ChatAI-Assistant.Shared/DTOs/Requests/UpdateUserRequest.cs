using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Requests
{
    public class UpdateUserRequest
    {
        [StringLength(100)]
        public string? DisplayName { get; set; }

        [StringLength(200)]
        public string? Avatar { get; set; }
    }
}
