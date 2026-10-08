namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// İsimden açık lisanslı portre arar. İsimden gidiyor, yüzden değil: bir kişiyi
/// görüntüsünden tanımak biyometrik veri işlemek olurdu.
/// </summary>
public interface IPortraitLookup
{
    /// <summary>Bulunamazsa ya da birden fazla kişiyle eşleşirse null.</summary>
    Task<string?> FindAsync(string fullName, CancellationToken cancellationToken = default);
}
