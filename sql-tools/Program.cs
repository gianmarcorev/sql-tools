using CommandLine;
using Deedle;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            return string.Format("Server=tcp:{0},1433;Database={1};" +
                "Persist Security Info=False;User ID={2};Password={3};Pooling=False;" +
                "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;" +
                "Connection Timeout=30;", server, db, user, pass);
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

        static DataTable getDataTabletFromCSVFile(string csv_file_path) {
            DataTable csvData = new DataTable();
            try {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path)) {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields) {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData) {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++) {
                            if (fieldData[i] == "") {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return csvData;
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

            Console.WriteLine("Loading input file...");
            var dataFrame = Frame.ReadCsv(filePath);
            
            // Most time we have a datetime in the first column

            Console.WriteLine("Loading complete.");

            string connString = getSqlConnectionString(options);
            using (var connection = new SqlConnection(connString)) {
                try {
                    Console.WriteLine("Connecting to SQL Server...");
                    //connection.Open();
                    Console.WriteLine("Connected successfully.");
                    Console.WriteLine("Checking if table exists...");
//                    if (!checkTable(options.Table, connection)) {
//                        Console.WriteLine("Error: table doesn't exist!");
//                        Console.Write(" Would you like to create a new table? [Y/n]: ");
//                        string res = Console.ReadLine();
//                        if (res.ToLower()[0] == 'n') {
//                            Console.WriteLine("Error: table doesn't exist!");
//#if DEBUG
//                            Console.ReadKey(true);
//#endif
//                            Environment.Exit((int)ExitCodes.SQL_ERR);
//                        }

                        // Infer column types
                        string[] columnTypes = new string[dataFrame.ColumnTypes.Count()];
                        for (int i = 0; i < dataFrame.ColumnTypes.Count(); i++) {
                            columnTypes[i] = Constants.getSqlType(dataFrame.ColumnTypes.ElementAt(i).Name);
                        }
                    //}
                    Console.WriteLine("Table exists");

                    //using (var command = new SqlCommand()) {
                    //    command.Connection = connection;
                    //    command.CommandType = System.Data.CommandType.Text;
                    //    command.CommandText = query;
                    //    int res = command.ExecuteNonQuery();
                    //    Console.WriteLine("Command executed, result is " + res);
                    //}
                }
                catch (InvalidOperationException e) {
                    Console.WriteLine("Error: " + e.Message);
                }
                catch (SqlException e) {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            

            //int i = 0;
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
