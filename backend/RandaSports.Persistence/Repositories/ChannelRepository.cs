using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class ChannelRepository(RandaSportsDbContext context)
    : GenericRepository<Channel>(context), IChannelRepository
{
    public Task<List<Channel>> GetDueForCheckAsync(
        int take,
        DateTimeOffset cutoff,
        CancellationToken cancellationToken = default) =>
        Context.Channels
            .Where(x => x.IsActive && (x.LastCheckedAt == null || x.LastCheckedAt <= cutoff))
            .OrderBy(x => x.LastCheckedAt == null ? 0 : 1)
            .ThenBy(x => x.LastCheckedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<Channel?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Context.Channels.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<Channel?> GetByHandleAsync(string handle, CancellationToken cancellationToken = default) =>
        Context.Channels.FirstOrDefaultAsync(x => x.Handle == handle, cancellationToken);

    public Task<Channel?> GetWithCommentatorsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Channels
            .Include(x => x.RegularCommentators)
            .Include(x => x.Sports)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<Channel>> GetAllWithCommentatorsAsync(CancellationToken cancellationToken = default) =>
        Context.Channels
            .AsNoTracking()
            .Include(x => x.RegularCommentators)
            .Include(x => x.Sports)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}
