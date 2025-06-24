using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Server.Data.Configurations;

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("UserPreferences");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.UserId)
            .IsRequired();

        // AI Preferences
        builder.Property(p => p.PreferredAIProvider)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(AIProvider.OpenAI);

        builder.Property(p => p.PreferredModel)
            .HasMaxLength(50);

        builder.Property(p => p.Temperature)
            .HasDefaultValue(0.7)
            .HasPrecision(3, 2);

        builder.Property(p => p.MaxTokens)
            .HasDefaultValue(1000);

        builder.Property(p => p.SystemPrompt)
            .HasMaxLength(1000);

        // UI Preferences
        builder.Property(p => p.Theme)
            .HasMaxLength(20)
            .HasDefaultValue("light");

        builder.Property(p => p.Language)
            .HasMaxLength(10)
            .HasDefaultValue("fr");

        builder.Property(p => p.EnableNotifications)
            .HasDefaultValue(true);

        builder.Property(p => p.EnableSoundEffects)
            .HasDefaultValue(true);

        builder.Property(p => p.ShowTypingIndicator)
            .HasDefaultValue(true);

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserPreferences_UserId");

        builder.HasIndex(p => p.PreferredAIProvider)
            .HasDatabaseName("IX_UserPreferences_AIProvider");

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_UserPreferences_Temperature",
            "[Temperature] >= 0.0 AND [Temperature] <= 2.0"));

        builder.ToTable(t => t.HasCheckConstraint("CK_UserPreferences_MaxTokens",
            "[MaxTokens] >= 1 AND [MaxTokens] <= 4000"));
    }
}