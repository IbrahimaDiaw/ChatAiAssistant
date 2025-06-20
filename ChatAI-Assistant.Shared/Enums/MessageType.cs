using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI_Assistant.Shared.Enums
{
    public enum MessageType
    {
        User = 0,
        AI = 1,
        System = 2,
        Error = 3,
        Typing = 4,
        Welcome = 5
    }
}
