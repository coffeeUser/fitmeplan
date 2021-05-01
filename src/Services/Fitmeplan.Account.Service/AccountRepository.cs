using System.Linq;
using Fitmeplan.Data.Entities;

namespace Fitmeplan.Account.Service
{
    public class AccountRepository
    {
        private readonly string _connectionString;

        public AccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public UserEntity GetUserEntity(int userId)
        {
            UserEntity user;

            using (var context = new AccountDbContext(_connectionString))
            {
                user = context.UserEntities.FirstOrDefault(x => x.Id == userId);
            }

            return user;
        }

        public void SaveUser(UserEntity userEntity)
        {
            using (var context = new AccountDbContext(_connectionString))
            {
                context.UserEntities.Update(userEntity);
            }
        }
    }
}
