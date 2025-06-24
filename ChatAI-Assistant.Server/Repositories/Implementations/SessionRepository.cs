using ChatAI_Assistant.Server.Data;
using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatAI_Assistant.Server.Repositories.Implementations;

public class SessionRepository : ISessionRepository
{
    private readonly ChatAiAssistantDbContext _context;
    private readonly ILogger<SessionRepository> _logger;

    public SessionRepository(ChatAiAssistantDbContext context, ILogger<SessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Session Operations

    public async Task<ChatSession> CreateAsync(ChatSession session)
    {
        if (session.Id == Guid.Empty)
            session.Id = Guid.NewGuid();

        session.CreatedAt = DateTime.UtcNow;
        session.LastActivity = DateTime.UtcNow;

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Session created: {SessionId} - {Title}", session.Id, session.Title);
        return session;
    }

    public async Task<ChatSession?> GetByIdAsync(Guid sessionId)
    {
        return await _context.Sessions
            .Include(s => s.CreatedBy)
            .Include(s => s.Participants.Where(p => p.IsActive))
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.IsActive);
    }

    public async Task<IEnumerable<ChatSession>> GetUserSessionsAsync(Guid userId, int limit = 50)
    {
        return await _context.Sessions
            .Where(s => s.Participants.Any(p => p.UserId == userId && p.IsActive) && s.IsActive)
            .OrderByDescending(s => s.LastActivity)
            .Take(limit)
            .Include(s => s.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatSession>> GetPublicSessionsAsync(int limit = 50)
    {
        return await _context.Sessions
            .Where(s => !s.IsPrivate && s.IsActive)
            .OrderByDescending(s => s.LastActivity)
            .Take(limit)
            .Include(s => s.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatSession>> SearchByTitleAsync(string searchTerm, int limit = 20)
    {
        return await _context.Sessions
            .Where(s => s.Title.Contains(searchTerm) && s.IsActive)
            .OrderByDescending(s => s.LastActivity)
            .Take(limit)
            .Include(s => s.CreatedBy)
            .ToListAsync();
    }

    public async Task<ChatSession> UpdateAsync(ChatSession session)
    {
        session.LastActivity = DateTime.UtcNow;
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Session updated: {SessionId}", session.Id);
        return session;
    }

    public async Task DeleteAsync(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogDebug("Session deleted: {SessionId}", sessionId);
        }
    }

    #endregion

    #region Participant Operations

    public async Task<SessionParticipant> AddParticipantAsync(SessionParticipant participant)
    {
        if (participant.Id == Guid.Empty)
            participant.Id = Guid.NewGuid();

        participant.JoinedAt = DateTime.UtcNow;
        participant.LastSeenAt = DateTime.UtcNow;

        _context.SessionParticipants.Add(participant);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Participant added: {UserId} to session {SessionId}",
            participant.UserId, participant.SessionId);
        return participant;
    }

    public async Task<SessionParticipant?> GetParticipantAsync(Guid sessionId, Guid userId)
    {
        return await _context.SessionParticipants
            .Include(p => p.User)
            .Include(p => p.Session)
            .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.UserId == userId);
    }

    public async Task<IEnumerable<SessionParticipant>> GetSessionParticipantsAsync(Guid sessionId)
    {
        return await _context.SessionParticipants
            .Where(p => p.SessionId == sessionId && p.IsActive)
            .Include(p => p.User)
            .OrderBy(p => p.JoinedAt)
            .ToListAsync();
    }

    public async Task<SessionParticipant> UpdateParticipantAsync(SessionParticipant participant)
    {
        participant.LastSeenAt = DateTime.UtcNow;
        _context.SessionParticipants.Update(participant);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Participant updated: {UserId} in session {SessionId}",
            participant.UserId, participant.SessionId);
        return participant;
    }

    public async Task RemoveParticipantAsync(Guid sessionId, Guid userId)
    {
        var participant = await GetParticipantAsync(sessionId, userId);
        if (participant != null)
        {
            participant.IsActive = false;
            participant.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogDebug("Participant removed: {UserId} from session {SessionId}", userId, sessionId);
        }
    }

    #endregion

    #region Activity Tracking

    public async Task UpdateLastActivityAsync(Guid sessionId, DateTime? timestamp = null)
    {
        await _context.Sessions
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastActivity, timestamp ?? DateTime.UtcNow));
    }

    public async Task UpdateParticipantLastSeenAsync(Guid sessionId, Guid userId, DateTime? timestamp = null)
    {
        await _context.SessionParticipants
            .Where(p => p.SessionId == sessionId && p.UserId == userId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.LastSeenAt, timestamp ?? DateTime.UtcNow));
    }

    #endregion

    #region Statistics

    public async Task<int> GetSessionMessageCountAsync(Guid sessionId)
    {
        return await _context.Messages
            .CountAsync(m => m.SessionId == sessionId && !m.IsDeleted);
    }

    public async Task<int> GetActiveParticipantCountAsync(Guid sessionId)
    {
        return await _context.SessionParticipants
            .CountAsync(p => p.SessionId == sessionId && p.IsActive);
    }

    public async Task UpdateSessionStatsAsync(Guid sessionId)
    {
        var messageCount = await GetSessionMessageCountAsync(sessionId);
        var participantCount = await GetActiveParticipantCountAsync(sessionId);

        await _context.Sessions
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.MessageCount, messageCount)
                .SetProperty(x => x.ParticipantCount, participantCount)
                .SetProperty(x => x.LastActivity, DateTime.UtcNow));
    }

    #endregion

    #region Validation

    public async Task<bool> ExistsAsync(Guid sessionId)
    {
        return await _context.Sessions
            .AnyAsync(s => s.Id == sessionId && s.IsActive);
    }

    public async Task<bool> IsUserParticipantAsync(Guid sessionId, Guid userId)
    {
        return await _context.SessionParticipants
            .AnyAsync(p => p.SessionId == sessionId && p.UserId == userId && p.IsActive);
    }

    public async Task<bool> IsUserModeratorAsync(Guid sessionId, Guid userId)
    {
        return await _context.SessionParticipants
            .AnyAsync(p => p.SessionId == sessionId && p.UserId == userId &&
                          p.IsActive && p.IsModerator);
    }

    #endregion
}
