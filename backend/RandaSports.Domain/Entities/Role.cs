using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Role : BaseEntity
{
    /// <summary>
    /// Kodla çalışırken kullanılan değişmez anahtar ("admin", "reader"). Jetona bu
    /// yazılıyor ve <c>[Authorize(Roles = ...)]</c> bunu okuyor; <see cref="Name"/>
    /// ekranda görünen ad olduğu için değişebilir, kod değişmiyor.
    /// </summary>
    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}

/// <summary>Kod içinde geçen sabit rol anahtarları; yazım hatası derlemede çıksın diye.</summary>
public static class RoleCodes
{
    public const string Admin = "admin";
    public const string Reader = "reader";
}
