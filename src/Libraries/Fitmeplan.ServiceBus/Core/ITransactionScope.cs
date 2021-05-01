namespace Fitmeplan.ServiceBus.Core
{
    public interface ITransactionScope
    {
        /// <summary>
        /// Begins the tran.
        /// </summary>
        void BeginTran();
        
        /// <summary>
        /// Commits the tran.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollbacks the tran.
        /// </summary>
        void Rollback();
    }
}
