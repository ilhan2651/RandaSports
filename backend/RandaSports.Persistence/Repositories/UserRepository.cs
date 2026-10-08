using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class UserRepository(RandaSportsDbContext context)
    : GenericRepository<User>(context), IUserRepository
{
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<User?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetWithPreferencesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Users
            .Include(x => x.FollowedTeams)
            .Include(x => x.FollowedSports)
            .Include(x => x.FollowedCommentators)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<Role?> GetRoleByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Context.Roles.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public Task<RefreshToken?> GetRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        Context.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public async Task AddRefreshTokenAsync(
        RefreshToken token,
        CancellationToken cancellationToken = default) =>
        await Context.RefreshTokens.AddAsync(token, cancellationToken);
}
