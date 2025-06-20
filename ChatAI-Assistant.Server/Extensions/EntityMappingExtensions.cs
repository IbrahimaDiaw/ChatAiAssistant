using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.AI;

namespace ChatAI_Assistant.Server.Extensions;

public static class EntityMappingExtensions
{
    // ChatMessage Entity -> DTO
    public static ChatMessageDto ToDto(this ChatMessage entity)
    {
        return new ChatMessageDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            UserId = entity.UserId,
            Content = entity.Content,
            Timestamp = entity.Timestamp,
            Type = entity.Type,
            IsFromAI = entity.IsFromAI,
            ConversationContext = entity.ConversationContext,
            ParentMessageId = entity.ParentMessageId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            Replies = entity.Replies?.Select(r => r.ToDto()).ToList() ?? new()
        };
    }

    // ChatMessage DTO -> Entity
    public static ChatMessage ToEntity(this ChatMessageDto dto)
    {
        return new ChatMessage
        {
            Id = dto.Id,
            SessionId = dto.SessionId,
            UserId = dto.UserId,
            Content = dto.Content,
            Timestamp = dto.Timestamp,
            Type = dto.Type,
            IsFromAI = dto.IsFromAI,
            ConversationContext = dto.ConversationContext,
            ParentMessageId = dto.ParentMessageId,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            IsDeleted = dto.IsDeleted
        };
    }

    // ChatSession Entity -> DTO
    public static ChatSessionDto ToDto(this ChatSession entity)
    {
        return new ChatSessionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            LastActivity = entity.LastActivity,
            IsActive = entity.IsActive,
            MaxParticipants = entity.MaxParticipants,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            ActiveParticipantCount = entity.ActiveParticipantCount,
            TotalMessageCount = entity.TotalMessageCount,
            LastMessageAt = entity.LastMessageAt,
            Participants = entity.Participants?.Select(p => p.ToDto()).ToList() ?? new(),
            Settings = entity.Settings?.ToDictionary(s => s.Key, s => s.Value) ?? new()
        };
    }

    // ChatSession DTO -> Entity
    public static ChatSession ToEntity(this ChatSessionDto dto)
    {
        return new ChatSession
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            LastActivity = dto.LastActivity,
            IsActive = dto.IsActive,
            MaxParticipants = dto.MaxParticipants,
            CreatedBy = dto.CreatedBy,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            IsDeleted = dto.IsDeleted
        };
    }

    // SessionParticipant Entity -> DTO
    public static SessionParticipantDto ToDto(this SessionParticipant entity)
    {
        return new SessionParticipantDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            UserId = entity.UserId,
            Username = entity.Username,
            DisplayName = entity.DisplayName,
            Avatar = entity.Avatar,
            JoinedAt = entity.JoinedAt,
            LeftAt = entity.LeftAt,
            IsActive = entity.IsActive,
            IsAdmin = entity.IsAdmin,
            CanSendMessages = entity.CanSendMessages,
            LastSeen = entity.LastSeen,
            SessionDuration = entity.SessionDuration,
            IsOnline = entity.IsOnline
        };
    }

    // SessionParticipant DTO -> Entity
    public static SessionParticipant ToEntity(this SessionParticipantDto dto)
    {
        return new SessionParticipant
        {
            Id = dto.Id,
            SessionId = dto.SessionId,
            UserId = dto.UserId,
            Username = dto.Username,
            DisplayName = dto.DisplayName,
            Avatar = dto.Avatar,
            JoinedAt = dto.JoinedAt,
            LeftAt = dto.LeftAt,
            IsActive = dto.IsActive,
            IsAdmin = dto.IsAdmin,
            CanSendMessages = dto.CanSendMessages,
            LastSeen = dto.LastSeen
        };
    }

    // ConversationContext Entity -> DTO
    public static ConversationContextDto ToDto(this ConversationContext entity)
    {
        var messages = new List<AIMessageContext>();

        if (!string.IsNullOrEmpty(entity.ContextData))
        {
            try
            {
                messages = System.Text.Json.JsonSerializer.Deserialize<List<AIMessageContext>>(entity.ContextData) ?? new();
            }
            catch
            {
                messages = new List<AIMessageContext>();
            }
        }

        return new ConversationContextDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            UserId = entity.UserId,
            ContextData = entity.ContextData,
            MessageCount = entity.MessageCount,
            ExpiresAt = entity.ExpiresAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            Messages = messages,
            CurrentTokenCount = messages.Sum(m => m.TokenCount)
        };
    }

    // ConversationContext DTO -> Entity
    public static ConversationContext ToEntity(this ConversationContextDto dto)
    {
        var contextData = dto.Messages.Any()
            ? System.Text.Json.JsonSerializer.Serialize(dto.Messages)
            : dto.ContextData;

        return new ConversationContext
        {
            Id = dto.Id,
            SessionId = dto.SessionId,
            UserId = dto.UserId,
            ContextData = contextData,
            MessageCount = dto.MessageCount,
            ExpiresAt = dto.ExpiresAt,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt ?? DateTime.UtcNow,
            IsDeleted = dto.IsDeleted
        };
    }

    // SessionSettings Entity -> DTO
    public static SessionSettingsDto ToDto(this SessionSettings entity)
    {
        return new SessionSettingsDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            Key = entity.Key,
            Value = entity.Value,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted
        };
    }

    // SessionSettings DTO -> Entity
    public static SessionSettings ToEntity(this SessionSettingsDto dto)
    {
        return new SessionSettings
        {
            Id = dto.Id,
            SessionId = dto.SessionId,
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            IsDeleted = dto.IsDeleted
        };
    }
}