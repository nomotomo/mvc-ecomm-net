using Catalog.Application.Responses;
using MediatR;

namespace Catalog.Application.Queries;

public class GetAllProductQuery: IRequest<IList<ProductResponse>>
{
    
}