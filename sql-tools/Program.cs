using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sql_tools
{
    class Options
    {
        [Option('v', "version", HelpText = "Display version number")]
        public bool Version { get; set; }

        [HelpOption('h', "help")]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.Version)
                {
                    Console.WriteLine("MSSQL Tools version 1.0");
                }
            }
        }
    }
}
