using Catalog.Core.Entities;
using MongoDB.Driver;
using System.Text.Json;

namespace Catalog.Infrastructure.Data;

public class TypeContextSeed
{
    public static void SeedData(IMongoCollection<ProductType> typeCollection)
    {
        // check if there is any data in the collection
        // if not, seed the data from the json file
        // if there is data, do nothing
        var existType = typeCollection.Find(p => true).Any();
        string path = Path.Combine("Data", "SeedData", "types.json");
        if (!existType)
        {
            // var typeData = File.ReadAllText(path);
            var typeData = File.ReadAllText("../Catalog.Infrastructure/Data/SeedData/types.json");
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