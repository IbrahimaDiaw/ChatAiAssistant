using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatAI_Assistant.Client.Services;
public class ChatClientService : IChatClientService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChatClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<QuickChatResponse> QuickStartChatAsync(QuickStartChatRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/chat/quick-start", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<QuickChatResponse>(content, _jsonOptions);
                return result!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new QuickChatResponse
            {
                Success = false,
                Message = $"API Error: {response.StatusCode}",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new QuickChatResponse
            {
                Success = false,
                Message = $"Network error: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<ChatResponse> ContinueChatAsync(ContinueChatRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/chat/continue", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChatResponse>(content, _jsonOptions);
                return result!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ChatResponse
            {
                Succeeded = false,
                Message = $"API Error: {response.StatusCode}",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new ChatResponse
            {
                Succeeded = false,
                Message = $"Network error: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<ChatResponse> SendMessageAsync(SendMessageRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/chat/messages", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChatResponse>(content, _jsonOptions);
                return result!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ChatResponse
            {
                Succeeded = false,
                Message = $"API Error: {response.StatusCode}",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new ChatResponse
            {
                Succeeded = false,
                Message = $"Network error: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(Guid sessionId, int limit = 50, DateTime? before = null, int page = 1)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"limit={limit}",
                $"page={page}"
            };

            if (before.HasValue)
            {
                queryParams.Add($"before={before.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"api/chat/sessions/{sessionId}/messages?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<ChatMessageDto>>>(content, _jsonOptions);
                return result!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return ApiResponse<List<ChatMessageDto>>.CreateError($"API Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ChatMessageDto>>.CreateError($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ConversationContextDto>> GetConversationContextAsync(Guid sessionId, int maxMessages = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/chat/sessions/{sessionId}/context?maxMessages={maxMessages}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<ConversationContextDto>>(content, _jsonOptions);
                return result!;
            }

            return ApiResponse<ConversationContextDto>.CreateError($"API Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<ConversationContextDto>.CreateError($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<MessageSummaryDto>>> GetRecentMessagesAsync(Guid sessionId, int count = 5)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/chat/sessions/{sessionId}/recent-messages?count={count}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<MessageSummaryDto>>>(content, _jsonOptions);
                return result!;
            }

            return ApiResponse<List<MessageSummaryDto>>.CreateError($"API Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MessageSummaryDto>>.CreateError($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<MessageSummaryDto>> GetLastMessageAsync(Guid sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/chat/sessions/{sessionId}/last-message");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<MessageSummaryDto>>(content, _jsonOptions);
                return result!;
            }

            return ApiResponse<MessageSummaryDto>.CreateError($"API Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<MessageSummaryDto>.CreateError($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteMessageAsync(Guid messageId, Guid userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/chat/messages/{messageId}?userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);
                return result!;
            }

            return ApiResponse<bool>.CreateError($"API Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.CreateError($"Network error: {ex.Message}");
        }
    }
}