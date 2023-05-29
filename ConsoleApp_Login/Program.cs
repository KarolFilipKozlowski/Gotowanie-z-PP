// Creates and configures the PnP host
using ConsoleApp_Login.PnPModel;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;

var host = new CreatePnPHost();
await host.PnPIHost.StartAsync();
using (var scope = host.PnPIHost.Services.CreateScope())
{
    // Ask an IPnPContextFactory from the host
    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

    // Create a PnPContext
    using (var context = await pnpContextFactory.CreateAsync(new Uri("https://karolkozlowski.sharepoint.com/sites/GotowaniezPP")))
    {
        // Load the Title property of the site's root web
        await context.Web.LoadAsync(p => p.Title);
        Console.WriteLine($"The title of the web is {context.Web.Title}");
    }
}