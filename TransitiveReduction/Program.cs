using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz;
using QuickGraph.Serialization;
using System;
using System.Collections.Generic;
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

        static void Main(string[] args)
        {
            

            var edges = new SEdge<int>[] {
new SEdge<int>(11 ,13),
new SEdge<int>(13 ,28),
new SEdge<int>(13 ,18),
new SEdge<int>(12 ,22),
new SEdge<int>(13 ,20),
new SEdge<int>(25 ,27),
new SEdge<int>(19 ,27),
new SEdge<int>(5 ,23),
new SEdge<int>(5 ,6),
new SEdge<int>(8 ,23),
new SEdge<int>(26 ,29),
new SEdge<int>(8 ,15),
new SEdge<int>(6 ,17),
new SEdge<int>(26 ,30),
new SEdge<int>(20 ,27),
new SEdge<int>(7 ,19),
new SEdge<int>(4 ,15),
new SEdge<int>(2 ,19),
new SEdge<int>(11 ,12),
new SEdge<int>(27 ,28),
new SEdge<int>(9 ,19),
new SEdge<int>(2 ,26),
new SEdge<int>(27 ,30),
new SEdge<int>(8 ,9),
new SEdge<int>(7 ,10),
new SEdge<int>(11 ,24),
new SEdge<int>(5 ,19),
new SEdge<int>(1 ,3),
new SEdge<int>(21 ,28),
new SEdge<int>(4 ,14),
new SEdge<int>(9 ,20),
new SEdge<int>(24 ,28),
new SEdge<int>(11 ,15),
new SEdge<int>(1 ,30),
new SEdge<int>(5 ,25),
new SEdge<int>(1 ,9),
new SEdge<int>(26 ,27),
new SEdge<int>(20 ,30),
new SEdge<int>(3 ,28),
new SEdge<int>(9 ,16)
            };

            graph = edges.ToBidirectionalGraph<int, SEdge<int>>();

            /*
            var graph = new BidirectionalGraph<int, SEdge<int>>();
            var deserializer = new GraphMLDeserializer<int, SEdge<int>, BidirectionalGraph<int, SEdge<int>>>();

            using (var xreader = XmlReader.Create("in.graphml"))
            {
                deserializer.Deserialize(xreader, graph, (string i) => Int32.Parse(i), (int s, int t, string id) => new SEdge<int>(s, t));
            }
            */

            f = new Form();
            f.Width = 1024;
            f.Height = 500;
            
            PictureBox pb = new PictureBox(){ Dock = DockStyle.Fill};
            
            pb.Top = 0;
            pb.Left = 0;

            f.Controls.Add(pb);
            f.Show();

            f.FormClosed += f_FormClosed;
            pb.Click += pb_Click;

            Application.Run();
        }

        static int step = 0;
        static void pb_Click(object sender, EventArgs e)
        {
            MemoryStream memstream = null;

            if (step == 0)
            {
                var outFileData = OutputToDotFile(graph);
                memstream = new MemoryStream(outFileData);

                step = 1;
            }
            else if (step == 1)
            {
                var algo = new TransitiveClosureAlgorithm(graph);
                algo.Compute();
                var transitiveClosure = algo.TransitiveClosure;

                //var serialzier = new GraphMLSerializer<int, SEdge<int>, BidirectionalGraph<int, SEdge<int>>>();
                //using (var xwriter = XmlWriter.Create("out.graphml"))
                //{
                //serialzier.Serialize(xwriter, transitiveClosure, (int v) => v.ToString(), (SEdge<int> edge) => String.Format("{0}-{1}", edge.Source.ToString(), edge.Target.ToString()));
                //}

                var outFileData = OutputToDotFile(transitiveClosure);
                memstream = new MemoryStream(outFileData);

                step = 0;
            }

            var pb = f.Controls[0] as PictureBox;
            pb.Image = new Bitmap(memstream);
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
                if (graph.InDegree(vertex) > 1)
                {
                    sb.AppendFormat("{0} [style = filled color = red];\n", vertex);
                }
                else
                {
                    sb.AppendFormat("{0} ;\n", vertex);
                }
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
    }
}
