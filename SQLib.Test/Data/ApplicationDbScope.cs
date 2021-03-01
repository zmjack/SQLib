using Microsoft.Data.Sqlite;
using SQLib.Sqlite;

namespace SQLib.Data.Test
{
    public class ApplicationDbScope : SqliteScope<ApplicationDbScope>
    {
        public const string CONNECT_STRING = "filename=sqlib.db";
        public static ApplicationDbScope UseDefault() => new ApplicationDbScope(CONNECT_STRING);

        public ApplicationDbScope(string connectionString) : base(connectionString) { }
    }

}
