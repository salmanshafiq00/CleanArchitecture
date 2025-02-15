using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Infrastructure.Identity;

public class IdentityContext(DbContextOptions<IdentityContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        builder.Entity<AppUser>(entity => entity.ToTable("Users"));

        builder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));

        builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));

        builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims"));

        builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));

        builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims"));

        builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens"));

        // Inline ApplicationUser configuration
        builder.Entity<AppUser>(entity =>
        {
            //entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.IsActive)
                .IsRequired();

            //entity.Property(u => u.UserType)
            //    .IsRequired();

            entity.Property(u => u.PhotoUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.NormalizedEmail)
                .HasMaxLength(256);

            entity.Property(u => u.UserName)
                .HasMaxLength(256);

            entity.HasIndex(u => u.UserName)
                .IsUnique();

            entity.Property(u => u.NormalizedUserName)
                .HasMaxLength(256);

            entity.Property(u => u.PhoneNumber)
                .HasMaxLength(20);
        });

        // Inline RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(rt => rt.Token)
                .IsUnique();

            entity.Property(rt => rt.Expires)
                .IsRequired();

            entity.Property(rt => rt.Created)
                .IsRequired();

            entity.Property(rt => rt.CreatedByIp)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(rt => rt.Revoked)
                .IsRequired(false);

            entity.Property(rt => rt.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.HasOne(rt => rt.ApplicationUser)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        });
    }
}
