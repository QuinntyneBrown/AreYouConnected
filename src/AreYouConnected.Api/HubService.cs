using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;

namespace AreYouConnected.Api
{

    public interface IHubService
    {
        Dictionary<string,string> Connections { get; set; }
        HubConnection GetHubConnection();
        HubConnection HubConnection { set; }
        bool IsConnected(string uniqueIdentifier, string connectionId = null);
        int GetConnectionsCount(string tenantId);
    }

    public class HubService : IHubService
    {
        public Dictionary<string, string> Connections { get; set; }
        = new Dictionary<string, string>();

        private HubConnection _hubConnection;

        public HubConnection HubConnection { set => _hubConnection = value; }

        public HubConnection GetHubConnection() => _hubConnection;

        public bool IsConnected(string uniqueIdentifier, string connectionId = null)
        {
            Connections.TryGetValue(uniqueIdentifier, out string value);

            if (string.IsNullOrEmpty(connectionId))
                return !string.IsNullOrEmpty(value);

            return value == connectionId;
        }

        public int GetConnectionsCount(string tenantId)
            => Connections.Where(x => x.Key.StartsWith(tenantId)).Count();
    }
}
