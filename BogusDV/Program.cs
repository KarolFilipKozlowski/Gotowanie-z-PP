using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using Bogus;
using BogusDV.PowerApps;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;

namespace BogusDV
{
    /// <summary>
    /// Demonstrates Azure authentication and execution of a Dataverse Web API function.
    /// </summary>
    class Program
    {
        static async Task Main()
        {
            Config config = App.InitializeApp();

            var service = new Service(config);

            List<EntityReference> recordsToDelete = new();
            bool deleteCreatedRecords = true;

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
                            .RuleFor(u => u.Date_Past, f => f.Date.Past(3))
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
                    var bogusDVrecord = new JObject {
                        { "cre80_title", fakeData.Guid.ToString() }, //Text
                        { "cre80_addressbuildingnumber", fakeData.Address_BuildingNumber }, //Text
                        { "cre80_addresscity", fakeData.Address_City }, //Text
                        { "cre80_addressstreetname", fakeData.Address_StreetName }, //Text
                        { "cre80_companycompanyname", fakeData.Company_CompanyName }, //Text
                        { "cre80_datepast", fakeData.Date_Past }, //Date only
                        { "cre80_financeamount", fakeData.Finance_Amount }, //Float
                        { "cre80_imagescats", fakeData.Images_Cats }, //Url
                        { "cre80_loremparagraphs", fakeData.Lorem_Paragraphs }, //Text (lorem ipsum)
                    };

                    //Zapis "kolekcji" jako rekord w DV:
                    HttpRequestMessage createRequest = new(
                        method: HttpMethod.Post,
                        requestUri: new Uri(
                            uriString: "cre80_boguses",
                            uriKind: UriKind.Relative))
                    {
                        Content = new StringContent(
                        content: bogusDVrecord.ToString(),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json")
                    };

                    HttpResponseMessage createResponse = await service.SendAsync(request: createRequest);

                    var bogusDVrecordItem = new Uri(createResponse.Headers.GetValues("OData-EntityId").FirstOrDefault());

                    //Console.WriteLine($"Nowy element został dodany, ID elementu: {bogusDVrecordItem..Id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }




        }        
    }
}