using Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace Infrastructure.Persistence.Outbox;

internal sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            InsertOutboxMessages(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            InsertOutboxMessages(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void InsertOutboxMessages(DbContext context)
    {
        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity);

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ToList().ForEach(e => e.ClearDomainEvents());

        if (domainEvents.Count == 0) return;

        var outboxMessages = domainEvents
            .Select(domainEvent => new OutboxMessage(
                Guid.NewGuid(),
                domainEvent.GetType().Name,
                JsonConvert.SerializeObject(domainEvent, SerializerSettings),
                DateTime.Now))
            .ToList();

        //var outboxMessages = context
        //    .ChangeTracker
        //    .Entries<BaseEntity>()
        //    .Select(entry => entry.Entity)
        //    .SelectMany(entity =>
        //    {
        //        var domainEvents = entity.DomainEvents;

        //        entity.ClearDomainEvents();

        //        return domainEvents;
        //    })
        //    .Select(domainEvent => new OutboxMessage(
        //        Guid.NewGuid(),
        //        domainEvent.GetType().Name,
        //        JsonConvert.SerializeObject(domainEvent, SerializerSettings),
        //        DateTime.Now))
        //    .ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
