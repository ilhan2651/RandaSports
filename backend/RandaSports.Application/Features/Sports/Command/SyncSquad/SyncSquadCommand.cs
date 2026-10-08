using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sports.Command.SyncSquad;

/// <summary>Takımın kadrosunu sağlayıcıdan çeker; oyuncuları ekler ya da günceller.</summary>
public sealed record SyncSquadCommand(Guid TeamId) : IRequest<Result<int>>;
