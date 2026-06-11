using FluentNHibernate.Mapping;
using HookSentry.Api.Features.Tenants.Domain;

namespace HookSentry.Api.Features.Tenants.Persistence;

public class TenantMap : ClassMap<Tenant>
{
    public TenantMap()
    {
        Table("tenants");
        Not.LazyLoad();

        Id(x => x.Id, "id").GeneratedBy.Assigned();

        Map(x => x.Name, "name").Length(255).Not.Nullable();
        Map(x => x.WebhookSecret, "webhook_secret").Length(512).Not.Nullable()
            .UniqueKey("uq_tenants_webhook_secret");
        Map(x => x.MaxTrys, "max_trys").Not.Nullable();
        Map(x => x.CircuitBreakerTimer, "circuit_breaker_timer").Not.Nullable();
        Map(x => x.CreatedAt, "created_at").Not.Nullable();
        Map(x => x.UpdatedAt, "updated_at").Not.Nullable();
    }
}
