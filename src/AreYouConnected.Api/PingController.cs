﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using AreYouConnected.Core;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AreYouConnected.Api.Features.Users
{
    [Authorize]
    [ApiController]
    [Route("api/ping")]
    public class PingController
    {
        private readonly IHubService _hubService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PingController> _logger;
                
        public PingController(
            IConfiguration configuration,
            IHubService hubService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PingController> logger            
            )
        {
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post([FromHeader]string connectionId, CancellationToken cancellationToken)
        {            
            var uniqueIdentifier = _httpContextAccessor.HttpContext.User.FindFirst("UniqueIdentifier").Value;

            if (!_hubService.IsConnected(uniqueIdentifier, connectionId)) 
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Invalid Operation",
                    Type = "https://api.areyouconnected.com/errors/invalidoperation",
                    Detail = "Invalid Operation as user is not connected.",
                    Status = (int)HttpStatusCode.BadRequest
                });

            await _hubService.GetHubConnection()
                .InvokeAsync("SendResult", new SendResultRequest {
                    UserId = uniqueIdentifier,
                    Result = "Pong"
                });

            return new OkResult();
        }        
    }    
}
