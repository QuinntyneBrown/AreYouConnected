using AreYouConnected.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AreYouConnected.Api.Features.Users
{
    [Authorize(Policy = "AreYouConnected")]
    [ApiController]
    [Route("api/ping")]
    public class PingController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHubService _hubService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PingController> _logger;
                
        public PingController(
            IAuthorizationService authorizationService,            
            IHubService hubService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PingController> logger            
            )
        {
            _authorizationService = authorizationService;
            _hubService = hubService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post([FromHeader]string connectionId, CancellationToken cancellationToken)
        { 
            var result = "Pong";

            // After some operation, make sure the user is still connected
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Strings.AreYouConnected);

            if (!authorizationResult.Succeeded) 
                return new UnauthorizedObjectResult(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Type = "https://api.areyouconnected.com/errors/invalidoperation",
                    Detail = "Unauthorized operation as user is not connected.",
                    Status = (int)HttpStatusCode.Unauthorized
                });

            // send the result via SignalR
            await _hubService.GetHubConnection()
                .InvokeAsync("SendResult", new SendResultRequest {
                    UserId = _httpContextAccessor.HttpContext.User.FindFirst(Strings.UniqueIdentifier).Value,
                    Result = result
                }, cancellationToken);

            return new OkResult();
        }        
    }    
}
