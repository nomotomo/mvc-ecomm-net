using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Infrastructure.Data;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository, IBrandRepository, ITypesRepository
{
    public ICatalogContext _context { get;  }
    public ProductRepository(ICatalogContext context)
    {
        _context = context;
    }
    async Task<IEnumerable<Product>> IProductRepository.GetAllProducts()
    {
        return await _context.Products.Find(p => true).ToListAsync();
    }
    async Task<Product> IProductRepository.GetProductById(string Id) 
    {
        return await _context.Products.Find(p => p.Id == Id).FirstOrDefaultAsync();
    }
    async Task<IEnumerable<Product>> IProductRepository.GetAllProductsByName(string type)
    {
        return await _context.Products.Find(p => p.Types.Name.ToLower() == type.ToLower()).ToListAsync();
    }
    async Task<IEnumerable<ProductBrand>> IBrandRepository.GetAllBrands()
    {
        return await _context.Brands.Find(p => true).ToListAsync();
    }
    async Task<IEnumerable<Product>> IProductRepository.GetAllProductByBrand(string brand)
    {
        return await _context.Products.Find(p => p.Brands.Name.ToLower() == brand.ToLower()).ToListAsync();
    }
    async Task<Product> IProductRepository.CreateProduct(Product product)
    {
        return await _context.Products.InsertOneAsync(product).ContinueWith(t => product);
    }
    async Task<Product> IProductRepository.UpdateProduct(Product product)
    {
        var updatedProduct = await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
        return updatedProduct.IsAcknowledged && updatedProduct.ModifiedCount > 0 ? product : null;
    }
    async Task<bool> IProductRepository.DeleteProduct(string id)
    {
        var deletedProduct = await _context.Products.DeleteOneAsync(p => p.Id == id);
        return deletedProduct.IsAcknowledged && deletedProduct.DeletedCount > 0;
    }
    async Task<IEnumerable<ProductType>> ITypesRepository.GetAllTypes()
    {
       return await _context.Types.Find(p => true).ToListAsync();
    }
}
