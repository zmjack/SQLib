using NStandard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SqlPlus
{
    public class SafeSql<TDbParameter>
        where TDbParameter : DbParameter, new()
    {
        public string Sql;
        public TDbParameter[] Parameters;

        public SafeSql(FormattableString formattableSql)
        {
            var args = formattableSql.GetArguments();
            var sql = formattableSql.Format;

            Sql = string.Format(sql, args.Select((v, i) => $"@p{i}").ToArray());
            Parameters = args.Select((value, i) =>
            {
                return new TDbParameter().Then(x =>
                {
                    x.ParameterName = $"@p{i}";
                    x.Value = value;
                    x.DbType = value.GetType() switch
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
                        _ => DbType.Object,
                    };
                });
            }).ToArray();
        }
    }
}
