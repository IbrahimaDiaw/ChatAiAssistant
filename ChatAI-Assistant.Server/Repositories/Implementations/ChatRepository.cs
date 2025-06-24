using ChatAI_Assistant.Server.Data;
using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatAI_Assistant.Server.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly ChatAiAssistantDbContext _context;
    private readonly ILogger<ChatRepository> _logger;

    public ChatRepository(ChatAiAssistantDbContext context, ILogger<ChatRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChatMessage> CreateMessageAsync(ChatMessage message)
    {
        try
        {
            if (message.Id == Guid.Empty)
                message.Id = Guid.NewGuid();

            message.Timestamp = DateTime.UtcNow;

            // Generate message hash for duplicate detection
            message.MessageHash = GenerateMessageHash(message.Content, message.UserId, message.SessionId);

            // Validation des valeurs pour éviter les erreurs de contrainte CHECK
            ValidateMessageBeforeInsert(message);

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Message created: {MessageId} in session {SessionId}", message.Id, message.SessionId);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creaCK_ChatMessages_TokensUsedting message in session {SessionId}", message.SessionId);
            throw new Exception("Failed to create message", ex);
        }
    }

    public async Task<ChatMessage?> GetMessageByIdAsync(Guid messageId)
    {
        return await _context.Messages
            .Include(m => m.User)
            .Include(m => m.Session)
            .Include(m => m.ParentMessage)
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);
    }

    public async Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(Guid sessionId, int limit = 50, DateTime? before = null)
    {
        var query = _context.Messages
            .Where(m => m.SessionId == sessionId && !m.IsDeleted)
            .Include(m => m.User)
            .OrderByDescending(m => m.Timestamp);

        if (before.HasValue)
        {
            query = (IOrderedQueryable<ChatMessage>)query.Where(m => m.Timestamp < before.Value);
        }

        return await query.Take(limit).ToListAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetConversationContextAsync(Guid sessionId, int maxMessages = 10)
    {
        return await _context.Messages
            .Where(m => m.SessionId == sessionId && !m.IsDeleted)
            .OrderByDescending(m => m.Timestamp)
            .Take(maxMessages)
            .Include(m => m.User)
            .OrderBy(m => m.Timestamp) // Reorder chronologically for context
            .ToListAsync();
    }

    public async Task<ChatMessage> UpdateMessageAsync(ChatMessage message)
    {
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;

        // Validation avant mise à jour
        ValidateMessageBeforeUpdate(message);

        _context.Messages.Update(message);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Message updated: {MessageId}", message.Id);
        return message;
    }

    public async Task SoftDeleteMessageAsync(Guid messageId)
    {
        await _context.Messages
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(m => m
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, DateTime.UtcNow));

        _logger.LogDebug("Message soft deleted: {MessageId}", messageId);
    }

    public async Task<int> GetSessionMessageCountAsync(Guid sessionId)
    {
        return await _context.Messages
            .CountAsync(m => m.SessionId == sessionId && !m.IsDeleted);
    }

    public async Task<ChatMessage?> GetLastMessageAsync(Guid sessionId)
    {
        return await _context.Messages
            .Where(m => m.SessionId == sessionId && !m.IsDeleted)
            .OrderByDescending(m => m.Timestamp)
            .Include(m => m.User)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(Guid sessionId, int count = 5)
    {
        return await _context.Messages
            .Where(m => m.SessionId == sessionId && !m.IsDeleted)
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .Include(m => m.User)
            .OrderBy(m => m.Timestamp) // Reorder chronologically
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesPagedAsync(Guid sessionId, int page, int pageSize)
    {
        return await _context.Messages
            .Where(m => m.SessionId == sessionId && !m.IsDeleted)
            .OrderByDescending(m => m.Timestamp)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Include(m => m.User)
            .ToListAsync();
    }

    public async Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId, DateTime readTimestamp)
    {
        // This could be implemented with a separate ReadReceipts table
        // For now, we'll update the participant's LastSeenAt
        await _context.SessionParticipants
            .Where(p => p.SessionId == sessionId && p.UserId == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.LastSeenAt, readTimestamp));
    }

    /// <summary>
    /// Valide les propriétés du message avant insertion pour éviter les erreurs de contrainte
    /// </summary>
    private static void ValidateMessageBeforeInsert(ChatMessage message)
    {
        // Validation TokensUsed : doit être null ou positif
        if (message.TokensUsed.HasValue && message.TokensUsed.Value <= 0)
        {
            message.TokensUsed = null;
        }

        // Validation AITemperature : doit être entre 0 et 2 (limites OpenAI/Claude)
        if (message.AITemperature.HasValue)
        {
            if (message.AITemperature.Value < 0)
                message.AITemperature = 0;
            else if (message.AITemperature.Value > 2)
                message.AITemperature = 2;
        }

        // Validation des champs string pour éviter les overflow
        if (!string.IsNullOrEmpty(message.AIModel) && message.AIModel.Length > 50)
        {
            message.AIModel = message.AIModel.Substring(0, 50);
        }

        if (!string.IsNullOrEmpty(message.MessageHash) && message.MessageHash.Length > 100)
        {
            message.MessageHash = message.MessageHash.Substring(0, 100);
        }

        // S'assurer que Content n'est pas null
        if (string.IsNullOrEmpty(message.Content))
        {
            message.Content = string.Empty;
        }
    }

    /// <summary>
    /// Valide les propriétés du message avant mise à jour
    /// </summary>
    private static void ValidateMessageBeforeUpdate(ChatMessage message)
    {
        ValidateMessageBeforeInsert(message); // Mêmes validations
    }

    private static string GenerateMessageHash(string content, Guid userId, Guid sessionId)
    {
        var input = $"{content}-{userId}-{sessionId}";
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(input))).Substring(0, 16);
    }
}