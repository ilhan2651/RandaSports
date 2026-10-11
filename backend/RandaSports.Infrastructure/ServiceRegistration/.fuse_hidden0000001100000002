using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Infrastructure.Ai;
using RandaSports.Infrastructure.Content;
using RandaSports.Infrastructure.Rss;
using RandaSports.Infrastructure.People;
using RandaSports.Infrastructure.Security;
using RandaSports.Infrastructure.Sports;
using RandaSports.Infrastructure.YouTube;

namespace RandaSports.Infrastructure.ServiceRegistration;

public static class ServiceRegistration
{
    private const string BotUserAgent = "RandaSportsBot/1.0 (+https://randasports.com)";

    /// <summary>
    /// YouTube kanal sayfası tarayıcı dışı isteklere eksik HTML döndürüyor. Aynı sorun
    /// yabancı haber sitelerinde de var: ESPN ve Cloudflare arkasındaki siteler tanımadıkları
    /// bot adına makale gövdesini vermiyor, bot koruması sayfası dönüyor.
    /// </summary>
    private const string BrowserUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36";

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Kimlik: parola özeti durumsuz, jeton üreticisi ayarları okuyor.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPersonRoleClassifier, GeminiPersonRoleClassifier>();

        services.AddHttpClient<IRssFeedReader, RssFeedReader>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BotUserAgent);
        });

        // Beslemeler bot adını kabul ediyor ama makale sayfaları etmiyor; gövdeyi
        // tarayıcı adıyla istiyoruz.
        services.AddHttpClient<IArticleContentExtractor, ArticleContentExtractor>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BrowserUserAgent);
        });

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        services.AddHttpClient<IGeminiClient, GeminiClient>((serviceProvider, client) =>
        {
            var geminiOptions = serviceProvider
                .GetRequiredService<IOptions<GeminiOptions>>().Value;

            client.BaseAddress = new Uri(geminiOptions.BaseUrl);

            // Video analizi metin isteklerinden çok daha uzun sürüyor.
            client.Timeout = TimeSpan.FromSeconds(geminiOptions.TimeoutSeconds);
        });

        // Wikidata anonim istekleri kısıtlıyor; kendimizi tanıtan bir User-Agent şart.
        // Portre ve logo aramaları aynı istemciyi paylaşıyor.
        services.AddHttpClient<WikidataClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.wikidata.org/");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RandaSports/1.0 (gorsel arama)");
        });

        services.AddScoped<IPortraitLookup, WikidataPortraitLookup>();
        services.AddScoped<ILogoLookup, WikidataLogoLookup>();

        services.AddHttpClient<IYouTubeChannelResolver, YouTubeChannelResolver>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BrowserUserAgent);
        });

        services.AddHttpClient<IYouTubeFeedReader, YouTubeFeedReader>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BotUserAgent);
        });

        // Süre bilgisi izleme sayfasından okunuyor; besleme vermiyor.
        services.AddHttpClient<IYouTubeVideoDurationReader, YouTubeVideoDurationReader>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(BrowserUserAgent);
        });

        // Anahtar tüm sporlarda ortak; adres ve kota spor başına ayrı.
        services.Configure<ApiSportsOptions>(configuration.GetSection(ApiSportsOptions.SectionName));
        services.Configure<ApiFootballOptions>(configuration.GetSection(ApiFootballOptions.SectionName));

        services.AddHttpClient<ISportsDataProvider, ApiFootballClient>((serviceProvider, client) =>
        {
            var account = serviceProvider.GetRequiredService<IOptions<ApiSportsOptions>>().Value;
            var football = serviceProvider.GetRequiredService<IOptions<ApiFootballOptions>>().Value;

            client.BaseAddress = new Uri(football.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("x-apisports-key", account.ApiKey);
        });

        return services;
    }
}
