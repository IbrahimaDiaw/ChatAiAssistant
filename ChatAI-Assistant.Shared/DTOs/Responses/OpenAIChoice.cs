using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class OpenAIChoice
    {
        public OpenAIMessage? Message { get; set; }
        public string? FinishReason { get; set; }
    }
}
