using System.Security.Cryptography;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.Security;

/// <summary>
/// PBKDF2-SHA256 ile parola özeti. Dış paket kullanmıyoruz; BCL'deki
/// <see cref="Rfc2898DeriveBytes"/> bu iş için yeterli.
///
/// Saklanan biçim: "pbkdf2.{tur}.{tuz}.{ozet}" — tur sayısı özetin içinde
/// durduğu için, ileride maliyeti artırdığımızda eski parolalar doğrulanmaya
/// devam ediyor.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const string Prefix = "pbkdf2";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 210_000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Prefix}.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        var parts = hash.Split('.');

        if (parts.Length != 4 || parts[0] != Prefix)
            return false;

        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
            return false;

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

        // Sabit zamanlı karşılaştırma: baytları tek tek kıyaslamak, cevabın ne kadar
        // sürdüğüne bakarak parolayı tahmin etmeye kapı aralıyor.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
