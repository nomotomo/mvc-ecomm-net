using Catalog.Core.Entities;

namespace Catalog.Core.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProducts();
    Task<Product> GetProductById(string id);
    Task<IEnumerable<Product>> GetAllProductsByType(string type);
    Task<IEnumerable<Product>> GetAllProductByBrand(string brand);
    Task<Product> CreateProduct(Product product);
    Task<Product> UpdateProduct(Product product);
    Task<bool> DeleteProduct(string id);
}