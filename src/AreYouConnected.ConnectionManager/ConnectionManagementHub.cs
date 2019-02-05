using AreYouConnected.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        Task ConnectedUsersChanged(Dictionary<string,string> connectedUsers);
        Task ConnectionId(string connectionId);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ConnectionManagementHub: Hub<IConnectionManagementHub> {
        
        public static ConcurrentDictionary<string, string> Users  = new ConcurrentDictionary<string, string>();
        
        private readonly ILogger<ConnectionManagementHub> _logger;

        public static BehaviorSubject<Dictionary<string,string>> ConnectedUsersChanged 
            = new BehaviorSubject<Dictionary<string, string>>(Users.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        
        public ConnectionManagementHub(ILogger<ConnectionManagementHub> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public override async Task OnConnectedAsync()
        {            
            if (!Context.User.IsInRole("System"))
            {                
                if (!Users.TryAdd(Context.UserIdentifier, Context.ConnectionId))
                {
                    Context.Abort();
                    return;
                } 
                
                await Groups.AddToGroupAsync(Context.ConnectionId, TenantId);

                await Clients.Group(TenantId).ShowUsersOnLine(Users.Where(x => x.Key.StartsWith(TenantId)).Count());

                await Clients.Caller.ConnectionId(Context.ConnectionId);

                ConnectedUsersChanged.OnNext(Users.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));                
            }

            await base.OnConnectedAsync();
        }

        [Authorize(Roles = "System")]
        public ChannelReader<Dictionary<string, string>> GetConnectedUsers()
        {
            var channel = Channel.CreateUnbounded<Dictionary<string,string>>();

            var disposable = ConnectedUsersChanged.Subscribe(x => channel.Writer.WriteAsync(x));

            channel.Reader.Completion.ContinueWith(task => disposable.Dispose());

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
                await Clients.All.ShowUsersOnLine(Users.Count);
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, TenantId);

                await Clients.Group(TenantId).ShowUsersOnLine(Users.Where(x => x.Key.StartsWith(TenantId)).Count());

                ConnectedUsersChanged.OnNext(Users.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }            
        } 
        
        public bool TryToRemoveConnectedUser(string uniqueIdentifier, string connectionId)
        {
            var connectedUserEntry = Users.FirstOrDefault(x => x.Key == uniqueIdentifier);

            if (connectedUserEntry.Value == connectionId)
            {
                Users.TryRemove(Context.UserIdentifier, out _);
                return true;
            }

            return false;
        }

        public string TenantId { get => Context.User?.FindFirst("TenantId")?.Value; }
    }
}
