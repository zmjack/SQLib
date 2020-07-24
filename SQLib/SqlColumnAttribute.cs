using System;

namespace SQLib
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SqlColumnAttribute : Attribute
    {
        public string Name { get; set; }

        public SqlColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
