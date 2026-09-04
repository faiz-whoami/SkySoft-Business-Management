using System.Configuration;
using System.Data.SqlClient;

namespace business_managment_system.Data
{
    public static class DbConnectionFactory
    {
        public static SqlConnection Create()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SkySoftDb"].ConnectionString;
            return new SqlConnection(connectionString);
        }
    }
}
