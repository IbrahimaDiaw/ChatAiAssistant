using ChatAI_Assistant.Server.Data.Configurations;
using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ChatAI_Assistant.Server.Data
{
    public class ChatAiAssistantDbContext : DbContext
    {
        public ChatAiAssistantDbContext(DbContextOptions<ChatAiAssistantDbContext> options)
            : base(options)
        {
        }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<SessionParticipant> SessionParticipants { get; set; }
        public DbSet<SessionSettings> SessionSettings { get; set; }
        public DbSet<ConversationContext> ConversationContexts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
            modelBuilder.ApplyConfiguration(new ChatSessionConfiguration());
            modelBuilder.ApplyConfiguration(new SessionParticipantConfiguration());
            modelBuilder.ApplyConfiguration(new SessionSettingsConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationContextConfiguration());

            SeedData(modelBuilder);
        }

        public override int SaveChanges()
        {
            HandleAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditFields();
            return  base.SaveChangesAsync(cancellationToken);
        }

        private void HandleAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                    {
                        entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }
            }
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default session
            var defaultSessionId = Guid.NewGuid();
            var defaultSession = new ChatSession
            {
                Id = defaultSessionId,
                Name = "General Chat",
                Description = "Default chat session for testing",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true,
                MaxParticipants = 10
            };

            modelBuilder.Entity<ChatSession>().HasData(defaultSession);

            // Seed welcome message
            var welcomeMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = defaultSessionId,
                UserId = Guid.NewGuid(),
                Content = "Welcome to the AI Chat! I'm here to help you. How can I assist you today?",
                Type = MessageType.Welcome,
                IsFromAI = true,
                CreatedAt = DateTime.UtcNow,
                Timestamp = DateTime.UtcNow
            };

            modelBuilder.Entity<ChatMessage>().HasData(welcomeMessage);
        }
    }
}
