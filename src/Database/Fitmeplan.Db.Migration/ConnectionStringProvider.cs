using System.Data.SqlClient;

namespace Fitmeplan.Db.Migration
{
    public class ConnectionStringProvider
    {
        private SqlConnectionStringBuilder builder;

        public ConnectionStringProvider(string connectionString)
        {
            builder = new SqlConnectionStringBuilder(connectionString);
        }

        public string GetConnectionString()
        {
            return builder.ConnectionString;
        }

        public string GetCreateDbConnectionString()
        {
            var temp = new SqlConnectionStringBuilder(builder.ConnectionString);
            temp.InitialCatalog = string.Empty;
            return temp.ConnectionString;
        }

        public string GetDatabaseName()
        {
            return builder.InitialCatalog;
        }

        public string GetUser()
        {
            return builder.UserID;
        }

        public string GetPassword()
        {
            return builder.Password;
        }

        public string GetServer()
        {
            return builder.DataSource;
        }
    }
}
