using Domain.Admin;

namespace Infrastructure.Persistence.Configurations.Admin;

internal sealed class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

        builder.Property(t => t.Title)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(t => t.Description)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(t => t.SenderId)
            .HasMaxLength(100);

        builder.Property(t => t.RecieverId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.RetryCount)
            .HasDefaultValue(0);

        builder.Property(x => x.IsSeen)
            .HasDefaultValue(false);

        builder.Property(x => x.IsProcessed)
            .HasDefaultValue(false);

        builder.Property(x => x.Group)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.Error)
            .HasMaxLength(200)
            .IsRequired(false);
    }
}
