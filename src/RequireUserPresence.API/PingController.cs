using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RequireUserPresence.API.Features.Users
{
    [Authorize]
    [ApiController]
    [Route("api/ping")]
    public class PingController
    {
        private readonly IConnectionManagerHubConnectionAccessor _connectionManagerHubConnectionAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PingController> _logger;
                
        public PingController(
            IConnectionManagerHubConnectionAccessor connectionManagerHubConnectionAccessor,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PingController> logger            
            )
        {
            _connectionManagerHubConnectionAccessor = connectionManagerHubConnectionAccessor ?? throw new ArgumentNullException(nameof(connectionManagerHubConnectionAccessor));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post(CancellationToken cancellationToken)
        {            
            var uniqueIdentifier = _httpContextAccessor.HttpContext.User.FindFirst("UniqueIdentifier").Value;

            if (!_connectionManagerHubConnectionAccessor.IsConnected(uniqueIdentifier)) 
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Invalid Operation",
                    Type = "https://api.requireuserpresence.com/errors/invalidoperation",
                    Detail = "Invalid Operation as user is not connected.",
                    Status = (int)HttpStatusCode.BadRequest
                });

            await _connectionManagerHubConnectionAccessor.GetHubConnection()
                .InvokeAsync("SendResult", uniqueIdentifier, "Pong");

            return new OkResult();
        }        
    }    
}
