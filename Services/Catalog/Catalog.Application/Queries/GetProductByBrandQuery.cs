using Catalog.Application.Responses;
using MediatR;

namespace Catalog.Application.Queries;

public class GetProductByBrandQuery(string name) : IRequest<IList<ProductResponse>>
{
    public string Brand { get; set; } = name;
}