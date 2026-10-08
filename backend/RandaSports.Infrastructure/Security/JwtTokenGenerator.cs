using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Infrastructure.Security;

/// <summary>
/// Jeton üretimi. Eski <c>JwtSecurityTokenHandler</c> yerine
/// <see cref="JsonWebTokenHandler"/> kullanılıyor: eski tip
/// <c>System.IdentityModel.Tokens.Jwt</c> ad alanında, yenisi
/// <c>Microsoft.IdentityModel.JsonWebTokens</c> ad alanında duruyor ve ikisi
/// <c>JwtRegisteredClaimNames</c> gibi aynı adlı tipler içeriyor. Tek ad alanında
/// kalarak o çakışmayı baştan engelliyoruz.
/// </summary>
public sealed class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    : IJwtTokenGenerator
{
    /// <summary>Güvenlik damgası talebi; jeton toplu iptalinde karşılaştırılıyor.</summary>
    public const string SecurityStampClaim = "sstamp";

    private readonly JwtOptions _options = options.Value;

    public AccessToken Generate(User user, IReadOnlyCollection<string> roleCodes)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.LifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),

            // Her jetonun kendi kimliği olsun: ileride iptal listesi tutulacaksa gerekli.
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(SecurityStampClaim, user.SecurityStamp)
        };

        if (!string.IsNullOrWhiteSpace(user.FullName))
            claims.Add(new Claim(ClaimTypes.Name, user.FullName));

        // Rol talebi ClaimTypes.Role adıyla yazılıyor; [Authorize(Roles = "...")]
        // varsayılan olarak bu adı okuyor.
        foreach (var code in roleCodes)
            claims.Add(new Claim(ClaimTypes.Role, code));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}
