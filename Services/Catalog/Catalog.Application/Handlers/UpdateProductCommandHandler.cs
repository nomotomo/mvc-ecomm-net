using Catalog.Application.Commands;
using Catalog.Application.Mappers;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class UpdateProductCommandHandler: IRequestHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IProductRepository _productRepository;
    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productEntity = await _productRepository.UpdateProduct(
            new Product
            {
                Id = request.Id,
                Name = request.Name,
                Summary = request.Summary,
                Description = request.Description,
                ImageFile = request.ImageFile,
                Price = request.Price,
                Brands = request.Brands,
                Types = request.Types
            });

        if (productEntity is null)
        {
            throw new ApplicationException("Product update failed..");
        }
        
        var productResponse = ProductMapper.Mapper.Map<ProductResponse>(productEntity);
        return productResponse;
    }
}