using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.ExtractVideoOpinions;

/// <param name="StoryMatchDays">Görüş, bu kadar günlük haberlerle eşleştirilmeye çalışılıyor.</param>
/// <param name="SegmentThresholdMinutes">Bu süreyi aşan video dilimlenerek işleniyor.</param>
/// <param name="SegmentMinutes">Dilim uzunluğu.</param>
/// <param name="SegmentOverlapSeconds">Dilim sınırındaki konuşma bölünmesin diye bindirme.</param>
/// <param name="MaxOpinionsPerVideo">Videodan yayına alınacak en fazla görüş.</param>
/// <param name="MaxSegmentsPerVideo">Bir videoya en fazla kaç çağrı yapılacak.</param>
public sealed record ExtractVideoOpinionsCommand(
    Guid VideoId,
    int StoryMatchDays = 4,
    int SegmentThresholdMinutes = 12,
    int SegmentMinutes = 8,
    int SegmentOverlapSeconds = 15,
    int MaxOpinionsPerVideo = 3,
    int MaxSegmentsPerVideo = 8) : IRequest<Result<int>>;
