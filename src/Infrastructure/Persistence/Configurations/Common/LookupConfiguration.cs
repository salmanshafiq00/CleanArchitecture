using Domain.Common;

namespace Infrastructure.Persistence.Configurations.Common;

internal sealed class LookupConfiguration : IEntityTypeConfiguration<Lookup>
{
    public void Configure(EntityTypeBuilder<Lookup> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.HasIndex(t => t.ParentId);

        builder.Property(t => t.Code)
            .HasMaxLength(10)
            .IsRequired(false);

        builder.HasIndex(t => t.DevCode);

        builder.Property(t => t.Description)
            .HasMaxLength(500);
    }
}
