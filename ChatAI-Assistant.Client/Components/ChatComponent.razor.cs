using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace ChatAI_Assistant.Client.Components
{
    public partial class ChatComponent
    {
        private ElementReference messagesContainer;

        protected override async Task OnInitializedAsync()
        {
            StateService.OnStateChanged += OnStateChanged;

            if (StateService.HasActiveSession)
            {
                await LoadMessages();
            }
        }

        private async void OnStateChanged()
        {
            await InvokeAsync(StateHasChanged);
            await ScrollToBottom();
        }

        private async Task LoadMessages()
        {
            if (!StateService.HasActiveSession) return;

            StateService.SetLoading(true);

            try
            {
                var response = await ChatApiService.GetMessagesAsync(StateService.CurrentSession!.Id, 50);

                if (response.Succeeded && response.Data != null)
                {
                    StateService.SetMessages(response.Data);
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "Loading Messages",
                        Detail = response.Message ?? "Could not load messages"
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message
                });
            }
            finally
            {
                StateService.SetLoading(false);
            }
        }

        private async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || !StateService.HasActiveSession || !StateService.IsLoggedIn)
                return;

            // Add user message immediately to UI
            var userMessage = new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                Content = message,
                IsFromAI = false,
                Username = StateService.CurrentUser!.Username,
                UserDisplayName = StateService.CurrentUser!.Username,
                Timestamp = DateTime.UtcNow,
                SessionId = StateService.CurrentSession!.Id,
                UserId = StateService.CurrentUser!.Id,
                Type = MessageType.User,
                IsEdited = false,
                IsDeleted = false
            };

            StateService.AddMessage(userMessage);
            StateService.SetSendingMessage(true);

            try
            {
                var request = new ContinueChatRequest
                {
                    SessionId = StateService.CurrentSession!.Id,
                    UserId = StateService.CurrentUser!.Id,
                    Message = message,
                    PreferredAIProvider = StateService.SelectedAIProvider
                };

                var response = await ChatApiService.ContinueChatAsync(request);

                if (response.Succeeded && response.Data != null)
                {
                    // Add the AI response
                    StateService.AddMessage(response.Data);

                    // Scroll to bottom after adding the AI response
                    await ScrollToBottom();
                }
                else
                {
                    // Mark user message as failed if AI response failed
                    userMessage.IsDeleted = true; // ou vous pouvez ajouter un champ Status si nécessaire
                    StateService.UpdateMessage(userMessage);

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Send Message Failed",
                        Detail = response.Message ?? "Could not send message"
                    });
                }
            }
            catch (Exception ex)
            {
                // Mark user message as failed on exception
                userMessage.IsDeleted = true; // ou vous pouvez ajouter un champ Status si nécessaire
                StateService.UpdateMessage(userMessage);

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message
                });
            }
            finally
            {
                StateService.SetSendingMessage(false);
                await ScrollToBottom();
            }
        }

        private async Task RefreshMessages()
        {
            await LoadMessages();
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Messages Refreshed",
                Detail = "Chat messages have been updated"
            });
        }

        private void ClearChat()
        {
            StateService.ClearMessages();
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Chat Cleared",
                Detail = "All messages have been removed from view"
            });
        }

        private async Task DeleteMessage(Guid messageId)
        {
            if (!StateService.IsLoggedIn) return;

            try
            {
                var response = await ChatApiService.DeleteMessageAsync(messageId, StateService.CurrentUser!.Id);

                if (response.Succeeded)
                {
                    StateService.RemoveMessage(messageId);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Message Deleted",
                        Detail = "Message has been removed"
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Delete Failed",
                        Detail = response.Message ?? "Could not delete message"
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message
                });
            }
        }

        private async Task ScrollToBottom()
        {
            try
            {
                await Task.Delay(100); // Small delay to ensure DOM is updated
                await JSRuntime.InvokeVoidAsync("chatHelpers.scrollToBottom", messagesContainer);
            }
            catch
            {
                // Ignore scrolling errors
            }
        }

        public void Dispose()
        {
            StateService.OnStateChanged -= OnStateChanged;
        }
    }
}