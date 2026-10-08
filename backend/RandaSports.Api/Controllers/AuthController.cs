using MediatR;
using Microsoft.AspNetCore.Mvc;
using RandaSports.Application.Features.Auth;

namespace RandaSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await mediator.Send(
            new RegisterCommand(request.Email, request.Password, request.FullName, ClientIp));

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password, ClientIp));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.RefreshToken, ClientIp));
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        var result = await mediator.Send(new LogoutCommand(request.RefreshToken));
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// İstemciden gelen adrese güvenmiyoruz; bağlantının kendi adresini yazıyoruz.
    /// Ters vekil arkasına girildiğinde ForwardedHeaders ayarlanmalı.
    /// </summary>
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    public sealed record RegisterRequest(string Email, string Password, string? FullName);

    public sealed record LoginRequest(string Email, string Password);

    public sealed record RefreshRequest(string RefreshToken);
}
