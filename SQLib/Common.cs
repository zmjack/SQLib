using NStandard;
using System;
using System.Linq;
using System.Reflection;

namespace SQLib
{
    public static class Common
    {
        public static CacheContainer<Type, ColumnInfo[]> EntityPropertiesCache = new CacheContainer<Type, ColumnInfo[]>
        {
            CacheMethod = type => () =>
            {
                var props = type.GetProperties()
                    .Where(x => x.CanWrite && x.CanRead)
                    .Where(x => !x.HasAttribute<NotSqlColumnAttribute>())
                    .Select(prop =>
                    {
                        var columnName = prop.GetCustomAttribute<SqlColumnAttribute>()?.Name ?? prop.Name;
                        return new ColumnInfo
                        {
                            Property = prop,
                            ColumnName = columnName,
                        };
                    });
                return props.ToArray();
            },
        };

    }
}
