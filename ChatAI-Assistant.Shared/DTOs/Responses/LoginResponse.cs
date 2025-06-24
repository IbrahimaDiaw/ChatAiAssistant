using ChatAI_Assistant.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.DTOs.Responses
{
    public class LoginResponse : ApiResponse<LoginDto>
    {
        public static LoginResponse Success(LoginDto login)
        {
            return new LoginResponse
            {
                Succeeded = true,
                Message = "Login successful",
                Data = login,
                Timestamp = DateTime.UtcNow
            };
        }

        public static LoginResponse BadRequest(string message)
        {
            return new LoginResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static LoginResponse Error(string message)
        {
            return new LoginResponse
            {
                Succeeded = false,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
