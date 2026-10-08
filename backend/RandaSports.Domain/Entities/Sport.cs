using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Sport : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Competition> Competitions { get; set; } = [];
}
