using ChatAI_Assistant.Server.Data.Entities;
using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.DTOs.Requests;

namespace ChatAI_Assistant.Server.Mapping.Extensions
{
    public static class ChatMappingExtensions
    {
        #region Entity to DTO

        public static ChatMessageDto ToDto(this ChatMessage message)
        {
            return new ChatMessageDto
            {
                Id = message.Id,
                SessionId = message.SessionId,
                UserId = message.UserId,
                Content = message.Content,
                Timestamp = message.Timestamp,
                Type = message.Type,
                IsFromAI = message.IsFromAI,
                ParentMessageId = message.ParentMessageId,
                AIProvider = message.AIProvider,
                AIModel = message.AIModel,
                TokensUsed = message.TokensUsed,
                Username = message.User?.Username ?? "",
                UserDisplayName = message.User?.DisplayName,
                IsEdited = message.IsEdited,
                IsDeleted = message.IsDeleted,
                EditedAt = message.EditedAt
            };
        }

        public static MessageSummaryDto ToSummaryDto(this ChatMessage message)
        {
            return new MessageSummaryDto
            {
                Id = message.Id,
                Content = message.Content.Length > 100 ?
                    message.Content.Substring(0, 97) + "..." :
                    message.Content,
                Timestamp = message.Timestamp,
                IsFromAI = message.IsFromAI,
                Username = message.User?.Username ?? ""
            };
        }

        public static ConversationContextDto ToContextDto(this IEnumerable<ChatMessage> messages)
        {
            return new ConversationContextDto
            {
                Messages = messages.Select(m => m.ToDto()).ToList(),
                TotalMessages = messages.Count(),
                OldestMessageTimestamp = messages.Min(m => m.Timestamp),
                NewestMessageTimestamp = messages.Max(m => m.Timestamp)
            };
        }

        #endregion

        #region DTO to Entity

        public static ChatMessage ToEntity(this SendMessageRequest request)
        {
            return new ChatMessage
            {
                SessionId = request.SessionId,
                UserId = request.UserId,
                Content = request.Content,
                AIProvider = request.PreferredAIProvider,
                IsFromAI = false
            };
        }

        public static ChatMessage ToAIEntity(this AIResponse aiResponse, Guid sessionId, Guid aiUserId)
        {
            return new ChatMessage
            {
                SessionId = sessionId,
                UserId = aiUserId, // Special AI user ID
                Content = aiResponse.Content,
                Type = Shared.Enums.MessageType.AI,
                IsFromAI = true,
                AIProvider = aiResponse.Provider,
                AIModel = aiResponse.Model,
                TokensUsed = aiResponse.TokensUsed,
                AITemperature = aiResponse.Temperature,
                AIContext = aiResponse.Context
            };
        }

        public static void UpdateFromRequest(this ChatMessage message, UpdateMessageRequest request)
        {
            message.Content = request.Content;
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
