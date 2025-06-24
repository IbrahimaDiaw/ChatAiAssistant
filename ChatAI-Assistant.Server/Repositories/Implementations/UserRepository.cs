using ChatAI_Assistant.Server.Data;
using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatAI_Assistant.Server.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ChatAiAssistantDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ChatAiAssistantDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    public async Task<User> CreateAsync(User user)
    {
        if (user.Id == Guid.Empty)
            user.Id = Guid.NewGuid();

        user.CreatedAt = DateTime.UtcNow;
        user.LastActivity = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User created: {UserId} - {Username}", user.Id, user.Username);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.LastActivity = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogDebug("User updated: {UserId}", user.Id);
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("User deactivated: {UserId}", id);
        }
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username && u.IsActive);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(int limit = 100)
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.LastActivity)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> SearchByUsernameAsync(string searchTerm, int limit = 20)
    {
        return await _context.Users
            .Where(u => u.IsActive && u.Username.Contains(searchTerm))
            .OrderBy(u => u.Username)
            .Take(limit)
            .ToListAsync();
    }

    public async Task UpdateLastActivityAsync(Guid userId, DateTime timestamp)
    {
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.LastActivity, timestamp));
    }

    public async Task<int> GetUserMessageCountAsync(Guid userId)
    {
        return await _context.Messages
            .CountAsync(m => m.UserId == userId && !m.IsDeleted);
    }

    public async Task<int> GetUserSessionCountAsync(Guid userId)
    {
        return await _context.SessionParticipants
            .Where(p => p.UserId == userId)
            .Select(p => p.SessionId)
            .Distinct()
            .CountAsync();
    }

    public async Task UpdateUserStatsAsync(Guid userId)
    {
        var messageCount = await GetUserMessageCountAsync(userId);
        var sessionCount = await GetUserSessionCountAsync(userId);

        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.TotalMessages, messageCount)
                .SetProperty(x => x.TotalSessions, sessionCount)
                .SetProperty(x => x.LastActivity, DateTime.UtcNow));
    }

    public async Task<UserPreferences?> GetPreferencesAsync(Guid userId)
    {
        return await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserPreferences> CreateOrUpdatePreferencesAsync(UserPreferences preferences)
    {
        var existing = await GetPreferencesAsync(preferences.UserId);

        if (existing != null)
        {
            // Update existing
            existing.PreferredAIProvider = preferences.PreferredAIProvider;
            existing.PreferredModel = preferences.PreferredModel;
            existing.Temperature = preferences.Temperature;
            existing.MaxTokens = preferences.MaxTokens;
            existing.SystemPrompt = preferences.SystemPrompt;
            existing.Theme = preferences.Theme;
            existing.Language = preferences.Language;
            existing.EnableNotifications = preferences.EnableNotifications;
            existing.EnableSoundEffects = preferences.EnableSoundEffects;
            existing.ShowTypingIndicator = preferences.ShowTypingIndicator;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.UserPreferences.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // Create new
            if (preferences.Id == Guid.Empty)
                preferences.Id = Guid.NewGuid();

            preferences.UpdatedAt = DateTime.UtcNow;
            _context.UserPreferences.Add(preferences);
            await _context.SaveChangesAsync();
            return preferences;
        }
    }
}