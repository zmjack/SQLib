using NStandard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
            var safeSql = new SafeSql<TDbParameter>(formattableSql);
            return UnsafeSqlQuery(transaction, safeSql.Sql, safeSql.Parameters);
        }

    }
}
