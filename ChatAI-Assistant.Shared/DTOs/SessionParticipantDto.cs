using System.ComponentModel.DataAnnotations;

namespace ChatAI_Assistant.Shared.DTOs;

public class SessionParticipantDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
    public string? DisplayName { get; set; }

    public string? Avatar { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LeftAt { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsAdmin { get; set; } = false;

    public bool CanSendMessages { get; set; } = true;

    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    // Computed properties
    public TimeSpan SessionDuration { get; set; } = TimeSpan.Zero;
    public bool IsOnline { get; set; } = false;

    // Display helpers
    public string DisplayText => DisplayName ?? Username;
    public string StatusText => IsOnline ? "Online" : $"Last seen {LastSeen:HH:mm}";
}