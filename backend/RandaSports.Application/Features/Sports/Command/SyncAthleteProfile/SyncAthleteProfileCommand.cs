using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sports.Command.SyncAthleteProfile;

/// <summary>Oyuncunun künyesini ve turnuva bazlı sezon istatistiklerini tazeler.</summary>
public sealed record SyncAthleteProfileCommand(Guid AthleteId, int Season) : IRequest<Result<bool>>;
