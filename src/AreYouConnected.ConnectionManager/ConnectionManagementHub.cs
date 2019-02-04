using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using AreYouConnected.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
                
                await Clients.User("System").ConnectedUsersChanged(Users.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }

            await base.OnConnectedAsync();
        }

        [Authorize(Roles = "System")]
        public async Task SendResult(SendResultRequest request)
            => await Clients.User(request.UserId).Result(request.Result);

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (!Context.User.IsInRole("System") && TryToRemoveConnectedUser(Context.UserIdentifier, Context.ConnectionId))
            {                                
                await Clients.All.ShowUsersOnLine(Users.Count);
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, TenantId);

                await Clients.Group(TenantId).ShowUsersOnLine(Users.Where(x => x.Key.StartsWith(TenantId)).Count());

                await Clients.User("System").ConnectedUsersChanged(Users.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }

            await base.OnDisconnectedAsync(exception);
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
