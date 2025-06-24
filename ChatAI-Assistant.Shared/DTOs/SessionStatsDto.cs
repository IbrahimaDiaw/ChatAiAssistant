using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs
{
    public class SessionStatsDto
    {
        public Guid SessionId { get; set; }
        public int MessageCount { get; set; }
        public int ParticipantCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
        public TimeSpan Duration => DateTime.UtcNow - CreatedAt;
    }
}
