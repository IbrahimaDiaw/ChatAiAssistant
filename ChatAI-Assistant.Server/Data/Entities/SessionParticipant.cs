using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAI_Assistant.Server.Data.Entities;

[Table("SessionParticipants")]
public class SessionParticipant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime? LeftAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
    public bool IsModerator { get; set; } = false;

    // Rôle dans la session
    [StringLength(20)]
    public string Role { get; set; } = "participant"; // participant, moderator, admin

    // Statistiques du participant
    public int MessagesCount { get; set; } = 0;

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public ChatSession Session { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}