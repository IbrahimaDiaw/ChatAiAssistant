using Radzen;

namespace ChatAI_Assistant.Client.Components
{
    public partial class UserProfileComponent
    {
        private void RefreshData()
        {
            // Force refresh of the component
            StateHasChanged();

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Refreshed",
                Detail = "Profile data has been refreshed"
            });
        }

        private void Logout()
        {
            StateService.ClearAll();

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Logged Out",
                Detail = "You have been successfully logged out"
            });
        }

        private void GoToLogin()
        {
            // This would typically trigger the login dialog
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Login",
                Detail = "Click the Login button in the header to get started"
            });
        }
    }
}