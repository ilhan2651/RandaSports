using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.ReviewOpinion;

/// <param name="Approve">true: yayına al, false: reddet.</param>
public sealed record ReviewOpinionCommand(
    Guid OpinionId,
    bool Approve,
    string? Note = null) : IRequest<Result<bool>>;
