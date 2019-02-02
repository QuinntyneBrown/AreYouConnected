using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RequireUserPresence.API.Features.Users
{
    [Authorize]
    [ApiController]
    [Route("api/ping")]
    public class PingController
    {
        private readonly ILogger<PingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConnectionManagerHubConnectionAccessor _connectionManagerHubConnectionAccessor;

        public PingController( 
            ILogger<PingController> logger,
            IHttpContextAccessor httpContextAccessor,
            IConnectionManagerHubConnectionAccessor connectionManagerHubConnectionAccessor
            )
        {
            _connectionManagerHubConnectionAccessor = connectionManagerHubConnectionAccessor;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task Post(CancellationToken cancellationToken)
        {            
            var uniqueIdentifier = _httpContextAccessor.HttpContext.User.FindFirst("UniqueIdentifier").Value;

            if (!_connectionManagerHubConnectionAccessor.IsConnected(uniqueIdentifier)) throw new Exception();
            
            await _connectionManagerHubConnectionAccessor.GetHubConnection()
                .InvokeAsync("SendResult", uniqueIdentifier, "Pong");
        }        
    }    
}
