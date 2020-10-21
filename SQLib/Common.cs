using NStandard;
using NStandard.Caching;
using System;
using System.Linq;
using System.Reflection;

namespace SQLib
{
    public static class Common
    {
        public static CacheSet<Type, ColumnInfo[]> EntityPropertiesCache = new CacheSet<Type, ColumnInfo[]>
        {
            CacheMethodBuilder = type => () =>
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

                foreach (var group in props.Select(x => x.ColumnName).GroupBy(x => x))
                {
                    if (group.Count() > 1) throw new ArgumentException($"More columns have the same name({group.Key}).");
                }

                return props.ToArray();
            },
        };

    }
}
