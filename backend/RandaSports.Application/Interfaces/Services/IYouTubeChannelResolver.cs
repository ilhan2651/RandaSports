namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Kanal referansından UC... kimliğini bulur. RSS beslemesi kullanıcı adını değil,
/// kanal kimliğini istiyor.
/// </summary>
public interface IYouTubeChannelResolver
{
    /// <param name="reference">"@sporx", kanal adresi veya doğrudan UC... kimliği.</param>
    Task<string?> ResolveChannelIdAsync(string reference, CancellationToken cancellationToken = default);
}
