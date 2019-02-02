using System;

namespace RequireUserPresence.Core
{
    public class UserIsAlreadyConnectedException: Exception
    {
        public UserIsAlreadyConnectedException()
            :base("User is already connected.") { }
    }
}
