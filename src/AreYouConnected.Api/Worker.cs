using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using AreYouConnected.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AreYouConnected.Api
{
    public class Worker: BackgroundService
    {
        private readonly IConnectionManagerHubConnectionAccessor _connectionManagerHubConnectionAccessor;
        private readonly ISecurityTokenFactory _securityTokenFactory;

        public Worker(
            IConnectionManagerHubConnectionAccessor connectionManagerHubConnectionAccessor,
            ISecurityTokenFactory securityTokenFactory)
        {
            _connectionManagerHubConnectionAccessor = connectionManagerHubConnectionAccessor;
            _securityTokenFactory = securityTokenFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = new HubConnectionBuilder()                
                .WithUrl("https://localhost:44337/hubs/connectionManagement", options => {
                    options.AccessTokenProvider = () => Task.FromResult(_securityTokenFactory.Create("System"));
                })
                .Build();

            await connection.StartAsync();
            
            _connectionManagerHubConnectionAccessor.HubConnection = connection;

            var channel = await connection
                .StreamAsChannelAsync<Dictionary<string,string>>("GetConnectedUsers", stoppingToken);

            while (await channel.WaitToReadAsync())
            {
                while (channel.TryRead(out var connectedUsers))
                {
                    _connectionManagerHubConnectionAccessor.ConnectedUsers = connectedUsers;
                }
            }
        }
    }
}
