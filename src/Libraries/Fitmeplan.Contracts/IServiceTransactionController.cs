namespace Fitmeplan.Contracts
{
    public interface IServiceTransactionController
    {
        void CommitTransactionAndReleaseConnection();
    }
}
