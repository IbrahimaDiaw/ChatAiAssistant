using ChatAI_Assistant.Server.Configurations;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Responses.Claude;
using ChatAI_Assistant.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace ChatAI_Assistant.Server.Services.AI
{
    public class ClaudeService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ClaudeSettings _config;
        private readonly ILogger<ClaudeService> _logger;

        public AIProvider ProviderType => AIProvider.Anthropic;

        public ClaudeService(HttpClient httpClient, ClaudeSettings config, ILogger<ClaudeService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ChatAI-POC/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<AIResponse> GenerateResponseAsync(string message, string? context = null)
        {
            var settings = new ClaudeSettings
            {
                Model = _config.Model,
                Temperature = _config.Temperature,
                MaxTokens = _config.MaxTokens,
                SystemPrompt = _config.SystemPrompt
            };

            return await GenerateResponseWithSettingsAsync(message, settings, context);
        }

        public async Task<AIResponse> GenerateResponseWithSettingsAsync(string message, ClaudeSettings settings, string? context = null)
        {
            try
            {
                var messages = BuildMessages(message, context);
                var requestPayload = CreateRequestPayload(messages, settings);

                _logger.LogDebug("Sending request to Claude API with {MessageCount} messages", messages.Count);

                var response = await SendRequestWithRetryAsync(requestPayload);

                if (response == null)
                {
                    return CreateErrorResponse("Failed to get response from Claude API");
                }

                return CreateSuccessResponse(response, settings);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Claude API");
                return CreateErrorResponse($"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling Claude API");
                return CreateErrorResponse("Request timeout - Claude API took too long to respond");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Claude API");
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

        private List<ClaudeMessage> BuildMessages(string message, string? context)
        {
            var messages = new List<ClaudeMessage>();

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
                        messages.Add(new ClaudeMessage { Role = role, Content = parts[1] });
                    }
                }
            }

            // Ajouter le message actuel
            messages.Add(new ClaudeMessage { Role = "user", Content = message });

            return messages;
        }

        private object CreateRequestPayload(List<ClaudeMessage> messages, ClaudeSettings settings)
        {
            var payload = new
            {
                model = settings.Model ?? _config.Model,
                max_tokens = settings.MaxTokens,
                temperature = settings.Temperature,
                messages = messages,
                stream = false
            };

            // Ajouter le system prompt si disponible
            if (!string.IsNullOrWhiteSpace(settings.SystemPrompt))
            {
                return new
                {
                    model = settings.Model ?? _config.Model,
                    max_tokens = settings.MaxTokens,
                    temperature = settings.Temperature,
                    system = settings.SystemPrompt,
                    messages = messages,
                    stream = false
                };
            }

            return payload;
        }

        private async Task<ClaudeApiResponse?> SendRequestWithRetryAsync(object requestPayload)
        {
            var json = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var response = await _httpClient.PostAsync("messages", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        _logger.LogDebug("Claude API Response: {Response}", responseContent);

                        var apiResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        return apiResponse;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Claude API returned {StatusCode}: {Error}", response.StatusCode, errorContent);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < 3)
                        {
                            // Claude utilise un header Retry-After
                            var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? (1000 * attempt);
                            await Task.Delay(TimeSpan.FromSeconds(retryAfter));
                            continue;
                        }

                        throw new HttpRequestException($"Claude API error: {response.StatusCode} - {errorContent}");
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

        private AIResponse CreateSuccessResponse(ClaudeApiResponse response, ClaudeSettings settings)
        {
            var content = response.Content?.FirstOrDefault();
            var textContent = content?.Text ?? "No response generated";
            var tokensUsed = response.Usage?.OutputTokens + response.Usage?.InputTokens ?? 0;

            return new AIResponse
            {
                Content = textContent.Trim(),
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
