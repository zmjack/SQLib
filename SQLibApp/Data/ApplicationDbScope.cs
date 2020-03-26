using Microsoft.Data.Sqlite;
using SQLib;

namespace SQLibApp.Data
{
    public class ApplicationDbScope : SqlScope<ApplicationDbScope, SqliteConnection, SqliteCommand, SqliteParameter>
    {
        public const string CONNECT_STRING = "filename=sqlplus.db";
        public static ApplicationDbScope UseDefault() => new ApplicationDbScope(new SqliteConnection(CONNECT_STRING));

        public ApplicationDbScope(SqliteConnection model) : base(model) { }
    }

}
