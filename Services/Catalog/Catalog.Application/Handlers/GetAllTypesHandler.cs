using AutoMapper;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using MediatR;

namespace Catalog.Application.Handlers;

public class GetAllTypesHandler: IRequestHandler<GetAllTypesQuery, IList<TypeResponse>> 
{
    private readonly ITypesRepository _typesRepository;
    private readonly IMapper _mapper;
    public GetAllTypesHandler(ITypesRepository typesRepository, IMapper mapper)
    {
        _typesRepository = typesRepository;
        _mapper = mapper;
    }

    public async Task<IList<TypeResponse>> Handle(GetAllTypesQuery request, CancellationToken cancellationToken)
    {
        var typesList = await _typesRepository.GetAllTypes();
        var typeResponseList = _mapper.Map<IList<ProductType>, IList<TypeResponse>>(typesList.ToList());
        return typeResponseList;
    }
}