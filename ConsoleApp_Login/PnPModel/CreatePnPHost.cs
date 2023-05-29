using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;

namespace ConsoleApp_Login.PnPModel
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

                services.AddPnPCore(options =>
                {
                    // Configure the interactive authentication provider as default
                    options.DefaultAuthenticationProvider = new InteractiveAuthenticationProvider()
                    {
                        ClientId = "3b8938ac-d31d-474e-8da9-ab34f3c3c45e",
                        RedirectUri = new Uri("http://localhost")
                    };
                });
            })
            .UseConsoleLifetime()
            .Build();
        }
    }
}
