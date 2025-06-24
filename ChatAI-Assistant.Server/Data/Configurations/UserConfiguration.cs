using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAI_Assistant.Server.Data.Entities;

namespace ChatAI_Assistant.Server.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100);

        builder.Property(u => u.Avatar)
            .HasMaxLength(200);

        builder.Property(u => u.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.LastActivity)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.TotalMessages)
            .HasDefaultValue(0);

        builder.Property(u => u.TotalSessions)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        builder.HasIndex(u => u.LastActivity)
            .HasDatabaseName("IX_Users_LastActivity");

        builder.HasIndex(u => new { u.IsActive, u.LastActivity })
            .HasDatabaseName("IX_Users_Active_LastActivity");

        // Relationships
        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.CreatedBy)
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Messages)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Participations)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Preferences)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}