using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sports.Command.FetchTeamLogos;

public sealed record FetchTeamLogosCommand(int Take) : IRequest<Result<int>>;
