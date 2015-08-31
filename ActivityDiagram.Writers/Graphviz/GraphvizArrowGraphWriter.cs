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
                if (vertex.Type == EventVertexType.GraphEnd || vertex.Type == EventVertexType.GraphStart)
                {
                    sb.AppendFormat("{0} [ label=\"\" style=filled fillcolor=black ];\n", vertex.Id);
                }
                else
                {
                    sb.AppendFormat("{0} [ label=\"\" ];\n", vertex.Id);
                }
            }

            foreach (var edge in graph.Edges)
            {
                var penWidth = "1";
                if (edge.IsCritical)
                {
                    penWidth = "3";
                }

                if (edge.Activity != null)
                {
                    sb.AppendFormat("{0} -> {1} [ id={2} label={2} penwidth=\"{3}\" ];\n", edge.Source.Id, edge.Target.Id, edge.Activity.Id, penWidth);
                    
                }
                else
                {
                    sb.AppendFormat("{0} -> {1} [ style=dashed penwidth=\"{2}\" ];\n", edge.Source.Id, edge.Target.Id, penWidth);
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
