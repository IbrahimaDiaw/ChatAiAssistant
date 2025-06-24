using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;

namespace ChatAI_Assistant.Server.Services.Sessions
{
    public interface ISessionService
    {
        // Session CRUD operations
        Task<SessionResponse> CreateSessionAsync(CreateSessionRequest request);
        Task<SessionResponse> GetSessionByIdAsync(Guid sessionId);
        Task<ApiResponse<List<ChatSessionDto>>> GetUserSessionsAsync(Guid userId, int limit = 50);
        Task<SessionResponse> UpdateSessionAsync(UpdateSessionRequest request);
        Task<ApiResponse<bool>> DeleteSessionAsync(Guid sessionId, Guid userId);

        // Participant management
        Task<ApiResponse<SessionParticipantDto>> AddParticipantAsync(JoinSessionRequest request);
        Task<ApiResponse<SessionParticipantDto>> GetParticipantAsync(Guid sessionId, Guid userId);
        Task<ApiResponse<List<SessionParticipantDto>>> GetSessionParticipantsAsync(Guid sessionId);
        Task<ApiResponse<bool>> RemoveParticipantAsync(Guid sessionId, Guid userId);
        Task<ApiResponse<bool>> UpdateParticipantRoleAsync(Guid sessionId, Guid userId, string role);

        // Session activity
        Task<ApiResponse<bool>> UpdateLastActivityAsync(Guid sessionId);
        Task<ApiResponse<bool>> UpdateParticipantActivityAsync(Guid sessionId, Guid userId);

        // Session statistics
        Task<ApiResponse<SessionStatsDto>> GetSessionStatsAsync(Guid sessionId);
        Task<ApiResponse<bool>> UpdateSessionStatsAsync(Guid sessionId);

        // Session search and discovery
        Task<ApiResponse<List<ChatSessionDto>>> SearchSessionsAsync(string searchTerm, int limit = 20);
        Task<ApiResponse<List<ChatSessionDto>>> GetPublicSessionsAsync(int limit = 50);
        Task<ApiResponse<List<ChatSessionDto>>> GetRecentSessionsAsync(Guid userId, int limit = 20);

        // Validation and access control
        Task<ApiResponse<bool>> ValidateSessionAccessAsync(Guid sessionId, Guid userId);
        Task<ApiResponse<bool>> CanUserJoinSessionAsync(Guid sessionId, Guid userId);
        Task<ApiResponse<bool>> IsUserSessionModeratorAsync(Guid sessionId, Guid userId);
    }

}
