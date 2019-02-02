using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RequireUserPresence.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace RequireUserPresence.ConnectionManager
{
    public interface IConnectionManagementHub
    {
        Task ShowUsersOnLine(int count);
        Task Result(string result);
        Task ConnectedUsersChanged(string[] connectedUsers);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ConnectionManagementHub: Hub<IConnectionManagementHub> {
        
        public static ConcurrentDictionary<string, byte> Users  = new ConcurrentDictionary<string, byte>();
        
        private readonly ILogger<ConnectionManagementHub> _logger;

        public ConnectionManagementHub(ILogger<ConnectionManagementHub> logger)
            => _logger = logger;

        public override async Task OnConnectedAsync()
        {
            if (!Context.User.IsInRole("System"))
            {
                if (!Users.TryAdd(Context.UserIdentifier, 0))
                    throw new UserIsAlreadyConnectedException();
                
                await Groups.AddToGroupAsync(Context.ConnectionId, TenantId);

                await Clients.Group(TenantId).ShowUsersOnLine(Users.Where(x => x.Key.StartsWith(TenantId)).Count());
                
                await Clients.User("System").ConnectedUsersChanged(Users.Select(x => x.Key).ToArray());
            }

            await base.OnConnectedAsync();
        }

        [Authorize(Roles = "System")]
        public async Task SendResult(string uniqueIdentifier, string result)
            => await Clients.User(uniqueIdentifier).Result(result);

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (!Context.User.IsInRole("System"))
            {
                Users.TryRemove(Context.UserIdentifier, out _);

                await Clients.All.ShowUsersOnLine(Users.Count);
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, TenantId);

                await Clients.Group(TenantId).ShowUsersOnLine(Users.Where(x => x.Key.StartsWith(TenantId)).Count());

                await Clients.User("System").ConnectedUsersChanged(Users.Select(x => x.Key).ToArray());
            }

            await base.OnDisconnectedAsync(exception);
        }        

        public string TenantId { get => Context.User?.FindFirst("TenantId")?.Value; }
    }
}
