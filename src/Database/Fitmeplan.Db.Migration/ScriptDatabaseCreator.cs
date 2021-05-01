using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Fitmeplan.Db.Migration
{
    public class ScriptDatabaseCreator : IDatabaseCreator
    {
        private readonly CreateDatabaseOptions _options;
        private readonly ConnectionStringProvider _connStringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"></see> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public ScriptDatabaseCreator(CreateDatabaseOptions options)
        {
            _options = options;
            _connStringProvider = new ConnectionStringProvider(_options.ConnectionString);
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            if (!_options.SchemaOnly)
            {
                CreateDatabase();
            }
            CreateTables();
            LoadData();
            SqlConnection.ClearAllPools();
        }

        public void Drop()
        {
            var connectionString = _connStringProvider.GetCreateDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string command = $"ALTER DATABASE [{_connStringProvider.GetDatabaseName()}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
                                 $"DROP DATABASE [{_connStringProvider.GetDatabaseName()}]";
                new SqlCommand(command, connection).ExecuteNonQuery();
            }
        }

        private void LoadData()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Immutable");
            Console.WriteLine($"Immutable data path:{path}");
            ImportFolder(path);

            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Test");
            Console.WriteLine($"Test data path:{path}");
            ImportFolder(path);
        }

        private void ImportFolder(string path)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.data");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);

                    var process = new Process();

                    var tableName = GetTableName(fileInfo);

                    var serverName = _connStringProvider.GetServer();
                    string serverNameOption = "(local)".Equals(serverName) ? string.Empty : $"-S {serverName}";
                    string command = $"\"{tableName}\" in {file} -q -k -d {_connStringProvider.GetDatabaseName()} -U {_connStringProvider.GetUser()} -P {_connStringProvider.GetPassword()} {serverNameOption} -w";

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        process.StartInfo.FileName = "bcp";
                        process.StartInfo.Arguments = command;
                    }
                    else
                    {
                        process.StartInfo.FileName = "/bin/bash";
                        //fix EOL with -r param
                        process.StartInfo.Arguments = $"-c \" /opt/mssql-tools/bin/bcp {command} -r'\\r\\n' \"";
                    }

                    Console.WriteLine($"Execute: {process.StartInfo.Arguments}");

                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("====================================="); 
                    Console.WriteLine($"Importing table: {tableName}");
                    Console.WriteLine("====================================="); 
                    process.Start();
                    var processOutput = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    Console.ResetColor();
                    
                    if (process.ExitCode != 0 || processOutput.Contains("error", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{Environment.NewLine}{processOutput}");
                        Console.WriteLine($"Exit code: {process.ExitCode}");
                        Console.ResetColor();
                        throw new Exception("Error on importing data");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"{Environment.NewLine}{processOutput}");
                        Console.WriteLine("Imported!");
                        Console.ResetColor();
                    }   
                }

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Path not found: {path}");
                Console.ResetColor();
            }
        }

        private string GetTableName(FileInfo dataFileInfo)
        {
            //when -k switch used then table format should be \"schema.table\"
            return $"{dataFileInfo.Name.Replace(dataFileInfo.Extension, string.Empty)}";
            //else [schema].[table], but this not work with tables with filtered indexes
            //return string.Join('.', dataFileInfo.Name.Replace(dataFileInfo.Extension, string.Empty).Split(".").Select(x => $"[{x}]"));
        }

        private void CreateTables()
        {
            using (var connection = new SqlConnection(_options.ConnectionString))
            {
                connection.Open();
                string command = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, _options.ScriptFile));
                new SqlCommand(command, connection).ExecuteNonQuery();
            }
        }

        private void CreateDatabase()
        {
            var connectionString = _connStringProvider.GetCreateDbConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string command = $"CREATE DATABASE [{_connStringProvider.GetDatabaseName()}]";
                new SqlCommand(command, connection).ExecuteNonQuery();
            }
        }
    }
}