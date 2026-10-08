using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;

namespace RandaSports.Application.Features.Commentary.Query.GetChannels;

public sealed record GetChannelsQuery : IRequest<Result<List<ChannelDto>>>;
