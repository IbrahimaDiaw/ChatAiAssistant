using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;
using ChatAI_Assistant.Server.Configurations;
using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Server.Mapping.Extensions;
using ChatAI_Assistant.Server.Repositories.Interfaces;
using ChatAI_Assistant.Server.Services.AI;
using ChatAI_Assistant.Shared.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using ChatAI_Assistant.Server.Hubs;

namespace ChatAI_Assistant.Server.Services.Chat;
public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IAIServiceFactory _aiServiceFactory;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatService> _logger;

    // Bot user ID - utilisé pour les messages AI
    private static readonly Guid BotUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public ChatService(
        IChatRepository chatRepository,
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IAIServiceFactory aiServiceFactory,
        IHubContext<ChatHub> hubContext,
        ILogger<ChatService> logger)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _aiServiceFactory = aiServiceFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    #region Message Operations

    public async Task<ChatResponse> SendMessageAsync(SendMessageRequest request)
    {
        try
        {
            _logger.LogDebug("Sending message from user {UserId} to session {SessionId}",
                request.UserId, request.SessionId);

            // Validation
            var validationResult = await ValidateMessageRequestAsync(request);
            if (!validationResult.IsValid)
            {
                return ChatResponse.BadRequest(validationResult.ErrorMessage);
            }

            // Créer le message
            var message = request.ToEntity();
            var createdMessage = await _chatRepository.CreateMessageAsync(message);

            // Mettre à jour l'activité de la session
            await _sessionRepository.UpdateLastActivityAsync(request.SessionId, DateTime.UtcNow);

            // Mettre à jour l'activité utilisateur
            await _userRepository.UpdateLastActivityAsync(request.UserId, DateTime.UtcNow);

            // Convertir en DTO
            var messageDto = createdMessage.ToDto();

            // Notifier en temps réel
            await NotifyMessageSentAsync(request.SessionId, messageDto);

            _logger.LogInformation("Message sent successfully: {MessageId} in session {SessionId}",
                createdMessage.Id, request.SessionId);

            return ChatResponse.Success(messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from user {UserId} to session {SessionId}",
                request.UserId, request.SessionId);
            return ChatResponse.Error("Failed to send message");
        }
    }

    public async Task<ChatResponse> SendMessageToBotAsync(Guid sessionId, Guid userId, string message, AIProvider? preferredProvider = null)
    {
        try
        {
            _logger.LogDebug("Sending message to bot for user {UserId} in session {SessionId}", userId, sessionId);

            // 1. Validation de base
            if (string.IsNullOrWhiteSpace(message))
            {
                return ChatResponse.BadRequest("Message content cannot be empty");
            }

            if (message.Length > 4000)
            {
                return ChatResponse.BadRequest("Message is too long (max 4000 characters)");
            }

            // 2. Vérifier l'accès utilisateur
            var accessResult = await ValidateUserAccessAsync(sessionId, userId);
            if (!accessResult.Succeeded || !accessResult.Data)
            {
                return ChatResponse.BadRequest("User does not have access to this session");
            }

            // 3. Sauvegarder le message utilisateur
            var userMessageRequest = new SendMessageRequest
            {
                SessionId = sessionId,
                UserId = userId,
                Content = message,
                PreferredAIProvider = preferredProvider
            };

            var userMessageResponse = await SendMessageAsync(userMessageRequest);
            if (!userMessageResponse.Succeeded)
            {
                return userMessageResponse;
            }

            var contextResponse = await GetConversationContextAsync(sessionId, 10);
            var context = contextResponse.Succeeded ?
                BuildContextString(contextResponse.Data!.Messages) :
                null;

            // 5. Obtenir le service AI approprié
            var user = await _userRepository.GetByIdAsync(userId);
            var aiProviderToUse = preferredProvider ?? user?.Preferences?.PreferredAIProvider ?? AIProvider.OpenAI;
            var aiService = _aiServiceFactory.GetService(aiProviderToUse);

            // 7. Générer la réponse AI
            var aiResponse = await aiService.GenerateResponseAsync(message, context);

            if (!aiResponse.Success)
            {
                _logger.LogWarning("AI service failed for session {SessionId}: {Error}",
                    sessionId, aiResponse.ErrorMessage);

                // Réponse de fallback
                aiResponse = CreateFallbackResponse(aiProviderToUse);
            }

            // 8. Créer et sauvegarder le message AI
            var botMessage = new ChatMessage
            {
                SessionId = sessionId,
                UserId = userId,
                Content = aiResponse.Content,
                Type = MessageType.AI,
                IsFromAI = true,
                AIProvider = aiResponse.Provider,
                AIModel = aiResponse.Model,
                TokensUsed = aiResponse.TokensUsed,
                AITemperature = aiResponse.Temperature,
                AIContext = context,
                ParentMessageId = userMessageResponse.Data!.Id,
                //Metadata = aiResponse.Metadata != null ?
                //    JsonSerializer.Serialize(aiResponse.Metadata) : null
            };

            var createdBotMessage = await _chatRepository.CreateMessageAsync(botMessage);
            var botMessageDto = createdBotMessage.ToDto();

            // 9. Enrichir les informations du bot
            EnrichBotMessage(botMessageDto, aiResponse.Provider);

            // 10. Notifier en temps réel
            await NotifyMessageSentAsync(sessionId, botMessageDto);

            _logger.LogInformation("Bot response generated for session {SessionId} using {Provider}, tokens: {TokensUsed}",
                sessionId, aiResponse.Provider, aiResponse.TokensUsed);

            return ChatResponse.Success(botMessageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bot response for session {SessionId}, user {UserId}",
                sessionId, userId);
            return ChatResponse.Error("Failed to generate bot response");
        }
    }

    public async Task<ChatResponse> GetMessageAsync(Guid messageId)
    {
        try
        {
            var message = await _chatRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return ChatResponse.NotFound($"Message with ID {messageId} not found");
            }

            var messageDto = message.ToDto();

            // Enrichir si c'est un message AI
            if (message.IsFromAI && message.AIProvider.HasValue)
            {
                EnrichBotMessage(messageDto, message.AIProvider.Value);
            }

            return ChatResponse.Success(messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving message {MessageId}", messageId);
            return ChatResponse.Error("Failed to retrieve message");
        }
    }

    public async Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(GetMessagesRequest request)
    {
        try
        {
            _logger.LogDebug("Getting messages for session {SessionId}, limit: {Limit}",
                request.SessionId, request.Limit);

            var messages = await _chatRepository.GetSessionMessagesAsync(
                request.SessionId,
                request.Limit,
                request.Before);

            var messageDtos = messages.Select(m =>
            {
                var dto = m.ToDto();
                if (m.IsFromAI && m.AIProvider.HasValue)
                {
                    EnrichBotMessage(dto, m.AIProvider.Value);
                }
                return dto;
            }).ToList();

            return ApiResponse<List<ChatMessageDto>>.CreateSuccess(messageDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages for session {SessionId}", request.SessionId);
            return ApiResponse<List<ChatMessageDto>>.CreateError("Failed to retrieve messages");
        }
    }

    public async Task<ChatResponse> UpdateMessageAsync(UpdateMessageRequest request)
    {
        try
        {
            var message = await _chatRepository.GetMessageByIdAsync(request.MessageId);
            if (message == null)
            {
                return ChatResponse.NotFound($"Message with ID {request.MessageId} not found");
            }

            // Seuls les messages utilisateur peuvent être édités
            if (message.IsFromAI)
            {
                return ChatResponse.BadRequest("AI messages cannot be edited");
            }

            // Mise à jour du message
            message.UpdateFromRequest(request);
            var updatedMessage = await _chatRepository.UpdateMessageAsync(message);
            var messageDto = updatedMessage.ToDto();

            // Notifier la mise à jour
            await _hubContext.Clients.Group($"session_{message.SessionId}")
                .SendAsync("MessageUpdated", messageDto);

            _logger.LogInformation("Message updated: {MessageId}", request.MessageId);
            return ChatResponse.Success(messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", request.MessageId);
            return ChatResponse.Error("Failed to update message");
        }
    }

    public async Task<ApiResponse<bool>> DeleteMessageAsync(DeleteMessageRequest request)
    {
        try
        {
            var message = await _chatRepository.GetMessageByIdAsync(request.MessageId);
            if (message == null)
            {
                return ApiResponse<bool>.CreateError("Message not found");
            }

            // Vérifier les permissions
            if (message.UserId != request.UserId && !message.IsFromAI)
            {
                return ApiResponse<bool>.CreateError("You can only delete your own messages");
            }

            await _chatRepository.SoftDeleteMessageAsync(request.MessageId);

            // Notifier la suppression
            await _hubContext.Clients.Group($"session_{message.SessionId}")
                .SendAsync("MessageDeleted", request.MessageId);

            _logger.LogInformation("Message deleted: {MessageId} by user {UserId}",
                request.MessageId, request.UserId);

            return ApiResponse<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", request.MessageId);
            return ApiResponse<bool>.CreateError("Failed to delete message");
        }
    }

    #endregion

    #region Context and Conversation

    public async Task<ApiResponse<ConversationContextDto>> GetConversationContextAsync(Guid sessionId, int maxMessages = 10)
    {
        try
        {
            var messages = await _chatRepository.GetConversationContextAsync(sessionId, maxMessages);
            var contextDto = messages.ToContextDto();

            // Enrichir les messages AI
            foreach (var msg in contextDto.Messages.Where(m => m.IsFromAI && m.AIProvider.HasValue))
            {
                EnrichBotMessage(msg, msg.AIProvider!.Value);
            }

            return ApiResponse<ConversationContextDto>.CreateSuccess(contextDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation context for session {SessionId}", sessionId);
            return ApiResponse<ConversationContextDto>.CreateError("Failed to retrieve conversation context");
        }
    }

    public async Task<ApiResponse<List<MessageSummaryDto>>> GetRecentMessagesAsync(Guid sessionId, int count = 5)
    {
        try
        {
            var messages = await _chatRepository.GetRecentMessagesAsync(sessionId, count);
            var summaryDtos = messages.Select(m =>
            {
                var dto = m.ToSummaryDto();
                if (m.IsFromAI && m.AIProvider.HasValue)
                {
                    dto.Username = GetBotName(m.AIProvider.Value);
                }
                return dto;
            }).ToList();

            return ApiResponse<List<MessageSummaryDto>>.CreateSuccess(summaryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent messages for session {SessionId}", sessionId);
            return ApiResponse<List<MessageSummaryDto>>.CreateError("Failed to retrieve recent messages");
        }
    }

    public async Task<ApiResponse<MessageSummaryDto>> GetLastMessageAsync(Guid sessionId)
    {
        try
        {
            var message = await _chatRepository.GetLastMessageAsync(sessionId);
            if (message == null)
            {
                return ApiResponse<MessageSummaryDto>.CreateError("No messages found in this session");
            }

            var summaryDto = message.ToSummaryDto();
            if (message.IsFromAI && message.AIProvider.HasValue)
            {
                summaryDto.Username = GetBotName(message.AIProvider.Value);
            }

            return ApiResponse<MessageSummaryDto>.CreateSuccess(summaryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving last message for session {SessionId}", sessionId);
            return ApiResponse<MessageSummaryDto>.CreateError("Failed to retrieve last message");
        }
    }

    #endregion

    #region Real-time Notifications

    public async Task NotifyMessageSentAsync(Guid sessionId, ChatMessageDto message)
    {
        try
        {
            await _hubContext.Clients.Group($"session_{sessionId}")
                .SendAsync("MessageReceived", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying message sent for session {SessionId}", sessionId);
        }
    }

    public async Task NotifyTypingAsync(Guid sessionId, Guid userId, bool isTyping)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                await _hubContext.Clients.Group($"session_{sessionId}")
                    .SendAsync("UserTyping", new
                    {
                        UserId = userId,
                        Username = user.Username,
                        IsTyping = isTyping
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying typing status for user {UserId} in session {SessionId}",
                userId, sessionId);
        }
    }

    public async Task NotifyUserJoinedAsync(Guid sessionId, UserDto user)
    {
        try
        {
            await _hubContext.Clients.Group($"session_{sessionId}")
                .SendAsync("UserJoined", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user joined for session {SessionId}", sessionId);
        }
    }

    public async Task NotifyUserLeftAsync(Guid sessionId, UserDto user)
    {
        try
        {
            await _hubContext.Clients.Group($"session_{sessionId}")
                .SendAsync("UserLeft", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user left for session {SessionId}", sessionId);
        }
    }

    #endregion

    #region Utility Operations

    public async Task<ApiResponse<bool>> MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
    {
        try
        {
            await _chatRepository.MarkMessagesAsReadAsync(sessionId, userId, DateTime.UtcNow);
            return ApiResponse<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read for user {UserId} in session {SessionId}",
                userId, sessionId);
            return ApiResponse<bool>.CreateError("Failed to mark messages as read");
        }
    }

    public  Task<ApiResponse<int>> GetUnreadCountAsync(Guid sessionId, Guid userId)
    {
        
            //// Cette méthode nécessiterait une implémentation dans le repository
            //// Pour l'instant, retournons 0
            //var count = await _chatRepository.GetConversationContextAsync(sessionId);
            //return ApiResponse<int>.CreateSuccess(count);

            throw new NotImplementedException("GetUnreadCountAsync is not implemented yet.");  
    }

    public async Task<ApiResponse<bool>> ValidateUserAccessAsync(Guid sessionId, Guid userId)
    {
        try
        {
            // Vérifier que l'utilisateur existe
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.CreateError("User not found");
            }

            // Vérifier que la session existe
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return ApiResponse<bool>.CreateError("Session not found");
            }

            // Vérifier que l'utilisateur a accès à la session
            var participant = await _sessionRepository.GetParticipantAsync(sessionId, userId);
            if (participant == null || !participant.IsActive)
            {
                return ApiResponse<bool>.CreateError("User does not have access to this session");
            }

            return ApiResponse<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user access for user {UserId} in session {SessionId}",
                userId, sessionId);
            return ApiResponse<bool>.CreateError("Failed to validate user access");
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<ValidationResult> ValidateMessageRequestAsync(SendMessageRequest request)
    {
        // Vérifier que l'utilisateur existe
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return ValidationResult.Invalid($"User with ID {request.UserId} not found");
        }

        // Vérifier que la session existe
        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null)
        {
            return ValidationResult.Invalid($"Session with ID {request.SessionId} not found");
        }

        // Vérifier que l'utilisateur participe à la session
        var participant = await _sessionRepository.GetParticipantAsync(request.SessionId, request.UserId);
        if (participant == null || !participant.IsActive)
        {
            return ValidationResult.Invalid("User is not a participant in this session");
        }

        // Vérifier que le contenu n'est pas vide
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return ValidationResult.Invalid("Message content cannot be empty");
        }

        // Vérifier la longueur du message
        if (request.Content.Length > 4000)
        {
            return ValidationResult.Invalid("Message content is too long (max 4000 characters)");
        }

        return ValidationResult.Valid();
    }
    private static string BuildContextString(List<ChatMessageDto> messages)
    {
        return string.Join("\n", messages
            .OrderBy(m => m.Timestamp)
            .Select(m => $"{m.Username}: {m.Content}"));
    }

    private static AIResponse CreateFallbackResponse(AIProvider provider)
    {
        return new AIResponse
        {
            Content = "Désolé, je rencontre actuellement des difficultés techniques. Veuillez réessayer dans quelques instants.",
            Provider = provider,
            Model = "fallback",
            Success = true,
            TokensUsed = 0,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static void EnrichBotMessage(ChatMessageDto messageDto, AIProvider provider)
    {
        messageDto.Username = GetBotName(provider);
        messageDto.UserDisplayName = GetBotDisplayName(provider);
    }

    private static string GetBotName(AIProvider provider) => provider switch
    {
        AIProvider.OpenAI => "🤖 ChatGPT",
        AIProvider.AzureOpenAI => "🤖 Azure AI",
        AIProvider.Anthropic => "🤖 Claude",
        AIProvider.Mock => "🤖 ChatAI Bot",
        _ => "🤖 AI Assistant"
    };

    private static string GetBotDisplayName(AIProvider provider) => provider switch
    {
        AIProvider.OpenAI => "Assistant ChatGPT",
        AIProvider.AzureOpenAI => "Assistant Azure OpenAI",
        AIProvider.Anthropic => "Assistant Claude",
        AIProvider.Mock => "Assistant IA Simple",
        _ => "Assistant IA"
    };
    #endregion
}
