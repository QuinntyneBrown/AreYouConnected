using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RequireUserPresence.Core;
using System;
using System.Threading.Tasks;

namespace RequireUserPresence.API
{
    [ApiController]
    [Route("api/users")]
    public class UsersController
    {
        private readonly ISecurityTokenFactory _securityTokenFactory;
        private readonly IConnectionManagerHubConnectionAccessor _connectionManagerHubConnectionAccessor;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IConnectionManagerHubConnectionAccessor connectionManagerHubConnectionAccessor,
            ILogger<UsersController> logger, 
            ISecurityTokenFactory securityTokenFactory)
        {
            _connectionManagerHubConnectionAccessor = connectionManagerHubConnectionAccessor;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _securityTokenFactory = securityTokenFactory ?? throw new ArgumentNullException(nameof(securityTokenFactory)); 
        }
        
        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<ActionResult<SignInResponse>> SignIn(SignInRequest request)
        {            
            var userId = Guid.NewGuid();

            var tenantId = $"{new Guid("f0f54a28-0714-4b1a-9012-b758213bff99")}";

            if (_connectionManagerHubConnectionAccessor.IsConnected($"{tenantId}-{request.Username}"))
                return new BadRequestObjectResult("User already connected!");

            if (_connectionManagerHubConnectionAccessor.GetConnectedUsersCountByTenantId(tenantId) > 0)
                return new BadRequestObjectResult("Licenses exhausted!");

            return await Task.FromResult(new SignInResponse
            {
                AccessToken = _securityTokenFactory.Create(tenantId, userId, request.Username),
                Username = request.Username,
                UserId = userId,
                TenantId = tenantId
            });
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
        public Guid UserId { get; set; }
    }
}
