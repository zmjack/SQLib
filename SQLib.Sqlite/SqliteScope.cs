using Microsoft.Data.Sqlite;

namespace SQLib.Sqlite
{
    public class SqliteScope : SqliteScope<SqliteScope>
    {
        public SqliteScope(SqliteConnection model) : base(model) { }
    }

    public class SqliteScope<TSelf> : SqlScope<TSelf, SqliteConnection, SqliteCommand, SqliteParameter>
        where TSelf : SqliteScope<TSelf>
    {
        public SqliteScope(SqliteConnection model) : base(model) { }
    }
}
