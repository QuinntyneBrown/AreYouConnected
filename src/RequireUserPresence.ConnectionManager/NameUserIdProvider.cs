using Microsoft.AspNetCore.SignalR;

namespace RequireUserPresence.ConnectionManager
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
            => connection.User.FindFirst("UniqueIdentifier").Value;
    }
}
