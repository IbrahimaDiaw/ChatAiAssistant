using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class AzureOpenAIApiResponse
    {
        public List<AzureOpenAIChoice>? Choices { get; set; }
        public AzureOpenAIUsage? Usage { get; set; }
    }
    public class AzureOpenAIChoice
    {
        public AzureOpenAIMessage? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    public class AzureOpenAIUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
