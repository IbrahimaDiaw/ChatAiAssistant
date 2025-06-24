using ChatAI_Assistant.Server.Configurations;
using ChatAI_Assistant.Server.Data;
using ChatAI_Assistant.Server.Hubs;
using ChatAI_Assistant.Server.Repositories.Implementations;
using ChatAI_Assistant.Server.Repositories.Interfaces;
using ChatAI_Assistant.Server.Services.AI;
using ChatAI_Assistant.Server.Services.Chat;
using ChatAI_Assistant.Server.Services.Sessions;
using ChatAI_Assistant.Server.Services.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ChatAI_Assistant.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ChatAiAssistantDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("defaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(30);
                    });
            });

            return services;
        }


        
        public static IServiceCollection AddChatServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration Bindings
            services.Configure<AISettings>(configuration.GetSection("AI"));
            services.Configure<OpenAISettings>(configuration.GetSection("AI:Providers:OpenAI"));
            services.Configure<AzureOpenAISettings>(configuration.GetSection("AI:Providers:AzureOpenAI"));
            services.Configure<ClaudeSettings>(configuration.GetSection("AI:Providers:Claude"));

            // Enregistrer AISettings comme service singleton
            services.AddSingleton<AISettings>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AISettings>>();
                return options.Value;
            });

            // Enregistrer les autres settings comme services
            services.AddSingleton<OpenAISettings>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OpenAISettings>>();
                return options.Value;
            });

            services.AddSingleton<AzureOpenAISettings>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AzureOpenAISettings>>();
                return options.Value;
            });

            services.AddSingleton<ClaudeSettings>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<ClaudeSettings>>();
                return options.Value;
            });
            // HttpClient Configuration
            services.AddHttpClient("OpenAI", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "ChatAI-POC/1.0");
            });

            services.AddHttpClient("AzureOpenAI", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "ChatAI-POC/1.0");
            });

            services.AddHttpClient("Claude", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Add("User-Agent", "ChatAI-POC/1.0");
            });

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();

            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<ISessionService, SessionService>();

            // AI Services Factory
            services.AddScoped<IAIServiceFactory, AIServiceFactory>();

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services, IHostEnvironment environment)
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = environment.IsDevelopment();
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
            });

            services.AddScoped<ChatHub>(); // Register ChatHub as a scope
            return services;
        }
    }
}
