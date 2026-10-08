using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Query.GetUnverifiedCommentators;

public sealed class GetUnverifiedCommentatorsQueryHandler(ICommentatorRepository commentatorRepository)
    : IRequestHandler<GetUnverifiedCommentatorsQuery, Result<List<UnverifiedCommentatorDto>>>
{
    public async Task<Result<List<UnverifiedCommentatorDto>>> Handle(
        GetUnverifiedCommentatorsQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await commentatorRepository.GetUnverifiedAsync(cancellationToken);

        var items = rows
            .Select(x => new UnverifiedCommentatorDto(
                x.Commentator.Id,
                x.Commentator.FullName,
                x.Commentator.Slug,
                x.Commentator.PhotoUrl,
                x.OpinionCount,
                x.ApprovedCount,
                x.LastOpinionAt,
                x.ChannelNames,
                [.. x.Sample.Select(o => new OpinionSampleDto(
                    o.Topic,
                    o.Quote,
                    o.VideoTitle,
                    o.YouTubeVideoId,
                    o.TimestampSeconds))]))
            .ToList();

        return Result<List<UnverifiedCommentatorDto>>.Ok(items);
    }
}
