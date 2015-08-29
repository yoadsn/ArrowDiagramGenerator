using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Writers.Graphml.Model
{
    public class GraphmlNodeBuilder
    {
        public static graphmlGraphNode BuildNormal(string id)
        {
            var node = new graphmlGraphNode();
            node.id = id;
            node.data = new data()
            {
                key = "d6",
                ShapeNode = new ShapeNode()
                {
                    Geometry = new ShapeNodeGeometry() { height = "30.0", width = "30.0" },
                    Fill = new ShapeNodeFill() { hasColor = "false", transparent = "false" },
                    BorderStyle = new ShapeNodeBorderStyle() { color = "#000000", type = "line", width = "1.0" },
                    Shape = new ShapeNodeShape() { type = "ellipse" },
                    NodeLabel = new ShapeNodeNodeLabel()
                    {
                        alignment = "center",
                        autoSizePolicy = "content",
                        fontFamily = "Dialog",
                        fontSize = "12",
                        fontStyle = "plain",
                        hasBackgroundColor = "false",
                        hasLineColor = "false",
                        hasText = "false",
                        height = "4.0",
                        modelName = "custom",
                        textColor = "#000000",
                        visible = "true",
                        width = "4.0",
                        x = "13.0",
                        y = "13.0",
                        LabelModel = new ShapeNodeNodeLabelLabelModel() { SmartNodeLabelModel = new ShapeNodeNodeLabelLabelModelSmartNodeLabelModel() { distance = "4.0" } },
                        ModelParameter = new ShapeNodeNodeLabelModelParameter() { SmartNodeLabelModelParameter = new ShapeNodeNodeLabelModelParameterSmartNodeLabelModelParameter() { labelRatioX = "0.0", labelRatioY = "0.0", nodeRatioX = "0.0", nodeRatioY = "0.0", offsetX = "0.0", offsetY = "0.0", upX = "0.0", upY = "-1.0" } }
                    }
                }

            };

            return node;
        }

        public static graphmlGraphNode BuildTerminator(string id)
        {
            var node = BuildNormal(id);
            node.data.ShapeNode.Fill.hasColor = "true";
            node.data.ShapeNode.Fill.color = "#000000";

            return node;
        }

        public static graphmlGraphNode BuildMilestone(string id, string label)
        {
            var node = BuildNormal(id);
            node.data.ShapeNode.NodeLabel.Text = label;
            node.data.ShapeNode.NodeLabel.hasText = "true";

            return node;
        }
    }
}
