using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandaSports.Application.Features.Articles.Query.GetLatest;

namespace RandaSports.Api.Controllers;

/// <summary>Ham haber listesi: yalnızca yönetim içindir, site konuları /api/stories üzerinden okuyor.</summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ArticlesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLatest(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sportId = null,
        [FromQuery] string? search = null)
    {
        var result = await mediator.Send(new GetLatestArticlesQuery(page, pageSize, sportId, search));
        return StatusCode((int)result.StatusCode, result);
    }
}
