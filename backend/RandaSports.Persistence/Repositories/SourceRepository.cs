using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class SourceRepository(RandaSportsDbContext context) : GenericRepository<Source>(context), ISourceRepository
{
    public Task<List<Source>> GetListAsync(bool? isActive, Guid? sportId, CancellationToken cancellationToken = default)
    {
        var query = Context.Sources
            .AsNoTracking()
            .Include(x => x.Sport)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        if (sportId.HasValue)
            query = query.Where(x => x.SportId == sportId.Value);

        return query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task<List<Source>> GetActiveRssSourcesAsync(CancellationToken cancellationToken = default) =>
        Context.Sources
            .AsNoTracking()
            .Where(x => x.IsActive && x.Type == SourceType.Rss)
            .ToListAsync(cancellationToken);

    public Task<Source?> GetWithSportAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Sources
            .AsNoTracking()
            .Include(x => x.Sport)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
