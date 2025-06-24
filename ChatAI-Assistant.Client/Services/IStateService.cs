using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.Enums;

public interface IStateService
{
    event Action? OnStateChanged;

    // User state
    UserDto? CurrentUser { get; }
    bool IsLoggedIn { get; }

    // Session state
    ChatSessionDto? CurrentSession { get; }
    bool HasActiveSession { get; }

    // Chat state
    List<ChatMessageDto> Messages { get; }
    bool IsLoading { get; }
    bool IsSendingMessage { get; }
    AIProvider SelectedAIProvider { get; }

    // Methods
    void SetUser(UserDto user);
    void SetSession(ChatSessionDto session);
    void AddMessage(ChatMessageDto message);
    void SetMessages(List<ChatMessageDto> messages);
    void SetLoading(bool isLoading);
    void SetSendingMessage(bool isSending);
    void SetAIProvider(AIProvider provider);
    void ClearMessages();
    void ClearSession();
    void ClearAll();
    void RemoveMessage(Guid messageId);
    void UpdateMessage(ChatMessageDto message);
}