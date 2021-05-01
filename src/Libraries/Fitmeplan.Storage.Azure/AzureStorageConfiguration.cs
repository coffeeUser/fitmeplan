namespace Fitmeplan.Storage.Azure
{
    public class AzureStorageConfiguration
    {
        /// <summary>
        /// Gets/sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets/sets the temporary container name.
        /// </summary>
        public string TempContainerName { get; set; }

        /// <summary>
        /// Gets/sets the main container name.
        /// </summary>
        public string MainContainerName { get; set; }

        /// <summary>
        /// Gets/sets the token expire period in minutes.
        /// </summary>
        public double TokenExpirePeriod { get; set; }
    }
}
