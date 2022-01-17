using NStandard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SQLib
{
    public class SafeSql<TDbParameter> where TDbParameter : DbParameter, new()
    {
        public string Sql;
        public TDbParameter[] Parameters;

        protected static TDbParameter CreateParameter(string parameterName, object value)
        {
            return new TDbParameter
            {
                ParameterName = parameterName,
                Value = value,
                DbType = value?.GetType() switch
                {
                    Type type when type == typeof(bool) => DbType.Boolean,
                    Type type when type == typeof(byte) => DbType.Byte,
                    Type type when type == typeof(sbyte) => DbType.SByte,
                    Type type when type == typeof(char) => DbType.Byte,
                    Type type when type == typeof(short) => DbType.Int16,
                    Type type when type == typeof(ushort) => DbType.UInt16,
                    Type type when type == typeof(int) => DbType.Int32,
                    Type type when type == typeof(uint) => DbType.UInt32,
                    Type type when type == typeof(long) => DbType.Int64,
                    Type type when type == typeof(ulong) => DbType.UInt64,
                    Type type when type == typeof(float) => DbType.Single,
                    Type type when type == typeof(double) => DbType.Double,
                    Type type when type == typeof(string) => DbType.String,
                    Type type when type == typeof(decimal) => DbType.Decimal,
                    Type type when type == typeof(DateTime) => DbType.DateTime,
                    Type type when type == typeof(byte[]) => DbType.Binary,
                    _ => DbType.Object,
                },
            };
        }

        public SafeSql(FormattableString formattableSql)
        {
            var args = formattableSql.GetArguments();
            var sql = formattableSql.Format;

            Sql = string.Format(sql, args.Select((value, i) =>
            {
                var type = value.GetType();
                var isArray = type.IsArray;
                if (isArray && type.GetElementType() != typeof(byte))
                {
                    var list = new List<string>();
                    var arr = value as Array;
                    var subIndex = 0;
                    foreach (var item in arr) list.Add($"@p{i}_{subIndex++}");
                    return $"({list.Join(", ")})";
                }
                else return $"@p{i}";
            }).ToArray());

            Parameters = args.SelectMany((value, i) =>
            {
                var list = new List<TDbParameter>();
                var type = value.GetType();
                var isArray = value.GetType().IsArray;
                if (isArray && type.GetElementType() != typeof(byte))
                {
                    var arr = value as Array;
                    var subIndex = 0;
                    foreach (var item in arr) list.Add(CreateParameter($"@p{i}_{subIndex++}", item));
                }
                else list.Add(CreateParameter($"@p{i}", value));

                return list;
            }).ToArray();
        }

    }
}
