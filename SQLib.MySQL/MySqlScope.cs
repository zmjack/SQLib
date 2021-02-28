using MySql.Data.MySqlClient;

namespace SQLib.MySQL
{
    public sealed class MySqlScope : MySqlScope<MySqlScope>
    {
        public MySqlScope(string connectionString) : this(new MySqlConnection(connectionString)) { }
        public MySqlScope(MySqlConnection model) : base(model) { }
    }

    public class MySqlScope<TSelf> : SqlScope<TSelf, MySqlConnection, MySqlCommand, MySqlParameter>
        where TSelf : MySqlScope<TSelf>
    {
        public MySqlScope(string connectionString) : this(new MySqlConnection(connectionString)) { }
        public MySqlScope(MySqlConnection model) : base(model) { }
    }
}
