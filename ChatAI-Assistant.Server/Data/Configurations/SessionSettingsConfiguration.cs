using ChatAI_Assistant.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAI_Assistant.Server.Data.Configurations
{
    public class SessionSettingsConfiguration : IEntityTypeConfiguration<SessionSettings>
    {
        public void Configure(EntityTypeBuilder<SessionSettings> builder)
        {
            builder.ToTable("SessionSettings");
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Id)
                .IsRequired();

            builder.Property(e => e.SessionId)
                .IsRequired();

            builder.Property(e => e.Key)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Value)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                .HasColumnType("datetime2");

            // Indexes
            builder.HasIndex(e => e.SessionId)
                .HasDatabaseName("IX_SessionSettings_SessionId");

            builder.HasIndex(e => new { e.SessionId, e.Key })
                .IsUnique()
                .HasDatabaseName("IX_SessionSettings_Session_Key");

            // Relationships
            builder.HasOne(e => e.Session)
                .WithMany(s => s.Settings)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
