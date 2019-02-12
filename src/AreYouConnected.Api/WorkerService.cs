using AreYouConnected.Core;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AreYouConnected.Api
{
    public class WorkerService: BackgroundService
    {
        private readonly IHubService _hubService;
        private readonly ISecurityTokenFactory _securityTokenFactory;
        private HubConnection _connection;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(
            IHubService hubService,
            ISecurityTokenFactory securityTokenFactory,
            ILogger<WorkerService> logger)
        {
            _hubService = hubService;
            _securityTokenFactory = securityTokenFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:44337/hubs/connectionManagement", options => {
                    options.AccessTokenProvider = () => Task.FromResult(_securityTokenFactory.Create(Strings.System));
                })
                .Build();

            _connection.On<Dictionary<string,string>>(Strings.ConnectionsChanged, connections
                => _hubService.Connections = new ConcurrentDictionary<string, string>(connections));

            _hubService.HubConnection = _connection;

            await OpenSignalRConnectionAsync();
        }

        private async Task OpenSignalRConnectionAsync()
        {
            var pauseBetweenFailures = TimeSpan.FromSeconds(20);
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(i => pauseBetweenFailures
                , (exception, timeSpan) =>
                {
                    
                });

            await retryPolicy.ExecuteAsync(async () => await TryOpenSignalRConnection());
        }
        
        private async Task<bool> TryOpenSignalRConnection()
        {
            await _connection.StartAsync();
            return true;
        }
    }
}
