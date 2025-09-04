using Catalog.Application.Commands;
using Catalog.Application.Mappers;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class CreateProductCommandHandler: IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IProductRepository _productRepository;
    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var productEntity = ProductMapper.Mapper.Map<Product>(request);
        if (productEntity is null)
        {
            throw new ApplicationException("Product mapping failed while creating a new product");
        }
        var createdProduct = await _productRepository.CreateProduct(productEntity);
        if (createdProduct is null)
        {
            throw new ApplicationException("Creating a new product failed");
        }
        var productResponse = ProductMapper.Mapper.Map<ProductResponse>(createdProduct);
        return productResponse;
    }
}