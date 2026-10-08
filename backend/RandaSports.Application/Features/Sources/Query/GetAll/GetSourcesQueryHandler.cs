using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sources.Query.GetAll;

public sealed class GetSourcesQueryHandler(ISourceRepository sourceRepository)
    : IRequestHandler<GetSourcesQuery, Result<List<SourceDto>>>
{
    public async Task<Result<List<SourceDto>>> Handle(GetSourcesQuery request, CancellationToken cancellationToken)
    {
        var sources = await sourceRepository.GetListAsync(request.IsActive, request.SportId, cancellationToken);
        return Result<List<SourceDto>>.Ok(sources.Select(x => x.ToDto()).ToList());
    }
}
