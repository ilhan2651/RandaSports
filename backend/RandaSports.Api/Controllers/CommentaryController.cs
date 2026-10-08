using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandaSports.Application.Features.Commentary.Command.AttachSpeaker;
using RandaSports.Application.Features.Commentary.Command.CreateChannel;
using RandaSports.Application.Features.Commentary.Command.DiscoverChannelVideos;
using RandaSports.Application.Features.Commentary.Command.ExtractVideoOpinions;
using RandaSports.Application.Features.Commentary.Command.ReviewCommentator;
using RandaSports.Application.Features.Commentary.Command.ReviewOpinion;
using RandaSports.Application.Features.Commentary.Command.UpdateChannel;
using RandaSports.Application.Features.Commentary.Command.UpdateCommentator;
using RandaSports.Application.Features.Commentary.Query.GetChannels;
using RandaSports.Application.Features.Commentary.Query.GetCommentators;
using RandaSports.Application.Features.Commentary.Query.GetLatestOpinions;
using RandaSports.Application.Features.Commentary.Query.GetOpinions;
using RandaSports.Application.Features.Commentary.Query.GetStoryOpinions;
using RandaSports.Application.Features.Commentary.Query.GetVideoStatus;
using RandaSports.Domain.Entities;
using RandaSports.Application.Features.Commentary.Query.GetUnverifiedCommentators;

namespace RandaSports.Api.Controllers;

/// <summary>
/// Yorumcu görüşleri. Varsayılan olarak tamamı yönetici rolü istiyor; sitenin
/// okuduğu uçlar tek tek [AllowAnonymous] ile açılıyor. Böylece sonradan eklenen bir
/// uç unutulursa açıkta kalmıyor, kapalı kalıyor. Okuyucu hesapları buraya giremiyor:
/// giriş yapmış olmak yönetim yetkisi vermiyor.
/// </summary>
[Authorize(Roles = RoleCodes.Admin)]
[ApiController]
[Route("api/[controller]")]
public class CommentaryController(IMediator mediator) : ControllerBase
{
    [HttpGet("channels")]
    public async Task<IActionResult> GetChannels()
    {
        var result = await mediator.Send(new GetChannelsQuery());
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("channels")]
    public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
    {
        var result = await mediator.Send(new CreateChannelCommand(request.Name, request.Reference, request.SportSlugs));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPatch("channels/{id:guid}")]
    public async Task<IActionResult> UpdateChannel(Guid id, [FromBody] UpdateChannelRequest request)
    {
        var result = await mediator.Send(
            new UpdateChannelCommand(id, request.Name, request.Reference, request.IsActive, request.SportSlugs));

        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Kanalı sırasını beklemeden tarar.</summary>
    [HttpPost("channels/{id:guid}/scan")]
    public async Task<IActionResult> ScanChannel(Guid id, [FromQuery] int maxAgeDays = 7, [FromQuery] int maxPerRun = 3)
    {
        var result = await mediator.Send(new DiscoverChannelVideosCommand(id, maxAgeDays, maxPerRun));
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Videoyu sırasını beklemeden modele izletir.</summary>
    /// <summary>
    /// Video boru hattının durumu. Görüş akmıyorsa ilk bakılacak yer: durum sayıları
    /// videoların Pending'de mi beklediğini, Skipped mi olduğunu yoksa hata mı aldığını
    /// gösteriyor.
    /// </summary>
    [HttpGet("videos")]
    public async Task<IActionResult> GetVideoStatus(
        [FromQuery] string? status = null,
        [FromQuery] int take = 50)
    {
        var result = await mediator.Send(new GetVideoStatusQuery(status, take));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("videos/{id:guid}/extract")]
    public async Task<IActionResult> ExtractVideo(Guid id)
    {
        var result = await mediator.Send(new ExtractVideoOpinionsCommand(id));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("opinions")]
    public async Task<IActionResult> GetOpinions(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetOpinionsQuery(status, page, pageSize));
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Siteye açık görüş akışı: yalnızca onaylananlar.</summary>
    [AllowAnonymous]
    [HttpGet("opinions/latest")]
    public async Task<IActionResult> GetLatestOpinions(
        [FromQuery] string? commentator = null,
        [FromQuery] string? team = null,
        [FromQuery] string? sport = null,
        [FromQuery] int days = 0,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await mediator.Send(
            new GetLatestOpinionsQuery(commentator, team, sport, days, page, pageSize));

        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Filtre çubuğu için: hakkında görüş bulunan takımlar.</summary>
    [AllowAnonymous]
    [HttpGet("teams")]
    public async Task<IActionResult> GetOpinionTeams()
    {
        var result = await mediator.Send(new GetOpinionTeamsQuery());
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Filtre çubuğu için: hakkında görüş bulunan branşlar.</summary>
    [AllowAnonymous]
    [HttpGet("sports")]
    public async Task<IActionResult> GetOpinionSports()
    {
        var result = await mediator.Send(new GetOpinionSportsQuery());
        return StatusCode((int)result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpGet("commentators")]
    public async Task<IActionResult> GetCommentators()
    {
        var result = await mediator.Send(new GetCommentatorsQuery());
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Videodan otomatik eklenmiş, onay bekleyen konuşmacılar.</summary>
    [HttpGet("commentators/unverified")]
    public async Task<IActionResult> GetUnverifiedCommentators()
    {
        var result = await mediator.Send(new GetUnverifiedCommentatorsQuery());
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("commentators/{id:guid}/verify")]
    public async Task<IActionResult> VerifyCommentator(Guid id)
    {
        var result = await mediator.Send(new ReviewCommentatorCommand(id, true));
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Yanlış eklenmiş kişiyi siler; görüşleri onay ekranına geri döner.</summary>
    [HttpPost("commentators/{id:guid}/reject")]
    public async Task<IActionResult> RejectCommentator(Guid id)
    {
        var result = await mediator.Send(new ReviewCommentatorCommand(id, false));
        return StatusCode((int)result.StatusCode, result);
    }

    [AllowAnonymous]
    /// <summary>
    /// Yorumcu görseli ve biyografisi. Görsel yalnızca buradan, elle giriliyor.
    /// </summary>
    [HttpPatch("commentators/{id:guid}")]
    public async Task<IActionResult> UpdateCommentator(
        Guid id,
        [FromBody] UpdateCommentatorRequest request)
    {
        var result = await mediator.Send(
            new UpdateCommentatorCommand(id, request.PhotoUrl, request.Bio));

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("commentators/{slug}")]
    public async Task<IActionResult> GetCommentator(string slug)
    {
        var result = await mediator.Send(new GetCommentatorQuery(slug));
        return StatusCode((int)result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpGet("opinions/story/{storyId:guid}")]
    public async Task<IActionResult> GetStoryOpinions(Guid storyId)
    {
        var result = await mediator.Send(new GetStoryOpinionsQuery(storyId));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("opinions/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewRequest? request)
    {
        var result = await mediator.Send(new ReviewOpinionCommand(id, true, request?.Note));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("opinions/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewRequest? request)
    {
        var result = await mediator.Send(new ReviewOpinionCommand(id, false, request?.Note));
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Konuşmacıyı sözlüğe ekler, görüşe ve kanalın kadrosuna bağlar.</summary>
    [HttpPost("opinions/{id:guid}/attach-speaker")]
    public async Task<IActionResult> AttachSpeaker(Guid id, [FromBody] AttachSpeakerRequest? request)
    {
        var result = await mediator.Send(new AttachSpeakerCommand(id, request?.FullName));
        return StatusCode((int)result.StatusCode, result);
    }

    public sealed record CreateChannelRequest(string Name, string Reference, string[]? SportSlugs);

    public sealed record UpdateChannelRequest(string? Name, string? Reference, bool? IsActive, string[]? SportSlugs);

    public sealed record UpdateCommentatorRequest(string? PhotoUrl, string? Bio);

    public sealed record ReviewRequest(string? Note);

    public sealed record AttachSpeakerRequest(string? FullName);
}
