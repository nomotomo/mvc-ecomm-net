using MongoDB.Bson.Serialization.Attributes;

namespace Catalog.Core.Entities;

public class ProductBrand : BaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; }
}