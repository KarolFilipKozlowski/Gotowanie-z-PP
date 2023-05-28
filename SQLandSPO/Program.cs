using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using SQLandSPO.Helper;
using SQLandSPO.Models;
using SQLandSPO.PnPModel;
using SQLandSPO.SPOHelper;
using System.Drawing;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

#region Połączenie z SQL i pobieranie danych:
//Entity Framework to biblioteka która uproszcza operacje SQL w C#. (https://learn.microsoft.com/en-us/ef/core/, https://www.entityframeworktutorial.net/efcore/entity-framework-core.aspx).
//Nawiązujemy połączenie z baza (tzw. context):
var adventureWorksLTContext = new AdventureWorksLTContext();

//W ramach tego context'u pobieramy _całą_ tabele:
var allProducts = adventureWorksLTContext.Product.ToList();

//C# gdy chcemy z Y zbioru wyciągać dane na podstawie X 'filtrów' użyjemy zapytań LINQ (https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/, https://www.tutorialsteacher.com/linq).
//Pobieramy z SQL tylko te rekordy które zostały 'dziś' dodane lub zmodyfikowane:
var newProducts = adventureWorksLTContext.Product.Where(w => w.ModifiedDate.Date == DateTime.Now.Date).ToList();
#endregion

#region SPO
//Utworzenie połączenie z witryną SPO:
var host = new CreatePnPHost();
await host.PnPIHost.StartAsync();
using (var scope = host.PnPIHost.Services.CreateScope())
{
    using (var context = await scope.ServiceProvider.GetRequiredService<IPnPContextFactory>().CreateAsync(name: "GotowaniezPP"))
    {
        //Łączenie z listą, w tym przypadku używam preferowanej przeze menie metody po jej id (https://pnp.github.io/pnpcore/using-the-sdk/lists-intro.html#getting-lists):
        var myList = context.Web.Lists.GetById(Guid.Parse("97a2170c-b5c1-4513-a528-1c8b3c7a7642"), p => p.Title,
                                                     p => p.Fields.QueryProperties(p => p.InternalName,
                                                                                   p => p.FieldTypeKind,
                                                                                   p => p.TypeAsString,
                                                                                   p => p.Title));

        //Metoda (coś jak re-używalne funkcje w PA) która zapisuje ma liście wszystkie produkty:
        await SPOHelper.AddProducts(allProducts, myList);

        //Collaborative Application Markup Language (CAML) (https://learn.microsoft.com/en-us/sharepoint/dev/schema/query-schema):
        /*
         * O ile LINQ jest relatywnie prosty do zrozumienia, tak CamlQuery jest dla mnie...
         * W dużym skrócie, CamlQuery będzie potrzebować w dwóch przypadkach:
         * 1) pobranie określonych elementów z listy
         * 2) wszystkich lub określonych gdy lista przekroczy próg 5k elementów
         * Więcej o pobieraniu danych znajdziemy w dokumentacji PnP: https://pnp.github.io/pnpcore/using-the-sdk/listitems-intro.html#reading-list-items.
        */

        #region przykłady CamlQuery
        //Przykład #1 - pobranie elementów utworzonych w dniu dzisiejszym (względnie):
        string camlQuery_CreatedToday = @"<View>
                                <Query><Where>" +
                                "<Eq>" +
                                "<FieldRef Name='Created' />" +
                                "<Value Type='DateTime' IncludeTimeValue='False'>" +
                                "<Today />" +
                                "</Value>" +
                                "</Eq>" +
                                "</Where></Query></View>";
        await SPOHelper.ShowListItems(context, camlQuery_CreatedToday, "Przykład #1");

        //Przykład #2 - pobranie elementów o kolorze X lub Z:
        string camlQuery_ColorIsYellowOrSilver = "<View><Query><Where>" +
                                "<In>" +
                                "<FieldRef Name='Color' />" +
                                "<Values>" +
                                "<Value Type='Text'>Yellow</Value>" +
                                "<Value Type='Text'>Silver</Value>" +
                                "</Values>" +
                                "</In>" +
                                "</Where></Query></View>";
        await SPOHelper.ShowListItems(context, camlQuery_ColorIsYellowOrSilver, "Przykład #2");

        //Przykład #3 - pobranie elementu o ID = 1:
        string camlQuery_IDeq1 = "<View><Query><Where>" +
                                "<Eq>" +
                                "<FieldRef Name='ID' />" +
                                "<Value Type='Integer'>200</Value>" +
                                "</Eq>" +
                                "</Where></Query></View>";
        await SPOHelper.ShowListItems(context, camlQuery_IDeq1, "Przykład #3");

        //Przykład #4 - pobranie elementu o ID > X:
        string camlQuery_IDgtX = "<View><Query><Where>" +
                                "<Gt>" +
                                "<FieldRef Name='ID' />" +
                                "<Value Type='Counter'>290</Value>" +
                                "</Gt>" +
                                "</Where></Query></View>";
        await SPOHelper.ShowListItems(context, camlQuery_IDgtX, "Przykład #4");

        //Przykład #4 - pobranie elementu utworzonych przez X:
        string camlQuery_ByXUserName = "<View><Query><Where>" +
                                "<Eq>" +
                                "<FieldRef Name='Author' />" +
                                "<Value Type='User'>" +
                                "Karol Kozłowski" +
                                "</Value>" +
                                "</Eq>" +
                                "</Where></Query></View>";
        await SPOHelper.ShowListItems(context, camlQuery_ByXUserName, "Przykład #5");
        #endregion

        /* camlex */

        #region Dodanie nowych 'z dziś' z bazy SQL do SPO
        //await AddProducts(allProducts, myList);
        #endregion

        #region Dodanie nowych 'z dziś' z SPO do SQL
        await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
        {
            ViewXml = @"<View>
                                <Query><Where>" +
                                "<Eq>" +
                                "<FieldRef Name='Created' />" +
                                "<Value Type='DateTime' IncludeTimeValue='False'>" +
                                "<Today />" +
                                "</Value>" +
                                "</Eq>" +
                                "</Where></Query></View>",
            DatesInUtc = true
        });
        SQLHelper.AddToSQL(adventureWorksLTContext, myList);
        #endregion
    }
}
#endregion