using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses.Claude
{
    public class ClaudeContent
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
