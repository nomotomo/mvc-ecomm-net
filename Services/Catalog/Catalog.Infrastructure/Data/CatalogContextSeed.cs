using MongoDB.Driver;
using Catalog.Core.Entities;
using System.Text.Json;

namespace Catalog.Infrastructure.Data.SeedData;

public class CatalogContextSeed
{
    public static void SeedData(IMongoCollection<Product> productCollection)
    {   
        // check if there is any data in the collection
        // if not, seed the data from the json file
        // if there is data, do nothing
        var existProduct = productCollection.Find(p => true).Any();
        string path = Path.Combine("Data", "SeedData", "products.json");
        if (!existProduct)
        {
            // var productData = File.ReadAllText(path);
            var productData = File.ReadAllText("../Catalog.Infrastructure/Data/SeedData/products.json");
            var products = JsonSerializer.Deserialize<List<Product>>(productData);
            if (products != null)
            {
                foreach (var product in products)
                {
                    productCollection.InsertOneAsync(product);
                }
            }
            
        }
    }
}