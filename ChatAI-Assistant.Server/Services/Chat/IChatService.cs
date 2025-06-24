using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Server.Services.Chat
{
    public interface IChatService
    {
        // Message operations
        Task<ChatResponse> SendMessageAsync(SendMessageRequest request);
        Task<ChatResponse> GetMessageAsync(Guid messageId);
        Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(GetMessagesRequest request);
        Task<ChatResponse> UpdateMessageAsync(UpdateMessageRequest request);
        Task<ApiResponse<bool>> DeleteMessageAsync(DeleteMessageRequest request);

        // Bot interactions
        Task<ChatResponse> SendMessageToBotAsync(Guid sessionId, Guid userId, string message, AIProvider? preferredProvider = null);
        Task<ApiResponse<ConversationContextDto>> GetConversationContextAsync(Guid sessionId, int maxMessages = 10);

        // Real-time operations
        Task NotifyMessageSentAsync(Guid sessionId, ChatMessageDto message);
        Task NotifyTypingAsync(Guid sessionId, Guid userId, bool isTyping);
        Task NotifyUserJoinedAsync(Guid sessionId, UserDto user);
        Task NotifyUserLeftAsync(Guid sessionId, UserDto user);

        // Utility operations
        Task<ApiResponse<bool>> MarkMessagesAsReadAsync(Guid sessionId, Guid userId);
        Task<ApiResponse<MessageSummaryDto>> GetLastMessageAsync(Guid sessionId);
        Task<ApiResponse<List<MessageSummaryDto>>> GetRecentMessagesAsync(Guid sessionId, int count = 5);
    }
}
