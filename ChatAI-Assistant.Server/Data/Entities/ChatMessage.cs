using ChatAI_Assistant.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAI_Assistant.Server.Data.Entities;

[Table("ChatMessages")]
public class ChatMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Content { get; set; } = string.Empty;

    [Column(TypeName = "datetime2")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public MessageType Type { get; set; } = MessageType.User;

    public bool IsFromAI { get; set; } = false;

    // Métadonnées du message
    public Guid? ParentMessageId { get; set; } // Pour les réponses/threads

    [StringLength(100)]
    public string? MessageHash { get; set; } // Pour détecter les doublons

    // Informations AI (si message AI)
    public AIProvider? AIProvider { get; set; }

    [StringLength(50)]
    public string? AIModel { get; set; }

    public int? TokensUsed { get; set; }
    public double? AITemperature { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? AIContext { get; set; } 

    // Statut du message
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    [Column(TypeName = "datetime2")]
    public DateTime? EditedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? DeletedAt { get; set; }

    // Métadonnées additionnelles
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public ChatSession Session { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(ParentMessageId))]
    public ChatMessage? ParentMessage { get; set; }

    public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
}