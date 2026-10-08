using Microsoft.EntityFrameworkCore;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Contexts;

public class RandaSportsDbContext(DbContextOptions<RandaSportsDbContext> options) : DbContext(options)
{
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Athlete> Athletes => Set<Athlete>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<Standing> Standings => Set<Standing>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Commentator> Commentators => Set<Commentator>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<Opinion> Opinions => Set<Opinion>();
    public DbSet<ProviderQuota> ProviderQuotas => Set<ProviderQuota>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RandaSportsDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (!type.IsEnum)
                    continue;

                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion<string>()
                    .HasMaxLength(32);
            }
        }
    }
}
