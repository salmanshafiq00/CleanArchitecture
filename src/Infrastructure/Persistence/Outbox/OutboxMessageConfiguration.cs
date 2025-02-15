namespace Infrastructure.Persistence.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessage");

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

        builder.Property(e => e.ProcessingLock)
            .IsRequired(false);

        builder.Property(e => e.LastProcessingAttempt)
            .IsRequired(false);

        builder.Property(e => e.RetryCount)
            .IsRequired(false);

    }
}
