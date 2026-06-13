using FluentNHibernate.Mapping;
using HookSentry.Domain.ApiKeys;

namespace HookSentry.Infrastructure.Persistence.Mappings;

public class ApiKeyMap : ClassMap<ApiKey>
{
    public ApiKeyMap()
    {
        Table("api_keys");
        Not.LazyLoad();

        Id(x => x.Id, "id").GeneratedBy.Assigned();

        Map(x => x.TenantId, "tenant_id").Not.Nullable();
        Map(x => x.KeyHash, "key_hash").Not.Nullable().Length(64);
        Map(x => x.Name, "name").Not.Nullable().Length(100);
        Map(x => x.IsActive, "is_active").Not.Nullable();
        Map(x => x.CreatedAt, "created_at").Not.Nullable();
        Map(x => x.RevokedAt, "revoked_at").Nullable();
    }
}
