using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Client.Models;

public class UserSession
{
    public string? SessionId { get; set; }
    public UserDto? User { get; set; }
    public ChatSessionDto? ActiveSession { get; set; }
    public AIProvider PreferredProvider { get; set; } = AIProvider.OpenAI;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Settings { get; set; } = new();
    public List<string> RecentSessions { get; set; } = new();

    public bool IsValid => !string.IsNullOrEmpty(SessionId) && User != null;
    public TimeSpan SessionDuration => DateTime.UtcNow - CreatedAt;
    public TimeSpan TimeSinceLastActivity => DateTime.UtcNow - LastActiveAt;

    public void UpdateLastActivity()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    public void SetSetting(string key, object value)
    {
        Settings[key] = value;
        UpdateLastActivity();
    }

    public T? GetSetting<T>(string key, T? defaultValue = default)
    {
        if (Settings.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public void AddRecentSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId)) return;

        RecentSessions.Remove(sessionId); // Remove if exists
        RecentSessions.Insert(0, sessionId); // Add to beginning

        // Keep only last 10 sessions
        if (RecentSessions.Count > 10)
        {
            RecentSessions = RecentSessions.Take(10).ToList();
        }

        UpdateLastActivity();
    }

    public void RemoveRecentSession(string sessionId)
    {
        RecentSessions.Remove(sessionId);
        UpdateLastActivity();
    }

    public void ClearRecentSessions()
    {
        RecentSessions.Clear();
        UpdateLastActivity();
    }

    public UserSessionData ToStorageModel()
    {
        return new UserSessionData
        {
            SessionId = SessionId,
            UserId = User?.Id,
            Username = User?.Username,
            PreferredProvider = PreferredProvider,
            CreatedAt = CreatedAt,
            LastActiveAt = LastActiveAt,
            Settings = Settings,
            RecentSessions = RecentSessions
        };
    }

    public static UserSession FromStorageModel(UserSessionData data, UserDto? user = null)
    {
        return new UserSession
        {
            SessionId = data.SessionId,
            User = user,
            PreferredProvider = data.PreferredProvider,
            CreatedAt = data.CreatedAt,
            LastActiveAt = data.LastActiveAt,
            Settings = data.Settings ?? new(),
            RecentSessions = data.RecentSessions ?? new()
        };
    }
}

// Simplified model for local storage
public class UserSessionData
{
    public string? SessionId { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public AIProvider PreferredProvider { get; set; } = AIProvider.OpenAI;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Settings { get; set; }
    public List<string>? RecentSessions { get; set; }
}