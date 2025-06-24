using ChatAI_Assistant.Server.Configurations;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Responses;
using ChatAI_Assistant.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace ChatAI_Assistant.Server.Services.AI
{
    public class AzureOpenAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly AzureOpenAISettings _config;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AIProvider ProviderType => AIProvider.AzureOpenAI;

        public AzureOpenAIService(HttpClient httpClient, AzureOpenAISettings config, ILogger<AzureOpenAIService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_config.Endpoint);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ChatAI-POC/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<AIResponse> GenerateResponseAsync(string message, string? context = null)
        {
            var settings = new AzureOpenAISettings
            {
                Model = _config.Model,
                Temperature = _config.Temperature,
                MaxTokens = _config.MaxTokens,
                SystemPrompt = _config.SystemPrompt
            };

            return await GenerateResponseWithSettingsAsync(message, settings, context);
        }

        public async Task<AIResponse> GenerateResponseWithSettingsAsync(string message, AzureOpenAISettings settings, string? context = null)
        {
            try
            {
                var messages = BuildMessages(message, context, settings.SystemPrompt);
                var requestPayload = CreateRequestPayload(messages, settings);

                _logger.LogDebug("Sending request to Azure OpenAI with deployment {DeploymentName}", _config.DeploymentName);

                var response = await SendRequestWithRetryAsync(requestPayload);

                if (response == null)
                {
                    return CreateErrorResponse("Failed to get response from Azure OpenAI");
                }

                return CreateSuccessResponse(response, settings);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Azure OpenAI");
                return CreateErrorResponse($"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling Azure OpenAI");
                return CreateErrorResponse("Request timeout - Azure OpenAI took too long to respond");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Azure OpenAI");
                return CreateErrorResponse($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var testResponse = await GenerateResponseAsync("Hello");
                return testResponse.Success;
            }
            catch
            {
                return false;
            }
        }

        private List<AzureOpenAIMessage> BuildMessages(string message, string? context, string? systemPrompt)
        {
            var messages = new List<AzureOpenAIMessage>();

            // Ajouter le prompt système
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                messages.Add(new AzureOpenAIMessage { Role = "system", Content = systemPrompt });
            }

            // Ajouter le contexte de conversation si disponible
            if (!string.IsNullOrWhiteSpace(context))
            {
                var contextLines = context.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in contextLines.TakeLast(5)) // Limiter le contexte
                {
                    if (line.Contains(": "))
                    {
                        var parts = line.Split(": ", 2);
                        var role = parts[0].Contains("🤖") ? "assistant" : "user";
                        messages.Add(new AzureOpenAIMessage { Role = role, Content = parts[1] });
                    }
                }
            }

            // Ajouter le message actuel
            messages.Add(new AzureOpenAIMessage { Role = "user", Content = message });

            return messages;
        }

        private object CreateRequestPayload(List<AzureOpenAIMessage> messages, AzureOpenAISettings settings)
        {
            return new
            {
                messages = messages,
                max_tokens = settings.MaxTokens,
                temperature = settings.Temperature,
                top_p = 1.0,
                frequency_penalty = 0.0,
                presence_penalty = 0.0,
                stream = false
            };
        }

        private async Task<AzureOpenAIApiResponse?> SendRequestWithRetryAsync(object requestPayload)
        {
            var json = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // URL spécifique à Azure OpenAI
            var url = $"/openai/deployments/{_config.DeploymentName}/chat/completions?api-version={_config.ApiVersion}";

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonSerializer.Deserialize<AzureOpenAIApiResponse>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        return apiResponse;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Azure OpenAI returned {StatusCode}: {Error}", response.StatusCode, errorContent);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < 3)
                        {
                            await Task.Delay(1000 * attempt); // Exponential backoff
                            continue;
                        }

                        throw new HttpRequestException($"Azure OpenAI error: {response.StatusCode}");
                    }
                }
                catch (Exception ex) when (attempt < 3)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying...", attempt);
                    await Task.Delay(1000 * attempt);
                }
            }

            return null;
        }

        private AIResponse CreateSuccessResponse(AzureOpenAIApiResponse response, AzureOpenAISettings settings)
        {
            var choice = response.Choices?.FirstOrDefault();
            var content = choice?.Message?.Content ?? "No response generated";
            var tokensUsed = response.Usage?.TotalTokens ?? 0;

            return new AIResponse
            {
                Content = content.Trim(),
                Provider = ProviderType,
                Model = settings.Model ?? _config.Model,
                TokensUsed = tokensUsed,
                Temperature = settings.Temperature,
                GeneratedAt = DateTime.UtcNow,
                Success = true
            };
        }

        private AIResponse CreateErrorResponse(string errorMessage)
        {
            return new AIResponse
            {
                Content = "Je rencontre actuellement des difficultés techniques. Veuillez réessayer dans quelques instants.",
                Provider = ProviderType,
                Model = _config.Model,
                Success = false,
                ErrorMessage = errorMessage,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
