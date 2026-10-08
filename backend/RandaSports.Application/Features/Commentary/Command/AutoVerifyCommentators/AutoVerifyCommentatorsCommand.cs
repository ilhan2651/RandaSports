using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.AutoVerifyCommentators;

/// <summary>Kanıtı yeterli olan isimleri insana sormadan doğrular.</summary>
public sealed record AutoVerifyCommentatorsCommand : IRequest<Result<int>>;
