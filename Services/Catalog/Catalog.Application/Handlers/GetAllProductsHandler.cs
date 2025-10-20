using AutoMapper;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Core.Specs;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllProductsHandler: IRequestHandler<GetAllProductQuery, Pagination<ProductResponse>>
{   
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    public GetAllProductsHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }
    public async Task<Pagination<ProductResponse>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
    {
        var productList = await _productRepository.GetAllProducts(request.CatalogSpecParams);
        var productResponseList = _mapper.Map<Pagination<ProductResponse>>(productList);
        return productResponseList;
    }
}