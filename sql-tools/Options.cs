using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql_tools
{
    class CommonSubOptions
    {
        [Option('h', "host", HelpText = "Host address")]
        public string Host { get; set; }

        [Option('d', "database", HelpText = "Database")]
        public string Database { get; set; }

        [Option('u', "user", HelpText = "Username")]
        public string User { get; set; }

        [Option('p', "password", HelpText = "Password")]
        public string Password { get; set; }
    }

    class QuerySubOptions : CommonSubOptions
    {
        [Option('q', "query", HelpText = "Query to be executed", MutuallyExclusiveSet = "query")]
        public string Query { get; set; }

        [Option('f', "file", HelpText = "Text file containing the query", MutuallyExclusiveSet = "query")]
        public string File { get; set; }
    }

    class Options
    {
        //public Options() {
        //    // Since we create this instance the parser will not overwrite it
        //    QueryVerb = new QuerySubOptions();
        //    TestVerb = new CommonSubOptions();
        //}

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
