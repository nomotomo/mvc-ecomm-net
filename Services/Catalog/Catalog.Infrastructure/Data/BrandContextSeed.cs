using MongoDB.Driver;
using Catalog.Core.Entities;
using System.Text.Json;

namespace Catalog.Infrastructure.Data;

public static class BrandContextSeed
{
    public static void SeedData(IMongoCollection<ProductBrand> brandCollection)
    {
        // check if there is any data in the collection
        // if not, seed the data from the json file
        // if there is data, do nothing
        var existBrand = brandCollection.Find(p => true).Any();
        string path = Path.Combine("Data", "SeedData", "brands.json");
        if (!existBrand)
        {
            
            // var brandData = File.ReadAllText(path);
            var brandData = File.ReadAllText("../Catalog.Infrastructure/Data/SeedData/brands.json");
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