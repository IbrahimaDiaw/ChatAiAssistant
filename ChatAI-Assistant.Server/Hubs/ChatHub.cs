using Microsoft.AspNetCore.SignalR;
using ChatAI_Assistant.Server.Services.Chat;
using ChatAI_Assistant.Server.Services.Sessions;
using ChatAI_Assistant.Server.Services.Users;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.Enums;
using System.Collections.Concurrent;
using ChatAI_Assistant.Shared.DTOs;

namespace ChatAI_Assistant.Server.Hubs;
/// <summary>
/// SignalR Hub pour la communication temps réel du chat
/// </summary>
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IUserService _userService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ChatHub> _logger;

    // Cache des connexions actives par utilisateur
    private static readonly ConcurrentDictionary<string, UserConnection> _connections = new();

    // Cache des utilisateurs en train de taper par session
    private static readonly ConcurrentDictionary<string, HashSet<string>> _typingUsers = new();

    public ChatHub(
        IChatService chatService,
        IUserService userService,
        ISessionService sessionService,
        ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _userService = userService;
        _sessionService = sessionService;
        _logger = logger;
    }

    #region Connection Management

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString();

        _logger.LogInformation("Client connected: {ConnectionId} from {UserAgent}",
            connectionId, userAgent);

        // Envoyer message de bienvenue
        await Clients.Caller.SendAsync("Welcome", new
        {
            ConnectionId = connectionId,
            Timestamp = DateTime.UtcNow,
            Message = "Connected to ChatAI"
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        try
        {
            // Nettoyer les connexions et notifier
            if (_connections.TryRemove(connectionId, out var userConnection))
            {
                // Retirer des groupes
                await Groups.RemoveFromGroupAsync(connectionId, $"session_{userConnection.SessionId}");
                await Groups.RemoveFromGroupAsync(connectionId, $"user_{userConnection.UserId}");

                // Arrêter l'indicateur de saisie si actif
                await StopTypingInternal(userConnection.SessionId, userConnection.UserId);

                // Notifier que l'utilisateur s'est déconnecté
                await Clients.Group($"session_{userConnection.SessionId}")
                    .SendAsync("UserDisconnected", new
                    {
                        UserId = userConnection.UserId,
                        Username = userConnection.Username,
                        DisconnectedAt = DateTime.UtcNow
                    });

                // Mettre à jour l'activité utilisateur
                try
                {
                    await _userService.UpdateLastActivityAsync(userConnection.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update user activity on disconnect for user {UserId}",
                        userConnection.UserId);
                }

                _logger.LogInformation("User disconnected: {Username} ({UserId}) from session {SessionId}",
                    userConnection.Username, userConnection.UserId, userConnection.SessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect cleanup for connection {ConnectionId}", connectionId);
        }

        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected with exception: {ConnectionId}", connectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Rejoindre une session de chat
    /// </summary>
    public async Task JoinSession(string sessionIdStr, string userIdStr, string username)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Type = "ValidationError",
                    Message = "Invalid session or user ID format",
                    Code = "INVALID_ID_FORMAT"
                });
                return;
            }

            // Valider l'accès utilisateur à la session
            var accessResult = await ValidateUserSessionAccessAsync(sessionId, userId);
            if (!accessResult.IsValid)
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Type = "AccessDenied",
                    Message = "You don't have access to this session",
                    Code = "ACCESS_DENIED"
                });
                return;
            }

            // Créer ou mettre à jour la connexion
            var userConnection = new UserConnection
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                SessionId = sessionId,
                Username = username,
                JoinedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            _connections.AddOrUpdate(Context.ConnectionId, userConnection, (key, existing) => userConnection);

            // Ajouter aux groupes SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Mettre à jour l'activité utilisateur
            await _userService.UpdateLastActivityAsync(userId);

            // Récupérer les infos utilisateur pour notification
            var userResponse = await _userService.GetUserByIdAsync(userId);
            if (userResponse.Succeeded)
            {
                await _chatService.NotifyUserJoinedAsync(sessionId, userResponse.Data!);
            }

            // Envoyer confirmation de connexion
            await Clients.Caller.SendAsync("JoinedSession", new
            {
                SessionId = sessionId,
                UserId = userId,
                Username = username,
                JoinedAt = DateTime.UtcNow,
                OnlineUsers = GetOnlineUsersInSession(sessionId)
            });

            _logger.LogInformation("User {Username} ({UserId}) joined session {SessionId}",
                username, userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining session {SessionId} for user {UserId}",
                sessionIdStr, userIdStr);

            await Clients.Caller.SendAsync("Error", new
            {
                Type = "InternalError",
                Message = "Failed to join session",
                Code = "JOIN_SESSION_FAILED"
            });
        }
    }

    /// <summary>
    /// Quitter une session de chat
    /// </summary>
    public async Task LeaveSession(string sessionIdStr, string userIdStr)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                return;
            }

            // Retirer des groupes
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Arrêter l'indicateur de saisie
            await StopTypingInternal(sessionId, userId);

            // Retirer de la connexion
            if (_connections.TryRemove(Context.ConnectionId, out var userConnection))
            {
                // Récupérer les infos utilisateur pour notification
                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (userResponse.Succeeded)
                {
                    await _chatService.NotifyUserLeftAsync(sessionId, userResponse.Data!);
                }
            }

            await Clients.Caller.SendAsync("LeftSession", new
            {
                SessionId = sessionId,
                UserId = userId,
                LeftAt = DateTime.UtcNow
            });

            _logger.LogInformation("User {UserId} left session {SessionId}", userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving session {SessionId} for user {UserId}",
                sessionIdStr, userIdStr);
        }
    }

    #endregion

    #region Message Operations

    /// <summary>
    /// Envoyer un message dans une session
    /// </summary>
    public async Task SendMessage(string sessionIdStr, string userIdStr, string content)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Type = "ValidationError",
                    Message = "Invalid session or user ID format",
                    Code = "INVALID_ID_FORMAT"
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Type = "ValidationError",
                    Message = "Message content cannot be empty",
                    Code = "EMPTY_MESSAGE"
                });
                return;
            }

            if (content.Length > 4000)
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Type = "ValidationError",
                    Message = "Message is too long (max 4000 characters)",
                    Code = "MESSAGE_TOO_LONG"
                });
                return;
            }

            // Arrêter l'indicateur de saisie
            await StopTypingInternal(sessionId, userId);

            // Envoyer le message via le service (qui génère aussi la réponse AI)
            var response = await _chatService.SendMessageToBotAsync(sessionId, userId, content);

            if (!response.Succeeded)
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Type = "MessageError",
                    Message = response.Message,
                    Code = "SEND_MESSAGE_FAILED"
                });
                return;
            }

            // Le service ChatService s'occupe déjà des notifications SignalR
            _logger.LogDebug("Message sent successfully via SignalR: {MessageId}", response.Data!.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message via SignalR for user {UserId} in session {SessionId}",
                userIdStr, sessionIdStr);

            await Clients.Caller.SendAsync("Error", new
            {
                Type = "InternalError",
                Message = "Failed to send message",
                Code = "SEND_MESSAGE_ERROR"
            });
        }
    }

    /// <summary>
    /// Envoyer un message simple (sans réponse AI automatique)
    /// </summary>
    public async Task SendSimpleMessage(string sessionIdStr, string userIdStr, string content)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid ID format" });
                return;
            }

            var request = new SendMessageRequest
            {
                SessionId = sessionId,
                UserId = userId,
                Content = content,
                PreferredAIProvider = null,
            };

            var response = await _chatService.SendMessageAsync(request);

            if (!response.Succeeded)
            {
                await Clients.Caller.SendAsync("Error", new { Message = response.Message });
                return;
            }

            // Le service se charge des notifications
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending simple message");
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message" });
        }
    }

    /// <summary>
    /// Modifier un message existant
    /// </summary>
    public async Task UpdateMessage(string messageIdStr, string content)
    {
        try
        {
            if (!Guid.TryParse(messageIdStr, out var messageId))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid message ID format" });
                return;
            }

            var request = new UpdateMessageRequest
            {
                MessageId = messageId,
                Content = content
            };

            var response = await _chatService.UpdateMessageAsync(request);

            if (!response.Succeeded)
            {
                await Clients.Caller.SendAsync("Error", new { Message = response.Message });
                return;
            }

            // Le service se charge des notifications
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId} via SignalR", messageIdStr);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to update message" });
        }
    }

    /// <summary>
    /// Supprimer un message
    /// </summary>
    public async Task DeleteMessage(string messageIdStr, string userIdStr)
    {
        try
        {
            if (!Guid.TryParse(messageIdStr, out var messageId) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid ID format" });
                return;
            }

            var request = new DeleteMessageRequest
            {
                MessageId = messageId,
                UserId = userId
            };

            var response = await _chatService.DeleteMessageAsync(request);

            if (!response.Succeeded)
            {
                await Clients.Caller.SendAsync("Error", new { Message = response.Message });
                return;
            }

            // Le service se charge des notifications
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId} via SignalR", messageIdStr);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to delete message" });
        }
    }

    #endregion

    #region Typing Indicators

    /// <summary>
    /// Démarrer l'indicateur de saisie
    /// </summary>
    public async Task StartTyping(string sessionIdStr, string userIdStr)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                return;
            }

            // Ajouter à la liste des utilisateurs en train de taper
            var sessionKey = sessionId.ToString();
            var userKey = userId.ToString();

            _typingUsers.AddOrUpdate(sessionKey,
                new HashSet<string> { userKey },
                (key, existing) => { existing.Add(userKey); return existing; });

            await _chatService.NotifyTypingAsync(sessionId, userId, true);

            // Programmer l'arrêt automatique après 5 secondes
            _ = Task.Delay(5000).ContinueWith(async _ =>
            {
                await StopTypingInternal(sessionId, userId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting typing indicator for user {UserId} in session {SessionId}",
                userIdStr, sessionIdStr);
        }
    }

    /// <summary>
    /// Arrêter l'indicateur de saisie
    /// </summary>
    public async Task StopTyping(string sessionIdStr, string userIdStr)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId) ||
                !Guid.TryParse(userIdStr, out var userId))
            {
                return;
            }

            await StopTypingInternal(sessionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping typing indicator for user {UserId} in session {SessionId}",
                userIdStr, sessionIdStr);
        }
    }

    private async Task StopTypingInternal(Guid sessionId, Guid userId)
    {
        try
        {
            var sessionKey = sessionId.ToString();
            var userKey = userId.ToString();

            // Retirer de la liste des utilisateurs en train de taper
            if (_typingUsers.TryGetValue(sessionKey, out var typingSet))
            {
                typingSet.Remove(userKey);
                if (!typingSet.Any())
                {
                    _typingUsers.TryRemove(sessionKey, out _);
                }
            }

            await _chatService.NotifyTypingAsync(sessionId, userId, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StopTypingInternal for user {UserId} in session {SessionId}",
                userId, sessionId);
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Obtenir la liste des utilisateurs en ligne dans une session
    /// </summary>
    public async Task GetOnlineUsers(string sessionIdStr)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId))
            {
                await Clients.Caller.SendAsync("Error", new { Message = "Invalid session ID format" });
                return;
            }

            var onlineUsers = GetOnlineUsersInSession(sessionId);

            await Clients.Caller.SendAsync("OnlineUsers", new
            {
                SessionId = sessionId,
                Users = onlineUsers,
                Count = onlineUsers.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online users for session {SessionId}", sessionIdStr);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to get online users" });
        }
    }

    /// <summary>
    /// Obtenir les utilisateurs en train de taper
    /// </summary>
    public async Task GetTypingUsers(string sessionIdStr)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId))
            {
                return;
            }

            var sessionKey = sessionId.ToString();
            var typingUserIds = _typingUsers.TryGetValue(sessionKey, out var typingSet) ?
                typingSet.ToList() : new List<string>();

            await Clients.Caller.SendAsync("TypingUsers", new
            {
                SessionId = sessionId,
                TypingUserIds = typingUserIds,
                Count = typingUserIds.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting typing users for session {SessionId}", sessionIdStr);
        }
    }

    /// <summary>
    /// Test de connectivité (ping/pong)
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", new
        {
            Timestamp = DateTime.UtcNow,
            ConnectionId = Context.ConnectionId
        });
    }

    /// <summary>
    /// Obtenir les statistiques de la session
    /// </summary>
    public async Task GetSessionStats(string sessionIdStr)
    {
        try
        {
            if (!Guid.TryParse(sessionIdStr, out var sessionId))
            {
                return;
            }

            var onlineUsers = GetOnlineUsersInSession(sessionId);
            var typingCount = _typingUsers.TryGetValue(sessionId.ToString(), out var typing) ? typing.Count : 0;

            await Clients.Caller.SendAsync("SessionStats", new
            {
                SessionId = sessionId,
                OnlineCount = onlineUsers.Count,
                TypingCount = typingCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session stats for {SessionId}", sessionIdStr);
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<ValidationResult> ValidateUserSessionAccessAsync(Guid sessionId, Guid userId)
    {
        try
        {
            // Vérifier que l'utilisateur existe
            var user = await _userService.GetUserByIdAsync(userId);
            if (!user.Succeeded || user.Data == null)
            {
                return ValidationResult.Invalid("User not found");
            }

            // Vérifier que la session existe
            var session = await _sessionService.GetSessionByIdAsync(sessionId);
            if (!session.Succeeded || session.Data == null)
            {
                return ValidationResult.Invalid("Session not found");
            }

            // Vérifier que l'utilisateur a accès à la session
            var participant = await _sessionService.GetParticipantAsync(sessionId, userId);
            if (!participant.Succeeded || participant.Data == null || !participant.Data.IsActive)
            {
                return ValidationResult.Invalid("User does not have access to this session");
            }

            return ValidationResult.Valid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user access for user {UserId} in session {SessionId}",
                userId, sessionId);
            return ValidationResult.Invalid("Failed to validate user access");
        }
    }

    private List<object> GetOnlineUsersInSession(Guid sessionId)
    {
        return _connections.Values
            .Where(c => c.SessionId == sessionId)
            .Select(c => new
            {
                c.UserId,
                c.Username,
                c.JoinedAt,
                LastActivity = c.LastActivity,
                ConnectionId = c.ConnectionId
            })
            .Cast<object>()
            .ToList();
    }

    private bool IsUserInSession(Guid sessionId, Guid userId)
    {
        return _connections.Values.Any(c => c.SessionId == sessionId && c.UserId == userId);
    }

    private UserConnection? GetUserConnection(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
    }

    private UserConnection? GetUserConnectionByUserId(Guid userId)
    {
        return _connections.Values.FirstOrDefault(c => c.UserId == userId);
    }

    #endregion
}

/// <summary>
/// Modèle pour gérer les connexions utilisateur
/// </summary>
public class UserConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}