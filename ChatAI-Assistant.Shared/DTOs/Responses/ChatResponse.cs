using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class ChatResponse : ApiResponse<ChatMessageDto>
    {
        public static ChatResponse Success(ChatMessageDto message)
        {
            return new ChatResponse
            {
                Succeeded = true,
                Message = "Message processed successfully",
                Data = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ChatResponse BadRequest(string message)
        {
            return new ChatResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ChatResponse NotFound(string message)
        {
            return new ChatResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ChatResponse Error(string message)
        {
            return new ChatResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
