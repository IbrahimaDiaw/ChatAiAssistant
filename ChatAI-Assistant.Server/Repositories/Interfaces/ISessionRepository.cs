using ChatAI_Assistant.Server.Data.Entities;

namespace ChatAI_Assistant.Server.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        // Session operations
        Task<ChatSession> CreateAsync(ChatSession session);
        Task<ChatSession?> GetByIdAsync(Guid sessionId);
        Task<IEnumerable<ChatSession>> GetUserSessionsAsync(Guid userId, int limit = 50);
        Task<IEnumerable<ChatSession>> GetPublicSessionsAsync(int limit = 50);
        Task<IEnumerable<ChatSession>> SearchByTitleAsync(string searchTerm, int limit = 20);
        Task<ChatSession> UpdateAsync(ChatSession session);
        Task DeleteAsync(Guid sessionId);

        // Participant operations
        Task<SessionParticipant> AddParticipantAsync(SessionParticipant participant);
        Task<SessionParticipant?> GetParticipantAsync(Guid sessionId, Guid userId);
        Task<IEnumerable<SessionParticipant>> GetSessionParticipantsAsync(Guid sessionId);
        Task<SessionParticipant> UpdateParticipantAsync(SessionParticipant participant);
        Task RemoveParticipantAsync(Guid sessionId, Guid userId);

        // Activity tracking
        Task UpdateLastActivityAsync(Guid sessionId, DateTime? timestamp = null);
        Task UpdateParticipantLastSeenAsync(Guid sessionId, Guid userId, DateTime? timestamp = null);

        // Statistics
        Task<int> GetSessionMessageCountAsync(Guid sessionId);
        Task<int> GetActiveParticipantCountAsync(Guid sessionId);
        Task UpdateSessionStatsAsync(Guid sessionId);

        // Validation
        Task<bool> ExistsAsync(Guid sessionId);
        Task<bool> IsUserParticipantAsync(Guid sessionId, Guid userId);
        Task<bool> IsUserModeratorAsync(Guid sessionId, Guid userId);
    }
}
