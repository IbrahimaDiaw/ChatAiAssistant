using ChatAI_Assistant.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.ToTable("ChatSessions");

        // Primary Key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.CreatedByUserId)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(s => s.LastActivity)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.IsPrivate)
            .HasDefaultValue(false);

        builder.Property(s => s.MessageCount)
            .HasDefaultValue(0);

        builder.Property(s => s.ParticipantCount)
            .HasDefaultValue(0);

        // AI Configuration
        builder.Property(s => s.SessionAIProvider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.SessionAIModel)
            .HasMaxLength(50);

        builder.Property(s => s.SessionContext)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(s => s.CreatedByUserId)
            .HasDatabaseName("IX_ChatSessions_CreatedByUserId");

        builder.HasIndex(s => s.LastActivity)
            .HasDatabaseName("IX_ChatSessions_LastActivity");

        builder.HasIndex(s => new { s.IsActive, s.LastActivity })
            .HasDatabaseName("IX_ChatSessions_Active_LastActivity");

        builder.HasIndex(s => new { s.CreatedByUserId, s.IsActive, s.LastActivity })
            .HasDatabaseName("IX_ChatSessions_User_Active_LastActivity");

        // Relationships
        builder.HasMany(s => s.Messages)
            .WithOne(m => m.Session)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Participants)
            .WithOne(p => p.Session)
            .HasForeignKey(p => p.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}