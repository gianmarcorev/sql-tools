using CommandLine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sql_tools
{
    class Program
    {
        enum ExitCodes
        {
            IOERR = 7
        }

        static string getAssemblyInfoVersion()
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) 
                as AssemblyInformationalVersionAttribute;
            return version.InformationalVersion;
        }

        static string getPassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
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

        static void performConnectionTest()
        {
            Console.Write("Server: ");
            string server = Console.ReadLine();
            Console.Write("Database: ");
            string database = Console.ReadLine();
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = getPassword();

            string connString = getSqlConnectionString(server, database, username, password);

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
            string server, database, username, password, query=string.Empty;
            #region COLLECTPARAMS
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
            #endregion
            string connString = getSqlConnectionString(server, database, username, password);

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
            //else if (options.Version) {
            //    Console.WriteLine("MSSQL Tools version " + getAssemblyInfoVersion());
            //}

#if DEBUG
            Console.ReadKey(true);
#endif
        }
    }
}
