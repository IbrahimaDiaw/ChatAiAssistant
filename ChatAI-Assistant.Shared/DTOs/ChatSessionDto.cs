using System.ComponentModel.DataAnnotations;

namespace ChatAI_Assistant.Shared.DTOs;

public class ChatSessionDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200, ErrorMessage = "Session name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public int MaxParticipants { get; set; } = 10;

    [Required]
    public string CreatedBy { get; set; } = string.Empty;

    // Audit fields from BaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Computed properties
    public int ActiveParticipantCount { get; set; } = 0;
    public int TotalMessageCount { get; set; } = 0;
    public DateTime? LastMessageAt { get; set; }

    // Related data
    public List<SessionParticipantDto> Participants { get; set; } = new();
    public List<ChatMessageDto> RecentMessages { get; set; } = new();
    public Dictionary<string, string> Settings { get; set; } = new();
}