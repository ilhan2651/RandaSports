using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class ProviderQuotaConfiguration : IEntityTypeConfiguration<ProviderQuota>
{
    public void Configure(EntityTypeBuilder<ProviderQuota> builder)
    {
        builder.Property(x => x.Provider).HasMaxLength(50);

        builder.HasIndex(x => new { x.Provider, x.Day }).IsUnique();
    }
}
