using System.Text;
using System.Text.Json;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.People;

internal static class PersonRolePromptBuilder
{
    public static string Build(IReadOnlyList<PersonRoleCandidate> candidates)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            """
            Aşağıda bir Türk spor yayını arşivinden çıkarılmış kişiler var. Her biri için
            kişinin ROLÜNÜ belirle.

            Roller:
            - commentator: Yorumcu, spiker, sunucu, spor yazarı, muhabir. Mesleği konuşmak olan kişi.
              Emekli bir futbolcu artık yorumculuk yapıyorsa BU gruba girer.
            - athlete: Aktif sporcu. Röportaj ya da maç sonu demeci veren oyuncu.
            - coach: Teknik direktör, antrenör, yardımcı antrenör.
            - official: Kulüp başkanı, yönetici, federasyon yetkilisi, hakem, menajer.
            - unknown: Kim olduğunu çıkaramıyorsan.

            Kurallar:
            - Emin değilsen unknown de ve güveni düşük ver. Yanlış bir rol vermektense
              bilmemek daha iyi.
            - Kararını kişinin KİM OLDUĞUNA göre ver, o videoda ne yaptığına göre değil.
              Yorumcu bir maçı anlatıyorsa da commentator'dır.
            - Alıntılara bak: yorumcu üçüncü şahıs olarak değerlendirir ("Fenerbahçe
              şunu yapmalı"), sporcu ve teknik direktör kendi takımından birinci çoğul
              şahısla söz eder ("biz", "bizim oyunumuz", "sahaya çıktığımızda").
            - İsim sana tanıdık gelmiyorsa tahmin etme, unknown de.

            Yalnızca şu biçimde JSON dizi döndür, başka hiçbir şey yazma:
            [{"fullName":"...","role":"commentator","confidence":0.0,"reason":"kısa gerekçe"}]

            Kişiler:
            """);

        foreach (var candidate in candidates)
        {
            builder.AppendLine();
            builder.AppendLine($"İsim: {candidate.FullName}");

            if (candidate.Channels.Count > 0)
                builder.AppendLine($"Kanallar: {string.Join(", ", candidate.Channels)}");

            foreach (var title in candidate.VideoTitles.Take(3))
                builder.AppendLine($"Video: {title}");

            foreach (var quote in candidate.Quotes.Take(2))
                builder.AppendLine($"Söylediği: {Kirp(quote, 300)}");
        }

        return builder.ToString();
    }

    public static List<PersonRoleRow> Parse(string json)
    {
        // Model bazen JSON'u ``` içine sarıyor; dizinin kendisini ayıklıyoruz.
        var start = json.IndexOf('[');
        var end = json.LastIndexOf(']');

        if (start < 0 || end <= start)
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<PersonRoleRow>>(
                json[start..(end + 1)],
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string Kirp(string value, int max) =>
        value.Length <= max ? value : value[..max];
}

public sealed record PersonRoleRow(string? FullName, string? Role, double? Confidence, string? Reason);
