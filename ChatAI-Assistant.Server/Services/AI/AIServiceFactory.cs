using ChatAI_Assistant.Server.Configurations;
using ChatAI_Assistant.Server.Services.Users;
using ChatAI_Assistant.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http;

namespace ChatAI_Assistant.Server.Services.AI
{
    public class AIServiceFactory : IAIServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AISettings _aiConfig;
        private readonly IUserService _userService;
        private readonly ILogger<AIServiceFactory> _logger;

        // Cache des services pour éviter les créations répétées
        private readonly ConcurrentDictionary<AIProvider, IAIService> _serviceCache = new();

        public AIServiceFactory(
            IServiceProvider serviceProvider,
            IOptions<AISettings> aiConfig,
            IUserService userService,
            ILogger<AIServiceFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _aiConfig = aiConfig.Value;
            _userService = userService;
            _logger = logger;
        }

        public IAIService GetService(AIProvider providerType)
        {
            return _serviceCache.GetOrAdd(providerType, CreateService);
        }

        public IAIService GetDefaultService()
        {
            return GetService(_aiConfig.DefaultProvider);
        }

        public async Task<IAIService> GetServiceForUserAsync(Guid userId)
        {
            try
            {
                var preferences = await _userService.GetUserPreferencesAsync(userId);
                return GetService(preferences.PreferredAIProvider);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user preferences for {UserId}, using default provider", userId);
                return GetDefaultService();
            }
        }

        public IAIService GetServiceForUser(Guid userId)
        {
            // Version synchrone pour compatibilité - utilise le default
            // Dans un vrai scénario, vous pourriez avoir un cache des préférences
            return GetDefaultService();
        }

        public IEnumerable<AIProvider> GetAvailableProviders()
        {
            var availableProviders = new List<AIProvider>();

            foreach (AIProvider provider in Enum.GetValues<AIProvider>())
            {
                if (IsProviderConfigured(provider))
                {
                    availableProviders.Add(provider);
                }
            }

            return availableProviders;
        }

        public async Task<bool> IsProviderHealthyAsync(AIProvider providerType)
        {
            try
            {
                var service = GetService(providerType);
                return await service.IsHealthyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for provider {Provider}", providerType);
                return false;
            }
        }

        private IAIService CreateService(AIProvider providerType)
        {
            try
            {
                return providerType switch
                {
                    AIProvider.OpenAI => CreateOpenAIService(),
                    AIProvider.AzureOpenAI => CreateAzureOpenAIService(),
                    AIProvider.Anthropic => CreateClaudeService(),
                    //AIProvider.Mock => CreateMockService(),
                    _ => throw new NotSupportedException($"AI Provider {providerType} is not supported")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create service for provider {Provider}", providerType);

                // Fallback sur le service Mock en cas d'erreur
                return CreateMockService();
            }
        }

        private IAIService CreateOpenAIService()
        {
            var config = _aiConfig.Providers.OpenAI;

            if (!config.Enabled || string.IsNullOrEmpty(config.ApiKey))
            {
                throw new InvalidOperationException("OpenAI service is not properly configured");
            }

            var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("OpenAI");
            var logger = _serviceProvider.GetRequiredService<ILogger<OpenAIService>>();

            return new OpenAIService(httpClient, config, logger);
        }

        private IAIService CreateAzureOpenAIService()
        {
            var config = _aiConfig.Providers.AzureOpenAI;

            if (!config.Enabled || string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.Endpoint))
            {
                throw new InvalidOperationException("Azure OpenAI service is not properly configured");
            }

            var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("AzureOpenAI");
            var logger = _serviceProvider.GetRequiredService<ILogger<AzureOpenAIService>>();

            return new AzureOpenAIService(httpClient, config, logger);
        }

        private IAIService CreateMockService()
        {
            //var logger = _serviceProvider.GetRequiredService<ILogger<SimpleBotAIService>>();
            //return new SimpleBotAIService(logger);
            throw new NotImplementedException("Mock service is not implemented yet");
        }

        private IAIService CreateClaudeService()
        {
            var config = _aiConfig.Providers.Claude;

            if (!config.Enabled || string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.BaseUrl))
            {
                throw new InvalidOperationException("Azure OpenAI service is not properly configured");
            }

            var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("AzureOpenAI");
            var logger = _serviceProvider.GetRequiredService<ILogger<ClaudeService>>();

            return new ClaudeService(httpClient, config, logger);
        }

        private bool IsProviderConfigured(AIProvider providerType)
        {
            try
            {
                return providerType switch
                {
                    AIProvider.OpenAI => IsOpenAIConfigured(),
                    AIProvider.AzureOpenAI => IsAzureOpenAIConfigured(),
                    AIProvider.Mock => true, // Mock est toujours disponible
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private bool IsOpenAIConfigured()
        {
            var config = _aiConfig.Providers.OpenAI;
            return config.Enabled && !string.IsNullOrEmpty(config.ApiKey);
        }

        private bool IsAzureOpenAIConfigured()
        {
            var config = _aiConfig.Providers.AzureOpenAI;
            return config.Enabled &&
                   !string.IsNullOrEmpty(config.ApiKey) &&
                   !string.IsNullOrEmpty(config.Endpoint) &&
                   !string.IsNullOrEmpty(config.DeploymentName);
        }
    }

}
