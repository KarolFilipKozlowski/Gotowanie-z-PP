using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using SQLandSPO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLandSPO.Helper
{
    public static class SQLHelper
    {
        public static void AddToSQL(AdventureWorksLTContext adventureWorksLTContext, IList myList)
        {
            foreach (var listItem in myList.Items.AsRequested())
            {
                try
                {
                    Product product = new()
                    {
                        Title = listItem["Title"].ToString(),
                        ProductNumber = listItem["ProductNumber"].ToString(),
                        Color = listItem["Color"] != null ? listItem["Color"].ToString() : null,
                        StandardCost = double.Parse(listItem["StandardCost"].ToString()),
                        ListPrice = double.Parse(listItem["ListPrice"].ToString()),
                        Size = listItem["Size"] != null ? listItem["Size"].ToString() : null,
                        Weight = listItem["Weight"] != null ? double.Parse(listItem["Weight"].ToString()) : null,
                        ProductCategoryID = listItem["ProductCategoryID"] != null ? int.Parse(listItem["ProductCategoryID"].ToString()) : null,
                        ProductModelID = listItem["ProductModelID"] != null ? int.Parse(listItem["ProductModelID"].ToString()) : null,
                        SellStartDate = DateTime.Parse(listItem["SellStartDate"].ToString()),
                        SellEndDate = listItem["SellEndDate"] != null ? DateTime.Parse(listItem["SellEndDate"].ToString()) : null,
                        DiscontinuedDate = listItem["DiscontinuedDate"] != null ? DateTime.Parse(listItem["DiscontinuedDate"].ToString()) : null
                    };

                    adventureWorksLTContext.Add(product);
                    adventureWorksLTContext.SaveChanges();
                    Console.WriteLine($"Product {product.Title} added to SQL.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot add item to SQL: {ex.Message}");
                }
            }
        }
    }
}
