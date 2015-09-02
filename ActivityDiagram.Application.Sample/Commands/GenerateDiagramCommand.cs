using ActivityDiagram.Contracts;
using ActivityDiagram.Generator;
using ActivityDiagram.Readers.CSV;
using ActivityDiagram.Readers.Mpp;
using ActivityDiagram.Writers.Graphml;
using ActivityDiagram.Writers.Graphviz;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Application.Sample.Commands
{
    class GenerateDiagramCommand : ConsoleCommand
    {
        public GenerateDiagramCommand()
        {
            this.IsCommand("gen", "Genreates an arrow diagram from an activity dependency graph.");

            this.HasOption("it|intype=", "The file type of the input activity dependencies file. Available types: csv, mpp. default: csv", s => { inputType = s ?? "csv"; });
            this.HasOption("ot|outtype=", "The file type of the output arrow diagram. Available types: graphml, dot. default: graphml", s => { outputType = s ?? "graphml"; });
            this.HasOption("o|output=", "The output file name. default: '<intput file>.out.type'", s => { outputFile = s ?? ""; });
            
            this.HasAdditionalArguments(1, "<input file>");
        }

        private string inputType = "csv";
        private string inputFile;
        private string outputType = "graphml";
        private string outputFile;
        public override int Run(string[] remainingArguments)
        {
            this.CheckRequiredArguments();
            this.inputType = this.inputType.ToLower();
            this.outputType = this.outputType.ToLower();

            inputFile = remainingArguments[0];
            if (String.IsNullOrEmpty(inputFile))
            {
                throw new ConsoleHelpAsException(String.Format("The input file name '{0}' is not valid", inputFile));
            }
            

            if (String.IsNullOrEmpty(outputFile))
            {
                outputFile = inputFile + ".out." + outputType;
            }
            

            var reader = GetReaderForType(inputType, inputFile);
            Console.WriteLine("Using activities input file {0}", inputFile);

            var writer = GetWriterForType(outputType, outputFile);
            Console.WriteLine("Using arrow diagram output file {0}", outputFile);

            if (writer != null && reader != null)
            {
                try
                {
                    CreateArrowDiagram(reader, writer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to generate the diagram. Exception:\n{0}", ex.ToString());
                    return -1;
                }
            }
            
            return 0;
        }

        private void CreateArrowDiagram(IActivitiesReader reader, IArrowGraphWriter writer)
        {

            Console.WriteLine("Reading activities...", inputFile);
            var activities = reader.Read();
            var graphGenerator = new ActivityArrowGraphGenerator(activities);
            Console.WriteLine("Generating Graph...", outputFile);
            var arrowGraph = graphGenerator.GenerateGraph();
            Console.WriteLine("Writing Graph...", inputFile);
            writer.Write(arrowGraph);
            Console.WriteLine("Done.", outputFile);
        }

        private IActivitiesReader GetReaderForType(string type, string intputFile)
        {
            try
            {
                switch (type)
                {
                    case "csv":
                        return GetCsvReader(inputFile);
                    case "mpp":
                        return GetMppReader(inputFile);
                    default:
                        throw new ConsoleHelpAsException(String.Format("The input type {0} is not supported", type));
                }
            }
            catch (ConsoleHelpAsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create reader. Exception:\n{0}", ex.ToString());
            }

            return null;
        }

        private IActivitiesReader GetMppReader(string inputFile)
        {
            return new MppActivitiesReader(inputFile);
        }

        private IActivitiesReader GetCsvReader(string inputFile)
        {
            return new CSVActivitiesReader(inputFile);
        }

        IArrowGraphWriter GetWriterForType(string type, string outputFile)
        {
            try
            {
                switch (type)
                {
                    case "graphml":
                        return GetGraphMLWriter(outputFile);
                    case "dot":
                        return GetGraphVizWriter(outputFile);
                    default:
                        throw new ConsoleHelpAsException(String.Format("The output type {0} is not supported", outputType));
                }
            }
            catch (ConsoleHelpAsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create writer. Exception:\n{0}", ex.ToString());
            }

            return null;
        }

        private IArrowGraphWriter GetGraphVizWriter(string outputFile)
        {
            return new GraphvizArrowGraphWriter(outputFile);
        }

        private IArrowGraphWriter GetGraphMLWriter(string outputFile)
        {
            return new GraphmlArrowGraphWriter(outputFile);
        }
    }
}
