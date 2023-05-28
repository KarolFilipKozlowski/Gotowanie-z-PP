using EmailsArchive.PnPModel;
using Microsoft.Extensions.DependencyInjection;
using MsgReader.Mime;
using OpenMcdf;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System.Text;

namespace EmailToList
{
    internal class Program
    {
        //Stała która przechowuje informacje do jakiej witryny z pliku appsettings.json chcemy się połączyć:
        const string SITETARGET = "GotowaniezPP";

        private static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); /* <- ta linia pozwala dekodować/zapisywać pliki w kodowaniu domyślnie nie obsługiwanym przez VS */

            //Katalog z wiadomościami email:
            var emlsDirectory = Directory.GetFiles(@"C:\EMLs");

            //Utworzenie połączenie z witryną SPO:
            var host = new CreatePnPHost();
            await host.PnPIHost.StartAsync();
            using (var scope = host.PnPIHost.Services.CreateScope())
            {
                using (var context = await scope.ServiceProvider.GetRequiredService<IPnPContextFactory>().CreateAsync(name: SITETARGET))
                {
                    //Łączenie z listą (https://pnp.github.io/pnpcore/using-the-sdk/lists-intro.html#getting-lists):
                    var myList = await context.Web.Lists.GetByTitleAsync("EmailsArchive", p => p.Title);

                    foreach (var emailFile in emlsDirectory)
                    {
                        try
                        {
                            //Wczytanie pliku do pamięci:
                            var emlFileInfo = new FileInfo(emailFile);
                            //Odczyta maila przed bibliotekę MsgReader (https://github.com/Sicos1977/MSGReader#read-properties-from-an-outlook-eml-message):
                            var msgReader = Message.Load(emlFileInfo);

                            //Przypisanie różnych parametrów wiadomości do zmiennych:
                            string sender = msgReader.Headers.From.Address;
                            List<string> recipients = msgReader.Headers.To.Select(s => s.Address).ToList();
                            string subject = msgReader.Headers.Subject;
                            string htmlBody = Encoding.UTF8.GetString(msgReader.HtmlBody.Body);
                            string importance = msgReader.Headers.Importance.ToString();
                            DateTime date = msgReader.Headers.DateSent;

                            //Tworzymy obiekt który reprezentacje pole "Person or Group" (https://pnp.github.io/pnpcore/using-the-sdk/listitems-fields.html#multi-user-fields):
                            /*
                             * W przypadku pracy z C#/PS nie możemy użyć emaila jako string w polu "Person or Group" (nie zależnie czy jest ustawiamy jedną osobę czy wiele).
                             * Większość pól przyjmuje standardowe typy danych, np. Text and Multiline text -> string, Number and Currency -> integer lub double.
                             * Ale nie które jak "Person or Group" muszę być odpowiedniego typu, trochę jak w przypadku metody Patch w PA, też należy zbudować dodatkową kolekcje reprezentującą osobę.
                            */
                            var userCollection = new FieldValueCollection();
                            foreach (var recipient in recipients)
                            {
                                try
                                {
                                    //Wyszukiwanie użytkownika na podstawie jego adresu email (https://pnp.github.io/pnpcore/using-the-sdk/security-users.html#addingensuring-a-user):
                                    var myUser = await context.Web.EnsureUserAsync(recipient);
                                    userCollection.Values.Add(new FieldUserValue(myUser));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Użytkownik {recipient} nie odnaleziony lub nie jest aktywny: {ex.Message}");
                                }
                            }

                            //Dodawanie elementów do listy (https://pnp.github.io/pnpcore/using-the-sdk/listitems-intro.html#adding-list-items):
                            /*
                             *  Dictionary możemy potraktować trochę jak JSONa w przypadku funkcji Patch w PA. 
                             *  W Dictionary mamy pole 'key' i 'value', key odpowiada nazwie kolumny w SPO (nazwa wew.) i musi być unikalne, value przechowuje dane (object = dowolny typ (string, int, fielduservalue).
                            */
                            Dictionary<string, object> newItem = new Dictionary<string, object>()
                        {
                            { "Sender", sender },
                            { "Title", subject },
                            { "Recipients", userCollection },
                            { "Body", htmlBody },
                            { "Created", date },
                            { "Modified", date }
                        };
                            //Zapis "kolekcji" jako rekord w SPO:
                            var addedItem = await myList.Items.AddAsync(newItem);
                            //Pobieramy utworzony element na podstawie jego id, wczytujemy właściwość przechowująca załączniki:
                            var myItem = await myList.Items.GetByIdAsync(addedItem.Id, p => p.AttachmentFiles);
                            Console.WriteLine($"Mail {emailFile} dodany do lity pod Id: {myItem.Id}.");

                            //W pętli sprawdzamy z pomocą biblioteki MsgReader
                            foreach (var attachment in msgReader.Attachments)
                            {
                                try
                                {
                                    //Zapisujemy plik załącznika na dysku (nie udało mi się tego zrobić w locie/pamięci)... 
                                    attachment.Save(new FileInfo(attachment.FileName));
                                    using (FileStream sourceStream = File.Open(attachment.FileName, FileMode.Open))
                                    {
                                        //Zapisujemy plik jako załącznik elementu (https://pnp.github.io/pnpcore/using-the-sdk/listitems-attachments.html#adding-a-list-item-attachment):
                                        await myItem.AttachmentFiles.AddAsync(attachment.FileName, sourceStream);
                                    }
                                    Console.WriteLine($"Załącznik: {attachment.FileName} dodany do elementu {myItem.Id}.");
                                    //Usuwam plik z dysku:
                                    File.Delete(attachment.FileName);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Nie można zapisać załącznika {attachment.FileName} do elementu {myItem.Id}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Nie można dodać maila {emailFile} do lity: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}