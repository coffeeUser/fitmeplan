using System;
using System.Threading.Tasks;

namespace Fitmeplan.Data.EntityFrameworkCore.Repository
{
    public class RepositoryBase<T> where T: ApplicationContext, new()
    {
        public RepositoryBase()
        {
        }

        /// <summary>
        /// Executes SQL command.
        /// </summary>
        /// <typeparam name="TU">The execution result.</typeparam>
        /// <param name="command">Fluent API command.</param>
        /// <returns></returns>
        public TU Execute<TU>(Func<TU> command)
        {
            using var applicationContext = new T();
            try
            {
                return command.Invoke();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Executes SQL command asynchronously.
        /// </summary>
        /// <typeparam name="TU">The execution result.</typeparam>
        /// <param name="command">Fluent API command.</param>
        /// <returns></returns>
        public Task<TU> ExecuteAsync<TU>(Func<TU> command)
        {
            using var applicationContext = new T();
            try
            {
                return Task.FromResult(command.Invoke());
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
