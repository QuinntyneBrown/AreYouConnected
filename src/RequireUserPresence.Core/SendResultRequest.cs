using System;

namespace RequireUserPresence.Core
{
    public class SendResultRequest
    {
        public Guid UserId { get; set; }
        public string Result { get; set; }
    }
}
