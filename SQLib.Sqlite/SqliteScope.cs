#if NET35 || NET40 || NET45 || NET451 || NET46
using System.Data.SQLite;
#else
using Microsoft.Data.Sqlite;
#endif

namespace SQLib.Sqlite
{
#if NET35 || NET40 || NET45 || NET451 || NET46
    public sealed class SqliteScope : SqliteScope<SqliteScope>
    {
        public SqliteScope(string connectionString) : this(new SQLiteConnection(connectionString)) { }
        public SqliteScope(SQLiteConnection model) : base(model) { }
    }

    public class SqliteScope<TSelf> : SqlScope<TSelf, SQLiteConnection, SQLiteCommand, SQLiteParameter>
        where TSelf : SqliteScope<TSelf>
    {
        public SqliteScope(string connectionString) : this(new SQLiteConnection(connectionString)) { }
        public SqliteScope(SQLiteConnection model) : base(model) { }
    }
#else
    public sealed class SqliteScope : SqliteScope<SqliteScope>
    {
        public SqliteScope(string connectionString) : this(new SqliteConnection(connectionString)) { }
        public SqliteScope(SqliteConnection model) : base(model) { }
    }

    public class SqliteScope<TSelf> : SqlScope<TSelf, SqliteConnection, SqliteCommand, SqliteParameter>
        where TSelf : SqliteScope<TSelf>
    {
        public SqliteScope(string connectionString) : this(new SqliteConnection(connectionString)) { }
        public SqliteScope(SqliteConnection model) : base(model) { }
    }
#endif
}