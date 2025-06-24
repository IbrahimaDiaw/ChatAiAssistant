using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.Enums;

namespace ChatAI_Assistant.Client.Models;

public class ChatState
{
    public UserDto? CurrentUser { get; set; }
    public ChatSessionDto? CurrentSession { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
    public bool IsLoading { get; set; }
    public bool IsSendingMessage { get; set; }
    public bool IsConnected { get; set; }
    public AIProvider SelectedAIProvider { get; set; } = AIProvider.OpenAI;
    public string? LastError { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public bool IsLoggedIn => CurrentUser != null;
    public bool HasActiveSession => CurrentSession != null;
    public int MessageCount => Messages.Count;
    public int UserMessageCount => Messages.Count(m => !m.IsFromAI);
    public int AssistantMessageCount => Messages.Count(m => m.IsFromAI);

    public void Reset()
    {
        CurrentUser = null;
        CurrentSession = null;
        Messages.Clear();
        IsLoading = false;
        IsSendingMessage = false;
        IsConnected = false;
        SelectedAIProvider = AIProvider.OpenAI;
        LastError = null;
        LastUpdated = DateTime.UtcNow;
    }

    public void SetUser(UserDto user)
    {
        CurrentUser = user;
        LastUpdated = DateTime.UtcNow;
    }

    public void SetSession(ChatSessionDto session)
    {
        CurrentSession = session;
        SelectedAIProvider = session.SessionAIProvider.Value;
        LastUpdated = DateTime.UtcNow;
    }

    public void AddMessage(ChatMessageDto message)
    {
        Messages.Add(message);
        LastUpdated = DateTime.UtcNow;
    }

    public void RemoveMessage(Guid messageId)
    {
        var message = Messages.FirstOrDefault(m => m.Id == messageId);
        if (message != null)
        {
            Messages.Remove(message);
            LastUpdated = DateTime.UtcNow;
        }
    }

    public void UpdateMessage(ChatMessageDto updatedMessage)
    {
        var index = Messages.FindIndex(m => m.Id == updatedMessage.Id);
        if (index >= 0)
        {
            Messages[index] = updatedMessage;
            LastUpdated = DateTime.UtcNow;
        }
    }

    public void ClearMessages()
    {
        Messages.Clear();
        LastUpdated = DateTime.UtcNow;
    }

    public ChatMessageDto? GetLastMessage()
    {
        return Messages.LastOrDefault();
    }

    public ChatMessageDto? GetLastUserMessage()
    {
        return Messages.LastOrDefault(m => !m.IsFromAI);
    }

    public ChatMessageDto? GetLastAssistantMessage()
    {
        return Messages.LastOrDefault(m => m.IsFromAI);
    }

    public List<ChatMessageDto> GetRecentMessages(int count = 10)
    {
        return Messages.TakeLast(count).ToList();
    }

    public List<ChatMessageDto> GetMessagesByProvider(AIProvider provider)
    {
        return Messages.Where(m => m.AIProvider == provider).ToList();
    }
}