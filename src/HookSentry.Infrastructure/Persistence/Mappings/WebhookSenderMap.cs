using FluentNHibernate.Mapping;
using HookSentry.Domain.Senders;

namespace HookSentry.Infrastructure.Persistence.Mappings;

public class WebhookSenderMap : ClassMap<WebhookSender>
{
    public WebhookSenderMap()
    {
        Table("webhook_senders");
        Not.LazyLoad();

        Id(x => x.Id, "id").GeneratedBy.Assigned();

        Map(x => x.DestinationId, "destination_id").Not.Nullable();
        Map(x => x.TenantId, "tenant_id").Not.Nullable();
        Map(x => x.Label, "label").Nullable();
        Map(x => x.IngestTokenHash, "ingest_token_hash").Nullable();
        Map(x => x.Mapping, "mapping").Nullable();
        Map(x => x.CreatedAt, "created_at").Not.Nullable();
        Map(x => x.UpdatedAt, "updated_at").Not.Nullable();
    }
}
