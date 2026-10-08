namespace RandaSports.Domain.Entities;

/// <summary>
/// Kullanıcı–rol bağı. Saf bir ara tablo değil: rolün ne zaman verildiğini
/// tutuyoruz, o yüzden EF'in örtük bağ tablosu yerine kendi varlığı var.
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTimeOffset AssignedAt { get; set; }
}
