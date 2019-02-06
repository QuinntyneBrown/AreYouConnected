using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using AreYouConnected.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AreYouConnected.Api
{
    public class Worker: BackgroundService
    {
        private readonly IHubService _hubService;
        private readonly ISecurityTokenFactory _securityTokenFactory;

        public Worker(
            IHubService hubService,
            ISecurityTokenFactory securityTokenFactory)
        {
            _hubService = hubService;
            _securityTokenFactory = securityTokenFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = new HubConnectionBuilder()                
                .WithUrl("https://localhost:44337/hubs/connectionManagement", options => {
                    options.AccessTokenProvider = () => Task.FromResult(_securityTokenFactory.Create("System"));
                })
                .Build();

            await connection.StartAsync(stoppingToken);
            
            _hubService.HubConnection = connection;

            var channel = await connection
                .StreamAsChannelAsync<Dictionary<string,string>>("GetConnections", stoppingToken);

            while (await channel.WaitToReadAsync(stoppingToken))
            {
                while (channel.TryRead(out var connections))
                {
                    _hubService.Connections = new ConcurrentDictionary<string, string>(connections);
                }
            }
        }
    }
}
