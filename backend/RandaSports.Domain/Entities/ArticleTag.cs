using RandaSports.Domain.Enums;

namespace RandaSports.Domain.Entities;

public class ArticleTag
{
    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public TaggedEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
}
