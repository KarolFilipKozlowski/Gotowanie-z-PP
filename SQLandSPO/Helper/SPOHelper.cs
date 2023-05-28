using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using SQLandSPO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLandSPO.SPOHelper
{
    public static class SPOHelper
    {
        public static async Task ShowListItems(PnPContext context, string viewXml, string text)
        {
            var myList = context.Web.Lists.GetById(Guid.Parse("97a2170c-b5c1-4513-a528-1c8b3c7a7642"), p => p.Title,
                                                 p => p.Fields.QueryProperties(p => p.InternalName,
                                                                               p => p.FieldTypeKind,
                                                                               p => p.TypeAsString,
                                                                               p => p.Title));

            await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
            {
                ViewXml = viewXml,
                DatesInUtc = true
            });

            Console.WriteLine($"------- {text} -------");
            Console.WriteLine("{0,-50}\t{1,-25}\t{2,-15}\t{3,-5}", "Product name", "Product number", "Color", "Id");
            foreach (var listItem in myList.Items.AsRequested())
            {
                Console.WriteLine("{0,-50}\t{1,-25}\t{2,-15}\t{3,-5}", listItem["Title"], listItem["ProductNumber"], listItem["Color"], listItem.Id);
            }
            Console.WriteLine("---------------------------\n");
        }

        public static async Task AddProducts(List<Product> products, IList myList)
        {
            foreach (var product in products)
            {
                try
                {
                    var item = product.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(product));
                    item.Remove("ProductId");
                    item.Remove("ModifiedDate");
                    var addedItem = await myList.Items.AddAsync(item);
                    Console.WriteLine($"Product {product.Title} added to SPO.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot add item to SPO: {ex.Message}");
                }
            }
        }
    }
}
