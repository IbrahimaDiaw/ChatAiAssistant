using ChatAI_Assistant.Shared.DTOs;
using ChatAI_Assistant.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace ChatAI_Assistant.Client.Components
{
    public partial class MessageComponent
    {
        [Parameter] public ChatMessageDto Message { get; set; } = new();
        [Parameter] public EventCallback<Guid> OnDelete { get; set; }

        private string GetMessageAlignment()
        {
            return !Message.IsFromAI ? "message-user" : "message-assistant";
        }

        private string GetMessageCardStyle()
        {
            var baseStyle = "rz-shadow-1";
            return !Message.IsFromAI ? $"{baseStyle} rz-background-color-primary-lighter" : baseStyle;
        }

        private string GetMessageStyle()
        {
            return !Message.IsFromAI ? "max-width: 70%;" : "max-width: 85%;";
        }

        private string GetSenderIcon()
        {
            return !Message.IsFromAI ? "person" : "smart_toy";
        }

        private string GetIconStyle()
        {
            var color = !Message.IsFromAI ? "var(--rz-primary)" : "var(--rz-secondary)";
            return $"color: {color};";
        }

        private string GetSenderName()
        {
            return !Message.IsFromAI ? (Message.UserDisplayName ?? Message.Username ?? "You") : "AI Assistant";
        }

        private BadgeStyle GetProviderBadgeStyle()
        {
            return Message.AIProvider switch
            {
                AIProvider.OpenAI => BadgeStyle.Success,
                AIProvider.AzureOpenAI => BadgeStyle.Info,
                AIProvider.Anthropic => BadgeStyle.Warning,
                _ => BadgeStyle.Secondary
            };
        }

        private bool CanDelete()
        {
            return !Message.IsFromAI && !Message.IsDeleted;
        }

        private async Task ShowContextMenu(MouseEventArgs args)
        {
            var menuItems = new List<ContextMenuItem>
        {
            new ContextMenuItem
            {
                Text = "Delete Message",
                Icon = "delete",
                Value = "delete"
            },
            new ContextMenuItem
            {
                Text = "Copy Text",
                Icon = "content_copy",
                Value = "copy"
            }
        };

            // ContextMenuService.Open(args, menuItems);

            // if (result?.Value?.ToString() == "delete")
            // {
            //     await OnDelete.InvokeAsync(Message.Id);
            // }
            // else if (result?.Value?.ToString() == "copy")
            // {
            //     await CopyToClipboard();
            // }
        }

        private async Task CopyToClipboard()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Message.Content);
            }
            catch
            {
                // Fallback for older browsers or when clipboard API is not available
            }
        }
    }
}