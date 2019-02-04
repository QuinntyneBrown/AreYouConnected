using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RequireUserPresence.Core;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RequireUserPresence.API
{
    [ApiController]
    [Route("api/users")]
    public class UsersController
    {
        private readonly IConnectionManagerHubConnectionAccessor _connectionManagerHubConnectionAccessor;
        private readonly ILogger<UsersController> _logger;
        private readonly ISecurityTokenFactory _securityTokenFactory;
        
        public UsersController(
            IConnectionManagerHubConnectionAccessor connectionManagerHubConnectionAccessor,
            ILogger<UsersController> logger, 
            ISecurityTokenFactory securityTokenFactory)
        {
            _connectionManagerHubConnectionAccessor = connectionManagerHubConnectionAccessor ?? throw new ArgumentNullException(nameof(connectionManagerHubConnectionAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _securityTokenFactory = securityTokenFactory ?? throw new ArgumentNullException(nameof(securityTokenFactory)); 
        }
        
        [HttpPost("token")]
        [ProducesResponseType(typeof(SignInResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SignIn(SignInRequest request)
        {            
            var userId = $"{Guid.NewGuid()}";

            var tenantId = $"{new Guid("f0f54a28-0714-4b1a-9012-b758213bff99")}";

            if (_connectionManagerHubConnectionAccessor.IsConnected($"{tenantId}-{request.Username}"))
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Login Failed",
                    Type = "https://api.requireuserpresence.com/errors/useralreadyloggedin",
                    Detail = "User already logged in.",
                    Status = StatusCodes.Status400BadRequest
                });

            if (_connectionManagerHubConnectionAccessor.GetConnectedUsersCountByTenantId(tenantId) > 1)
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "Login Failed",
                    Type = "https://api.requireuserpresence.com/errors/connectionlimitreached",
                    Detail = "Connections limit reached.",
                    Status = StatusCodes.Status400BadRequest
                });

            return await Task.FromResult(new OkObjectResult(new SignInResponse
            {
                AccessToken = _securityTokenFactory.Create(tenantId, userId, request.Username),
                Username = request.Username,
                UserId = userId,
                TenantId = tenantId
            }));
        }        
    }

    public class SignInRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignInResponse
    {
        public string TenantId { get; set; }
        public string AccessToken { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
    }
}
