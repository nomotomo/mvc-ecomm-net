using Catalog.Application.Responses;
using MediatR;

namespace Catalog.Application.Queries;

public class GetProductByNameQuery(string name) : IRequest<List<ProductResponse>>
{
    public string name { get; set; } = name;
}