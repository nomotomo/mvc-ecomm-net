using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Core.Specs;
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
    async Task<Pagination<Product>> IProductRepository.GetAllProducts(CatalogSpecParams specParams)
    {
        var builder = Builders<Product>.Filter;
        var filter = builder.Empty;
        if (!string.IsNullOrEmpty(specParams.Search))
        {
            filter &= builder.Where(x => x.Name.ToLower().Contains(specParams.Search.ToLower()));
        }
        if (!string.IsNullOrEmpty(specParams.BrandId))
        {
            filter &= builder.Eq(x => x.Brands.Id, specParams.BrandId);
        }

        if (!string.IsNullOrEmpty(specParams.TypeId))
        {
            var type = specParams.TypeId;
            filter &= builder.Eq(x => x.Types.Id, type);
        }
        var totalCount = await _context.Products.CountDocumentsAsync(filter);
        var data = await DataFilter(specParams, filter);
        return new Pagination<Product>(
            specParams.PageIndex,
            specParams.PageSize,
            (int)totalCount,
            data
        );
    }
    async Task<Product> IProductRepository.GetProductById(string Id) 
    {
        return await _context.Products.Find(p => p.Id == Id).FirstOrDefaultAsync();
    }
    async Task<IEnumerable<Product>> IProductRepository.GetAllProductsByName(string type)
    {
        return await _context.Products.Find(p => p.Name.ToLower() == type.ToLower()).ToListAsync();
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
    
    private async Task<IReadOnlyList<Product>> DataFilter(CatalogSpecParams catalogSpecParams, FilterDefinition<Product> filter)
    {
        var sortDefn = Builders<Product>.Sort.Ascending("Name"); // Default
        if (!string.IsNullOrEmpty(catalogSpecParams.Sort))
        {
            switch (catalogSpecParams.Sort)
            {
                case "priceAsc":
                    sortDefn = Builders<Product>.Sort.Ascending(p => p.Price);
                    break;
                case "priceDesc":
                    sortDefn = Builders<Product>.Sort.Descending(p => p.Price);
                    break;
                default:
                    sortDefn = Builders<Product>.Sort.Ascending(p => p.Name);
                    break;

            }
        }
        return await _context
            .Products
            .Find(filter)
            .Sort(sortDefn)
            .Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
            .Limit(catalogSpecParams.PageSize)
            .ToListAsync();
    }
}
