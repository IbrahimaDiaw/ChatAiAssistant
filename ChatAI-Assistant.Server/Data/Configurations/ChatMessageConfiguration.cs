using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Id)
                .IsRequired();

            builder.Property(e => e.SessionId)
  
                .IsRequired();

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.Content)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(e => e.Timestamp)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.Type)
                .HasConversion<int>()
                .HasDefaultValue(MessageType.User);

            builder.Property(e => e.ConversationContext)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.ParentMessageId)
                .HasMaxLength(36);

            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                .HasColumnType("datetime2");

            // Indexes
            builder.HasIndex(e => e.SessionId)
                .HasDatabaseName("IX_ChatMessages_SessionId");

            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_ChatMessages_UserId");

            builder.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_ChatMessages_Timestamp");

            builder.HasIndex(e => new { e.SessionId, e.Timestamp })
                .HasDatabaseName("IX_ChatMessages_Session_Timestamp");

            // Relationships
            builder.HasOne(e => e.Session)
                .WithMany(s => s.Messages)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ParentMessage)
                .WithMany(m => m.Replies)
                .HasForeignKey(e => e.ParentMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Computed columns (not mapped)
            builder.Ignore(e => e.Session.ActiveParticipantCount);
            builder.Ignore(e => e.Session.TotalMessageCount);
            builder.Ignore(e => e.Session.LastMessageAt);
        }
    }
}
