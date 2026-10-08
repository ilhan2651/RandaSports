using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Persistence.Contexts;
using RandaSports.Persistence.Interceptors;
using RandaSports.Persistence.Matching;
using RandaSports.Persistence.Quotas;
using RandaSports.Persistence.Repositories;
using RandaSports.Persistence.Seeders;

namespace RandaSports.Persistence.ServiceRegistration;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default bulunamadı.");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddMemoryCache();

        services.AddDbContext<RandaSportsDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISourceRepository, SourceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<ISportsRepository, SportsRepository>();
        services.AddScoped<ISportsReadRepository, SportsReadRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<ICommentatorRepository, CommentatorRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IOpinionRepository, OpinionRepository>();
        services.AddScoped<ITeamMatcher, TeamMatcher>();
        services.AddScoped<ISportDictionary, SportDictionary>();
        services.AddScoped<IProviderQuota, ProviderQuotaService>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<DataSeeder>();

        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}
