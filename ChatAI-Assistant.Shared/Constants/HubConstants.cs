using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.Constants
{
    public static class HubConstants
    {
        // Hub endpoint
        public const string HubUrl = "/chathub";

        // Client Methods
        public const string ReceiveMessage = "ReceiveMessage";
        public const string UserJoined = "UserJoined";
        public const string UserLeft = "UserLeft";
        public const string TypingStarted = "TypingStarted";
        public const string TypingStopped = "TypingStopped";
        public const string ConnectionStatusChanged = "ConnectionStatusChanged";
        public const string LoadPreviousMessages = "LoadPreviousMessages";
        /// Server Methods
        public const string SendMessage = "SendMessage";
        public const string JoinSession = "JoinSession";
        public const string LeaveSession = "LeaveSession";
        public const string StartTyping = "StartTyping";
        public const string StopTyping = "StopTyping";
        public const string GetMessageHistory = "GetMessageHistory";

        public const string SessionGroupPrefix = "Session_";
    }
}
