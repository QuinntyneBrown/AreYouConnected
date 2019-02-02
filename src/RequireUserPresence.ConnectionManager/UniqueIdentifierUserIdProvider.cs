using Microsoft.AspNetCore.SignalR;

namespace RequireUserPresence.ConnectionManager
{
    public class UniqueIdentifierUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
            => connection.User.FindFirst("UniqueIdentifier").Value;
    }
}
