using System.ComponentModel.DataAnnotations;

namespace ChatAI_Assistant.Shared.DTOs;

public class SessionParticipantDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsModerator { get; set; }
    public string Role { get; set; } = "User";
    public int MessagesCount { get; set; }
    public bool IsOnline { get; set; } // Calculé côté client
}