using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen.Blazor;

namespace ChatAI_Assistant.Client.Components
{
    public partial class ChatInput
    {
        [Parameter] public EventCallback<string> OnSendMessage { get; set; }
        [Parameter] public bool IsDisabled { get; set; }

        private string messageText = string.Empty;
        private bool showTypingIndicator = false;
        private RadzenTextArea textAreaRef = new();
        private System.Timers.Timer? typingTimer;

        protected override void OnInitialized()
        {
            // Initialize typing indicator timer
            typingTimer = new System.Timers.Timer(1000); // 1 second delay
            typingTimer.Elapsed += OnTypingTimerElapsed;
            typingTimer.AutoReset = false;
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            // Handle typing indicator
            if (!showTypingIndicator && !string.IsNullOrEmpty(messageText))
            {
                showTypingIndicator = true;
                StateHasChanged();
            }

            // Reset typing timer
            typingTimer?.Stop();
            typingTimer?.Start();

            // Handle send on Enter (but not Shift+Enter)
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await SendMessage();
            }
            // Auto-resize textarea
            else if (e.Key == "Enter" && e.ShiftKey)
            {
                await Task.Delay(10); // Small delay to let the text update
                await AutoResizeTextArea();
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(messageText) || IsDisabled)
                return;

            var message = messageText.Trim();
            messageText = string.Empty;
            showTypingIndicator = false;

            await OnSendMessage.InvokeAsync(message);
            await FocusTextArea();
            await AutoResizeTextArea();
        }

        private async Task AttachFile()
        {
            // TODO: Implement file attachment
            await JSRuntime.InvokeVoidAsync("alert", "File attachment coming soon!");
        }

        private string GetCharacterCountStyle()
        {
            var remaining = 2000 - messageText.Length;
            if (remaining < 100)
                return "rz-text-danger-color";
            else if (remaining < 200)
                return "rz-text-warning-color";
            else
                return "rz-text-secondary-color";
        }

        private async Task FocusTextArea()
        {
            try
            {
                await textAreaRef.Element.FocusAsync();
            }
            catch
            {
                // Ignore focus errors
            }
        }

        private async Task AutoResizeTextArea()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("autoResizeTextArea", textAreaRef.Element);
            }
            catch
            {
                // Ignore resize errors
            }
        }

        private void OnTypingTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            showTypingIndicator = false;
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            typingTimer?.Dispose();
        }
    }
}