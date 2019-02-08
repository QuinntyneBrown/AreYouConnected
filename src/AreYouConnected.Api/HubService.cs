using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Linq;

namespace AreYouConnected.Api
{
    public interface IHubService
    {
        ConcurrentDictionary<string,string> Connections { get; set; }
        HubConnection GetHubConnection();
        HubConnection HubConnection { set; }
        bool IsConnected(string uniqueIdentifier, string connectionId = null);
        int Count(string tenantId);
    }

    public class HubService : IHubService
    {
        public ConcurrentDictionary<string, string> Connections { get; set; }
        = new ConcurrentDictionary<string, string>();

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

        public int Count(string tenantId)
            => Connections.Where(x => x.Key.StartsWith(tenantId)).Count();
    }
}
