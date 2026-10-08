using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class SportConfiguration : IEntityTypeConfiguration<Sport>
{
    public void Configure(EntityTypeBuilder<Sport> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Slug).HasMaxLength(100);

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
