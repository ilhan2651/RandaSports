using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Articles.Command.CollectFromSource;

public sealed record CollectSourceCommand(Guid SourceId) : IRequest<Result<int>>;
