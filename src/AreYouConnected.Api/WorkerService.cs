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

            await StartConnectionAsync();
        }

        private async Task StartConnectionAsync()
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(20)
                , (exception, timeSpan) =>
                {
                    _logger.Log(LogLevel.Error, exception.Message);
                });

            await retryPolicy.ExecuteAsync(async () => await TryToStartConnectionAsync());
        }
        
        private async Task<bool> TryToStartConnectionAsync()
        {
            await _connection.StartAsync();
            return true;
        }
    }
}
