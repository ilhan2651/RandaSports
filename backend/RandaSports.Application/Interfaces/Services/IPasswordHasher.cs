namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Parola özetleme. Uygulama katmanı hangi algoritmanın kullanıldığını bilmiyor;
/// algoritma değişirse yalnızca Infrastructure tarafı değişiyor.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>Kayıtlı özet bozuksa ya da parola tutmuyorsa false döner, hata fırlatmaz.</summary>
    bool Verify(string password, string hash);
}
