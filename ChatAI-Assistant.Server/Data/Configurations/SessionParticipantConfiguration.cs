using ChatAI_Assistant.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations
{
    public class SessionParticipantConfiguration : IEntityTypeConfiguration<SessionParticipant>
    {
        public void Configure(EntityTypeBuilder<SessionParticipant> builder)
        {
            builder.ToTable("SessionParticipants");
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Id)
                .IsRequired();

            builder.Property(e => e.SessionId)
                .IsRequired();

            builder.Property(e => e.UserId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.DisplayName)
                .HasMaxLength(100);

            builder.Property(e => e.Avatar)
                .HasMaxLength(500);

            builder.Property(e => e.JoinedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.LeftAt)
                .HasColumnType("datetime2");

            builder.Property(e => e.LastSeen)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(e => e.SessionId)
                .HasDatabaseName("IX_Participants_SessionId");

            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Participants_UserId");

            builder.HasIndex(e => new { e.SessionId, e.UserId })
                .IsUnique()
                .HasDatabaseName("IX_Participants_Session_User");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Participants_IsActive");

            // Relationships
            builder.HasOne(e => e.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Computed columns (not mapped)
            builder.Ignore(e => e.SessionDuration);
            builder.Ignore(e => e.IsOnline);

        }
    }
}
