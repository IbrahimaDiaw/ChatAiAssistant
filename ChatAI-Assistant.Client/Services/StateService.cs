using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Common;
using ChatAI_Assistant.Shared.Enums;

public class StateService : IStateService
{
    private UserDto? _currentUser;
    private ChatSessionDto? _currentSession;
    private List<ChatMessageDto> _messages = new();
    private bool _isLoading = false;
    private bool _isSendingMessage = false;
    private AIProvider _selectedAIProvider = AIProvider.OpenAI;

    public event Action? OnStateChanged;

    public UserDto? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentUser != null;

    public ChatSessionDto? CurrentSession => _currentSession;
    public bool HasActiveSession => _currentSession != null;

    public List<ChatMessageDto> Messages => _messages.ToList(); // Return copy to prevent external modification
    public bool IsLoading => _isLoading;
    public bool IsSendingMessage => _isSendingMessage;
    public AIProvider SelectedAIProvider => _selectedAIProvider;

    public void SetUser(UserDto user)
    {
        _currentUser = user;
        NotifyStateChanged();
    }

    public void SetSession(ChatSessionDto session)
    {
        _currentSession = session;
        _selectedAIProvider = session.SessionAIProvider.Value;
        NotifyStateChanged();
    }

    public void AddMessage(ChatMessageDto message)
    {
        _messages.Add(message);
        NotifyStateChanged();
    }

    public void SetMessages(List<ChatMessageDto> messages)
    {
        _messages = messages.ToList();
        NotifyStateChanged();
    }

    public void SetLoading(bool isLoading)
    {
        _isLoading = isLoading;
        NotifyStateChanged();
    }

    public void SetSendingMessage(bool isSending)
    {
        _isSendingMessage = isSending;
        NotifyStateChanged();
    }

    public void SetAIProvider(AIProvider provider)
    {
        _selectedAIProvider = provider;
        NotifyStateChanged();
    }

    public void ClearMessages()
    {
        _messages.Clear();
        NotifyStateChanged();
    }

    public void ClearSession()
    {
        _currentSession = null;
        _messages.Clear();
        NotifyStateChanged();
    }

    public void ClearAll()
    {
        _currentUser = null;
        _currentSession = null;
        _messages.Clear();
        _isLoading = false;
        _isSendingMessage = false;
        _selectedAIProvider = AIProvider.OpenAI;
        NotifyStateChanged();
    }

    public void RemoveMessage(Guid messageId)
    {
        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message != null)
        {
            _messages.Remove(message);
            NotifyStateChanged();
        }
    }

    public void UpdateMessage(ChatMessageDto message)
    {
        var index = _messages.FindIndex(m => m.Id == message.Id);
        if (index >= 0)
        {
            _messages[index] = message;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}