using ChatAI_Assistant.Server.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAI_Assistant.Server.Data.Entities;

[Table("Users")]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [StringLength(200)]
    public string? Avatar { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // User statistics
    public int TotalMessages { get; set; } = 0;
    public int TotalSessions { get; set; } = 0;

    // Navigation properties
    public ICollection<ChatSession> Sessions { get; set; } = new List<ChatSession>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<SessionParticipant> Participations { get; set; } = new List<SessionParticipant>();
    public UserPreferences? Preferences { get; set; }
}