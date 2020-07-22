using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SQLib
{
    public class ColumnInfo
    {
        public PropertyInfo Property { get; set; }
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }

        public string Name => ColumnName ?? PropertyName;

    }

}
