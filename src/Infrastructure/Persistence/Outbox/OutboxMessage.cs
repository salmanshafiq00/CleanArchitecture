using Newtonsoft.Json;

namespace Infrastructure.Persistence.Outbox;

internal sealed record OutboxMessage(
    Guid Id,
    string Type,
    string Content,
    DateTime CreatedOn
)
{
    public DateTime? ProcessedOn { get; set; } = null;
    public string? Error { get; set; } = null;
    public int? RetryCount { get; set; }
    public Guid? ProcessingLock { get; set; }
    public DateTime? LastProcessingAttempt { get; set; }

    private OutboxMessage() : this(Guid.Empty, string.Empty, string.Empty, DateTime.MinValue) { }
}
