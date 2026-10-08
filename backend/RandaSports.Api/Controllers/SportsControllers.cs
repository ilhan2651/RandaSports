using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandaSports.Application.Features.Sports.Command.UpdateTeamLogo;
using RandaSports.Domain.Entities;
using RandaSports.Application.Features.Sports.Query.GetAthlete;
using RandaSports.Application.Features.Sports.Query.GetFixtures;
using RandaSports.Application.Features.Sports.Query.GetSports;
using RandaSports.Application.Features.Sports.Query.GetStandings;
using RandaSports.Application.Features.Sports.Query.GetTeam;

namespace RandaSports.Api.Controllers;

/// <summary>Branş listesi: menü, filtreler ve site haritası buradan besleniyor.</summary>
[ApiController]
[Route("api/[controller]")]
public class SportsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool onlyWithStories = false)
    {
        var result = await mediator.Send(new GetSportsQuery(onlyWithStories));
        return StatusCode((int)result.StatusCode, result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class StandingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string sport = "futbol",
        [FromQuery] string? competition = null)
    {
        var result = await mediator.Send(new GetStandingsQuery(sport, competition));
        return StatusCode((int)result.StatusCode, result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class FixturesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string sport = "futbol",
        [FromQuery] string? team = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
    {
        var result = await mediator.Send(new GetFixturesQuery(sport, team, from, to));
        return StatusCode((int)result.StatusCode, result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        var result = await mediator.Send(new GetTeamQuery(slug));
        return StatusCode((int)result.StatusCode, result);
    }
}

[ApiController]
[Route("api/admin/teams")]
[Authorize(Roles = RoleCodes.Admin)]
public class AdminTeamsController(IMediator mediator) : ControllerBase
{
    /// <summary>Logosu olmayanlar başta olmak üzere tüm takımlar.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await mediator.Send(new GetTeamLogosQuery());
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>Arka plan işinin bulamadığı logoyu elle girmek için.</summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateLogo(Guid id, [FromBody] TeamLogoRequest request)
    {
        var result = await mediator.Send(new UpdateTeamLogoCommand(id, request.LogoUrl));
        return StatusCode((int)result.StatusCode, result);
    }

    public sealed record TeamLogoRequest(string? LogoUrl);
}

[ApiController]
[Route("api/[controller]")]
public class AthletesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        var result = await mediator.Send(new GetAthleteQuery(slug));
        return StatusCode((int)result.StatusCode, result);
    }
}
