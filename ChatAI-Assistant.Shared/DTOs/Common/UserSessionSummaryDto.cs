using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Common
{
    public class UserSessionSummaryDto
    {
        public Guid SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
        public int MessageCount { get; set; }
        public int ParticipantCount { get; set; }
        public bool IsPrivate { get; set; }
        public MessageSummaryDto? LastMessage { get; set; }
    }

}
