using Domain.Admin;
using Domain.Common;

namespace Infrastructure.Persistence.Configurations.Admin;

internal sealed class AppMenuConfiguration : IEntityTypeConfiguration<AppMenu>
{
    public void Configure(EntityTypeBuilder<AppMenu> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

        builder.Property(t => t.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(t => t.Label).IsUnique(false);

        builder.Property(t => t.RouterLink)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Tooltip)
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(200);

        builder.HasIndex(t => t.ParentId);

        builder.HasOne<LookupDetail>()
            .WithMany()
            .HasForeignKey(x => x.MenuTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
