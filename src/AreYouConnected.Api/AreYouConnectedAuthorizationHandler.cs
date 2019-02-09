using AreYouConnected.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace AreYouConnected.Api
{

    public class AreYouConnectedRequirement : IAuthorizationRequirement { }

    public class AreYouConnectedAuthorizationHandler: AuthorizationHandler<AreYouConnectedRequirement>
    {
        private readonly ILogger<AreYouConnectedRequirement> _logger;
        private readonly IHubService _hubService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AreYouConnectedAuthorizationHandler(
            IHubService hubService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AreYouConnectedRequirement> logger
            )
        {
            _hubService = hubService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
      
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AreYouConnectedRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var isSystem = context.User.IsInRole(Strings.System);

            if ((!isSystem && IsConnected(httpContext)) || isSystem)
                context.Succeed(requirement);

            await Task.CompletedTask;
        }

        private bool IsConnected(HttpContext httpContext)
        {
            var uniqueIdentifier = httpContext.User.FindFirst(Strings.UniqueIdentifier).Value;

            httpContext.Request.Headers.TryGetValue(Strings.ConnectionId, out StringValues connectionId);

            return _hubService.IsConnected(uniqueIdentifier, connectionId);
        }
    }
}
