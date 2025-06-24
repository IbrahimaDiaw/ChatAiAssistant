using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class OpenAIApiResponse
    {
        public List<OpenAIChoice>? Choices { get; set; }
        public OpenAIUsage? Usage { get; set; }
    }
}
