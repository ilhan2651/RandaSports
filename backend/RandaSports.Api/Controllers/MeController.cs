using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandaSports.Application.Features.Users;

namespace RandaSports.Api.Controllers;

/// <summary>
/// Giriş yapmış kullanıcının kendi verisi. Kimliği her zaman jetondan okuyoruz;
/// istemciden gelen bir kullanıcı kimliğine asla bakmıyoruz, yoksa herkes
/// başkasının tercihlerini okuyup yazabilirdi.
/// </summary>
[Authorize]
[ApiController]
[Route("api/me")]
public class MeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (UserId is not { } id)
            return Unauthorized();

        var result = await mediator.Send(new GetMeQuery(id));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("tercihler")]
    public async Task<IActionResult> GetPreferences()
    {
        if (UserId is not { } id)
            return Unauthorized();

        var result = await mediator.Send(new GetPreferencesQuery(id));
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Sihirbazın seçenekleri: branşlar, yorumcular, takımlar — tek istekte.</summary>
    [HttpGet("secenekler")]
    public async Task<IActionResult> GetOnboardingOptions()
    {
        var result = await mediator.Send(new GetOnboardingOptionsQuery());
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Tercihlere göre süzülmüş haber ve görüş akışı.</summary>
    [HttpGet("akis")]
    public async Task<IActionResult> GetFeed([FromQuery] int? haber, [FromQuery] int? yorum)
    {
        if (UserId is not { } id)
            return Unauthorized();

        var result = await mediator.Send(new GetFeedQuery(id, haber ?? 24, yorum ?? 24));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPut("tercihler")]
    public async Task<IActionResult> UpdatePreferences([FromBody] PreferencesRequest request)
    {
        if (UserId is not { } id)
            return Unauthorized();

        var result = await mediator.Send(new UpdatePreferencesCommand(
            id,
            request.TeamIds ?? [],
            request.SportIds ?? [],
            request.CommentatorIds ?? [],
            request.CompleteOnboarding));

        return StatusCode((int)result.StatusCode, result);
    }

    private Guid? UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)
            ? id
            : null;

    public sealed record PreferencesRequest(
        List<Guid>? TeamIds,
        List<Guid>? SportIds,
        List<Guid>? CommentatorIds,
        bool CompleteOnboarding = false);
}
