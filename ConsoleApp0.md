# Pierwsza aplikacja C#:

**( :star: ) - do uzupełnienia na prezentacji**

## GutHub repos:

- https://github.com/pnp/pnpcore

# Instrukcja

## Tworzymy solucji z projektem:

1. Uruchamiamy VS jako administrator ( :star: ).

2. Tworzymy nowy projekt.

3. Wybieramy `Console App`, tworzymy projekt jak i solucje.

4. Jako `Framework` wybieramy `.NET 7.0` lub `.NET 6.0` ( :star: ).

## Pobieramy wymagane pakiety NuGet:

- Microsoft.Extensions.Hosting*

- PnP.Core

## Piszemy pierwszy kod:

1. Klikając na projekt wybieramy `Add` -> `Folder` o nazwie `PnPModel`.

2. Klikając na tym folderze wybieramy `Add` -> `New item`, plik nazwiemy `CreatePnPHost.cs`.

3. Podajemy nowe `usings`, zamieniając pierwsze 5 linii następującymi:
````C#
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Services.Builder.Configuration;
using PnP.Core.Auth.Services.Builder.Configuration;
````

4. Następnie zamieniamy całe ciało klasy:
````C#
internal class CreatePnPHost
{
}
````
na kod:
````C#
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
````

5. Otwieramy plik `Program`, i jeżeli jest to **`.NET 7.0`** ( :star: ) po prostu całość kody zamieniamy na:
````C#
// Creates and configures the PnP host
using ConsoleApp0.PnPModel;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;

const string SITETARGET = "site";

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
    }
}
````

6. Ponownie klikając na projekt wybieramy `Add` -> `New item` i dodajemy plik konfiguracyjny nazwie `appsettings.json`. W oknie właściwości tego pliku należy zmienić opcję `Copy to Output Directory` na `Copy if newer`.
Sam plik uzupełniamy treścią: ( :star: )
````JSON
{
  "PnPCore": {
    "DisableTelemetry": "false",
    "HttpRequests": {
      "UserAgent": "ISV|contoso|ProductX",
      "Timeout": "100",
      "SharePointRest": {
        "UseRetryAfterHeader": "false",
        "MaxRetries": "10",
        "DelayInSeconds": "3",
        "UseIncrementalDelay": "true"
      },
      "MicrosoftGraph": {
        "UseRetryAfterHeader": "true",
        "MaxRetries": "10",
        "DelayInSeconds": "3",
        "UseIncrementalDelay": "true"
      }
    },
    "PnPContext": {
      "GraphFirst": "true",
      "GraphCanUseBeta": "true",
      "GraphAlwaysUseBeta": "false"
    },
    "Credentials": {
      "DefaultConfiguration": "x509Certificate",
      "Configurations": {
        "x509Certificate": {
          "ClientId": "00000000-0000-0000-0000-000000000000",
          "TenantId": "00000000-0000-0000-0000-000000000000",
          "X509Certificate": {
            "StoreName": "My",
            "StoreLocation": "LocalMachine",
            "ThumbPrint": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
          }
        }
      }
    },
    "Sites": {
      "site": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/site",
        "AuthenticationProviderName": "x509Certificate"
      }
    }
  }
}
````

7. Konfiguracja aplikacji: 
- `ClientId` podajemy ID aplikacji AAD
- `TenantId` podajemy ID tenantu
- `ThumbPrint` podajemy ID certyfikatu
- `SiteUrl` podajemy url witryny testowej ( :star: )

8. Po zapisaniu ustawień uruchamiamy aplikację z pomocą `CTRL` + `F5`.

9. Zapisanie aplikacja jak szablon VS:

W menu Project -> Export Template, wybieramy dany projekt i z pomocą prostego generatora tworzymy szablon. W ten sposób z menu nowego projektu VS będzie można od razu rozpocząć pisanie kodu mają gotowy wzorzec.

##
### by CitDev for Power Platfrom Polska
:thumbsup: [Karol Kozłowski - Citizen Developer](https://citdev.pl/)