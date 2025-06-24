using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Server.Data.Entities;

[Table("UserPreferences")]
public class UserPreferences
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    // Préférences AI
    public AIProvider PreferredAIProvider { get; set; } = AIProvider.OpenAI;

    [StringLength(50)]
    public string? PreferredModel { get; set; }

    [Range(0.0, 2.0)]
    public double Temperature { get; set; } = 0.7;

    [Range(1, 4000)]
    public int MaxTokens { get; set; } = 1000;

    [StringLength(1000)]
    public string? SystemPrompt { get; set; }

    // Préférences interface
    [StringLength(20)]
    public string Theme { get; set; } = "light"; // light, dark, auto

    [StringLength(10)]
    public string Language { get; set; } = "fr"; // fr, en, es, etc.

    public bool EnableNotifications { get; set; } = true;
    public bool EnableSoundEffects { get; set; } = true;
    public bool ShowTypingIndicator { get; set; } = true;

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}