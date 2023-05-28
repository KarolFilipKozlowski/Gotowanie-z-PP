using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;

namespace SQLandSPO.PnPModel
{
    public class CreatePnPHost
    {
        /// <summary>
        /// PnP Host
        /// </summary>
        public IHost PnPIHost { get; private set; }

        public CreatePnPHost()
        {
            PnPIHost = Host.CreateDefaultBuilder()
            .ConfigureServices((pnpContext, services) =>
            {
                services.AddLogging(builder =>
                {
                    builder.AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddFilter("PnP.Core.Auth", LogLevel.Warning)
                            .AddConsole();
                });

                // Add PnP Core SDK
                services.AddPnPCore();
                services.Configure<PnPCoreOptions>(pnpContext.Configuration.GetSection("PnPCore"));

                // Add the PnP Core SDK Authentication Providers
                services.AddPnPCoreAuthentication();
                services.Configure<PnPCoreAuthenticationOptions>(pnpContext.Configuration.GetSection("PnPCore"));
            })
            .UseConsoleLifetime()
            .Build();
        }
    }
}
