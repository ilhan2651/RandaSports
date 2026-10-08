using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sources.Command.Delete;

public sealed record DeleteSourceCommand(Guid Id) : IRequest<Result>;
