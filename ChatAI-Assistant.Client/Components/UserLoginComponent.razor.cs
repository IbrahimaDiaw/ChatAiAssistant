using ChatAI_Assistant.Shared.DTOs.Requests;
using ChatAI_Assistant.Shared.Enums;
using Radzen;

namespace ChatAI_Assistant.Client.Components
{
    public partial class UserLoginComponent
    {
        private QuickStartChatRequest model = new();
        private bool isLoading = false;

        private readonly List<object> aiProviders = new()
    {
        new { Text = "OpenAI", Value = AIProvider.OpenAI },
        new { Text = "AzureOpenAI", Value = AIProvider.AzureOpenAI },
        new { Text = "Claude", Value = AIProvider.Anthropic }
    };

        protected override void OnInitialized()
        {
            model.PreferredAIProvider = AIProvider.OpenAI;
        }

        private async Task OnSubmit()
        {
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Validation Error",
                    Detail = "Username is required"
                });
                return;
            }

            isLoading = true;
            StateHasChanged();

            try
            {
                var response = await ChatClientService.QuickStartChatAsync(model);

                if (response.Success && response.User != null && response.Session != null)
                {
                    // Update state with user and session
                    StateService.SetUser(response.User!);
                    StateService.SetSession(response.Session!);

                    // Add bot response if available
                    if (response.BotResponse != null)
                    {
                        StateService.AddMessage(response.BotResponse);
                    }

                    DialogService.Close(true);
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Login Failed",
                        Detail = response.Message ?? "Unknown error occurred"
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
                isLoading = false;
                StateHasChanged();
            }
        }
    }
}