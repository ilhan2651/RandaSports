using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Services;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

public interface IJwtTokenGenerator
{
    /// <summary>Roller jetona yazılıyor; <c>[Authorize(Roles = ...)]</c> onları okuyor.</summary>
    AccessToken Generate(User user, IReadOnlyCollection<string> roleCodes);
}
