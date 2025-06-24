using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;

namespace ChatAI_Assistant.Client.Services;

public interface ISignalRService
{
    event Action<ChatMessageDto>? OnMessageReceived;
    event Action<UserTypingEventArgs>? OnUserTyping;
    event Action? OnConnected;
    event Action? OnDisconnected;

    Task StartAsync();
    Task StopAsync();
    Task JoinSessionAsync(Guid sessionId, Guid userId, string username);
    Task LeaveSessionAsync(Guid sessionId, Guid userId);
    Task SendMessageAsync(Guid sessionId, Guid userId, string content);
    Task StartTypingAsync(Guid sessionId, Guid userId);
    Task StopTypingAsync(Guid sessionId, Guid userId);
    bool IsConnected { get; }
}

public class UserTypingEventArgs
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
}