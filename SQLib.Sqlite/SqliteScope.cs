using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLib.Sqlite
{
    public class SqliteScope<TSelf> : SqlScope<TSelf, SqliteConnection, SqliteCommand, SqliteParameter>
        where TSelf : SqliteScope<TSelf>
    {
        public SqliteScope(SqliteConnection model) : base(model)
        {
        }
    }
}
