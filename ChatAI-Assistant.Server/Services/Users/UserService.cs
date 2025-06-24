using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Server.Mapping.Extensions;
using ChatAI_Assistant.Server.Repositories.Interfaces;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;

namespace ChatAI_Assistant.Server.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return UserResponse.NotFound($"User with ID {userId} not found");
        }

        var userDto = user.ToDto();
        return UserResponse.Success(userDto);
    }

    public async Task<UserResponse> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return UserResponse.BadRequest("Username cannot be empty");
        }

        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            return UserResponse.NotFound($"User with username '{username}' not found");
        }

        var userDto = user.ToDto();
        return UserResponse.Success(userDto);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return UserResponse.BadRequest("Username is required");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username))
        {
            return UserResponse.Conflict($"Username '{request.Username}' already exists");
        }

        try
        {
            // Create user entity
            var user = new User
            {
                Username = request.Username,
                DisplayName = request.DisplayName ?? request.Username,
                Avatar = request.Avatar
            };

            // Save user
            var createdUser = await _userRepository.CreateAsync(user);

            // Create default preferences
            var preferences = new UserPreferences
            {
                UserId = createdUser.Id,
                PreferredAIProvider = Shared.Enums.AIProvider.OpenAI,
                Temperature = 0.7,
                MaxTokens = 1000,
                Theme = "light",
                Language = "fr"
            };

            await _userRepository.CreateOrUpdatePreferencesAsync(preferences);

            _logger.LogInformation("User created successfully: {UserId} - {Username}",
                createdUser.Id, createdUser.Username);

            var userDto = createdUser.ToDto();
            return UserResponse.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", request.Username);
            return UserResponse.Error("Failed to create user");
        }
    }

    public async Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return UserResponse.NotFound($"User with ID {userId} not found");
        }

        try
        {
            // Update properties
            if (!string.IsNullOrWhiteSpace(request.DisplayName))
                user.DisplayName = request.DisplayName;

            if (!string.IsNullOrWhiteSpace(request.Avatar))
                user.Avatar = request.Avatar;

            var updatedUser = await _userRepository.UpdateAsync(user);
            var userDto = updatedUser.ToDto();

            _logger.LogDebug("User updated: {UserId}", userId);
            return UserResponse.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return UserResponse.Error("Failed to update user");
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            await _userRepository.DeleteAsync(userId);
            _logger.LogInformation("User deleted: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return false;
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return LoginResponse.BadRequest("Username is required");
        }

        try
        {
            var userResponse = await GetOrCreateUserAsync(request.Username);
            if (userResponse == null)
            {
                return LoginResponse.Error("Login failed");
            }

            var user = userResponse.Data;
            // Update last activity
            await UpdateLastActivityAsync(user.Id);

            var loginDto = new LoginDto
            {
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Avatar = user.Avatar,
                LoginTimestamp = DateTime.UtcNow
            };

            _logger.LogInformation("User logged in: {UserId} - {Username}",
                user.Id, user.Username);

            return LoginResponse.Success(loginDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return LoginResponse.Error("Login failed");
        }
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        try
        {
            await UpdateLastActivityAsync(userId);
            _logger.LogInformation("User logged out: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserResponse> GetOrCreateUserAsync(string username)
    {
        // Try to get existing user
        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser != null)
        {
            var userDto = existingUser.ToDto();
            return UserResponse.Success(userDto);
        }

        // Create new user
        var createRequest = new CreateUserRequest
        {
            Username = username,
            DisplayName = username
        };

        return await CreateUserAsync(createRequest);
    }

    public async Task<UserPreferencesDto> GetUserPreferencesAsync(Guid userId)
    {
        var preferences = await _userRepository.GetPreferencesAsync(userId);

        if (preferences == null)
        {
            // Create default preferences
            preferences = new UserPreferences
            {
                UserId = userId,
                PreferredAIProvider = Shared.Enums.AIProvider.OpenAI,
                Temperature = 0.7,
                MaxTokens = 1000,
                Theme = "light",
                Language = "en"
            };

            await _userRepository.CreateOrUpdatePreferencesAsync(preferences);
        }

        return preferences.ToDto();
    }

    public async Task<UserPreferencesDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferencesRequest request)
    {
        var preferences = await _userRepository.GetPreferencesAsync(userId) ?? new UserPreferences
        {
            UserId = userId
        };

        // Update from request
        preferences.PreferredAIProvider = request.PreferredAIProvider;
        preferences.PreferredModel = request.PreferredModel;
        preferences.Temperature = request.Temperature;
        preferences.MaxTokens = request.MaxTokens;
        preferences.SystemPrompt = request.SystemPrompt;
        preferences.Theme = request.Theme;
        preferences.Language = request.Language;
        preferences.EnableNotifications = request.EnableNotifications;
        preferences.EnableSoundEffects = request.EnableSoundEffects;
        preferences.ShowTypingIndicator = request.ShowTypingIndicator;

        var updatedPreferences = await _userRepository.CreateOrUpdatePreferencesAsync(preferences);

        _logger.LogDebug("User preferences updated: {UserId}", userId);
        return updatedPreferences.ToDto();
    }

    public async Task UpdateLastActivityAsync(Guid userId)
    {
        await _userRepository.UpdateLastActivityAsync(userId, DateTime.UtcNow);
    }

    public async Task<IEnumerable<UserSessionSummaryDto>> GetUserSessionHistoryAsync(Guid userId, int limit = 50)
    {
        var sessions = await _sessionRepository.GetUserSessionsAsync(userId, limit);
        return sessions.Select(s => s.ToSessionSummaryDto());
    }

    public async Task<UserStatsDto> GetUserStatsAsync(Guid userId)
    {
        var messageCount = await _userRepository.GetUserMessageCountAsync(userId);
        var sessionCount = await _userRepository.GetUserSessionCountAsync(userId);

        // Update cached stats
        await _userRepository.UpdateUserStatsAsync(userId);

        return new UserStatsDto
        {
            UserId = userId,
            TotalMessages = messageCount,
            TotalSessions = sessionCount,
            LastActivity = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Array.Empty<UserDto>();
        }

        var users = await _userRepository.SearchByUsernameAsync(searchTerm, limit);
        return users.Select(u => u.ToDto());
    }

    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync(int limit = 100)
    {
        var users = await _userRepository.GetActiveUsersAsync(limit);
        return users.Select(u => u.ToDto());
    }
}