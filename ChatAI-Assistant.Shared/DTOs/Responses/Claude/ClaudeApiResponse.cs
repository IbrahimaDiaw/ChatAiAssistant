using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses.Claude
{
    public class ClaudeApiResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<ClaudeContent>? Content { get; set; }
        public string Model { get; set; } = string.Empty;
        public string? StopReason { get; set; }
        public string? StopSequence { get; set; }
        public ClaudeUsage? Usage { get; set; }
    }
}
