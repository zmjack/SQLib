using NStandard;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

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
                    .Where(x => !x.HasAttribute<NotMappedAttribute>())
                    .Select(prop =>
                    {
                        var column = prop.GetCustomAttribute<ColumnAttribute>();
                        return new ColumnInfo
                        {
                            Property = prop,
                            PropertyName = prop.Name,
                            ColumnName = column?.Name,
                        };
                    });
                return props.ToArray();
            },
        };

    }
}
