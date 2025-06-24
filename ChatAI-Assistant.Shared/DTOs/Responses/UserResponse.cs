using ChatAI_Assistant.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class UserResponse : ApiResponse<UserDto>
    {
        public static UserResponse Success(UserDto user)
        {
            return new UserResponse
            {
                Succeeded = true,
                Message = "Success",
                Data = user,
                Timestamp = DateTime.UtcNow
            };
        }

        public static UserResponse NotFound(string message)
        {
            return new UserResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static UserResponse BadRequest(string message)
        {
            return new UserResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static UserResponse Conflict(string message)
        {
            return new UserResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static UserResponse Error(string message)
        {
            return new UserResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
