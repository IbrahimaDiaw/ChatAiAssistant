using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.Constants
{
    public static class ChatConstants
    {
        // Message limits
        public const int MaxMessageLength = 1000;
        public const int MaxMessagesPerSession = 1000;
        public const int RecentMessagesCount = 50;

        // Session settings
        public const int SessionTimeoutMinutes = 30;
        public const int MaxUsersPerSession = 10;

        // AI settings
        public const int AIResponseTimeoutSeconds = 30;
        public const string DefaultAIUsername = "AI Assistant";

        // Cache settings
        public const string RedisCacheKeyPrefix = "chat:";
        public const int CacheExpirationHours = 24;

        // Default messages
        public const string WelcomeMessage = "Welcome to the chat! How can I help you today?";
        public const string ErrorMessage = "Sorry, something went wrong. Please try again.";
        public const string AIUnavailableMessage = "AI service is temporarily unavailable. Please try again later.";
    }
}
