using Fitmeplan.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitmeplan.Account.Service
{
    public class AccountDbContext : DbContext
    {
        private readonly string _connectionString;

        public AccountDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserLoginEntity>()
                .HasKey(x => new { x.LoginProvider, x.ProviderKey });
        }

        public DbSet<UserEntity> UserEntities { get; set; }
        public DbSet<RoleEntity> RoleEntities { get; set; }
        public DbSet<UserLoginEntity> UserLoginEntities { get; set; }
        public DbSet<UserAccountEntity> UserAccountEntities { get; set; }
    }
}
