using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RandaSports.Application.Common;
using RandaSports.Application.Common.Behaviours;
using RandaSports.Application.Common.Filtering;
using RandaSports.Application.Common.Options;
using RandaSports.Application.Features.Auth;

namespace RandaSports.Application.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            cfg.LicenseKey = configuration["MediatR:LicenseKey"];
        });

        services.Configure<AuthPolicyOptions>(configuration.GetSection(AuthPolicyOptions.SectionName));
        services.AddScoped<AuthTokenIssuer>();

        services.Configure<ArticleFilterOptions>(configuration.GetSection(ArticleFilterOptions.SectionName));
        services.AddSingleton<IArticleFilter, ArticleFilter>();

        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
