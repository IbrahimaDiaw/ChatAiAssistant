using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;

namespace ChatAI_Assistant.Client.Services
{
    public interface IChatClientService
    {
        Task<QuickChatResponse> QuickStartChatAsync(QuickStartChatRequest request);
        Task<ChatResponse> ContinueChatAsync(ContinueChatRequest request);
        Task<ChatResponse> SendMessageAsync(SendMessageRequest request);
        Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(Guid sessionId, int limit = 50, DateTime? before = null, int page = 1);
        Task<ApiResponse<ConversationContextDto>> GetConversationContextAsync(Guid sessionId, int maxMessages = 10);
        Task<ApiResponse<List<MessageSummaryDto>>> GetRecentMessagesAsync(Guid sessionId, int count = 5);
        Task<ApiResponse<MessageSummaryDto>> GetLastMessageAsync(Guid sessionId);
        Task<ApiResponse<bool>> DeleteMessageAsync(Guid messageId, Guid userId);
    }
}
