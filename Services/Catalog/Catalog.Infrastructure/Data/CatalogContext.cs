using MongoDB.Driver;
using Catalog.Core.Entities;
using Catalog.Infrastructure.Data.SeedData;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infrastructure.Data;

public class CatalogContext : ICatalogContext
{
    public IMongoCollection<Product> Products { get; }
    public IMongoCollection<ProductType> Types { get; }
    public IMongoCollection<ProductBrand> Brands { get; }

    public CatalogContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString")); 
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
        Brands = database.GetCollection<ProductBrand>(
            configuration.GetValue<string>("DatabaseSettings:BrandsCollection"));
        Products = database.GetCollection<Product>(
            configuration.GetValue<string>("DatabaseSettings:CollectionName"));
        Types = database.GetCollection<ProductType>(
            configuration.GetValue<string>("DatabaseSettings:TypesCollection"));
        
        
        CatalogContextSeed.SeedData(Products);
        BrandContextSeed.SeedData(Brands);
        TypeContextSeed.SeedData(Types);
    }
}