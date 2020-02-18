using NStandard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace SqlPlus
{
    /// <summary>
    /// Easy to use and secure SQL Executor
    /// </summary>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TDbCommand"></typeparam>
    /// <typeparam name="TDbParameter"></typeparam>
    public abstract class SqlScope<TSelf, TDbConnection, TDbCommand, TDbParameter> : Scope<TSelf>
        where TSelf : SqlScope<TSelf, TDbConnection, TDbCommand, TDbParameter>
        where TDbConnection : DbConnection, new()
        where TDbCommand : DbCommand, new()
        where TDbParameter : DbParameter, new()
    {
        public delegate void OnExecutedDelegate(TDbCommand command);

        public readonly TDbConnection Connection;
        public DbTransaction CurrentTransaction => TransactionScope<TSelf, TDbConnection, TDbCommand, TDbParameter>.Current?.Transaction;
        public event OnExecutedDelegate OnExecuted;

        public SqlScope(TDbConnection model)
        {
            Connection = model;
            Connection.Open();
        }

        public override void Disposing()
        {
            Connection.Close();
            base.Disposing();
        }

        public TransactionScope<TSelf, TDbConnection, TDbCommand, TDbParameter> BeginTransactionScope()
        {
            return new TransactionScope<TSelf, TDbConnection, TDbCommand, TDbParameter>(Connection.BeginTransaction());
        }

        [Obsolete("SQL injection may be triggered using this method.")]
        public int UnsafeSql(string sql) => UnsafeSql(CurrentTransaction, sql);
        [Obsolete("SQL injection may be triggered using this method.")]
        public int UnsafeSql(DbTransaction transaction, string sql)
        {
            var command = UnsafeSqlCommand(transaction, sql);
            var ret = command.ExecuteNonQuery();
            OnExecuted?.Invoke(command);
            return ret;
        }

        public int Sql(string sql, TDbParameter[] parameters) => Sql(CurrentTransaction, sql, parameters);
        public int Sql(DbTransaction transaction, string sql, TDbParameter[] parameters)
        {
            var command = SqlCommand(transaction, sql, parameters);
            var ret = command.ExecuteNonQuery();
            OnExecuted?.Invoke(command);
            return ret;
        }

        public int Sql(FormattableString formattableSql) => Sql(CurrentTransaction, formattableSql);
        public int Sql(DbTransaction transaction, FormattableString formattableSql)
        {
            var command = SqlCommand(transaction, formattableSql);
            var ret = command.ExecuteNonQuery();
            OnExecuted?.Invoke(command);
            return ret;
        }

        [Obsolete("SQL injection may be triggered using this method.")]
        public Dictionary<string, object>[] UnsafeSqlQuery(string sql) => UnsafeSqlQuery(CurrentTransaction, sql);
        [Obsolete("SQL injection may be triggered using this method.")]
        public Dictionary<string, object>[] UnsafeSqlQuery(DbTransaction transaction, string sql)
        {
            var ret = new List<Dictionary<string, object>>();
            var command = UnsafeSqlCommand(transaction, sql);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var dict = new Dictionary<string, object>();
                foreach (var i in new int[reader.FieldCount].Let(i => i))
                    dict[reader.GetName(i)] = reader.GetValue(i);
                ret.Add(dict);
            }
            reader.Close();
            OnExecuted?.Invoke(command);

            return ret.ToArray();
        }

        public Dictionary<string, object>[] SqlQuery(string sql, TDbParameter[] parameters) => SqlQuery(CurrentTransaction, sql, parameters);
        public Dictionary<string, object>[] SqlQuery(DbTransaction transaction, string sql, TDbParameter[] parameters)
        {
            var ret = new List<Dictionary<string, object>>();
            var command = SqlCommand(transaction, sql, parameters);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var dict = new Dictionary<string, object>();
                foreach (var i in new int[reader.FieldCount].Let(i => i))
                    dict[reader.GetName(i)] = reader.GetValue(i);
                ret.Add(dict);
            }
            reader.Close();
            OnExecuted?.Invoke(command);

            return ret.ToArray();
        }

        public Dictionary<string, object>[] SqlQuery(FormattableString formattableSql) => SqlQuery(CurrentTransaction, formattableSql);
        public Dictionary<string, object>[] SqlQuery(DbTransaction transaction, FormattableString formattableSql)
        {
            var ret = new List<Dictionary<string, object>>();
            var command = SqlCommand(transaction, formattableSql);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var dict = new Dictionary<string, object>();
                foreach (var i in new int[reader.FieldCount].Let(i => i))
                    dict[reader.GetName(i)] = reader.GetValue(i);
                ret.Add(dict);
            }
            reader.Close();
            OnExecuted?.Invoke(command);

            return ret.ToArray();
        }

        [Obsolete("SQL injection may be triggered using this method.")]
        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        protected TDbCommand UnsafeSqlCommand(DbTransaction transaction, string sql)
        {
            var cmd = new TDbCommand
            {
                CommandText = sql,
                Connection = Connection,
                Transaction = transaction,
            };
            return cmd;
        }

        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        protected TDbCommand SqlCommand(DbTransaction transaction, string sql, TDbParameter[] parameters)
        {
            var cmd = new TDbCommand
            {
                CommandText = sql,
                Connection = Connection,
                Transaction = transaction,
            };

            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                    cmd.Parameters.Add(parameters[i]);
            }

            return cmd;
        }

        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        protected TDbCommand SqlCommand(DbTransaction transaction, FormattableString formattableSql)
        {
            var args = formattableSql.GetArguments();
            var sql = formattableSql.Format;
            var cmd = new TDbCommand
            {
                CommandText = formattableSql.ArgumentCount.For(count =>
                {
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                            sql = sql.Replace($"{{{i}}}", $"@p{i}");
                    }
                    return sql;
                }),
                Connection = Connection,
                Transaction = transaction,
            };

            for (int i = 0; i < formattableSql.ArgumentCount; i++)
            {
                cmd.Parameters.Add(new TDbParameter().Then(x =>
                {
                    x.ParameterName = $"@p{i}";
                    x.Value = args[i];
                    x.DbType = args[i].For(value => value.GetType() switch
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
                    });
                }));
            }
            return cmd;
        }

    }
}
