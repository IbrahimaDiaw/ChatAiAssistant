using ChatAI_Assistant.Server.Services.Users;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ChatAI_Assistant.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(LoginResponse.CreateValidationError(errors));
        }

        var response = await _userService.LoginAsync(request);

        if (!response.Succeeded)
        {
            return response.Message.Contains("required") ? BadRequest(response) : NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost("logout/{userId:guid}")]
    public async Task<IActionResult> Logout(Guid userId)
    {
        var success = await _userService.LogoutAsync(userId);

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.CreateError("Logout failed"));
        }

        return Ok(ApiResponse<bool>.CreateSuccess(true, "Logged out successfully"));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserResponse>> GetUser(Guid userId)
    {
        var response = await _userService.GetUserByIdAsync(userId);

        if (!response.Succeeded)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserResponse>> GetUserByUsername(string username)
    {
        var response = await _userService.GetUserByUsernameAsync(username);

        if (!response.Succeeded)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(UserResponse.CreateValidationError(errors));
        }

        var response = await _userService.CreateUserAsync(request);

        if (!response.Succeeded)
        {
            return response.Message.Contains("exists") ? Conflict(response) : BadRequest(response);
        }

        return CreatedAtAction(nameof(GetUser), new { userId = response.Data!.Id }, response);
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(UserResponse.CreateValidationError(errors));
        }

        var response = await _userService.UpdateUserAsync(userId, request);

        if (!response.Succeeded)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var success = await _userService.DeleteUserAsync(userId);

        if (!success)
        {
            return NotFound(ApiResponse<bool>.CreateError("User not found"));
        }

        return Ok(ApiResponse<bool>.CreateSuccess(true, "User deleted successfully"));
    }

    [HttpGet("{userId:guid}/preferences")]
    public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(Guid userId)
    {
        var preferences = await _userService.GetUserPreferencesAsync(userId);
        return Ok(ApiResponse<UserPreferencesDto>.CreateSuccess(preferences));
    }

    [HttpPut("{userId:guid}/preferences")]
    public async Task<ActionResult<UserPreferencesDto>> UpdateUserPreferences(
        Guid userId,
        [FromBody] UpdateUserPreferencesRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.CreateValidationError(errors));
        }

        var preferences = await _userService.UpdateUserPreferencesAsync(userId, request);
        return Ok(ApiResponse<UserPreferencesDto>.CreateSuccess(preferences));
    }

    [HttpGet("{userId:guid}/sessions")]
    public async Task<ActionResult<List<UserSessionSummaryDto>>> GetUserSessions(Guid userId, int limit = 50)
    {
        var sessions = await _userService.GetUserSessionHistoryAsync(userId, limit);
        return Ok(ApiResponse<List<UserSessionSummaryDto>>.CreateSuccess(sessions.ToList()));
    }

    [HttpGet("{userId:guid}/stats")]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(Guid userId)
    {
        var stats = await _userService.GetUserStatsAsync(userId);
        return Ok(ApiResponse<UserStatsDto>.CreateSuccess(stats));
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string searchTerm, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(ApiResponse<List<UserDto>>.CreateError("Search term is required"));
        }

        var users = await _userService.SearchUsersAsync(searchTerm, limit);
        return Ok(ApiResponse<List<UserDto>>.CreateSuccess(users.ToList()));
    }

    [HttpPost("{userId:guid}/activity")]
    public async Task<IActionResult> UpdateLastActivity(Guid userId)
    {
        await _userService.UpdateLastActivityAsync(userId);
        return Ok(ApiResponse<bool>.CreateSuccess(true, "Activity updated"));
    }
}