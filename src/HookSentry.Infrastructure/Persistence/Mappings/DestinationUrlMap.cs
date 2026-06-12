using FluentNHibernate.Mapping;
using HookSentry.Domain.Destinations;

namespace HookSentry.Infrastructure.Persistence.Mappings;

public class DestinationUrlMap : ClassMap<DestinationUrl>
{
    public DestinationUrlMap()
    {
        Table("urls_destino");
        Not.LazyLoad();

        Id(x => x.Id, "id").GeneratedBy.Assigned();

        Map(x => x.TenantId, "tenant_id").Not.Nullable();
        Map(x => x.Url, "url").Not.Nullable();
        Map(x => x.Status, "status").Not.Nullable().CustomType<DestinationUrlStatus>();
        Map(x => x.ServerRateLimit, "server_rate_limit").Not.Nullable();
        Map(x => x.AuthType, "auth_type").Nullable().CustomType<DestinationAuthType>();
        Map(x => x.CredentialsEncrypted, "credentials_encrypted").Nullable();
        Map(x => x.IngestTokenHash, "ingest_token_hash").Nullable();
        Map(x => x.CreatedAt, "created_at").Not.Nullable();
        Map(x => x.UpdatedAt, "updated_at").Not.Nullable();
    }
}
