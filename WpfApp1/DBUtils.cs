using System.Data.SqlClient;

namespace WpfApp1
{
    class DBUtils
    {
        public static SqlConnection GetSqlConnection(string dbName, string serverName)
        {
            return new SqlConnection($"Data Source={serverName};Initial Catalog={dbName};Integrated Security=True");
        }
    }
}
