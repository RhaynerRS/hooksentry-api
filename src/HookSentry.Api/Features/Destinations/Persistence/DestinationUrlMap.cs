using FluentNHibernate.Mapping;
using HookSentry.Api.Features.Destinations.Domain;

namespace HookSentry.Api.Features.Destinations.Persistence;

public class DestinationUrlMap : ClassMap<DestinationUrl>
{
    public DestinationUrlMap()
    {
        Table("urls_destino");

        Id(x => x.Id, "id").GeneratedBy.Assigned();

        Map(x => x.TenantId, "tenant_id").Not.Nullable();
        Map(x => x.Url, "url").Not.Nullable();
        Map(x => x.Status, "status").Not.Nullable().CustomType<DestinationUrlStatus>();
        Map(x => x.ServerRateLimit, "server_rate_limit").Not.Nullable();
        Map(x => x.CreatedAt, "created_at").Not.Nullable();
        Map(x => x.UpdatedAt, "updated_at").Not.Nullable();
    }
}
