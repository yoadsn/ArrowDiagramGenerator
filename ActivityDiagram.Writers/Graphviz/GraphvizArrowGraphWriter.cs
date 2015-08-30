using ActivityDiagram.Contracts;
using ActivityDiagram.Contracts.Model.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Writers.Graphviz
{
    public class GraphvizArrowGraphWriter : IArrowGraphWriter
    {
        private readonly string filename;
        public GraphvizArrowGraphWriter(string filename)
        {
            this.filename = filename;
        }

        public void Write(ActivityArrowGraph graph)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("digraph G {\n");
            foreach (var vertex in graph.Vertices)
            {
                sb.AppendFormat("{0} [ label=\"\" ];\n", vertex.Id);
            }

            foreach (var edge in graph.Edges)
            {
                if (edge.Activity != null)
                {
                    var penWidth = "1";
                    if (edge.IsCritical)
                    {
                        penWidth = "3";
                    }
                    
                    sb.AppendFormat("{0} -> {1} [ id={2} label={2} penwidth=\"{3}\" ];\n", edge.Source.Id, edge.Target.Id, edge.Activity.Id, penWidth);
                    
                }
                else
                {
                    sb.AppendFormat("{0} -> {1} [ style=dashed ];\n", edge.Source.Id, edge.Target.Id);
                }
            }
            sb.Append("}");

            using (var fwriter = File.Create(filename))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
                fwriter.Write(info, 0, info.Length);
            }
        }
    }
}
