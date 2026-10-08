using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.FetchCommentatorPortraits;

public sealed record FetchCommentatorPortraitsCommand(int Take) : IRequest<Result<int>>;
