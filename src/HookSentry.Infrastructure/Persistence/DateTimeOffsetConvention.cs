using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace HookSentry.Infrastructure.Persistence;

public class DateTimeOffsetConvention : IPropertyConvention
{
    public void Apply(IPropertyInstance instance)
    {
        var type = instance.Property.PropertyType;
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(DateTimeOffset))
            instance.CustomType<NpgsqlDateTimeOffsetType>();
    }
}
