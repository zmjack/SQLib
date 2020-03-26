using Microsoft.Data.Sqlite;
using SQLib;

namespace SQLibApp.Data
{
    public class ApplicationDbScope : SqlScope<ApplicationDbScope, SqliteConnection, SqliteCommand, SqliteParameter>
    {
        public const string CONNECTION_STRING = "filename=sqlplus.db";
        public static ApplicationDbScope UseDefault() => new ApplicationDbScope(new SqliteConnection(CONNECTION_STRING));

        public ApplicationDbScope(SqliteConnection conn) : base(conn) { }
    }

}
