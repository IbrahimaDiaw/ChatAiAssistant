using ChatAI_Assistant.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations;

public class SessionParticipantConfiguration : IEntityTypeConfiguration<SessionParticipant>
{
    public void Configure(EntityTypeBuilder<SessionParticipant> builder)
    {
        builder.ToTable("SessionParticipants");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.SessionId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.JoinedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.LeftAt)
            .HasColumnType("datetime2");

        builder.Property(p => p.LastSeenAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.IsModerator)
            .HasDefaultValue(false);

        builder.Property(p => p.Role)
            .HasMaxLength(20)
            .HasDefaultValue("participant");

        builder.Property(p => p.MessagesCount)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(p => new { p.SessionId, p.UserId })
            .IsUnique()
            .HasDatabaseName("IX_SessionParticipants_Session_User");

        builder.HasIndex(p => new { p.SessionId, p.IsActive })
            .HasDatabaseName("IX_SessionParticipants_Session_Active");

        builder.HasIndex(p => new { p.UserId, p.IsActive })
            .HasDatabaseName("IX_SessionParticipants_User_Active");

        builder.HasIndex(p => p.LastSeenAt)
            .HasDatabaseName("IX_SessionParticipants_LastSeenAt");

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_SessionParticipants_Role",
            "[Role] IN ('participant', 'moderator', 'admin')"));
    }
}