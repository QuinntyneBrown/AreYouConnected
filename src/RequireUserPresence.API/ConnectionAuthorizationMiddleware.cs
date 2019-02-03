using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
namespace RequireUserPresence.API
{
    public class ConnectionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public ConnectionAuthorizationMiddleware(RequestDelegate next)
            => _next = next;

        public async Task Invoke(HttpContext httpContext)
        {
            var connectionManagerHubConnectionAccessor = httpContext.RequestServices.GetService<IConnectionManagerHubConnectionAccessor>();

            var identity = httpContext.User.Identity;
            
            if(identity.IsAuthenticated && !httpContext.User.IsInRole("System"))
            {
                var uniqueIdentifier = httpContext.User.FindFirst("UniqueIdentifier").Value;

                httpContext.Request.Headers.TryGetValue("ConnectionId", out StringValues connectionId);

                if (connectionManagerHubConnectionAccessor.IsConnected(uniqueIdentifier, connectionId))
                {
                    await _next.Invoke(httpContext);
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    await httpContext.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(
                        new ProblemDetails {
                            Title = "Unauthorized",
                            Type = "",
                            Detail = "",
                            Status = StatusCodes.Status401Unauthorized
                        }));
                }
            } else
            {
                await _next.Invoke(httpContext);
            }            
        }
    }
}
