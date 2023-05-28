using Bogus;
using BogusSPO.PnPModel;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;

//Utworzenie połączenie z witryną SPO:
CreatePnPHost host = new CreatePnPHost();
await host.PnPIHost.StartAsync();
using (var scope = host.PnPIHost.Services.CreateScope())
{
    using var context = await scope.ServiceProvider.GetRequiredService<IPnPContextFactory>().CreateAsync(name: "GotowaniezPP");

    //Łączenie z listą (https://pnp.github.io/pnpcore/using-the-sdk/lists-intro.html#getting-lists):
    var listBogus = context.Web.Lists.GetByTitle("Bogus", p => p.Title);

    //Tworzymy instancji obiektu z losowymi danymi za pomocą biblioteki Bogus -> https://github.com/bchavez/Bogus
    //Seed używamy, jeżeli chcemy aby wygenerowane dane powtarzały się.
    Randomizer.Seed = new Random(31062023);
    Faker<FakeDataModel> fakerDataModel = new Faker<FakeDataModel>("en") //Opcje lokalizacji: https://github.com/bchavez/Bogus#locales.
                    //Listę danych które można wygenerować znajdziemy na https://github.com/bchavez/Bogus#bogus-api-support.
                    .RuleFor(u => u.Id, f => f.Random.Int(1000, 10000))
                    .RuleFor(u => u.Guid, f => f.Random.Guid())
                    .RuleFor(u => u.Address_StreetName, f => f.Address.StreetAddress())
                    .RuleFor(u => u.Address_City, f => f.Address.City())
                    .RuleFor(u => u.Address_BuildingNumber, f => f.Address.BuildingNumber())
                    .RuleFor(u => u.Company_CompanyName, f => f.Company.CompanyName())
                    .RuleFor(u => u.Date_Past, f => f.Date.Past(-3))
                    .RuleFor(u => u.Finance_Amount, f => f.Random.Double(0, 100))
                    .RuleFor(u => u.Images_Cats, f => f.Image.LoremFlickrUrl(keywords: "kitten"))
                    .RuleFor(u => u.Lorem_Paragraphs, f => f.Lorem.Paragraph());

    //"Kolejka" singleFakeData zawiera tylko jeden "rekord":
    FakeDataModel singleFakeData = fakerDataModel.Generate();
    //"Kolejka" aFewFakeDate zawiera x "rekordów", zgodnie z GenerateBetween(min x, max y):
    List<FakeDataModel> aFewFakeDate = fakerDataModel.GenerateBetween(10, 10);

    foreach (FakeDataModel fakeData in aFewFakeDate)
    {
        try
        {
            //Dodawanie elementów do listy (https://pnp.github.io/pnpcore/using-the-sdk/listitems-intro.html#adding-list-items):
            /*
             *  Dictionary możemy potraktować trochę jak JSONa w przypadku funkcji Patch w PA. 
             *  W Dictionary mamy pole 'key' i 'value', key odpowiada nazwie kolumny w SPO (nazwa wew.) i musi być unikalne, value przechowuje dane (object = dowolny typ (string, int, Fieldurlvalue).
            */
            Dictionary<string, object> item = new Dictionary<string, object>()
            {
                { "Title", fakeData.Guid.ToString() },
                { "Address_StreetName", fakeData.Address_StreetName },
                { "Address_City", fakeData.Address_City },
                { "Address_BuildingNumber", fakeData.Address_BuildingNumber },
                { "Company_CompanyName", fakeData.Company_CompanyName },
                { "Date_Past", fakeData.Date_Past },
                { "Finance_Amount", fakeData.Finance_Amount },
                //Url fields: (https://pnp.github.io/pnpcore/using-the-sdk/listitems-fields.html#url-fields):
                { "Images_Cats", new FieldUrlValue(fakeData.Images_Cats, $"Kotek nr {fakeData.Id}!") },
                { "Lorem_Paragraphs", fakeData.Lorem_Paragraphs },
            };
            //Zapis "kolekcji" jako rekord w SPO:
            IListItem addedItem = await listBogus.Items.AddAsync(item);
            Console.WriteLine($"Nowy element został dodany, ID elementu: {addedItem.Id}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}