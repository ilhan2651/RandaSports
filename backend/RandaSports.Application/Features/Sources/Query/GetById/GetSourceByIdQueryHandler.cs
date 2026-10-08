using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sources.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sources.Query.GetById;

public sealed class GetSourceByIdQueryHandler(ISourceRepository sourceRepository)
    : IRequestHandler<GetSourceByIdQuery, Result<SourceDto>>
{
    public async Task<Result<SourceDto>> Handle(GetSourceByIdQuery request, CancellationToken cancellationToken)
    {
        var source = await sourceRepository.GetWithSportAsync(request.Id, cancellationToken);

        return source is null
            ? Result<SourceDto>.Fail("Kaynak bulunamadı.", HttpStatusCode.NotFound)
            : Result<SourceDto>.Ok(source.ToDto());
    }
}
