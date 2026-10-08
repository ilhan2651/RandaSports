using System.Text.RegularExpressions;

namespace RandaSports.Application.Common.Text;

/// <summary>
/// Kanal referansını tek biçime getirir. Yönetim ekranına kullanıcı adı, tam adres
/// veya kanal kimliği yapıştırılabiliyor; hepsi aynı alana yazılıyor.
/// </summary>
public static partial class ChannelReference
{
    public const int MaxLength = 100;

    public static string Normalize(string reference)
    {
        var trimmed = reference.Trim();

        if (trimmed.Length == 0)
            return trimmed;

        // Doğrudan kanal kimliği verilmişse olduğu gibi duruyor.
        var id = ChannelIdRegex().Match(trimmed);
        if (id.Success)
            return id.Groups["id"].Value;

        // Adresten kullanıcı adını çıkarıyoruz: ".../@sporx/videos" → "@sporx"
        var handle = HandleInUrlRegex().Match(trimmed);
        if (handle.Success)
            return Clip($"@{handle.Groups["handle"].Value}");

        if (trimmed.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("youtube.com", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("www.youtube.com", StringComparison.OrdinalIgnoreCase))
            return Clip(trimmed);

        return Clip($"@{trimmed.TrimStart('@')}");
    }

    private static string Clip(string value) =>
        value.Length <= MaxLength ? value : value[..MaxLength];

    [GeneratedRegex(@"(?:^|/)(?<id>UC[\w-]{20,})(?:$|/|\?)")]
    private static partial Regex ChannelIdRegex();

    [GeneratedRegex(@"youtube\.com/@(?<handle>[\w.\-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex HandleInUrlRegex();
}
