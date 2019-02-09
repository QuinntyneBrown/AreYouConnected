using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Mesh.AspNetCore.Data;
using Microsoft.ServiceFabric.Mesh.Data.Collections;
using Serilog;

namespace AreYouConnected.ConnectionManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseReliableCollectionsService("Connections", ReliableCollectionMode.Volatile)
            .UseStartup<Startup>()            
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddEnvironmentVariables();
            })
            .ConfigureLogging((hostingContext, config) =>
            {

            })
            .UseApplicationInsights()
            .UseSerilog((builderContext, config) =>
            {
                config
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });
    }
}
