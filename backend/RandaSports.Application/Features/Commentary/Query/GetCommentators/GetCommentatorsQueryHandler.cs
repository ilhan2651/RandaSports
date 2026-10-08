using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Query.GetCommentators;

public sealed class GetCommentatorsQueryHandler(ICommentatorRepository commentatorRepository)
    : IRequestHandler<GetCommentatorsQuery, Result<List<CommentatorDto>>>
{
    public async Task<Result<List<CommentatorDto>>> Handle(
        GetCommentatorsQuery request,
        CancellationToken cancellationToken)
    {
        var profiles = await commentatorRepository.GetProfilesAsync(cancellationToken);

        return Result<List<CommentatorDto>>.Ok(profiles.Select(x => x.ToDto()).ToList());
    }
}

public sealed class GetCommentatorQueryHandler(ICommentatorRepository commentatorRepository)
    : IRequestHandler<GetCommentatorQuery, Result<CommentatorDto>>
{
    public async Task<Result<CommentatorDto>> Handle(
        GetCommentatorQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await commentatorRepository.GetProfileBySlugAsync(request.Slug, cancellationToken);

        if (profile is null)
            return Result<CommentatorDto>.Fail("Yorumcu bulunamadı.", HttpStatusCode.NotFound);

        return Result<CommentatorDto>.Ok(profile.ToDto());
    }
}
