using ChatAI_Assistant.Server.Services.Chat;
using ChatAI_Assistant.Server.Services.Sessions;
using ChatAI_Assistant.Server.Services.Users;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAI_Assistant.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ISessionService _sessionService;
    private readonly IUserService _userService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        ISessionService sessionService,
        IUserService userService,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _sessionService = sessionService;
        _userService = userService;
        _logger = logger;
    }

    #region Message Operations

    /// <summary>
    /// Envoyer un message dans une session
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ChatResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.CreateValidationError(errors));
            }

            _logger.LogDebug("Sending message from user {UserId} to session {SessionId}",
                request.UserId, request.SessionId);

            var response = await _chatService.SendMessageAsync(request);

            if (!response.Succeeded)
            {
                return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage endpoint");
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Envoyer un message au bot (avec réponse AI automatique)
    /// </summary>
    [HttpPost("messages/bot")]
    [ProducesResponseType(typeof(ChatResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ChatResponse>> SendMessageToBot([FromBody] SendMessageRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.CreateValidationError(errors));
            }

            _logger.LogDebug("Sending message to bot from user {UserId} in session {SessionId}",
                request.UserId, request.SessionId);

            var response = await _chatService.SendMessageToBotAsync(
                request.SessionId,
                request.UserId,
                request.Content,
                request.PreferredAIProvider);

            if (!response.Succeeded)
            {
                return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessageToBot endpoint");
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Récupérer un message spécifique
    /// </summary>
    [HttpGet("messages/{messageId:guid}")]
    [ProducesResponseType(typeof(ChatResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ChatResponse>> GetMessage(Guid messageId)
    {
        try
        {
            var response = await _chatService.GetMessageAsync(messageId);

            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Récupérer les messages d'une session (avec pagination)
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}/messages")]
    [ProducesResponseType(typeof(ApiResponse<List<ChatMessageDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult> GetMessages(
        Guid sessionId,
        [FromQuery] int limit = 50,
        [FromQuery] DateTime? before = null,
        [FromQuery] int page = 1)
    {
        try
        {
            if (limit > 100)
            {
                return BadRequest(ApiResponse<object>.CreateError("Limit cannot exceed 100"));
            }

            if (page < 1)
            {
                return BadRequest(ApiResponse<object>.CreateError("Page must be greater than 0"));
            }

            var request = new GetMessagesRequest
            {
                SessionId = sessionId,
                Limit = limit,
                Before = before,
                Page = page
            };

            var response = await _chatService.GetMessagesAsync(request);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for session {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Modifier un message existant
    /// </summary>
    [HttpPut("messages/{messageId:guid}")]
    [ProducesResponseType(typeof(ChatResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ChatResponse>> UpdateMessage(Guid messageId, [FromBody] UpdateMessageRequest request)
    {
        try
        {
            if (messageId != request.MessageId)
            {
                return BadRequest(ApiResponse<object>.CreateError("Message ID mismatch"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.CreateValidationError(errors));
            }

            var response = await _chatService.UpdateMessageAsync(request);

            if (!response.Succeeded)
            {
                return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Supprimer un message
    /// </summary>
    [HttpDelete("messages/{messageId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult> DeleteMessage(Guid messageId, [FromQuery] Guid userId)
    {
        try
        {
            var request = new DeleteMessageRequest
            {
                MessageId = messageId,
                UserId = userId
            };

            var response = await _chatService.DeleteMessageAsync(request);

            if (!response.Succeeded)
            {
                return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    #endregion

    #region Context and Conversation

    /// <summary>
    /// Récupérer le contexte de conversation d'une session
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}/context")]
    [ProducesResponseType(typeof(ApiResponse<ConversationContextDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult> GetConversationContext(Guid sessionId, [FromQuery] int maxMessages = 10)
    {
        try
        {
            if (maxMessages > 50)
            {
                return BadRequest(ApiResponse<object>.CreateError("Max messages cannot exceed 50"));
            }

            var response = await _chatService.GetConversationContextAsync(sessionId, maxMessages);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation context for session {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Récupérer les messages récents d'une session
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}/recent-messages")]
    [ProducesResponseType(typeof(ApiResponse<List<MessageSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult> GetRecentMessages(Guid sessionId, [FromQuery] int count = 5)
    {
        try
        {
            if (count > 20)
            {
                return BadRequest(ApiResponse<object>.CreateError("Count cannot exceed 20"));
            }

            var response = await _chatService.GetRecentMessagesAsync(sessionId, count);

            if (!response.Succeeded)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent messages for session {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Récupérer le dernier message d'une session
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}/last-message")]
    [ProducesResponseType(typeof(ApiResponse<MessageSummaryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult> GetLastMessage(Guid sessionId)
    {
        try
        {
            var response = await _chatService.GetLastMessageAsync(sessionId);

            if (!response.Succeeded)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last message for session {SessionId}", sessionId);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    #endregion

    #region Quick Chat (Simplified Flow)

    /// <summary>
    /// Démarrer un chat rapide (username → session → premier message)
    /// </summary>
    [HttpPost("quick-start")]
    [ProducesResponseType(typeof(QuickChatResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<QuickChatResponse>> QuickStartChat([FromBody] QuickStartChatRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.CreateValidationError(errors));
            }

            _logger.LogInformation("Quick start chat for username: {Username}", request.Username);

            // 1. Créer ou récupérer l'utilisateur
            var userResponse = await _userService.GetOrCreateUserAsync(request.Username);
            if (!userResponse.Succeeded)
            {
                return BadRequest(ApiResponse<object>.CreateError("Failed to create/get user"));
            }

            var user = userResponse.Data!;

            // 2. Créer une nouvelle session
            var sessionRequest = new CreateSessionRequest
            {
                Title = $"Chat - {request.Username} - {DateTime.Now:yyyy-MM-dd HH:mm}",
                Description = "Quick chat session",
                CreatedByUserId = user.Id,
                IsPrivate = true,
                PreferredAIProvider = request.PreferredAIProvider
            };

            var sessionResponse = await _sessionService.CreateSessionAsync(sessionRequest);
            if (!sessionResponse.Succeeded)
            {
                return BadRequest(ApiResponse<object>.CreateError("Failed to create session"));
            }

            var session = sessionResponse.Data!;

            // 3. Envoyer le premier message au bot si fourni
            ChatMessageDto? botResponse = null;
            if (!string.IsNullOrWhiteSpace(request.InitialMessage))
            {
                var botMessageResponse = await _chatService.SendMessageToBotAsync(
                    session.Id,
                    user.Id,
                    request.InitialMessage,
                    request.PreferredAIProvider);

                if (botMessageResponse.Succeeded)
                {
                    botResponse = botMessageResponse.Data;
                }
            }

            // 4. Construire la réponse
            var quickChatResponse = new QuickChatResponse
            {
                Success = true,
                User = user,
                Session = session,
                BotResponse = botResponse,
                Message = "Chat started successfully"
            };

            _logger.LogInformation("Quick chat started successfully for user {UserId} in session {SessionId}",
                user.Id, session.Id);

            return Ok(quickChatResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in quick start chat for username: {Username}", request.Username);
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    /// <summary>
    /// Continuer un chat existant avec un message
    /// </summary>
    [HttpPost("continue")]
    [ProducesResponseType(typeof(ChatResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ChatResponse>> ContinueChat([FromBody] ContinueChatRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.CreateValidationError(errors));
            }

            var response = await _chatService.SendMessageToBotAsync(
                request.SessionId,
                request.UserId,
                request.Message,
                request.PreferredAIProvider);

            return response.Succeeded ? Ok(response) : BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing chat");
            return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        }
    }

    #endregion
}
