using System;
using System.IO;
using System.Linq;
using System.Text;
using Renci.SshNet;

namespace Fitmeplan.Storage.Client.Ftp
{
    public class FtpClient
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly string _path;
        private readonly string _fileName;
        private readonly string _logFileName;
        private readonly int _connectionRetryAttempts;

        public FtpClient(FtpClientConfiguration configuration)
        : this(configuration.Host, configuration.Username, configuration.Password, configuration.Path, configuration.FileName, configuration.LogFileName, configuration.ConnectionRetryAttempts)
        {

        }

        public FtpClient(string host, string username, string password, string path, string fileName, string logFileName, int connectionRetryAttempts)
        {
            _host = host;
            _username = username;
            _password = password;
            _path = path;
            _fileName = fileName;
            _logFileName = logFileName;
            _connectionRetryAttempts = connectionRetryAttempts;
        }

        public (byte[], string) ReadFileContent(string organisationId, string fileName = null)
        {
            var extension = string.Empty;
            fileName ??= _fileName;
            var connectionInfo = new ConnectionInfo(_host, _username, new PasswordAuthenticationMethod(_username, _password));
            byte[] result = null;
            using (var sftp = new SftpClient(connectionInfo))
            {
                sftp.KeepAliveInterval = TimeSpan.FromSeconds(60);
                sftp.ConnectionInfo.Timeout = TimeSpan.FromSeconds(180);
                sftp.OperationTimeout = TimeSpan.FromSeconds(180);
                
                var attempts = 0;
                do
                {
                    try
                    {
                        sftp.Connect();
                    }
                    catch (Renci.SshNet.Common.SshConnectionException e)
                    {
                        attempts++;
                    }
                } while (attempts < _connectionRetryAttempts && !sftp.IsConnected);

                var folders = sftp.ListDirectory(_path);
                if (folders.Any(x => string.Equals(x.Name, organisationId)))
                {
                    var files = sftp.ListDirectory(_path + "/" + organisationId);

                    var file = files.FirstOrDefault(x => x.Name.Contains(fileName));
                    if (file != null)
                    {
                        extension = file.Name.Substring(file.Name.LastIndexOf('.'));
                        using (var memoryStream = new MemoryStream())
                        {
                            sftp.DownloadFile(file.FullName, memoryStream);
                            sftp.Disconnect();
                            result = memoryStream.ToArray();
                        }
                    }
                }
                sftp.Disconnect();
            }
            return (result ?? new byte[0], extension);
        }

        public void WriteResult(string organisationId, DateTime? startTime, DateTime? endTime, bool success, string errors, string logFileName = null)
        {
            logFileName ??= _logFileName;
            var connectionInfo = new ConnectionInfo(_host, _username, new PasswordAuthenticationMethod(_username, _password));
            using (var sftp = new SftpClient(connectionInfo))
            {
                sftp.KeepAliveInterval = TimeSpan.FromSeconds(60);
                sftp.ConnectionInfo.Timeout = TimeSpan.FromSeconds(180);
                sftp.OperationTimeout = TimeSpan.FromSeconds(180);
                
                var attempts = 0;
                do
                {
                    try
                    {
                        sftp.Connect();
                    }
                    catch (Renci.SshNet.Common.SshConnectionException e)
                    {
                        attempts++;
                    }
                } while (attempts < _connectionRetryAttempts && !sftp.IsConnected);
                
                var folders = sftp.ListDirectory(_path);
                if (folders.Any(x => string.Equals(x.Name, organisationId)))
                {
                    var files = sftp.ListDirectory(_path + "/" + organisationId);

                    var logFile = files.FirstOrDefault(x => string.Equals(x.Name, logFileName));
                    if (logFile != null)
                    {
                        sftp.DeleteFile(logFile.FullName);
                    }

                    var content = new StringBuilder();
                    content.Append($"Data import start time: {startTime}");
                    content.AppendLine();
                    content.Append($"Data import end time: {endTime}");
                    content.AppendLine();
                    var dataImportResult = success ? "Success" : "Failed";
                    content.Append($"Data import result: {dataImportResult}");
                    content.AppendLine();
                    content.Append("Data import errors:");
                    content.AppendLine();
                    content.Append(errors);
                    using (var memoryStream = new MemoryStream())
                    {
                        var streamWriter = new StreamWriter(memoryStream);
                        streamWriter.Write(content.ToString());
                        streamWriter.Flush();
                        memoryStream.Position = 0;

                        sftp.UploadFile(memoryStream, _path + "/" + organisationId + "/" + logFileName);
                    }
                }
                sftp.Disconnect();
            }
        }
    }
}
