using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;

namespace ChatAI_Assistant.Server.Mapping.Extensions;

public static class UserMappingExtensions
{
    #region Entity to DTO

    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            LastActivity = user.LastActivity,
            IsActive = user.IsActive,
            TotalMessages = user.TotalMessages,
            TotalSessions = user.TotalSessions
        };
    }

    public static UserPreferencesDto ToDto(this UserPreferences preferences)
    {
        return new UserPreferencesDto
        {
            Id = preferences.Id,
            UserId = preferences.UserId,
            PreferredAIProvider = preferences.PreferredAIProvider,
            PreferredModel = preferences.PreferredModel,
            Temperature = preferences.Temperature,
            MaxTokens = preferences.MaxTokens,
            SystemPrompt = preferences.SystemPrompt,
            Theme = preferences.Theme,
            Language = preferences.Language,
            EnableNotifications = preferences.EnableNotifications,
            EnableSoundEffects = preferences.EnableSoundEffects,
            ShowTypingIndicator = preferences.ShowTypingIndicator,
            UpdatedAt = preferences.UpdatedAt
        };
    }

    public static LoginDto ToLoginDto(this User user)
    {
        return new LoginDto
        {
            UserId = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            LoginTimestamp = DateTime.UtcNow
        };
    }

    public static UserStatsDto ToStatsDto(this User user)
    {
        return new UserStatsDto
        {
            UserId = user.Id,
            TotalMessages = user.TotalMessages,
            TotalSessions = user.TotalSessions,
            LastActivity = user.LastActivity
        };
    }

    #endregion

    #region DTO to Entity

    public static User ToEntity(this CreateUserRequest request)
    {
        return new User
        {
            Username = request.Username,
            DisplayName = request.DisplayName ?? request.Username,
            Avatar = request.Avatar,
            IsActive = true
        };
    }

    public static void UpdateFromDto(this User user, UpdateUserRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.DisplayName = request.DisplayName;

        if (!string.IsNullOrWhiteSpace(request.Avatar))
            user.Avatar = request.Avatar;
    }

    public static UserPreferences ToEntity(this UpdateUserPreferencesRequest request, Guid userId)
    {
        return new UserPreferences
        {
            UserId = userId,
            PreferredAIProvider = request.PreferredAIProvider,
            PreferredModel = request.PreferredModel,
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            SystemPrompt = request.SystemPrompt,
            Theme = request.Theme,
            Language = request.Language,
            EnableNotifications = request.EnableNotifications,
            EnableSoundEffects = request.EnableSoundEffects,
            ShowTypingIndicator = request.ShowTypingIndicator
        };
    }

    #endregion
}