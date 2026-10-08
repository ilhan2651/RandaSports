using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Commentary.Command.AutoVerifyCommentators;
using RandaSports.Application.Features.Commentary.Command.ClassifyCommentatorRoles;
using RandaSports.Application.Features.Commentary.Command.FetchCommentatorPortraits;

namespace RandaSports.Worker.Workers;

/// <summary>
/// Kişi onayı kuyruğunu kendi kendine boşaltır. Üç iş yapıyor: rolü belirlenmemiş
/// kişilerin rolünü modele sorar, yorumcu olduğu belirlenen ve kanıtı yeterli
/// isimleri doğrular, doğrulanmış ama görseli olmayanlara portre arar. Amaç insanı
/// döngüden çıkarmak.
/// </summary>
public sealed class CommentatorMaintenanceWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<CommentatorMaintenanceOptions> options,
    ILogger<CommentatorMaintenanceWorker> logger) : BackgroundService
{
    private readonly CommentatorMaintenanceOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Yorumcu bakımı kapalı.");
            return;
        }

        logger.LogInformation("Yorumcu bakımı başladı.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Yorumcu bakım turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Sıra önemli: önce rol belirleniyor, sonra doğrulama. Tersi olsaydı rolü
        // henüz bilinmeyen biri kanıt eşiğini geçip doğrulanırdı.
        var classified = await mediator.Send(
            new ClassifyCommentatorRolesCommand(_options.RoleBatchSize),
            cancellationToken);

        if (classified.IsFail)
            logger.LogWarning("Rol tespiti başarısız: {Message}", classified.Message);

        var verified = await mediator.Send(new AutoVerifyCommentatorsCommand(), cancellationToken);

        if (verified.IsFail)
            logger.LogWarning("Otomatik doğrulama başarısız: {Message}", verified.Message);

        if (!_options.FetchPortraits)
            return;

        var portraits = await mediator.Send(
            new FetchCommentatorPortraitsCommand(_options.PortraitBatchSize),
            cancellationToken);

        if (portraits.IsFail)
            logger.LogWarning("Portre araması başarısız: {Message}", portraits.Message);
    }
}
