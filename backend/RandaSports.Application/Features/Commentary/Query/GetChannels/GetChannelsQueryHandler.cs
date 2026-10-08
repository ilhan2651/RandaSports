using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Query.GetChannels;

public sealed class GetChannelsQueryHandler(IChannelRepository channelRepository)
    : IRequestHandler<GetChannelsQuery, Result<List<ChannelDto>>>
{
    public async Task<Result<List<ChannelDto>>> Handle(
        GetChannelsQuery request,
        CancellationToken cancellationToken)
    {
        var channels = await channelRepository.GetAllWithCommentatorsAsync(cancellationToken);

        return Result<List<ChannelDto>>.Ok(channels.Select(x => x.ToDto()).ToList());
    }
}
