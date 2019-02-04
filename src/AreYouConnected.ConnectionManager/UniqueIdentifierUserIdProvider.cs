using Microsoft.AspNetCore.SignalR;

namespace AreYouConnected.ConnectionManager
{
    public class UniqueIdentifierUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
            => connection.User.FindFirst("UniqueIdentifier").Value;
    }
}
