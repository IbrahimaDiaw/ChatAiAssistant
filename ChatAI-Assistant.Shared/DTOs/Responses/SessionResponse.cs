using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class SessionResponse : ApiResponse<ChatSessionDto>
    {
        public static SessionResponse Success(ChatSessionDto session)
        {
            return new SessionResponse
            {
                Succeeded = true,
                Message = "Session operation successful",
                Data = session,
                Timestamp = DateTime.UtcNow
            };
        }

        public static SessionResponse BadRequest(string message)
        {
            return new SessionResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static SessionResponse NotFound(string message)
        {
            return new SessionResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static SessionResponse Conflict(string message)
        {
            return new SessionResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static SessionResponse Error(string message)
        {
            return new SessionResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

}
