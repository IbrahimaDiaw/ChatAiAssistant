using ChatAI_Assistant.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations
{
    public class ConversationContextConfiguration : IEntityTypeConfiguration<ConversationContext>
    {
        public void Configure(EntityTypeBuilder<ConversationContext> builder)
        {
            builder.ToTable("ConversationContexts");
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Id)
                .IsRequired();

            builder.Property(e => e.SessionId)
                .IsRequired();

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.ContextData)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ExpiresAt)
                .HasColumnType("datetime2");

            // Indexes
            builder.HasIndex(e => e.SessionId)
                .HasDatabaseName("IX_ConversationContexts_SessionId");

            builder.HasIndex(e => new { e.SessionId, e.UserId })
                .IsUnique()
                .HasDatabaseName("IX_ConversationContexts_Session_User");

            builder.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_ConversationContexts_ExpiresAt");

            // Relationships
            builder.HasOne(e => e.Session)
                .WithMany()
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
