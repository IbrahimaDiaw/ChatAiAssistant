using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Server.Mapping.Extensions;
using ChatAI_Assistant.Server.Repositories.Interfaces;
using ChatAI_Assistant.Server.Services.Users;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;
using Microsoft.Extensions.Options;

namespace ChatAI_Assistant.Server.Services.Sessions
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            ISessionRepository sessionRepository,
            IUserRepository userRepository,
            IChatRepository chatRepository,
            ILogger<SessionService> logger)
        {
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _logger = logger;
        }

        #region Session CRUD Operations

        public async Task<SessionResponse> CreateSessionAsync(CreateSessionRequest request)
        {
            try
            {
                _logger.LogDebug("Creating session: {Title} for user {UserId}", request.Title, request.CreatedByUserId);

                // Validation
                var validationResult = await ValidateCreateSessionRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    return SessionResponse.BadRequest(validationResult.ErrorMessage);
                }

                // Créer la session
                var session = request.ToEntity();
                session.Id = Guid.NewGuid();
                session.CreatedAt = DateTime.UtcNow;
                session.LastActivity = DateTime.UtcNow;
                session.IsActive = true;
                session.MessageCount = 0;
                session.ParticipantCount = 1; // Le créateur

                var createdSession = await _sessionRepository.CreateAsync(session);

                // Ajouter le créateur comme participant admin
                var creatorParticipant = new SessionParticipant
                {
                    Id = Guid.NewGuid(),
                    SessionId = createdSession.Id,
                    UserId = request.CreatedByUserId,
                    Role = "admin",
                    IsModerator = true,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };

                await _sessionRepository.AddParticipantAsync(creatorParticipant);

                // Mettre à jour les stats utilisateur
                await _userRepository.UpdateUserStatsAsync(request.CreatedByUserId);

                var sessionDto = createdSession.ToDto();

                _logger.LogInformation("Session created successfully: {SessionId} - {Title}",
                    createdSession.Id, createdSession.Title);

                return SessionResponse.Success(sessionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session for user {UserId}", request.CreatedByUserId);
                return SessionResponse.Error("Failed to create session");
            }
        }

        public async Task<SessionResponse> GetSessionByIdAsync(Guid sessionId)
        {
            try
            {
                var session = await _sessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return SessionResponse.NotFound($"Session with ID {sessionId} not found");
                }

                var sessionDto = session.ToDto();
                return SessionResponse.Success(sessionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
                return SessionResponse.Error("Failed to retrieve session");
            }
        }

        public async Task<ApiResponse<List<ChatSessionDto>>> GetUserSessionsAsync(Guid userId, int limit = 50)
        {
            try
            {
                var sessions = await _sessionRepository.GetUserSessionsAsync(userId, limit);
                var sessionDtos = sessions.Select(s => s.ToDto()).ToList();

                return ApiResponse<List<ChatSessionDto>>.CreateSuccess(sessionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for user {UserId}", userId);
                return ApiResponse<List<ChatSessionDto>>.CreateError("Failed to retrieve user sessions");
            }
        }

        public async Task<SessionResponse> UpdateSessionAsync(UpdateSessionRequest request)
        {
            try
            {
                var session = await _sessionRepository.GetByIdAsync(request.SessionId);
                if (session == null)
                {
                    return SessionResponse.NotFound($"Session with ID {request.SessionId} not found");
                }

                // Vérifier les permissions (seul le créateur ou un admin peut modifier)
                // Cette logique peut être étendue selon vos besoins

                // Mettre à jour les propriétés
                session.UpdateFromRequest(request);
                session.LastActivity = DateTime.UtcNow;

                var updatedSession = await _sessionRepository.UpdateAsync(session);
                var sessionDto = updatedSession.ToDto();

                _logger.LogInformation("Session updated: {SessionId}", request.SessionId);
                return SessionResponse.Success(sessionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId}", request.SessionId);
                return SessionResponse.Error("Failed to update session");
            }
        }

        public async Task<ApiResponse<bool>> DeleteSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var session = await _sessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return ApiResponse<bool>.CreateError("Session not found");
                }

                // Vérifier que l'utilisateur peut supprimer la session
                if (session.CreatedByUserId != userId)
                {
                    var isModerator = await _sessionRepository.IsUserModeratorAsync(sessionId, userId);
                    if (!isModerator)
                    {
                        return ApiResponse<bool>.CreateError("You don't have permission to delete this session");
                    }
                }

                await _sessionRepository.DeleteAsync(sessionId);

                _logger.LogInformation("Session deleted: {SessionId} by user {UserId}", sessionId, userId);
                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                return ApiResponse<bool>.CreateError("Failed to delete session");
            }
        }

        #endregion

        #region Participant Management

        public async Task<ApiResponse<SessionParticipantDto>> AddParticipantAsync(JoinSessionRequest request)
        {
            try
            {
                _logger.LogDebug("Adding participant {UserId} to session {SessionId}",
                    request.UserId, request.SessionId);

                // Vérifier que la session existe
                var session = await _sessionRepository.GetByIdAsync(request.SessionId);
                if (session == null)
                {
                    return ApiResponse<SessionParticipantDto>.CreateError("Session not found");
                }

                // Vérifier que l'utilisateur existe
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return ApiResponse<SessionParticipantDto>.CreateError("User not found");
                }

                // Vérifier si l'utilisateur est déjà participant
                var existingParticipant = await _sessionRepository.GetParticipantAsync(request.SessionId, request.UserId);
                if (existingParticipant != null)
                {
                    if (existingParticipant.IsActive)
                    {
                        return ApiResponse<SessionParticipantDto>.CreateError("User is already a participant in this session");
                    }
                    else
                    {
                        // Réactiver la participation
                        existingParticipant.IsActive = true;
                        existingParticipant.JoinedAt = DateTime.UtcNow;
                        existingParticipant.LastSeenAt = DateTime.UtcNow;
                        existingParticipant.LeftAt = null;

                        var reactivatedParticipant = await _sessionRepository.UpdateParticipantAsync(existingParticipant);
                        await _sessionRepository.UpdateSessionStatsAsync(request.SessionId);

                        return ApiResponse<SessionParticipantDto>.CreateSuccess(reactivatedParticipant.ToDto());
                    }
                }

                // Créer nouveau participant
                var participant = request.ToEntity();
                participant.Id = Guid.NewGuid();
                participant.JoinedAt = DateTime.UtcNow;
                participant.LastSeenAt = DateTime.UtcNow;
                participant.IsActive = true;

                var createdParticipant = await _sessionRepository.AddParticipantAsync(participant);

                // Mettre à jour les statistiques
                await _sessionRepository.UpdateSessionStatsAsync(request.SessionId);

                _logger.LogInformation("Participant added: {UserId} to session {SessionId}",
                    request.UserId, request.SessionId);

                return ApiResponse<SessionParticipantDto>.CreateSuccess(createdParticipant.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant {UserId} to session {SessionId}",
                    request.UserId, request.SessionId);
                return ApiResponse<SessionParticipantDto>.CreateError("Failed to add participant");
            }
        }

        public async Task<ApiResponse<SessionParticipantDto>> GetParticipantAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var participant = await _sessionRepository.GetParticipantAsync(sessionId, userId);
                if (participant == null)
                {
                    return ApiResponse<SessionParticipantDto>.CreateError("Participant not found");
                }

                var participantDto = participant.ToDto();
                return ApiResponse<SessionParticipantDto>.CreateSuccess(participantDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participant {UserId} from session {SessionId}",
                    userId, sessionId);
                return ApiResponse<SessionParticipantDto>.CreateError("Failed to retrieve participant");
            }
        }

        public async Task<ApiResponse<List<SessionParticipantDto>>> GetSessionParticipantsAsync(Guid sessionId)
        {
            try
            {
                var participants = await _sessionRepository.GetSessionParticipantsAsync(sessionId);
                var participantDtos = participants.Select(p => p.ToDto()).ToList();

                return ApiResponse<List<SessionParticipantDto>>.CreateSuccess(participantDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participants for session {SessionId}", sessionId);
                return ApiResponse<List<SessionParticipantDto>>.CreateError("Failed to retrieve participants");
            }
        }

        public async Task<ApiResponse<bool>> RemoveParticipantAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var participant = await _sessionRepository.GetParticipantAsync(sessionId, userId);
                if (participant == null)
                {
                    return ApiResponse<bool>.CreateError("Participant not found");
                }

                await _sessionRepository.RemoveParticipantAsync(sessionId, userId);
                await _sessionRepository.UpdateSessionStatsAsync(sessionId);

                _logger.LogInformation("Participant removed: {UserId} from session {SessionId}", userId, sessionId);
                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing participant {UserId} from session {SessionId}",
                    userId, sessionId);
                return ApiResponse<bool>.CreateError("Failed to remove participant");
            }
        }

        public async Task<ApiResponse<bool>> UpdateParticipantRoleAsync(Guid sessionId, Guid userId, string role)
        {
            try
            {
                var participant = await _sessionRepository.GetParticipantAsync(sessionId, userId);
                if (participant == null)
                {
                    return ApiResponse<bool>.CreateError("Participant not found");
                }

                // Valider le rôle
                var validRoles = new[] { "participant", "moderator", "admin" };
                if (!validRoles.Contains(role.ToLower()))
                {
                    return ApiResponse<bool>.CreateError("Invalid role specified");
                }

                participant.Role = role.ToLower();
                participant.IsModerator = role.ToLower() is "moderator" or "admin";

                await _sessionRepository.UpdateParticipantAsync(participant);

                _logger.LogInformation("Participant role updated: {UserId} in session {SessionId} to {Role}",
                    userId, sessionId, role);

                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating participant role for {UserId} in session {SessionId}",
                    userId, sessionId);
                return ApiResponse<bool>.CreateError("Failed to update participant role");
            }
        }

        #endregion

        #region Session Activity

        public async Task<ApiResponse<bool>> UpdateLastActivityAsync(Guid sessionId)
        {
            try
            {
                await _sessionRepository.UpdateLastActivityAsync(sessionId, DateTime.UtcNow);
                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last activity for session {SessionId}", sessionId);
                return ApiResponse<bool>.CreateError("Failed to update session activity");
            }
        }

        public async Task<ApiResponse<bool>> UpdateParticipantActivityAsync(Guid sessionId, Guid userId)
        {
            try
            {
                await _sessionRepository.UpdateParticipantLastSeenAsync(sessionId, userId, DateTime.UtcNow);
                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating participant activity for {UserId} in session {SessionId}",
                    userId, sessionId);
                return ApiResponse<bool>.CreateError("Failed to update participant activity");
            }
        }

        #endregion

        #region Session Statistics

        public async Task<ApiResponse<SessionStatsDto>> GetSessionStatsAsync(Guid sessionId)
        {
            try
            {
                var session = await _sessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return ApiResponse<SessionStatsDto>.CreateError("Session not found");
                }

                var messageCount = await _sessionRepository.GetSessionMessageCountAsync(sessionId);
                var activeParticipants = await _sessionRepository.GetActiveParticipantCountAsync(sessionId);

                var stats = new SessionStatsDto
                {
                    SessionId = sessionId,
                    MessageCount = messageCount,
                    ParticipantCount = activeParticipants,
                    CreatedAt = session.CreatedAt,
                    LastActivity = session.LastActivity,
                    IsActive = session.IsActive
                };

                return ApiResponse<SessionStatsDto>.CreateSuccess(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats for session {SessionId}", sessionId);
                return ApiResponse<SessionStatsDto>.CreateError("Failed to retrieve session stats");
            }
        }

        public async Task<ApiResponse<bool>> UpdateSessionStatsAsync(Guid sessionId)
        {
            try
            {
                await _sessionRepository.UpdateSessionStatsAsync(sessionId);
                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stats for session {SessionId}", sessionId);
                return ApiResponse<bool>.CreateError("Failed to update session stats");
            }
        }

        #endregion

        #region Session Search and Discovery

        public async Task<ApiResponse<List<ChatSessionDto>>> SearchSessionsAsync(string searchTerm, int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ApiResponse<List<ChatSessionDto>>.CreateError("Search term cannot be empty");
                }

                var sessions = await _sessionRepository.SearchByTitleAsync(searchTerm, limit);
                var sessionDtos = sessions.Select(s => s.ToDto()).ToList();

                return ApiResponse<List<ChatSessionDto>>.CreateSuccess(sessionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching sessions with term: {SearchTerm}", searchTerm);
                return ApiResponse<List<ChatSessionDto>>.CreateError("Failed to search sessions");
            }
        }

        public async Task<ApiResponse<List<ChatSessionDto>>> GetPublicSessionsAsync(int limit = 50)
        {
            try
            {
                var sessions = await _sessionRepository.GetPublicSessionsAsync(limit);
                var sessionDtos = sessions.Select(s => s.ToDto()).ToList();

                return ApiResponse<List<ChatSessionDto>>.CreateSuccess(sessionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public sessions");
                return ApiResponse<List<ChatSessionDto>>.CreateError("Failed to retrieve public sessions");
            }
        }

        public async Task<ApiResponse<List<ChatSessionDto>>> GetRecentSessionsAsync(Guid userId, int limit = 20)
        {
            try
            {
                var sessions = await _sessionRepository.GetUserSessionsAsync(userId, limit);
                var recentSessions = sessions
                    .OrderByDescending(s => s.LastActivity)
                    .Take(limit)
                    .Select(s => s.ToDto())
                    .ToList();

                return ApiResponse<List<ChatSessionDto>>.CreateSuccess(recentSessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent sessions for user {UserId}", userId);
                return ApiResponse<List<ChatSessionDto>>.CreateError("Failed to retrieve recent sessions");
            }
        }

        #endregion

        #region Validation and Access Control

        public async Task<ApiResponse<bool>> ValidateSessionAccessAsync(Guid sessionId, Guid userId)
        {
            try
            {
                // Vérifier que la session existe
                var sessionExists = await _sessionRepository.ExistsAsync(sessionId);
                if (!sessionExists)
                {
                    return ApiResponse<bool>.CreateError("Session not found");
                }

                // Vérifier que l'utilisateur est participant
                var isParticipant = await _sessionRepository.IsUserParticipantAsync(sessionId, userId);
                if (!isParticipant)
                {
                    return ApiResponse<bool>.CreateError("User is not a participant in this session");
                }

                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session access for user {UserId} in session {SessionId}",
                    userId, sessionId);
                return ApiResponse<bool>.CreateError("Failed to validate session access");
            }
        }

        public async Task<ApiResponse<bool>> CanUserJoinSessionAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var session = await _sessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return ApiResponse<bool>.CreateError("Session not found");
                }

                if (!session.IsActive)
                {
                    return ApiResponse<bool>.CreateError("Session is not active");
                }

                // Vérifier si l'utilisateur est déjà participant
                var isAlreadyParticipant = await _sessionRepository.IsUserParticipantAsync(sessionId, userId);
                if (isAlreadyParticipant)
                {
                    return ApiResponse<bool>.CreateError("User is already a participant");
                }

                // Autres règles de validation peuvent être ajoutées ici
                // (ex: session privée, limite de participants, etc.)

                return ApiResponse<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can join session {SessionId}",
                    userId, sessionId);
                return ApiResponse<bool>.CreateError("Failed to check join permission");
            }
        }

        public async Task<ApiResponse<bool>> IsUserSessionModeratorAsync(Guid sessionId, Guid userId)
        {
            try
            {
                var isModerator = await _sessionRepository.IsUserModeratorAsync(sessionId, userId);
                return ApiResponse<bool>.CreateSuccess(isModerator);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking moderator status for user {UserId} in session {SessionId}",
                    userId, sessionId);
                return ApiResponse<bool>.CreateError("Failed to check moderator status");
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<ValidationResult> ValidateCreateSessionRequestAsync(CreateSessionRequest request)
        {
            // Vérifier que l'utilisateur existe
            var user = await _userRepository.GetByIdAsync(request.CreatedByUserId);
            if (user == null)
            {
                return ValidationResult.Invalid($"User with ID {request.CreatedByUserId} not found");
            }

            // Vérifier que le titre n'est pas vide
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return ValidationResult.Invalid("Session title cannot be empty");
            }

            // Vérifier la longueur du titre
            if (request.Title.Length > 200)
            {
                return ValidationResult.Invalid("Session title is too long (max 200 characters)");
            }

            // Vérifier la longueur de la description
            if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 500)
            {
                return ValidationResult.Invalid("Session description is too long (max 500 characters)");
            }

            return ValidationResult.Valid();
        }

        #endregion
    }


}
