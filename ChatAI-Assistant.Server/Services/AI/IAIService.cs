using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Server.Services.AI
{
    public interface IAIService
    {
        AIProvider ProviderType { get; }
        Task<AIResponse> GenerateResponseAsync(string message, string? context = null);
        Task<bool> IsHealthyAsync();
    }
}
