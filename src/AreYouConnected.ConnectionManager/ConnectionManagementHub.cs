using AreYouConnected.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AreYouConnected.ConnectionManager
{
    public interface IConnectionManagementHub
    {
        Task ShowUsersOnLine(int count);
        Task Result(string result);
        Task ConnectionId(string connectionId);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ConnectionManagementHub: Hub<IConnectionManagementHub> {
        
        public static ConcurrentDictionary<string, string> Connections  
            = new ConcurrentDictionary<string, string>();
        
        private readonly ILogger<ConnectionManagementHub> _logger;
        
        public static BehaviorSubject<Dictionary<string,string>> ConnectionsChanged 
            = new BehaviorSubject<Dictionary<string, string>>(Connections.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        
        public ConnectionManagementHub(ILogger<ConnectionManagementHub> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public override async Task OnConnectedAsync()
        {            
            if (!Context.User.IsInRole("System"))
            {                
                if (!Connections.TryAdd(Context.UserIdentifier, Context.ConnectionId))
                {
                    Context.Abort();
                    return;
                } 
                
                await Groups.AddToGroupAsync(Context.ConnectionId, TenantId);

                await Clients.Group(TenantId).ShowUsersOnLine(Connections.Where(x => x.Key.StartsWith(TenantId)).Count());

                await Clients.Caller.ConnectionId(Context.ConnectionId);

                ConnectionsChanged.OnNext(Connections.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));                
            }

            await base.OnConnectedAsync();
        }

        [Authorize(Roles = "System")]
        public ChannelReader<Dictionary<string, string>> GetConnections()
        {
            var channel = Channel.CreateUnbounded<Dictionary<string,string>>();

            var disposable = ConnectionsChanged.Subscribe(x => channel.Writer.WriteAsync(x));

            channel.Reader.Completion.ContinueWith(_ => disposable.Dispose());

            return channel.Reader;            
        }

        [Authorize(Roles = "System")]
        public async Task SendResult(SendResultRequest request)
            => await Clients.User(request.UserId).Result(request.Result);

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            if (!Context.User.IsInRole("System") && TryToRemoveConnectedUser(Context.UserIdentifier, Context.ConnectionId))
            {                                
                await Clients.All.ShowUsersOnLine(Connections.Count);
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, TenantId);

                await Clients.Group(TenantId).ShowUsersOnLine(Connections.Where(x => x.Key.StartsWith(TenantId)).Count());

                ConnectionsChanged.OnNext(Connections.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }            
        } 
        
        public bool TryToRemoveConnectedUser(string uniqueIdentifier, string connectionId)
        {
            var connectedUserEntry = Connections.FirstOrDefault(x => x.Key == uniqueIdentifier);

            if (connectedUserEntry.Value == connectionId)
            {
                Connections.TryRemove(Context.UserIdentifier, out _);
                return true;
            }

            return false;
        }

        public string TenantId { get => Context.User?.FindFirst("TenantId")?.Value; }
    }

    public class UniqueIdentifierUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
            => connection.User.FindFirst("UniqueIdentifier").Value;
    }
}
