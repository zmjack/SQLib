using NStandard;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SQLib
{
    public class TransactionScope<TSqlScope, TDbConnection, TDbCommand, TDbParameter> : Scope<TransactionScope<TSqlScope, TDbConnection, TDbCommand, TDbParameter>>
        where TSqlScope : SqlScope<TSqlScope, TDbConnection, TDbCommand, TDbParameter>
        where TDbConnection : DbConnection, new()
        where TDbCommand : DbCommand, new()
        where TDbParameter : DbParameter, new()
    {
        public DbTransaction Transaction;

        public TransactionScope(DbTransaction transaction)
        {
            Transaction = transaction;
        }
    }
}
