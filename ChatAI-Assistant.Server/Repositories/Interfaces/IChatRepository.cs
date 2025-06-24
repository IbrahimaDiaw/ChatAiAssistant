using ChatAI_Assistant.Server.Data.Entities;

namespace ChatAI_Assistant.Server.Repositories.Interfaces
{
    public interface IChatRepository
    {
        // Messages
        Task<ChatMessage> CreateMessageAsync(ChatMessage message);
        Task<ChatMessage?> GetMessageByIdAsync(Guid messageId);
        Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(Guid sessionId, int limit = 50, DateTime? before = null);
        Task<IEnumerable<ChatMessage>> GetConversationContextAsync(Guid sessionId, int maxMessages = 10);
        Task<ChatMessage> UpdateMessageAsync(ChatMessage message);
        Task SoftDeleteMessageAsync(Guid messageId);

        // Message statistics
        Task<int> GetSessionMessageCountAsync(Guid sessionId);
        Task<ChatMessage?> GetLastMessageAsync(Guid sessionId);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(Guid sessionId, int count = 5);

        // Bulk operations
        Task<IEnumerable<ChatMessage>> GetMessagesPagedAsync(Guid sessionId, int page, int pageSize);
        Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId, DateTime readTimestamp);
    }
}
