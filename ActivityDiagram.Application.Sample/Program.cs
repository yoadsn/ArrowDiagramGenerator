using ActivityDiagram.Generator;
using ActivityDiagram.Readers.CSV;
using ActivityDiagram.Writers.Graphml;
using ActivityDiagram.Writers.Graphviz;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Application.Sample
{
    class Program
    {
        static int Main(string[] args)
        {
            return ConsoleCommandDispatcher.DispatchCommand(GetCommands(), args, Console.Out);
        }

        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}
