using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace HastConsole
{
    class Options : CommandLineOptionsBase
    {
        [Option("i", "input", Required = true, HelpText = "Path to the input file to transpile.")]
        public string InputFilePath { get; set; }

        // For future use
        [Option("f", "full", HelpText = "Full transpiling.")]
        public bool Full { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
