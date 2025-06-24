using ChatAI_Assistant.Server.Data.Entities;

namespace ChatAI_Assistant.Server.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Basic CRUD
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(Guid id);

        // Specific queries
        Task<bool> ExistsByUsernameAsync(string username);
        Task<IEnumerable<User>> GetActiveUsersAsync(int limit = 100);
        Task<IEnumerable<User>> SearchByUsernameAsync(string searchTerm, int limit = 20);
        Task UpdateLastActivityAsync(Guid userId, DateTime timestamp);

        // Statistics
        Task<int> GetUserMessageCountAsync(Guid userId);
        Task<int> GetUserSessionCountAsync(Guid userId);
        Task UpdateUserStatsAsync(Guid userId);

        // Preferences
        Task<UserPreferences?> GetPreferencesAsync(Guid userId);
        Task<UserPreferences> CreateOrUpdatePreferencesAsync(UserPreferences preferences);
    }
}
