using ActivityDiagram.Writers.Graphml.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Writers.Graphml.Model
{
    internal class ArrowDiagramGraphmlXmlDocumentBuilder
    {
        private List<graphmlGraphEdge> edges;
        private List<graphmlGraphNode> nodes;

        public ArrowDiagramGraphmlXmlDocumentBuilder()
        {
            this.edges = new List<graphmlGraphEdge>();
            this.nodes = new List<graphmlGraphNode>();
        }

        public void AddNode(int id, GraphmlNodeType type, string label = null)
        {
            var nodeId = FormatNodeId(id);

            switch (type)
            {
                case GraphmlNodeType.Normal:
                    nodes.Add(GraphmlNodeBuilder.BuildNormal(nodeId));
                    break;
                case GraphmlNodeType.Milestone:
                    nodes.Add(GraphmlNodeBuilder.BuildMilestone(nodeId, label));
                    break;
                case GraphmlNodeType.GraphEnd:
                case GraphmlNodeType.GraphStart:
                    nodes.Add(GraphmlNodeBuilder.BuildTerminator(nodeId));
                    break;
                default: break;
            }
        }

        public void AddEdge(int id, int sourceNodeId, int targetNodeId, GraphmlEdgeType type, string label = null)
        {
            var edgeId = FormatEdgeId(id);
            var stringSourceNodeId = FormatNodeId(sourceNodeId);
            var stringTargetNodeId = FormatNodeId(targetNodeId);

            switch (type)
            {
                case GraphmlEdgeType.Activity:
                    edges.Add(GraphmlEdgeBuilder.BuildActivity(edgeId, stringSourceNodeId, stringTargetNodeId, label));
                    break;
                case GraphmlEdgeType.Dummy:
                    edges.Add(GraphmlEdgeBuilder.BuildDummy(edgeId, stringSourceNodeId, stringTargetNodeId));
                    break;
                default: break;
            }

        }

        private string FormatNodeId(int id)
        {
            return String.Format("n{0}", id);
        }

        private string FormatEdgeId(int id)
        {
            return String.Format("e{0}", id);
        }

        public graphml Build()
        {
            var graph = new graphmlGraph();
            graph.id = "G";
            graph.edgedefault = "directed";
            graph.node = nodes.ToArray();
            graph.edge = edges.ToArray();

            return BuildGraphmlInternal(graph);
        }

        private graphml BuildGraphmlInternal(graphmlGraph graph)
        {
            return new graphml()
            {
                Items = new object[]
                {
                    new graphmlKey() { @for = "node", id = "d6", yfilestype = "nodegraphics" },
                    new graphmlKey() { @for = "edge", id = "d10", yfilestype = "edgegraphics" },
                    graph
                }
            };
        }
    }
}
