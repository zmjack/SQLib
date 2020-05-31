using NStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SQLib
{
    public static class Common
    {
        public static CacheContainer<Type, PropertyInfo[]> EntityPropertiesCache = new CacheContainer<Type, PropertyInfo[]>
        {
            CacheMethod = type => () =>
            {
                var props = type.GetProperties().Where(x => x.CanWrite && x.CanRead);
                return props.ToArray();
            },
        };

    }
}
