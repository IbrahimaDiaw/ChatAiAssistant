using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAI_Assistant.Server.Data.Entities;

[Table("ChatSessions")]
public class ChatSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    // Créateur de la session
    [Required]
    public Guid CreatedByUserId { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
    public bool IsPrivate { get; set; } = false;

    // Métadonnées de session
    public int MessageCount { get; set; } = 0;
    public int ParticipantCount { get; set; } = 0;

    // Configuration AI pour cette session
    public AIProvider? SessionAIProvider { get; set; }

    [StringLength(50)]
    public string? SessionAIModel { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? SessionContext { get; set; } // Context spécifique à la session

    // Navigation properties
    [ForeignKey(nameof(CreatedByUserId))]
    public User CreatedBy { get; set; } = null!;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
}