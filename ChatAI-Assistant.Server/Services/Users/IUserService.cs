using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;
using ChatAI_Assistant.Shared.DTOs;

namespace ChatAI_Assistant.Server.Services.Users;

public interface IUserService
{
    // User management
    Task<UserResponse> GetUserByIdAsync(Guid userId);
    Task<UserResponse> GetUserByUsernameAsync(string username);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid userId);

    // Authentication (simple username-based)
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(Guid userId);
    Task<UserResponse> GetOrCreateUserAsync(string username);

    // User preferences
    Task<UserPreferencesDto> GetUserPreferencesAsync(Guid userId);
    Task<UserPreferencesDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferencesRequest request);

    // User activities
    Task UpdateLastActivityAsync(Guid userId);
    Task<IEnumerable<UserSessionSummaryDto>> GetUserSessionHistoryAsync(Guid userId, int limit = 50);
    Task<UserStatsDto> GetUserStatsAsync(Guid userId);

    // Search & discovery
    Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm, int limit = 20);
    Task<IEnumerable<UserDto>> GetActiveUsersAsync(int limit = 100);
}