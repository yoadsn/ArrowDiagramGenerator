﻿using ActivityDiagram.Contracts;
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

        public GraphmlArrowGraphWriter(string filename)
        {
            xmlSerializer = new XmlSerializer(typeof(graphml));
            this.outputFilename = filename;
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
                        var edgeType = GraphmlEdgeType.Activity;
                        if (edge.IsCritical)
                        {
                            edgeType = GraphmlEdgeType.CriticalActivity;
                        }

                        graphmlXmlDocumentBuilder.AddEdge(edge.Id, edge.Source.Id, edge.Target.Id, edgeType, edge.Activity.Id.ToString());
                    }
                    else
                    {
                        var edgeType = GraphmlEdgeType.Dummy;
                        if (edge.IsCritical)
                        {
                            edgeType = GraphmlEdgeType.CriticalDummy;
                        }

                        graphmlXmlDocumentBuilder.AddEdge(edge.Id, edge.Source.Id, edge.Target.Id, edgeType);
                    }
                }

                xmlSerializer.Serialize(streamWriter, graphmlXmlDocumentBuilder.Build());
                streamWriter.Close();
            }
        }
    }
}
