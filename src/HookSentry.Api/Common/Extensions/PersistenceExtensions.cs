using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using HookSentry.Api.Common.Persistence;
using HookSentry.Api.Features.Tenants.Persistence;
using NHibernate;

namespace HookSentry.Api.Common.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var sessionFactory = Fluently.Configure()
            .Database(PostgreSQLConfiguration.PostgreSQL82
                .ConnectionString(connectionString))
            .Mappings(m => m.FluentMappings
                .AddFromAssemblyOf<TenantMap>()
                .Conventions.Add<DateTimeOffsetConvention>())
            .BuildSessionFactory();

        services.AddSingleton(sessionFactory);
        services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());

        return services;
    }
}
