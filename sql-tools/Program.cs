using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace sql_tools
{
    class Program {
        enum ExitCodes {
            IOERR = 7,
            TYPE_ERR = 8,
            SQL_ERR = 9
        }

        static string getAssemblyInfoVersion() {
            var version = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))
                as AssemblyInformationalVersionAttribute;
            return version.InformationalVersion;
        }

        static string getPassword() {
            StringBuilder sb = new StringBuilder();
            while (true) {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter) {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace) {
                    if (sb.Length > 0) {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }
                    continue;
                }
                Console.Write('*');
                sb.Append(cki.KeyChar);
            }
            return sb.ToString();
        }

        static string getSqlConnectionString(string server, string db, string user, string pass) {
            //return string.Format("Server=tcp:{0},1433;Database={1};" +
            //    "Persist Security Info=False;User ID={2};Password={3};Pooling=False;" +
            //    "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;" +
            //    "Connection Timeout=30;", server, db, user, pass);
            // TEMP
            return Constants.LOCALDB_CONN_STR;
        }

        static string getSqlConnectionString(CommonSubOptions options) {
            string server, database, username, password;

            if (options.Host != null) {
                server = options.Host;
            }
            else {
                Console.Write("Server: ");
                server = Console.ReadLine();
            }

            if (options.Database != null) {
                database = options.Database;
            }
            else {
                Console.Write("Database: ");
                database = Console.ReadLine();
            }

            if (options.User != null) {
                username = options.User;
            }
            else {
                Console.Write("Username: ");
                username = Console.ReadLine();
            }

            if (options.Password != null) {
                password = options.Password;
            }
            else {
                Console.Write("Password: ");
                password = getPassword();
            }
#if DEBUG
            Console.WriteLine("Server:   " + server);
            Console.WriteLine("Database: " + database);
            Console.WriteLine("Username: " + username);
            Console.WriteLine("Password: " + password);
#endif
            return getSqlConnectionString(server, database, username, password);
        }

        static string getSqlConnectionString() {
            Console.Write("Server: ");
            string server = Console.ReadLine();
            Console.Write("Database: ");
            string database = Console.ReadLine();
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = getPassword();

            return getSqlConnectionString(server, database, username, password);
        }

        static void performConnectionTest()
        {
            string connString = getSqlConnectionString();

            using (var connection = new SqlConnection(connString))
            {
                try {
                    connection.Open();
                    Console.WriteLine("Connected successfully.");
                }
                catch (InvalidOperationException e) {
                    Console.WriteLine("Error: " + e.Message);
                }
                catch (SqlException e) {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        static void performQuery(QuerySubOptions options)
        {
            string  query=string.Empty;
            
            string connString = getSqlConnectionString(options);

            if (options.File != null) {
                if (!System.IO.File.Exists(options.File)) {
                    Console.WriteLine("Error: the specified file doesn't exist.");
                    Environment.Exit((int)ExitCodes.IOERR);
                }
                string text = System.IO.File.ReadAllText(options.File);
#if DEBUG
                Console.WriteLine(text);
#endif
                query = text;
            }
            else if (options.Query != null) {
                // TODO: check and fix
                query = options.Query;
            }

            // SQL connection and query
            using (var connection = new SqlConnection(connString)) {
                try {
                    connection.Open();
                    Console.WriteLine("Connected successfully.");

                    using (var command = new SqlCommand()) {
                        command.Connection = connection;
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = query;
                        int res = command.ExecuteNonQuery();
                        Console.WriteLine("Command executed, result is " + res);
                    }
                }
                catch (InvalidOperationException e) {
                    Console.WriteLine("Error: " + e.Message);
                }
                catch (SqlException e) {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        static DataTable getDataTableFromCsv(string path, bool hasHeaders) {
            string header = hasHeaders ? "Yes" : "No";

            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties=\"Text;HDR=" + header + "\""))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command)) {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        static bool checkTable(string table, SqlConnection conn)
        {
            using (var cmd = new SqlCommand()) {
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = string.Format(Constants.CHECK_TABLE, table);
                return (int)cmd.ExecuteScalar() == 1;
            }
        }

        static void createTable(string name, SqlConnection conn) {
            // Build query
        }

        static void bulkInsert(BulkSubOptions options)
        {
            string filePath = options.Input;
            InputFileType type = options.Type;

            if (type == InputFileType.HDF) {
                Console.WriteLine("Error: HDF files are currently not supported");
                Environment.Exit((int)ExitCodes.TYPE_ERR);
            }

            if (!System.IO.File.Exists(filePath)) {
                Console.WriteLine("Error: file not found");
                Environment.Exit((int)ExitCodes.IOERR);
            }

            Console.WriteLine("WARNING: remember to use a table only if you are uploading data " +
                "with all columns, including the primary key. Otherwise be sure to use a view " +
                "of the table's columns or an error will raise.");

            //Console.WriteLine("Loading input file...");
            //var dataFrame = Frame.ReadCsv(filePath, hasHeaders:!options.NoHeaders,
            //    separators:options.Separator);
            //DataTable data = getDataTableFromCsv(filePath, !options.NoHeaders);
            //Console.WriteLine("Loading complete.");

            string connString = getSqlConnectionString(options);
            using (var connection = new SqlConnection(connString)) {
                try {
                    Console.WriteLine("Connecting to SQL Server...");
                    connection.Open();
                    Console.WriteLine("Connected successfully.");
                    Console.WriteLine("Checking if table exists...");

                    // Check if table exists
    //                if (!checkTable(destination, connection)) {
    //                    // Create table
    //                    Console.WriteLine("Error: table doesn't exist!");
    //                    Console.WriteLine("WARNING: the table can be automatically created " +
    //                        "inferring data types from loaded data, but this is still experimental.");
    //                    Console.WriteLine("It's highly recommended to create the table manually.");
    //                    Console.Write("Would you like to automatically create a new table? [Y/n]: ");
    //                    string res = Console.ReadLine();
    //                    if (res != string.Empty && res.ToLower()[0] == 'n') {
    //                        Console.WriteLine("Error: table doesn't exist!");
    //#if DEBUG
    //                        Console.ReadKey(true);
    //#endif
    //                        Environment.Exit((int)ExitCodes.SQL_ERR);
    //                    }

    //                    // Infer column types
    //                    string[] columnTypes = new string[dataFrame.ColumnTypes.Count()];
    //                    for (int i = 0; i < dataFrame.ColumnTypes.Count(); i++) {
    //                        columnTypes[i] = Constants.getSqlType(dataFrame.ColumnTypes.ElementAt(i).Name);
    //                    }

    //                    Console.Write("Table name: ");
    //                    string tableName = Console.ReadLine();

    //                    // Build create table script
    //                    // Columns
    //                    StringBuilder sb = new StringBuilder();
    //                    sb.AppendLine("ID int PRIMARY KEY NOT NULL,");
    //                    for (int i = 0; i < dataFrame.ColumnCount; i++) {
    //                        sb.Append(dataFrame.ColumnKeys.ElementAt(i) + " " +
    //                            columnTypes[i] + " " + "NOT NULL");
    //                        if (i < dataFrame.ColumnCount - 1) {
    //                            sb.AppendLine(",");
    //                        }
    //                    }

                        //string tableQuery = string.Format(Constants.CREATE_TABLE_BODY, "dbo", tableName, sb.ToString());
                        //using (var command = new SqlCommand()) {
                        //    command.Connection = connection;
                        //    command.CommandType = System.Data.CommandType.Text;
                        //    command.CommandText = tableQuery;
                        //    int tableRes = command.ExecuteNonQuery();
                        //    Console.WriteLine("Command executed, result is " + res);
                        //}
                    //}

                    //Console.WriteLine("Table exists");
                    //using (var bulkCopy = new SqlBulkCopy(connection)) {
                    //    bulkCopy.DestinationTableName = options.Table;

                    //    try {
                    //        //bulkCopy.WriteToServer(dataFrame.ToDataTable().);
                    //    }
                    //    catch (Exception) {

                    //        throw;
                    //    }
                    //}

                    using (var cmd = new SqlCommand()) {
                        cmd.Connection = connection;
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = string.Format(Constants.BULK_INSERT_CSV,
                            options.Destination, options.Input, options.Separator,
                            options.HasHeaders ? "2" : "1");
                        int res = cmd.ExecuteNonQuery();
                        Console.WriteLine("Command executed, number of rows affected: " + res);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        static void Main(string[] args)
        {
            string invokedVerb = null;
            object invokedVerbInstance = null;

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
                {
                    // if parsing succeeds the verb name and correct instance
                    // will be passed to onVerbCommand delegate (string,object)
                    invokedVerb = verb;
                    invokedVerbInstance = subOptions;
                }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);  
            }

            if (invokedVerb == "test") {
                performConnectionTest();
            }

            if (invokedVerb == "query") {
                performQuery((QuerySubOptions)invokedVerbInstance);
            }

            if (invokedVerb == "bulk") {
                bulkInsert((BulkSubOptions)invokedVerbInstance);
            }
            //else if (options.Version) {
            //    Console.WriteLine("MSSQL Tools version " + getAssemblyInfoVersion());
            //}

#if DEBUG
            Console.ReadKey(true);
#endif
        }
    }
}
