using ChatAI_Assistant.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace ChatAI_Assistant.Shared.DTOs;

public class ChatSessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrivate { get; set; }
    public int MessageCount { get; set; }
    public int ParticipantCount { get; set; }
    public AIProvider? SessionAIProvider { get; set; }
    public string? SessionAIModel { get; set; }
    public List<ChatMessageDto> RecentMessages { get; set; } = new();
    public List<SessionParticipantDto> Participants { get; set; } = new();
}