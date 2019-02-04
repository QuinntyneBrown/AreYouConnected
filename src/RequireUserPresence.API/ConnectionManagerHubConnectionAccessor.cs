using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;

namespace RequireUserPresence.API
{

    public interface IConnectionManagerHubConnectionAccessor
    {
        Dictionary<string,string> ConnectedUsers { get; set; }
        HubConnection GetHubConnection();
        HubConnection HubConnection { set; }
        bool IsConnected(string uniqueIdentifier, string connectionId = null);
        int GetConnectedUsersCountByTenantId(string tenantId);
    }

    public class ConnectionManagerHubConnectionAccessor : IConnectionManagerHubConnectionAccessor
    {
        public Dictionary<string, string> ConnectedUsers { get; set; }
        = new Dictionary<string, string>();

        private HubConnection _hubConnection;

        public HubConnection HubConnection { set => _hubConnection = value; }

        public HubConnection GetHubConnection() => _hubConnection;

        public bool IsConnected(string uniqueIdentifier, string connectionId = null)
        {
            ConnectedUsers.TryGetValue(uniqueIdentifier, out string value);

            if (string.IsNullOrEmpty(connectionId))
                return !string.IsNullOrEmpty(value);

            return value == connectionId;
        }

        public int GetConnectedUsersCountByTenantId(string tenantId)
            => ConnectedUsers.Where(x => x.Key.StartsWith(tenantId)).Count();
    }
}
