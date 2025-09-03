using MongoDB.Driver;
using Catalog.Core.Entities;
using System.Text.Json;

namespace Catalog.Infrastructure.Data;

public static class BrandContextSeed
{
    public static void SeedData(IMongoCollection<ProductBrand> brandCollection)
    {
        var existBrand = brandCollection.Find(p => true).Any();
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Data/SeedData/brands.json");
        if (!existBrand)
        {
            var brandData = File.ReadAllText(path);
            var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandData);
            if (brands != null)
            {
                foreach (var brand in brands)
                {
                    brandCollection.InsertOneAsync(brand);
                }
            }
            
        }
    }
}