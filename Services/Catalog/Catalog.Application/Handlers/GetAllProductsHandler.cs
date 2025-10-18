using AutoMapper;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllProductsHandler: IRequestHandler<GetAllProductQuery, IList<ProductResponse>>
{   
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    public GetAllProductsHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }
    public async Task<IList<ProductResponse>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
    {
        var productList = await _productRepository.GetAllProducts();
        var productResponseList = _mapper.Map<IList<Product>, IList<ProductResponse>>(productList.ToList());
        return productResponseList;
    }
}