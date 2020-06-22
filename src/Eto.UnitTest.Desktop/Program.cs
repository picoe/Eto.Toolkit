using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;

namespace Eto.UnitTest.App
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // options
            var assemblyOption = new Option(new[] { "-a", "--assembly" }, "The assembly(ies) to test.")
            {
                Argument = new Argument<FileInfo[]> { Arity = ArgumentArity.ZeroOrMore }
            };

            // Add them to the root command
            var rootCommand = new RootCommand { Description = "Eto.UnitTest" };
            rootCommand.AddOption(assemblyOption);


            var result = rootCommand.Parse(args);
            FileInfo[] assemblies = null;
            if (result.HasOption(assemblyOption))
            {
                assemblies = result.FindResultFor(assemblyOption)?.GetValueOrDefault<FileInfo[]>();
            }

            new Application().Run(new MainForm(assemblies));
            return 0;
        }
    }
}
