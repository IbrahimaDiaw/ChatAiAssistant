using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatAI_Assistant.Client.Services;

public class SignalRService : ISignalRService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly ILogger<SignalRService> _logger;

    public event Action<ChatMessageDto>? OnMessageReceived;
    public event Action<UserTypingEventArgs>? OnUserTyping;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRService(ILogger<SignalRService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync()
    {
        if (_hubConnection != null) return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7000/chathub") // URL de votre serveur
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
            .Build();

        // Event handlers
        _hubConnection.On<ChatMessageDto>("MessageReceived", (message) =>
        {
            _logger.LogDebug("Message received: {MessageId}", message.Id);
            OnMessageReceived?.Invoke(message);
        });

        _hubConnection.On<object>("UserTyping", (data) =>
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                var typingData = System.Text.Json.JsonSerializer.Deserialize<UserTypingEventArgs>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                if (typingData != null)
                {
                    OnUserTyping?.Invoke(typingData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserTyping event");
            }
        });

        _hubConnection.On<UserDto>("UserJoined", (user) =>
        {
            _logger.LogDebug("User joined: {Username}", user.Username);
        });

        _hubConnection.On<UserDto>("UserLeft", (user) =>
        {
            _logger.LogDebug("User left: {Username}", user.Username);
        });

        _hubConnection.On<object>("Error", (error) =>
        {
            _logger.LogError("SignalR Error: {Error}", error);
        });

        // Connection events
        _hubConnection.Reconnecting += (error) =>
        {
            _logger.LogWarning("SignalR reconnecting: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            _logger.LogInformation("SignalR reconnected: {ConnectionId}", connectionId);
            OnConnected?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Closed += (error) =>
        {
            _logger.LogWarning("SignalR connection closed: {Error}", error?.Message);
            OnDisconnected?.Invoke();
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("SignalR connected successfully");
            OnConnected?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SignalR connection");
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            OnDisconnected?.Invoke();
        }
    }

    public async Task JoinSessionAsync(Guid sessionId, Guid userId, string username)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.InvokeAsync("JoinSession", sessionId.ToString(), userId.ToString(), username);
                _logger.LogDebug("Joined session: {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining session {SessionId}", sessionId);
                throw;
            }
        }
    }

    public async Task LeaveSessionAsync(Guid sessionId, Guid userId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.InvokeAsync("LeaveSession", sessionId.ToString(), userId.ToString());
                _logger.LogDebug("Left session: {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving session {SessionId}", sessionId);
            }
        }
    }

    public async Task SendMessageAsync(Guid sessionId, Guid userId, string content)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.InvokeAsync("SendMessage", sessionId.ToString(), userId.ToString(), content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message via SignalR");
                throw;
            }
        }
    }

    public async Task StartTypingAsync(Guid sessionId, Guid userId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.InvokeAsync("StartTyping", sessionId.ToString(), userId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting typing indicator");
            }
        }
    }

    public async Task StopTypingAsync(Guid sessionId, Guid userId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.InvokeAsync("StopTyping", sessionId.ToString(), userId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping typing indicator");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}