using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Commentary.Query.GetOpinions;

public sealed class GetOpinionsQueryHandler(IOpinionRepository opinionRepository)
    : IRequestHandler<GetOpinionsQuery, Result<Paged<OpinionDto>>>
{
    private const int MaxPageSize = 50;

    public async Task<Result<Paged<OpinionDto>>> Handle(
        GetOpinionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, MaxPageSize);

        OpinionStatus? status = Enum.TryParse<OpinionStatus>(request.Status, true, out var parsed)
            ? parsed
            : null;

        var opinions = await opinionRepository.GetPagedAsync(status, page, pageSize, cancellationToken);

        var dto = new Paged<OpinionDto>(
            opinions.Items.Select(x => x.ToDto()).ToList(),
            opinions.Total,
            opinions.Page,
            opinions.PageSize);

        return Result<Paged<OpinionDto>>.Ok(dto);
    }
}
