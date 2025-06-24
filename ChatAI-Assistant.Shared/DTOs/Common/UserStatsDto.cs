using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Common
{
    public class UserStatsDto
    {
        public Guid UserId { get; set; }
        public int TotalMessages { get; set; }
        public int TotalSessions { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
