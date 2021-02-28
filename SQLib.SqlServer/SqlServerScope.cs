#if NET35 || NET40 || NET45 || NET451 || NET46
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace SQLib.MySQL
{
    public sealed class SqlServerScope : SqlServerScope<SqlServerScope>
    {
        public SqlServerScope(string connectionString) : this(new SqlConnection(connectionString)) { }
        public SqlServerScope(SqlConnection model) : base(model) { }
    }

    public class SqlServerScope<TSelf> : SqlScope<TSelf, SqlConnection, SqlCommand, SqlParameter>
        where TSelf : SqlServerScope<TSelf>
    {
        public SqlServerScope(string connectionString) : this(new SqlConnection(connectionString)) { }
        public SqlServerScope(SqlConnection model) : base(model) { }
    }
}
