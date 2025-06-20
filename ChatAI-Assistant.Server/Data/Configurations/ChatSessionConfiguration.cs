using ChatAI_Assistant.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations
{
    public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.ToTable("ChatSessions");
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Id)
                .IsRequired();

            builder.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.LastActivity)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.MaxParticipants)
                .HasDefaultValue(10);

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .HasColumnType("datetime2");

            // Indexes
            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_ChatSessions_CreatedAt");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_ChatSessions_IsActive");

            builder.HasIndex(e => e.CreatedBy)
                .HasDatabaseName("IX_ChatSessions_CreatedBy");

            // Computed columns (not mapped to database)
            builder.Ignore(e => e.ActiveParticipantCount);
            builder.Ignore(e => e.TotalMessageCount);
            builder.Ignore(e => e.LastMessageAt);
        }
    }
}
