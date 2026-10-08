namespace RandaSports.Application.Interfaces.Services;

/// <param name="IsNational">Milli takımsa logo yerine ülkenin bayrağı aranıyor.</param>
public sealed record LogoRequest(string Name, bool IsNational, string? Country);

/// <summary>
/// Takım logosu arar. Bulamazsa null döner — uydurmuyor, yaklaşık eşleşme kabul
/// etmiyor. Yanlış logo basmak, logosuz kalmaktan kötü.
/// </summary>
public interface ILogoLookup
{
    Task<string?> FindAsync(LogoRequest request, CancellationToken cancellationToken = default);
}
