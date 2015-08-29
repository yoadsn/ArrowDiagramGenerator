using ActivityDiagram.Contracts;
using ActivityDiagram.Contracts.Model.Graph;
using ActivityDiagram.Writers.Graphml.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ActivityDiagram.Writers.Graphml
{
    public class GraphmlArrowGraphWriter : IArrowGraphWriter
    {
        private readonly XmlSerializer xmlSerializer;
        private readonly string outputFilename;

        public GraphmlArrowGraphWriter(string outputFilename)
        {
            xmlSerializer = new XmlSerializer(typeof(graphml));
            this.outputFilename = outputFilename;
        }

        public void Write(ActivityArrowGraph graph)
        {
            using (var streamWriter = new StreamWriter(outputFilename))
            {
                ArrowDiagramGraphmlXmlDocumentBuilder graphmlXmlDocumentBuilder = new ArrowDiagramGraphmlXmlDocumentBuilder();

                foreach (var vertex in graph.Vertices)
                {
                    switch (vertex.Type)
                    {
                        case EventVertexType.Normal:
                            graphmlXmlDocumentBuilder.AddNode(vertex.Id, GraphmlNodeType.Normal);
                            break;
                        case EventVertexType.Milestone:
                            graphmlXmlDocumentBuilder.AddNode(vertex.Id, GraphmlNodeType.Milestone, vertex.MilestoneActivity.Id.ToString());
                            break;
                        case EventVertexType.GraphStart:
                            graphmlXmlDocumentBuilder.AddNode(vertex.Id, GraphmlNodeType.GraphStart);
                            break;
                        case EventVertexType.GraphEnd:
                            graphmlXmlDocumentBuilder.AddNode(vertex.Id, GraphmlNodeType.GraphEnd);
                            break;
                        default: break;

                    }
                }

                foreach (var edge in graph.Edges)
                {
                    if (edge.Activity != null)
                    {
                        graphmlXmlDocumentBuilder.AddEdge(edge.Id, edge.Source.Id, edge.Target.Id, GraphmlEdgeType.Activity, edge.Id.ToString());
                    }
                    else
                    {
                        graphmlXmlDocumentBuilder.AddEdge(edge.Id, edge.Source.Id, edge.Target.Id, GraphmlEdgeType.Dummy);
                    }
                }

                xmlSerializer.Serialize(streamWriter, graphmlXmlDocumentBuilder.Build());
                streamWriter.Close();
            }
        }
    }
}
