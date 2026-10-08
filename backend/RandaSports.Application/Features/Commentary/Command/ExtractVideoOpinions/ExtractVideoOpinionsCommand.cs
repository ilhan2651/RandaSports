using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.ExtractVideoOpinions;

/// <param name="StoryMatchDays">Görüş, bu kadar günlük haberlerle eşleştirilmeye çalışılıyor.</param>
public sealed record ExtractVideoOpinionsCommand(
    Guid VideoId,
    int StoryMatchDays = 4) : IRequest<Result<int>>;
