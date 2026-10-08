using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

/// <summary>Sağlayıcı başına günlük istek sayacı.</summary>
public class ProviderQuota : BaseEntity
{
    public required string Provider { get; set; }
    public DateOnly Day { get; set; }
    public int Used { get; set; }
    public int DailyLimit { get; set; }
}
