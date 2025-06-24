using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;

namespace ChatAI_Assistant.Server.Mapping.Extensions;

public static class SessionMappingExtensions
{
    #region Entity to DTO

    public static ChatSessionDto ToDto(this ChatSession session)
    {
        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            Description = session.Description,
            CreatedByUserId = session.CreatedByUserId,
            CreatedByUsername = session.CreatedBy?.Username ?? "",
            CreatedAt = session.CreatedAt,
            LastActivity = session.LastActivity,
            IsActive = session.IsActive,
            IsPrivate = session.IsPrivate,
            MessageCount = session.MessageCount,
            ParticipantCount = session.ParticipantCount,
            SessionAIProvider = session.SessionAIProvider,
            SessionAIModel = session.SessionAIModel,
            RecentMessages = session.Messages?
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Timestamp)
                .Take(5)
                .Select(m => m.ToDto())
                .ToList() ?? new List<ChatMessageDto>(),
            Participants = session.Participants?
                .Where(p => p.IsActive)
                .Select(p => p.ToDto())
                .ToList() ?? new List<SessionParticipantDto>()
        };
    }

    public static UserSessionSummaryDto ToSessionSummaryDto(this ChatSession session)
    {
        return new UserSessionSummaryDto
        {
            SessionId = session.Id,
            Title = session.Title,
            LastActivity = session.LastActivity,
            MessageCount = session.MessageCount,
            ParticipantCount = session.ParticipantCount,
            IsPrivate = session.IsPrivate,
            LastMessage = session.Messages?
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault()?.ToSummaryDto()
        };
    }

    public static SessionParticipantDto ToDto(this SessionParticipant participant)
    {
        return new SessionParticipantDto
        {
            Id = participant.Id,
            SessionId = participant.SessionId,
            UserId = participant.UserId,
            Username = participant.User?.Username ?? "",
            DisplayName = participant.User?.DisplayName,
            JoinedAt = participant.JoinedAt,
            LeftAt = participant.LeftAt,
            LastSeenAt = participant.LastSeenAt,
            IsActive = participant.IsActive,
            IsModerator = participant.IsModerator,
            Role = participant.Role,
            MessagesCount = participant.MessagesCount,
            IsOnline = DateTime.UtcNow.Subtract(participant.LastSeenAt).TotalMinutes < 5
        };
    }

    #endregion

    #region DTO to Entity

    public static ChatSession ToEntity(this CreateSessionRequest request)
    {
        return new ChatSession
        {
            Title = request.Title,
            Description = request.Description,
            CreatedByUserId = request.CreatedByUserId,
            IsPrivate = request.IsPrivate,
            SessionAIProvider = request.PreferredAIProvider,
            SessionAIModel = request.PreferredAIModel,
            SessionContext = request.InitialContext
        };
    }

    public static SessionParticipant ToEntity(this JoinSessionRequest request)
    {
        return new SessionParticipant
        {
            SessionId = request.SessionId,
            UserId = request.UserId,
            Role = "participant",
            IsActive = true
        };
    }

    public static void UpdateFromRequest(this ChatSession session, UpdateSessionRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Title))
            session.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Description))
            session.Description = request.Description;

        if (request.IsPrivate.HasValue)
            session.IsPrivate = request.IsPrivate.Value;

        if (request.SessionAIProvider.HasValue)
            session.SessionAIProvider = request.SessionAIProvider.Value;

        if (!string.IsNullOrWhiteSpace(request.SessionAIModel))
            session.SessionAIModel = request.SessionAIModel;
    }

    #endregion
}