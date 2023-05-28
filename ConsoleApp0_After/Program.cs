// Creates and configures the PnP host
using ConsoleApp0_After.PnPModel;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;

const string SITETARGET = "GotowaniezPP";

var host = new CreatePnPHost();
await host.PnPIHost.StartAsync();
using (var scope = host.PnPIHost.Services.CreateScope())
{
    // Create the PnPContext
    // Documentation -> https://pnp.github.io/pnpcore/
    using (var context = await scope.ServiceProvider.GetRequiredService<IPnPContextFactory>().CreateAsync(name: SITETARGET))
    {
        // Load the Title property of the site's root web
        await context.Web.LoadAsync(p => p.Title);
        Console.WriteLine($"The title of the web is {context.Web.Title}");

        // Sample 1: Load all lists in the web with their default properties
        await context.Web.LoadAsync(p => p.Lists);

        // Sample 2: Load the web content types + all lists with their content types and the content type field links
        await context.Web.LoadAsync(p => p.Title,
                                   p => p.ContentTypes.QueryProperties(p => p.Name),
                                   p => p.Lists.QueryProperties(p => p.Id,
                                                                p => p.TemplateType,
                                                                p => p.Title,
                                                                p => p.DocumentTemplate,
                                       p => p.ContentTypes.QueryProperties(p => p.Name,
                                            p => p.FieldLinks.QueryProperties(p => p.Name)))
                                  );

        // Process the document libraries
        // Documentation -> https://pnp.github.io/pnpcore/using-the-sdk/lists-intro.html#enumerating-lists
        foreach (var list in context.Web.Lists.AsRequested().Where(p => p.TemplateType == ListTemplateType.DocumentLibrary))
        {
            // Use the list
            Console.WriteLine(list.Title);
        }

        var myList = await context.Web.Lists.GetByTitleAsync("EmailsArchive", p => p.Title);
    }
}