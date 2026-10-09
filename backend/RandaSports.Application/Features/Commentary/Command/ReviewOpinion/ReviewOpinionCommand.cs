using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.ReviewOpinion;

/// <param name="Approve">true: yayına al, false: reddet.</param>
/// <param name="Quote">
/// Düzeltilmiş alıntı. Ön kontrol modelin başka bir cümle duyduğunu söylediğinde
/// onay ekranı duyulan hali tek tıkla buraya geçiriyor; görüşü reddedip yeniden
/// çıkarmaya gerek kalmıyor.
/// </param>
/// <param name="TimestampSeconds">Düzeltilmiş an.</param>
public sealed record ReviewOpinionCommand(
    Guid OpinionId,
    bool Approve,
    string? Note = null,
    string? Quote = null,
    int? TimestampSeconds = null) : IRequest<Result<bool>>;

/// <summary>
/// Birden çok görüşü tek seferde onaylar ya da reddeder. Ön kontrolden "birebir"
/// çıkanları toplu geçirmek için; ekranda tek tek tıklamak zaman alıyor.
/// </summary>
public sealed record ReviewOpinionsCommand(
    List<Guid> OpinionIds,
    bool Approve,
    string? Note = null) : IRequest<Result<int>>;
