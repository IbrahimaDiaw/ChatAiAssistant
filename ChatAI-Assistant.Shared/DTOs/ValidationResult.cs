using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static ValidationResult Valid() => new() { IsValid = true };
        public static ValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}
