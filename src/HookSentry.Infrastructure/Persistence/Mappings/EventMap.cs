using FluentNHibernate.Mapping;
using HookSentry.Domain.Events;

namespace HookSentry.Infrastructure.Persistence.Mappings;

public class EventMap : ClassMap<Event>
{
    public EventMap()
    {
        Table("eventos");
        Not.LazyLoad();

        Id(x => x.Id, "id").GeneratedBy.Assigned();

        Map(x => x.TenantId, "tenant_id").Not.Nullable();
        Map(x => x.DestinationUrlId, "urls_destino_id").Not.Nullable();
        Map(x => x.Payload, "payload").Not.Nullable();
        Map(x => x.Status, "status").Not.Nullable().CustomType<EventStatus>();
        Map(x => x.IdempotencyKey, "idempotency_key").Nullable();
        Map(x => x.CurrentRetryCount, "current_retry_count").Not.Nullable();
        Map(x => x.NextAttemptAt, "next_attempt_at").Nullable();
        Map(x => x.AcceptedAt, "accepted_at").Not.Nullable();
        Map(x => x.DeliveredAt, "delivered_at").Nullable();
    }
}
