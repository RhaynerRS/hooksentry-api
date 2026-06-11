using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace HookSentry.Api.Common.Persistence;

public class NpgsqlDateTimeOffsetType : IUserType
{
    public SqlType[] SqlTypes => [new SqlType(DbType.DateTimeOffset)];
    public Type ReturnedType => typeof(DateTimeOffset);
    public bool IsMutable => false;

    public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var raw = NHibernateUtil.UtcDateTime.NullSafeGet(rs, names, session, owner);
        if (raw is null) return DateTimeOffset.MinValue;
        if (raw is DateTimeOffset dto) return dto;
        var dt = (DateTime)raw;
        return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeSpan.Zero);
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        if (value is null or DBNull)
        {
            ((IDataParameter)cmd.Parameters[index]).Value = DBNull.Value;
            return;
        }
        var dto = (DateTimeOffset)value;
        ((IDataParameter)cmd.Parameters[index]).Value = dto.UtcDateTime;
    }

    public object DeepCopy(object value) => value;
    public object Replace(object original, object target, object owner) => original;
    public object Assemble(object cached, object owner) => cached;
    public object Disassemble(object value) => value;

    public new bool Equals(object? x, object? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(object x) => x.GetHashCode();
}
