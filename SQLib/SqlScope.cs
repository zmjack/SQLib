using NStandard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SQLib
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

        public int UnsafeSql(string sql, TDbParameter[] parameters = null) => UnsafeSql(CurrentTransaction, sql, parameters);
        public int UnsafeSql(DbTransaction transaction, string sql, TDbParameter[] parameters = null)
        {
            var command = new TDbCommand
            {
                Transaction = transaction,
                CommandText = sql,
                Connection = Connection,
            };
            if (parameters != null) command.Parameters.AddRange(parameters);

            var ret = command.ExecuteNonQuery();
            OnExecuted?.Invoke(command);
            return ret;
        }
        public int Sql(FormattableString formattableSql) => Sql(CurrentTransaction, formattableSql);
        public int Sql(DbTransaction transaction, FormattableString formattableSql)
        {
            var safeSql = new SafeSql<TDbParameter>(formattableSql);
            return UnsafeSql(transaction, safeSql.Sql, safeSql.Parameters);
        }

        public Dictionary<string, object>[] UnsafeSqlQuery(string sql, TDbParameter[] parameters = null) => UnsafeSqlQuery(CurrentTransaction, sql, parameters);
        public Dictionary<string, object>[] UnsafeSqlQuery(DbTransaction transaction, string sql, TDbParameter[] parameters = null)
        {
            var ret = new List<Dictionary<string, object>>();
            var command = new TDbCommand
            {
                Transaction = transaction,
                CommandText = sql,
                Connection = Connection,
            };
            if (parameters != null) command.Parameters.AddRange(parameters);

            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var dict = new Dictionary<string, object>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    dict[fieldName] = reader.GetValue(i);
                }
                ret.Add(dict);
            }
            reader.Close();
            OnExecuted?.Invoke(command);

            return ret.ToArray();
        }

        public TEntity[] UnsafeSqlQuery<TEntity>(string sql, TDbParameter[] parameters = null) where TEntity : class, new() => UnsafeSqlQuery<TEntity>(CurrentTransaction, sql, parameters);
        public TEntity[] UnsafeSqlQuery<TEntity>(DbTransaction transaction, string sql, TDbParameter[] parameters = null)
            where TEntity : class, new()
        {
            var ret = new List<TEntity>();
            var command = new TDbCommand
            {
                Transaction = transaction,
                CommandText = sql,
                Connection = Connection,
            };
            if (parameters != null) command.Parameters.AddRange(parameters);

            var columns = Common.EntityPropertiesCache[typeof(TEntity)].Value;
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var entity = new TEntity();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    var column = columns.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));

                    if (column != null)
                    {
                        switch (column.Property.PropertyType)
                        {
                            case Type type when type == typeof(bool): column.Property.SetValue(entity, reader.GetBoolean(i)); break;
                            case Type type when type == typeof(byte): column.Property.SetValue(entity, reader.GetByte(i)); break;
                            case Type type when type == typeof(sbyte): column.Property.SetValue(entity, (sbyte)reader.GetByte(i)); break;
                            case Type type when type == typeof(char): column.Property.SetValue(entity, reader.GetChar(i)); break;
                            case Type type when type == typeof(short): column.Property.SetValue(entity, reader.GetInt16(i)); break;
                            case Type type when type == typeof(ushort): column.Property.SetValue(entity, (ushort)reader.GetInt16(i)); break;
                            case Type type when type == typeof(int): column.Property.SetValue(entity, reader.GetInt32(i)); break;
                            case Type type when type == typeof(uint): column.Property.SetValue(entity, (uint)reader.GetInt32(i)); break;
                            case Type type when type == typeof(long): column.Property.SetValue(entity, reader.GetInt64(i)); break;
                            case Type type when type == typeof(ulong): column.Property.SetValue(entity, (ulong)reader.GetInt64(i)); break;
                            case Type type when type == typeof(float): column.Property.SetValue(entity, reader.GetFloat(i)); break;
                            case Type type when type == typeof(double): column.Property.SetValue(entity, reader.GetDouble(i)); break;
                            case Type type when type == typeof(string): column.Property.SetValue(entity, reader.GetString(i)); break;
                            case Type type when type == typeof(decimal): column.Property.SetValue(entity, reader.GetDecimal(i)); break;
                            case Type type when type == typeof(DateTime): column.Property.SetValue(entity, reader.GetDateTime(i)); break;
                        }
                    }
                }
                ret.Add(entity);
            }
            reader.Close();
            OnExecuted?.Invoke(command);

            return ret.ToArray();
        }

        public Dictionary<string, object>[] SqlQuery(FormattableString formattableSql) => SqlQuery(CurrentTransaction, formattableSql);
        public Dictionary<string, object>[] SqlQuery(DbTransaction transaction, FormattableString formattableSql)
        {
            var safeSql = new SafeSql<TDbParameter>(formattableSql);
            return UnsafeSqlQuery(transaction, safeSql.Sql, safeSql.Parameters);
        }

        public TEntity[] SqlQuery<TEntity>(FormattableString formattableSql) where TEntity : class, new() => SqlQuery<TEntity>(CurrentTransaction, formattableSql);
        public TEntity[] SqlQuery<TEntity>(DbTransaction transaction, FormattableString formattableSql)
            where TEntity : class, new()
        {
            var safeSql = new SafeSql<TDbParameter>(formattableSql);
            return UnsafeSqlQuery<TEntity>(transaction, safeSql.Sql, safeSql.Parameters);
        }

    }
}
