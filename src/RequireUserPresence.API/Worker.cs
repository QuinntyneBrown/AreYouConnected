using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using RequireUserPresence.Core;
using System.Threading;
using System.Threading.Tasks;

namespace RequireUserPresence.API
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
            
            connection.On<string[]>("ConnectedUsersChanged", (connectedUsers) 
                => _connectionManagerHubConnectionAccessor.ConnectedUserUniqueIdentifiers = connectedUsers);

            _connectionManagerHubConnectionAccessor.HubConnection = connection;

            await connection.StartAsync();            
        }
    }
}
