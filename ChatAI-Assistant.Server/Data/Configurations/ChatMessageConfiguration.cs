using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        // Primary Key
        builder.HasKey(m => m.Id);

        // Properties
        builder.Property(m => m.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(m => m.SessionId)
            .IsRequired();

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.Content)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(m => m.Timestamp)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(MessageType.User);

        builder.Property(m => m.IsFromAI)
            .HasDefaultValue(false);

        // Message Metadata
        builder.Property(m => m.ParentMessageId);

        builder.Property(m => m.MessageHash)
            .HasMaxLength(100);

        // AI Metadata
        builder.Property(m => m.AIProvider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.AIModel)
            .HasMaxLength(50);

        builder.Property(m => m.AITemperature)
            .HasPrecision(3, 2);

        builder.Property(m => m.AIContext)
            .HasColumnType("nvarchar(max)");

        // Message Status
        builder.Property(m => m.IsEdited)
            .HasDefaultValue(false);

        builder.Property(m => m.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(m => m.EditedAt)
            .HasColumnType("datetime2");

        builder.Property(m => m.DeletedAt)
            .HasColumnType("datetime2");

        builder.Property(m => m.Metadata)
            .HasColumnType("nvarchar(max)");

        // Indexes pour performance
        builder.HasIndex(m => new { m.SessionId, m.Timestamp })
            .HasDatabaseName("IX_ChatMessages_Session_Timestamp");

        builder.HasIndex(m => m.Timestamp)
            .HasDatabaseName("IX_ChatMessages_Timestamp");

        builder.HasIndex(m => new { m.UserId, m.Timestamp })
            .HasDatabaseName("IX_ChatMessages_User_Timestamp");

        builder.HasIndex(m => new { m.SessionId, m.IsDeleted, m.Timestamp })
            .HasDatabaseName("IX_ChatMessages_Session_NotDeleted_Timestamp");

        builder.HasIndex(m => m.MessageHash)
            .HasDatabaseName("IX_ChatMessages_MessageHash");

        builder.HasIndex(m => new { m.IsFromAI, m.AIProvider })
            .HasDatabaseName("IX_ChatMessages_AI_Provider");

        // Relationships
        builder.HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ParentMessage)
            .WithMany(m => m.Replies)
            .HasForeignKey(m => m.ParentMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_ChatMessages_AITemperature",
            "[AITemperature] IS NULL OR ([AITemperature] >= 0.0 AND [AITemperature] <= 2.0)"));

        builder.ToTable(t => t.HasCheckConstraint("CK_ChatMessages_TokensUsed",
            "[TokensUsed] IS NULL OR [TokensUsed] > 0"));
    }
}