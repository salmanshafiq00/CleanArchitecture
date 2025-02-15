using Domain.Common;

namespace Infrastructure.Persistence.Configurations.Common;

internal sealed class LookupDetailConfiguration : IEntityTypeConfiguration<LookupDetail>
{
    public void Configure(EntityTypeBuilder<LookupDetail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(t => new { t.LookupId, t.Name }).IsUnique();

        builder.Property(t => t.Code)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.HasOne(t => t.Lookup)
            .WithMany()
            .HasForeignKey(t => t.LookupId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(t => t.LookupId);
    }
}
