namespace Fitmeplan.Storage.Client.Ftp
{
    public class FtpClientConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string LogFileName { get; set; }
        public int ConnectionRetryAttempts { get; set; }
    }
}
