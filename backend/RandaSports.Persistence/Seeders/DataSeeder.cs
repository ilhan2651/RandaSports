using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Seeders;

public class DataSeeder(RandaSportsDbContext context, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedSportsAsync(cancellationToken);
        await SeedSourcesAsync(cancellationToken);
        await SeedTeamsAsync(cancellationToken);
        await SeedCommentatorsAsync(cancellationToken);
        await SeedChannelsAsync(cancellationToken);

        // Geri dolumlar en sonda ve ayrı ayrı korumalı: bunlar bir kerelik veri tamiri,
        // biri patladığında asıl tohumlamayı (branş, kaynak, takım, kanal) götürmemeli.
        // Şema henüz güncellenmemişken story_teams sorgusu tüm seed'i düşürüyordu.
        await RunBackfillAsync("görüş branşları", BackfillOpinionSportsAsync, cancellationToken);
        await RunBackfillAsync("haber takımları", BackfillStoryTeamsAsync, cancellationToken);
    }

    private async Task RunBackfillAsync(
        string ad,
        Func<CancellationToken, Task> islem,
        CancellationToken cancellationToken)
    {
        try
        {
            await islem(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // İzlemede kalan değişiklikler sonraki kaydetmeye karışmasın.
            context.ChangeTracker.Clear();

            logger.LogWarning(
                ex,
                "Geri dolum atlandı ({Ad}). Migration eksikse önce onu uygula.",
                ad);
        }
    }

    /// <summary>
    /// Listeye yeni eklenen beslemeleri yazar. Yalnızca EKLER: adresi zaten kayıtlı olan
    /// kaynağa dokunmaz — kapatılmış ya da aralığı elle değiştirilmiş bir kaynağın
    /// her açılışta geri gelmesi istenmiyor.
    /// </summary>
    private async Task SeedSourcesAsync(CancellationToken cancellationToken)
    {
        var sportIds = await context.Sports
            .ToDictionaryAsync(x => x.Slug, x => x.Id, cancellationToken);

        var existingUrls = await context.Sources
            .Select(x => x.Url)
            .ToListAsync(cancellationToken);

        var urls = existingUrls.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sources = new List<Source>();

        foreach (var seed in SourceSeedData.Sources)
        {
            if (!urls.Add(seed.Url))
                continue;

            if (!sportIds.TryGetValue(seed.SportSlug, out var sportId))
            {
                logger.LogWarning(
                    "Branş bulunamadı, kaynak eklenmedi: {Source} ({Sport})",
                    seed.Name,
                    seed.SportSlug);
                continue;
            }

            sources.Add(new Source
            {
                Name = seed.Name,
                Url = seed.Url,
                Type = SourceType.Rss,
                SportId = sportId,
                Language = seed.Language,
                FetchIntervalMinutes = seed.FetchIntervalMinutes,
                Priority = seed.Priority
            });
        }

        if (sources.Count == 0)
            return;

        context.Sources.AddRange(sources);

        if (await SaveToleratingRaceAsync("Kaynaklar", cancellationToken))
            logger.LogInformation(
                "{Count} yeni kaynak eklendi: {Names}",
                sources.Count,
                string.Join(", ", sources.Select(x => x.Name)));
    }

    /// <summary>
    /// Yorumcu listesi konuşmacı doğrulamasının sözlüğü. Takma adlar sonradan
    /// genişletilebildiği için mevcut kayıtlar da güncelleniyor.
    /// </summary>
    private async Task SeedCommentatorsAsync(CancellationToken cancellationToken)
    {
        var sportIds = await context.Sports
            .ToDictionaryAsync(x => x.Slug, x => x.Id, cancellationToken);

        var existing = await context.Commentators
            .Include(x => x.Sports)
            .ToDictionaryAsync(x => x.Slug, cancellationToken);

        var added = new List<Commentator>();
        var updatedCount = 0;

        foreach (var seed in CommentatorSeedData.Commentators)
        {
            var slug = TextNormalizer.Slugify(seed.FullName, 150);
            if (string.IsNullOrEmpty(slug))
                continue;

            if (existing.TryGetValue(slug, out var commentator))
            {
                var changed = false;

                if (!commentator.Aliases.SequenceEqual(seed.Aliases))
                {
                    commentator.Aliases = [.. seed.Aliases];
                    changed = true;
                }

                // Listede adı geçen herkes doğrulanmış sayılıyor; kayıt daha önce
                // videodan otomatik eklenmişse burada yükseltiliyor.
                if (!commentator.IsVerified)
                {
                    commentator.IsVerified = true;
                    changed = true;
                }

                if (changed)
                    updatedCount++;

                continue;
            }

            var created = new Commentator
            {
                FullName = seed.FullName,
                Slug = slug,
                Aliases = [.. seed.Aliases],
                IsVerified = true
            };

            // Branş eşlemesi yorumcuyu doğru listede göstermek için; zorunlu değil.
            if (sportIds.TryGetValue(seed.SportSlug, out var sportId))
            {
                var sport = await context.Sports.FindAsync([sportId], cancellationToken);
                if (sport is not null)
                    created.Sports.Add(sport);
            }

            added.Add(created);
        }

        if (added.Count > 0)
            context.Commentators.AddRange(added);

        if (added.Count == 0 && updatedCount == 0)
            return;

        if (await SaveToleratingRaceAsync("Yorumcular", cancellationToken))
            logger.LogInformation("Yorumcular: {Added} eklendi, {Updated} güncellendi.", added.Count, updatedCount);
    }

    /// <summary>
    /// Listeye yeni eklenen kanalları yazar. Yalnızca EKLER: mevcut kanalın adını,
    /// kullanıcı adını ya da aktifliğini değiştirmez — onlar yönetim ekranından
    /// düzenleniyor ve seed'in üzerine yazması istenmiyor. Kullanıcı adı zaten
    /// kayıtlıysa (kapatılmış olsa bile) o kanal atlanıyor.
    /// </summary>
    private async Task BackfillChannelSportsAsync(
        Dictionary<string, Sport> sports,
        CancellationToken cancellationToken)
    {
        var seedSports = CommentatorSeedData.Channels
            .Where(x => x.SportSlugs is { Length: > 0 })
            .ToDictionary(
                x => x.Handle.StartsWith('@') ? x.Handle : $"@{x.Handle}",
                x => x.SportSlugs!,
                StringComparer.OrdinalIgnoreCase);

        if (seedSports.Count == 0)
            return;

        var eksikler = await context.Channels
            .Include(x => x.Sports)
            .Where(x => x.Sports.Count == 0)
            .ToListAsync(cancellationToken);

        var tamamlanan = 0;

        foreach (var channel in eksikler)
        {
            if (!seedSports.TryGetValue(channel.Handle, out var slugs))
                continue;

            foreach (var slug in slugs)
                if (sports.TryGetValue(slug, out var sport))
                    channel.Sports.Add(sport);

            if (channel.Sports.Count > 0)
                tamamlanan++;
        }

        if (tamamlanan == 0)
            return;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("{Count} kanalın branşı tamamlandı.", tamamlanan);
    }

    /// <summary>
    /// Branş alanı sonradan eklendi; o tarihten önceki görüşlerde boş duruyor ve
    /// branş filtresinde hiç görünmüyorlar. Konu edilen takımdan tek seferde
    /// dolduruyoruz. Dolu olana dokunmuyor, yani her açılışta güvenle çalışıyor
    /// ve bir kez tamamlandıktan sonra hiçbir satıra yazmıyor.
    ///
    /// Takımı olmayan eski görüşler boşta kalıyor: kanal etiketi o dönem yoktu,
    /// modelin kararı da yoktu — geriye dönük uydurmaktansa boş bırakmak doğru.
    /// </summary>
    private async Task BackfillOpinionSportsAsync(CancellationToken cancellationToken)
    {
        var eksikVar = await context.Opinions
            .AnyAsync(x => x.SportId == null && x.TeamId != null, cancellationToken);

        if (!eksikVar)
            return;

        var takimlar = await context.Teams
            .AsNoTracking()
            .Select(x => new { x.Id, x.SportId })
            .ToListAsync(cancellationToken);

        var toplam = 0;

        // Branş başına tek güncelleme: 174 takım için tek tek sorgu atmıyoruz.
        foreach (var grup in takimlar.GroupBy(x => x.SportId))
        {
            var takimIds = grup.Select(x => x.Id).ToList();
            var sportId = grup.Key;

            toplam += await context.Opinions
                .Where(x => x.SportId == null
                            && x.TeamId != null
                            && takimIds.Contains(x.TeamId.Value))
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.SportId, sportId),
                    cancellationToken);
        }

        if (toplam > 0)
            logger.LogInformation("{Count} görüşün branşı takımından dolduruldu.", toplam);
    }

    /// <summary>
    /// Story.Teams ilişkisi sonradan eklendi; eski haberlerin takımı yalnızca
    /// ClusterKey metninin içinde duruyor ("futbol|mac-sonucu|galatasaray+kasimpasa|2026-09-28").
    /// Üçüncü bölümü ayırıp slug'ları takımlarla eşliyoruz — benzerlik araması değil,
    /// birebir eşleşme; sporcu adları zaten hiçbir takıma denk gelmiyor.
    /// </summary>
    private async Task BackfillStoryTeamsAsync(CancellationToken cancellationToken)
    {
        var bosHaberler = await context.Stories
            .AsNoTracking()
            .Where(x => !x.Teams.Any())
            .Select(x => new { x.Id, x.ClusterKey })
            .ToListAsync(cancellationToken);

        if (bosHaberler.Count == 0)
            return;

        var takimIdBySlug = await context.Teams
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Slug, x => x.Id, cancellationToken);

        // Haber başına eşleşen takım kimlikleri. Eşleşmeyen haber hiç listeye girmiyor;
        // yoksa sporcu haberleri her açılışta yeniden taranırdı.
        var plan = new Dictionary<Guid, List<Guid>>();

        foreach (var haber in bosHaberler)
        {
            var bolumler = haber.ClusterKey.Split('|');
            if (bolumler.Length < 3)
                continue;

            var bulunan = bolumler[2]
                .Split('+', StringSplitOptions.RemoveEmptyEntries)
                .Select(slug => takimIdBySlug.TryGetValue(slug, out var id) ? id : (Guid?)null)
                .Where(x => x is not null)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            if (bulunan.Count > 0)
                plan[haber.Id] = bulunan;
        }

        if (plan.Count == 0)
            return;

        var toplam = 0;

        // Parça parça kaydediyoruz; binlerce haberi tek seferde izlemeye almak belleği şişiriyor.
        foreach (var kume in plan.Chunk(200))
        {
            var storyIds = kume.Select(x => x.Key).ToList();
            var teamIds = kume.SelectMany(x => x.Value).Distinct().ToList();

            var haberler = await context.Stories
                .Include(x => x.Teams)
                .Where(x => storyIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var takimlar = await context.Teams
                .Where(x => teamIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            foreach (var haber in haberler)
            {
                foreach (var teamId in plan[haber.Id])
                {
                    if (takimlar.TryGetValue(teamId, out var takim) && !haber.Teams.Contains(takim))
                    {
                        haber.Teams.Add(takim);
                        toplam++;
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();
        }

        logger.LogInformation(
            "{Count} haber-takım bağı ClusterKey'den dolduruldu ({Stories} haber).",
            toplam,
            plan.Count);
    }

    private async Task SeedChannelsAsync(CancellationToken cancellationToken)
    {
        var existingHandles = await context.Channels
            .Select(x => x.Handle)
            .ToListAsync(cancellationToken);

        var existingSlugs = await context.Channels
            .Select(x => x.Slug)
            .ToListAsync(cancellationToken);

        var handles = existingHandles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var slugs = existingSlugs.ToHashSet(StringComparer.Ordinal);

        // Kanal–branş bağını kurmak için; takip edilen hâlde alıyoruz ki
        // koleksiyona doğrudan eklenebilsinler.
        var sports = await context.Sports.ToDictionaryAsync(x => x.Slug, cancellationToken);

        var channels = new List<Channel>();

        foreach (var seed in CommentatorSeedData.Channels)
        {
            var handle = seed.Handle.StartsWith('@') ? seed.Handle : $"@{seed.Handle}";
            var slug = TextNormalizer.Slugify(seed.Name, 150);

            if (string.IsNullOrEmpty(slug) || !handles.Add(handle) || !slugs.Add(slug))
                continue;

            var channel = new Channel
            {
                Name = seed.Name,
                Slug = slug,
                Handle = handle,
                YouTubeChannelId = seed.YouTubeChannelId
            };

            foreach (var sportSlug in seed.SportSlugs ?? [])
            {
                if (sports.TryGetValue(sportSlug, out var sport))
                    channel.Sports.Add(sport);
                else
                    logger.LogWarning(
                        "Kanalın branşı bulunamadı, atlanıyor: {Channel} → {Sport}",
                        seed.Name,
                        sportSlug);
            }

            channels.Add(channel);
        }

        // Var olan kanalların branşı eksikse tamamlıyoruz. Yalnızca "yoksa ekle"
        // mantığıyla çalışsaydı, kanal bir kez branşsız oluştuğunda (örneğin şema
        // değişince) bir daha hiç düzelmezdi. Dolu olana dokunmuyoruz: yönetim
        // ekranından yapılan değişikliği ezmesin.
        await BackfillChannelSportsAsync(sports, cancellationToken);

        if (channels.Count == 0)
            return;

        context.Channels.AddRange(channels);

        if (await SaveToleratingRaceAsync("Kanallar", cancellationToken))
            logger.LogInformation(
                "{Count} yeni kanal eklendi: {Names}",
                channels.Count,
                string.Join(", ", channels.Select(x => x.Name)));
    }

    private async Task SeedSportsAsync(CancellationToken cancellationToken)
    {
        var existingSlugs = await context.Sports
            .Select(x => x.Slug)
            .ToListAsync(cancellationToken);

        var missing = SportSeedData.Sports
            .Where(x => !existingSlugs.Contains(x.Slug))
            .Select(x => new Sport { Name = x.Name, Slug = x.Slug, DisplayOrder = x.DisplayOrder })
            .ToList();

        if (missing.Count == 0)
            return;

        context.Sports.AddRange(missing);

        if (await SaveToleratingRaceAsync("Branşlar", cancellationToken))
            logger.LogInformation(
                "{Count} branş eklendi: {Names}",
                missing.Count,
                string.Join(", ", missing.Select(x => x.Name)));
    }

    private async Task SeedTeamsAsync(CancellationToken cancellationToken)
    {
        var added = 0;
        var updated = 0;

        added += await SeedTeamGroupAsync("futbol", TeamSeedData.FootballClubs, cancellationToken, count => updated += count);
        added += await SeedTeamGroupAsync("futbol", TeamSeedData.NationalTeams, cancellationToken, count => updated += count);
        added += await SeedTeamGroupAsync("basketbol", TeamSeedData.BasketballClubs, cancellationToken, count => updated += count);

        if (added > 0 || updated > 0)
            logger.LogInformation("Takım sözlüğü: {Added} eklendi, {Updated} güncellendi.", added, updated);
    }

    private async Task<int> SeedTeamGroupAsync(
        string sportSlug,
        TeamSeed[] seeds,
        CancellationToken cancellationToken,
        Action<int> reportUpdated)
    {
        var sportId = await context.Sports
            .Where(x => x.Slug == sportSlug)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (sportId == Guid.Empty)
        {
            logger.LogWarning("Branş bulunamadı, takımlar eklenmedi: {Sport}", sportSlug);
            return 0;
        }

        var existing = await context.Teams
            .Where(x => x.SportId == sportId)
            .ToDictionaryAsync(x => x.Slug, cancellationToken);

        var newTeams = new List<Team>();
        var updatedCount = 0;

        foreach (var seed in seeds)
        {
            var slug = TextNormalizer.Slugify(seed.Name, 150);
            if (string.IsNullOrEmpty(slug))
                continue;

            if (existing.TryGetValue(slug, out var team))
            {
                // Sözlüğe sonradan takma ad eklediğimizde mevcut kayıt da güncellensin.
                if (!team.Aliases.SequenceEqual(seed.Aliases))
                {
                    team.Aliases = [.. seed.Aliases];
                    updatedCount++;
                }

                continue;
            }

            newTeams.Add(new Team
            {
                SportId = sportId,
                Name = seed.Name,
                Slug = slug,
                Country = seed.Country,
                IsNational = seed.IsNational,
                Aliases = [.. seed.Aliases]
            });
        }

        if (newTeams.Count > 0)
            context.Teams.AddRange(newTeams);

        if (newTeams.Count == 0 && updatedCount == 0)
            return 0;

        if (!await SaveToleratingRaceAsync($"Takımlar ({sportSlug})", cancellationToken))
            return 0;

        reportUpdated(updatedCount);
        return newTeams.Count;
    }

    /// <summary>
    /// API ve worker aynı anda ayağa kalkarsa ikisi de seed etmeye çalışır ve biri
    /// benzersizlik kısıtına çarpar. Bu bir hata değil: diğeri işi yapmış, devam ediyoruz.
    /// </summary>
    private async Task<bool> SaveToleratingRaceAsync(string step, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
        {
            context.ChangeTracker.Clear();
            logger.LogInformation("{Step}: başka bir süreç eklemiş, atlandı. ({Reason})", step, ex.InnerException?.Message ?? ex.Message);
            return false;
        }
    }
}
