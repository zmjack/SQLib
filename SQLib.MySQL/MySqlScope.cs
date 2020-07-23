using MySql.Data.MySqlClient;

namespace SQLib.MySQL
{
    public class MySqlScope<TSelf> : SqlScope<TSelf, MySqlConnection, MySqlCommand, MySqlParameter>
        where TSelf : MySqlScope<TSelf>
    {
        public MySqlScope(MySqlConnection model) : base(model)
        {
        }
    }
}
