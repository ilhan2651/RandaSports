using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.Url).HasMaxLength(500);
        builder.Property(x => x.Language).HasMaxLength(5);
        builder.Property(x => x.LastError).HasMaxLength(1000);

        builder.HasOne(x => x.Sport)
            .WithMany()
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Url).IsUnique();
    }
}
