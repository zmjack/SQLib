using Microsoft.Data.Sqlite;
using SQLib.Sqlite;

namespace SQLib.Data.Test
{
    public class ApplicationDbScope : SqliteScope<ApplicationDbScope>
    {
        public const string CONNECT_STRING = "filename=sqlib.db";
        public static ApplicationDbScope UseDefault() => new ApplicationDbScope(new SqliteConnection(CONNECT_STRING));

        public ApplicationDbScope(SqliteConnection model) : base(model) { }
    }

}
