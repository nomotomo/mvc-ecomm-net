using MongoDB.Driver;
using Catalog.Core.Entities;
using System.Text.Json;

namespace Catalog.Infrastructure.Data.SeedData;

public class CatalogContextSeed
{
    public static void SeedData(IMongoCollection<Product> productCollection)
    {
        var existProduct = productCollection.Find(p => true).Any();
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Data/SeedData/products.json");
        if (!existProduct)
        {
            var productData = File.ReadAllText(path);
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