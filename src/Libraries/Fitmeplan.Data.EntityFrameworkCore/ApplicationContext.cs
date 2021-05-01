using Microsoft.EntityFrameworkCore;

namespace Fitmeplan.Data.EntityFrameworkCore
{
    public class ApplicationContext : DbContext, IApplicationContext
    {
        private readonly string _connectionString;

        public ApplicationContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
