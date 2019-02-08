using AreYouConnected.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace AreYouConnected.ConnectionManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration) 
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public bool UseAzureSignalR => Configuration[ServiceOptions.ConnectionStringDefaultKey] != null;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy(Strings.CorsPolicy,
                builder => builder
                .WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(isOriginAllowed: _ => true)
                .AllowCredentials()));

            var signalRServerBuilder = services.AddSignalR();
           
            if (UseAzureSignalR) 
                signalRServerBuilder.AddAzureSignalR();
            
            services.AddSingleton<IUserIdProvider, UniqueIdentifierUserIdProvider>();

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler
            {
                InboundClaimTypeMap = new Dictionary<string, string>()
            };

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.SecurityTokenValidators.Clear();
                    options.SecurityTokenValidators.Add(jwtSecurityTokenHandler);
                    options.TokenValidationParameters = SecurityTokenFactory.CreateValidationParameters();
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Request.Query.TryGetValue("access_token", out StringValues token);                            
                            if (!string.IsNullOrEmpty(token))
                                context.Token = token;

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(Strings.CorsPolicy);

            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseMvc();

            if (UseAzureSignalR)
            {
                app.UseAzureSignalR(routes =>
                {
                    routes.MapHub<ConnectionManagementHub>("/hubs/connectionManagement");
                });
            }
            else
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<ConnectionManagementHub>("/hubs/connectionManagement");
                });
            }
        }
    }
}
