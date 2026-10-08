using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256);
        builder.Property(x => x.FullName).HasMaxLength(150);
        builder.Property(x => x.PasswordHash).HasMaxLength(400);
        builder.Property(x => x.SecurityStamp).HasMaxLength(64);

        // Benzersizlik normalleştirilmiş sütunda: aynı adresin farklı yazımıyla
        // ikinci hesap açılamasın.
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();

        // Takip bağları: taşıdıkları ek veri olmadığı için EF'in örtük bağ tablosu
        // yetiyor, ayrı varlık yazmıyoruz.
        builder.HasMany(x => x.FollowedTeams).WithMany();
        builder.HasMany(x => x.FollowedSports).WithMany();
        builder.HasMany(x => x.FollowedCommentators).WithMany();
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(300);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Üzerinde kullanıcı duran rol silinemesin; önce bağ kaldırılsın.
        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(x => x.TokenHash).HasMaxLength(128);
        builder.Property(x => x.CreatedIp).HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash).IsUnique();

        // Çıkışta ve süresi geçenleri temizlerken kullanıcıya göre tarıyoruz.
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
