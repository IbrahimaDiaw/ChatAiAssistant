using ChatAI_Assistant.Server.Data.Configurations;
using ChatAI_Assistant.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatAI_Assistant.Server.Data;

public class ChatAiAssistantDbContext : DbContext
{
    public ChatAiAssistantDbContext(DbContextOptions<ChatAiAssistantDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserPreferences> UserPreferences { get; set; } = null!;
    public DbSet<ChatSession> Sessions { get; set; } = null!;
    public DbSet<SessionParticipant> SessionParticipants { get; set; } = null!;
    public DbSet<ChatMessage> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserPreferencesConfiguration());
        modelBuilder.ApplyConfiguration(new ChatSessionConfiguration());
        modelBuilder.ApplyConfiguration(new SessionParticipantConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());

        // Global configurations
        ConfigureGlobalSettings(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureGlobalSettings(ModelBuilder modelBuilder)
    {
        // Configuration globale pour tous les Guid Id
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty?.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
            }
        }

        // Configuration globale pour les DateTime
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("datetime2");
        }
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Données de test pour développement
        var testUserId = Guid.NewGuid();
        var testSessionId = Guid.NewGuid();
        var testParticipantId = Guid.NewGuid();
        var testPreferencesId = Guid.NewGuid();

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = testUserId,
                Username = "testuser",
                DisplayName = "Test User",
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true
            }
        );

        modelBuilder.Entity<UserPreferences>().HasData(
            new UserPreferences
            {
                Id = testPreferencesId,
                UserId = testUserId,
                PreferredAIProvider = Shared.Enums.AIProvider.OpenAI,
                Temperature = 0.7,
                MaxTokens = 1000,
                Theme = "light",
                Language = "fr",
                UpdatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<ChatSession>().HasData(
            new ChatSession
            {
                Id = testSessionId,
                Title = "Session de Test",
                Description = "Session de test pour développement",
                CreatedByUserId = testUserId,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true,
                MessageCount = 0,
                ParticipantCount = 1
            }
        );

        modelBuilder.Entity<SessionParticipant>().HasData(
            new SessionParticipant
            {
                Id = testParticipantId,
                SessionId = testSessionId,
                UserId = testUserId,
                JoinedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow,
                IsActive = true,
                Role = "admin"
            }
        );
    }
}