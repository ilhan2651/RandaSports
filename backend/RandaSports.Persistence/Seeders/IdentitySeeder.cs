using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Seeders;

/// <summary>
/// Rolleri kurar ve hiç yönetici yoksa ayarlardaki hesabı açar. Roller her açılışta
/// kontrol ediliyor — kod yeni bir rol tanımlarsa elle müdahale gerekmesin diye.
/// Yönetici hesabı ise yalnızca hiç yönetici yokken açılıyor: bu bir parola
/// sıfırlama yolu değil.
/// </summary>
public sealed class IdentitySeeder(
    RandaSportsDbContext context,
    IPasswordHasher passwordHasher,
    IOptions<AdminSeedOptions> options,
    TimeProvider timeProvider,
    ILogger<IdentitySeeder> logger)
{
    private readonly AdminSeedOptions _options = options.Value;

    private static readonly (string Code, string Name, string Description)[] Roles =
    [
        (RoleCodes.Admin, "Yönetici", "Yönetim ekranlarına ve yönetim uçlarına erişir."),
        (RoleCodes.Reader, "Okuyucu", "Takip listesi tutar, kişiye özel bülten alır.")
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        await SeedRolesAsync(now, cancellationToken);
        await SeedAdminAsync(now, cancellationToken);
    }

    private async Task SeedRolesAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var mevcut = await context.Roles
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        var eksik = Roles.Where(x => !mevcut.Contains(x.Code)).ToList();

        if (eksik.Count == 0)
            return;

        foreach (var (code, name, description) in eksik)
            context.Roles.Add(new Role
            {
                Code = code,
                Name = name,
                Description = description,
                CreatedAt = now,
                UpdatedAt = now
            });

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("{Count} rol eklendi.", eksik.Count);
    }

    private async Task SeedAdminAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var adminRole = await context.Roles
            .FirstOrDefaultAsync(x => x.Code == RoleCodes.Admin, cancellationToken);

        if (adminRole is null)
            return;

        var yoneticiVar = await context.UserRoles
            .AnyAsync(x => x.RoleId == adminRole.Id, cancellationToken);

        if (yoneticiVar)
            return;

        if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            logger.LogWarning(
                "Yönetici hesabı yok ve AdminSeed ayarı boş. Yönetim ekranlarına girilemez; "
                + "appsettings.Development.json içine AdminSeed:Email ve AdminSeed:Password ekleyin.");

            return;
        }

        var email = _options.Email.Trim();
        var normalized = email.ToUpperInvariant();

        // Hesap okuyucu olarak açılmış olabilir; o zaman yeni kullanıcı yaratmak
        // yerine mevcut kayda yönetici rolünü ekliyoruz.
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalized, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Email = email,
                NormalizedEmail = normalized,
                PasswordHash = passwordHasher.Hash(_options.Password),
                FullName = string.IsNullOrWhiteSpace(_options.FullName) ? null : _options.FullName.Trim(),
                IsEmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.Users.Add(user);
        }

        context.UserRoles.Add(new UserRole
        {
            User = user,
            Role = adminRole,
            AssignedAt = now
        });

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Yönetici hesabı hazırlandı: {Email}", user.Email);
    }
}
