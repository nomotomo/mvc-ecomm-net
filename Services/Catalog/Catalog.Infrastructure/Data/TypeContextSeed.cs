using Catalog.Core.Entities;
using MongoDB.Driver;
using System.Text.Json;

namespace Catalog.Infrastructure.Data;

public class TypeContextSeed
{
    public static void SeedData(IMongoCollection<ProductType> typeCollection)
    {
        var existType = typeCollection.Find(p => true).Any();
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Data/SeedData/types.json");
        if (!existType)
        {
            var typeData = File.ReadAllText(path);
            var types = JsonSerializer.Deserialize<List<ProductType>>(typeData);
            if (types != null)
            {
                foreach (var type in types)
                {
                    typeCollection.InsertOneAsync(type);
                }
            }
            
        }
    }
}