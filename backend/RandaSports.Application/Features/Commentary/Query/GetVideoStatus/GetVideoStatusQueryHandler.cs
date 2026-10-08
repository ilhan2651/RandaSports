using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Query.GetVideoStatus;

public sealed class GetVideoStatusQueryHandler(IVideoRepository videoRepository)
    : IRequestHandler<GetVideoStatusQuery, Result<VideoStatusReportDto>>
{
    private const int MaxTake = 200;

    public async Task<Result<VideoStatusReportDto>> Handle(
        GetVideoStatusQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, MaxTake);

        var counts = await videoRepository.GetStatusCountsAsync(cancellationToken);
        var rows = await videoRepository.GetStatusRowsAsync(request.Status, take, cancellationToken);

        return Result<VideoStatusReportDto>.Ok(
            new VideoStatusReportDto(counts, rows.Select(x => x.ToDto()).ToList()));
    }
}
