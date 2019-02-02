﻿using Microsoft.AspNetCore.SignalR.Client;
using System.Linq;

namespace RequireUserPresence.API
{

    public interface IConnectionManagerHubConnectionAccessor
    {
        string[] ConnectedUserUniqueIdentifiers { get; set; }
        HubConnection GetHubConnection();
        HubConnection HubConnection { set; }
        bool IsConnected(string uniqueIdentifier);
    }

    public class ConnectionManagerHubConnectionAccessor : IConnectionManagerHubConnectionAccessor
    {
        public string[] ConnectedUserUniqueIdentifiers { get; set; }
        = new string[] { };

        private HubConnection _hubConnection;
        public HubConnection HubConnection { set => _hubConnection = value; }
        public HubConnection GetHubConnection() => _hubConnection;
        public bool IsConnected(string uniqueIdentifier)
            => ConnectedUserUniqueIdentifiers.Contains(uniqueIdentifier);
    }
}
