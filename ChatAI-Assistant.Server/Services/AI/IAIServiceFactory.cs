using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Server.Services.AI
{
    public interface IAIServiceFactory
    {
        IAIService GetService(AIProvider providerType);
        IAIService GetDefaultService();
        IAIService GetServiceForUser(Guid userId);
        IEnumerable<AIProvider> GetAvailableProviders();
        Task<bool> IsProviderHealthyAsync(AIProvider providerType);
    }
}
