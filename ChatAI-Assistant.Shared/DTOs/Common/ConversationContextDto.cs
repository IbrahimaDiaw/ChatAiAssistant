using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Common
{
    public class ConversationContextDto
    {
        public List<ChatMessageDto> Messages { get; set; } = new();
        public int TotalMessages { get; set; }
        public DateTime OldestMessageTimestamp { get; set; }
        public DateTime NewestMessageTimestamp { get; set; }
    }
}
