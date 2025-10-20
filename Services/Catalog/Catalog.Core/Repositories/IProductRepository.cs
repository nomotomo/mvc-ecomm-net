using Catalog.Core.Entities;
using Catalog.Core.Specs;

namespace Catalog.Core.Repositories;

public interface IProductRepository
{
    Task<Pagination<Product>> GetAllProducts(CatalogSpecParams catalogSpecParams);
    Task<Product> GetProductById(string id);
    Task<IEnumerable<Product>> GetAllProductsByName(string name);
    Task<IEnumerable<Product>> GetAllProductByBrand(string brand);
    Task<Product> CreateProduct(Product product);
    Task<Product> UpdateProduct(Product product);
    Task<bool> DeleteProduct(string id);
}