using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandaSports.Application.Features.Sources.Command.Create;
using RandaSports.Application.Features.Sources.Command.Delete;
using RandaSports.Application.Features.Sources.Command.Update;
using RandaSports.Application.Features.Sources.Query.GetAll;
using RandaSports.Application.Features.Sources.Query.GetById;

namespace RandaSports.Api.Controllers;

/// <summary>Kaynak yönetimi: tamamı yönetim ucudur, site bu listeyi kullanmıyor.</summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SourcesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, [FromQuery] Guid? sportId)
    {
        var result = await mediator.Send(new GetSourcesQuery(isActive, sportId));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetSourceByIdQuery(id));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSourceCommand command)
    {
        var result = await mediator.Send(command);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSourceCommand command)
    {
        var result = await mediator.Send(command with { Id = id });
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteSourceCommand(id));
        return StatusCode((int)result.StatusCode, result);
    }
}
