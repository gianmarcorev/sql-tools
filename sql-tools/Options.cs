using CommandLine;
using CommandLine.Text;

namespace sql_tools
{
    enum InputFileType
    {
        CSV,
        HDF
    }

    class CommonSubOptions
    {
        [Option('H', "host", HelpText = "Host address")]
        public string Host { get; set; }

        [Option('D', "database", HelpText = "Database")]
        public string Database { get; set; }

        [Option('U', "user", HelpText = "Username")]
        public string User { get; set; }

        [Option('P', "password", HelpText = "Password")]
        public string Password { get; set; }
    }

    class QuerySubOptions : CommonSubOptions
    {
        [Option('q', "query", HelpText = "Query to be executed", MutuallyExclusiveSet = "query")]
        public string Query { get; set; }

        [Option('f', "file", HelpText = "Text file containing the query", MutuallyExclusiveSet = "query")]
        public string File { get; set; }
    }

    class BulkSubOptions : CommonSubOptions
    {
        [Option('i', "input", HelpText = "Input file containing data to be uploaded", Required = true)]
        public string Input { get; set; }

        [Option("type", HelpText = "Input file type (default CSV)", DefaultValue = InputFileType.CSV)]
        public InputFileType Type { get; set; }

        [Option("destination", HelpText = "Destination, can be a table or a view", Required = true)]
        public string Destination { get; set; }

        [Option("separator", HelpText = "Separator character for CSV files (default ',')", DefaultValue = ",")]
        public string Separator { get; set; }

        [Option("has-headers", HelpText = "Set to indicate that CSV has headers row")]
        public bool HasHeaders { get; set; }
    }

    class Options
    {
        //public Options() {
        //    // Since we create this instance the parser will not overwrite it
        //    QueryVerb = new QuerySubOptions();
        //    TestVerb = new CommonSubOptions();
        //}

        [VerbOption("bulk", HelpText = "Bulk insert to SQL Database")]
        public BulkSubOptions BulkVerb { get; set; }

        [VerbOption("query", HelpText = "Execute a query")]
        public QuerySubOptions QueryVerb { get; set; }

        [VerbOption("test", HelpText = "Perform connection test")]
        public CommonSubOptions TestVerb { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
