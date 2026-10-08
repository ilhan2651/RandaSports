using MediatR;
using Microsoft.AspNetCore.Mvc;
using RandaSports.Application.Features.Stories.Query.GetStories;
using RandaSports.Application.Features.Stories.Query.GetStoryBySlug;

namespace RandaSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sport = null,
        [FromQuery] string? search = null)
    {
        var result = await mediator.Send(new GetStoriesQuery(page, pageSize, sport, search));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await mediator.Send(new GetStoryBySlugQuery(slug));
        return StatusCode((int)result.StatusCode, result);
    }
}
