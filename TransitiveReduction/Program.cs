using ActivityArrowDiagramGenerator;
using ActivityArrowDiagramGenerator.Model;
using ActivityDiagram.Contracts.Model.Activities;
using ActivityDiagram.Readers.CSV;
using ActivityDiagram.Writers.Graphml;
using ActivityDiagram.Writers.Graphviz;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using net.sf.mpxj;
using net.sf.mpxj.mpp;
using net.sf.mpxj.MpxjUtilities;
using net.sf.mpxj.reader;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz;
using QuickGraph.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TransitiveReduction
{
    class Program
    {
        static Form f;
        static BidirectionalGraph<int, SEdge<int>> graph;
        //static BidirectionalGraph<ADVertex, ADEdge<ADVertex>> adGraph;

        private static void listHierarchy(ProjectFile file)
        {
            foreach (net.sf.mpxj.Task task in file.ChildTasks.ToIEnumerable())
            {
                System.Console.WriteLine("Task: " + task.Name);
                listHierarchy(task, " ");
            }

            System.Console.WriteLine();
        }

        private static void listHierarchy(net.sf.mpxj.Task task, String indent)
        {
            foreach (net.sf.mpxj.Task child in task.ChildTasks.ToIEnumerable())
            {
                System.Console.WriteLine(indent + "Task: " + child.Name);
                listHierarchy(child, indent + " ");
            }
        }

        static void Main(string[] args)
        {
            #region Debug Data
            var edges = new SEdge<int>[] {
                new SEdge<int>(1 ,2),
                new SEdge<int>(2 ,3),
                new SEdge<int>(3 ,8),
                new SEdge<int>(3 ,6),
                new SEdge<int>(3 ,10),
                new SEdge<int>(3 ,7),
                new SEdge<int>(3 ,9),
                new SEdge<int>(6 ,12),
                new SEdge<int>(10 ,12),
                new SEdge<int>(3 ,4),
                new SEdge<int>(6 ,11),
                new SEdge<int>(9 ,11),
                new SEdge<int>(6 ,13),
                new SEdge<int>(6 ,16),
                new SEdge<int>(12 ,14),
                new SEdge<int>(13 ,14),
                new SEdge<int>(12 ,15),
                new SEdge<int>(13 ,15),

                new SEdge<int>(4 ,5),
                
                new SEdge<int>(7 ,17),
                new SEdge<int>(8 ,17),
                new SEdge<int>(11 ,17),
                new SEdge<int>(14 ,17),
                new SEdge<int>(15 ,17),

                new SEdge<int>(7 ,18),
                new SEdge<int>(8 ,18),
                new SEdge<int>(15 ,18),
                new SEdge<int>(16 ,18),

                new SEdge<int>(17 ,19),
                new SEdge<int>(18 ,19),

                new SEdge<int>(17 ,20),
                
                new SEdge<int>(5 ,21),
                new SEdge<int>(19 ,21),
                new SEdge<int>(20 ,21)
            };

            graph = edges.ToBidirectionalGraph<int, SEdge<int>>();
            #endregion

            /*
            ProjectReader reader = ProjectReaderUtility.getProjectReader("example3.mpp");
            ProjectFile mpx = reader.read("example3.mpp");

            var edges = new List<SEdge<int>>();
            foreach (net.sf.mpxj.Task task in mpx.AllTasks.ToIEnumerable())
            {
                var id = task.ID.intValue();
                var preds = task.Predecessors;
                if (preds != null && !preds.isEmpty())
                {
                    foreach (Relation pred in preds.ToIEnumerable())
                    {
                        var edge = new SEdge<int>(pred.TargetTask.ID.intValue(), pred.SourceTask.ID.intValue());
                        edges.Add(edge);
                    }
                }
            }

            graph = edges.ToBidirectionalGraph<int, SEdge<int>>();
            */

            

            var csvReader = new CSVActivitiesReader("example3.csv");
            var deps = csvReader.Read().ToList();

            //var activityDependencies = graph.Vertices.Select(vert => new ActivityDependency(new Activity(vert), graph.InEdges(vert).Select(edge => edge.Source).ToList()));
            var adgraphgenerator = new ActivityArrowGraphGenerator(deps);
            var adgraph = adgraphgenerator.GenerateGraph();

            var gwriter = new GraphmlArrowGraphWriter("example3.graphml");
            gwriter.Write(adgraph);

            var gvwriter = new GraphvizArrowGraphWriter("example3.dot");
            gvwriter.Write(adgraph);

            /*
            f = new Form();
            f.Width = 700;
            f.Height = 1024;
            
            PictureBox pb = new PictureBox(){ Dock = DockStyle.Fill};
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            
            pb.Top = 0;
            pb.Left = 0;

            f.Controls.Add(pb);
            f.Show();

            f.FormClosed += f_FormClosed;
            pb.Click += pb_Click;

            Application.Run();
            */
        }

        static int step = 0;
        static void pb_Click(object sender, EventArgs e)
        {
            bool shouldExit = false;
            MemoryStream memstream = null;

            if (step == 0)
            {
                var outFileData = OutputToDotFile(graph);
                memstream = new MemoryStream(outFileData);

                var serialzier = new GraphMLSerializer<int, SEdge<int>, BidirectionalGraph<int, SEdge<int>>>();
                using (var xwriter = XmlWriter.Create("out.graphml"))
                {
                    serialzier.Serialize(xwriter, graph, (int v) => v.ToString(), (SEdge<int> edge) => String.Format("{0}-{1}", edge.Source.ToString(), edge.Target.ToString()));
                }
            }
            else if (step == 1)
            {
                var algo = new TransitiveReductionAlgorithm<int, SEdge<int>>(graph);
                algo.Compute();
                graph = algo.TransitiveReduction;

                //var serialzier = new GraphMLSerializer<int, SEdge<int>, BidirectionalGraph<int, SEdge<int>>>();
                //using (var xwriter = XmlWriter.Create("out.graphml"))
                //{
                //serialzier.Serialize(xwriter, transitiveClosure, (int v) => v.ToString(), (SEdge<int> edge) => String.Format("{0}-{1}", edge.Source.ToString(), edge.Target.ToString()));
                //}

                var outFileData = OutputToDotFile(graph);
                memstream = new MemoryStream(outFileData);
            }
            else if (step == 2)
            {
                //adGraph = GenerateADGraph(graph);

                //var outFileData = OutputToDotFile(adGraph);
                //memstream = new MemoryStream(outFileData);
                // 
            }
            else if (step == 3)
            {
                //RedirectADGraph(adGraph);

                //var outFileData = OutputToDotFile(adGraph);
                //memstream = new MemoryStream(outFileData);
            }
            else if (step == 4)
            {
                //MergeADGraph(adGraph);

                //var outFileData = OutputToDotFile(adGraph);
                //memstream = new MemoryStream(outFileData);
            }
            else
            {
                shouldExit = true;
            }

            if (memstream != null)
            {
                var pb = f.Controls[0] as PictureBox;
                pb.Image = new Bitmap(memstream);
            }
            step++;


            if (shouldExit) f.Close();
        }

        static void f_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        public static byte[] OutputToDotFile(BidirectionalGraph<int, SEdge<int>> graph)
        {
            byte[] result = null;
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery, getProcessStartInfoQuery, registerLayoutPluginCommand);

            StringBuilder sb = new StringBuilder();

            sb.Append("digraph G {\n");
            foreach (var vertex in graph.Vertices)
            {
                sb.AppendFormat("{0} ;\n", vertex);
            }

            foreach (var edge in graph.Edges)
            {
                sb.AppendFormat("{0} -> {1} ;\n", edge.Source, edge.Target);
            }
            sb.Append("}");

            using (var fwriter = File.Create("out.dot"))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
                fwriter.Write(info, 0, info.Length);  
            }

            result = wrapper.GenerateGraph(sb.ToString(), Enums.GraphReturnType.Png);

            return result;
        }

        /*
        public static byte[] OutputToDotFile(BidirectionalGraph<ADVertex, ADEdge<ADVertex>> graph)
        {
            byte[] result = null;
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery, getProcessStartInfoQuery, registerLayoutPluginCommand);

            StringBuilder sb = new StringBuilder();

            sb.Append("digraph G {\n");
            foreach (var vertex in graph.Vertices)
            {
                sb.AppendFormat("{0} [ id={1} ];\n", vertex, vertex.ToString());
            }

            foreach (var edge in graph.Edges)
            {
                if (edge.ActivityId.HasValue)
                {
                    sb.AppendFormat("{0} -> {1} [ id={2} label={2} ];\n", edge.Source, edge.Target, edge.ActivityId);
                }
                else
                {
                    sb.AppendFormat("{0} -> {1} [ style=dashed ];\n", edge.Source, edge.Target);
                }
            }
            sb.Append("}");

            using (var fwriter = File.Create("out" + step.ToString() + ".dot"))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
                fwriter.Write(info, 0, info.Length);
            }

            result = wrapper.GenerateGraph(sb.ToString(), Enums.GraphReturnType.Png);

            return result;
        }
        */
    }
}
