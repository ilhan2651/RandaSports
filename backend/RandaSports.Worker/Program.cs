using RandaSports.Application.DependencyInjection;
using RandaSports.Infrastructure.ServiceRegistration;
using RandaSports.Persistence.Seeders;
using RandaSports.Persistence.ServiceRegistration;
using RandaSports.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<AiEnrichmentOptions>(
    builder.Configuration.GetSection(AiEnrichmentOptions.SectionName));

builder.Services.Configure<StoryClusteringOptions>(
    builder.Configuration.GetSection(StoryClusteringOptions.SectionName));

builder.Services.Configure<StoryWriterOptions>(
    builder.Configuration.GetSection(StoryWriterOptions.SectionName));

builder.Services.Configure<SportsSyncOptions>(
    builder.Configuration.GetSection(SportsSyncOptions.SectionName));

builder.Services.Configure<VideoDiscoveryOptions>(
    builder.Configuration.GetSection(VideoDiscoveryOptions.SectionName));

builder.Services.Configure<OpinionExtractionOptions>(
    builder.Configuration.GetSection(OpinionExtractionOptions.SectionName));

builder.Services.Configure<CommentatorMaintenanceOptions>(
    builder.Configuration.GetSection(CommentatorMaintenanceOptions.SectionName));

builder.Services.Configure<TeamLogoOptions>(
    builder.Configuration.GetSection(TeamLogoOptions.SectionName));

builder.Services.AddHostedService<RssCollectorWorker>();
builder.Services.AddHostedService<AiEnrichmentWorker>();
builder.Services.AddHostedService<StoryClusteringWorker>();
builder.Services.AddHostedService<StoryWriterWorker>();
builder.Services.AddHostedService<SportsSyncWorker>();
builder.Services.AddHostedService<VideoDiscoveryWorker>();
builder.Services.AddHostedService<OpinionExtractionWorker>();
builder.Services.AddHostedService<CommentatorMaintenanceWorker>();
builder.Services.AddHostedService<TeamLogoWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Migration'ı API uyguluyor; worker sadece geride kalmışsa uyarıyor.
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().RunAsync(applyMigrations: false);

    // Takım ve yorumcu sözlüğü kümeleme ile konuşmacı doğrulaması için şart.
    try
    {
        await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Seed çalıştırılamadı, API tarafında tamamlanacak.");
    }
}

await host.RunAsync();
