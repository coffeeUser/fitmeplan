using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Fitmeplan.Db.Migration
{
    public class DatabaseExporter : IDatabaseExporter
    {
        private readonly CreateDatabaseOptions _options;
        private readonly ConnectionStringProvider _connStringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"></see> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DatabaseExporter(CreateDatabaseOptions options)
        {
            _options = options;
            _connStringProvider = new ConnectionStringProvider(_options.ConnectionString);
        }

        public void Run()
        {
            List<string> tables = new List<string>();

            var connectionString = _connStringProvider.GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string command = "select s.name as SchemaName, t.name as TableName  from sys.tables t\r\n" +
                                 "inner join sys.schemas s on t.schema_id = s.schema_id\r\n" +
                                 "inner join sysindexes AS i on i.id = t.object_id\r\n " +
                                 "INNER JOIN sysobjects AS o ON i.id = o.id \r\n" +
                                 "WHERE i.indid < 2 \r\n " +
                                 "AND OBJECTPROPERTY(o.id, \'IsMSShipped\') = 0 \r\n " +
                                 "AND i.rowcnt > 0";
                var reader = new SqlCommand(command, connection).ExecuteReader();
                while (reader.Read())
                {
                    tables.Add($"{reader[0]}.{reader[1]}");
                }
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var table in tables)
            {
                ExportTable(path, table);
            }
        }

        private void ExportTable(string path, string tableName)
        {
            var process = new Process();

            string file = Path.Combine(path, tableName.ToLower() + ".data");

            string command = $"\"{tableName}\" out {file} -q -k -d {_connStringProvider.GetDatabaseName()} -U {_connStringProvider.GetUser()} -P {_connStringProvider.GetPassword()} -S {_connStringProvider.GetServer()} -w";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo.FileName = "bcp";
                process.StartInfo.Arguments = command;
            }
            else
            {
                process.StartInfo.FileName = "/bin/bash";
                //fix EOL with -r param
                process.StartInfo.Arguments = $"-c \" bcp {command} -r'\\r\\n' \"";
            }

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
                    
            Console.WriteLine($"{tableName} exporting");
            process.Start();
            var processOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Exit code: {process.ExitCode}");
                Console.WriteLine($"Error(s): {Environment.NewLine}{processOutput}");
            }
            else
            {
                Console.WriteLine($"{tableName} completed");
            }   
        }
    }
}