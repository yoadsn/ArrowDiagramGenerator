using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model.Graph
{
    public class ActivityArrowGraph
    {
        private readonly HashSet<EventVertex> vertice;
        private readonly HashSet<ActivityEdge> edges;

        public void AddActivityEdge(ActivityEdge edge)
        {
            if (vertice.Contains(edge.Source) && vertice.Contains(edge.Target))
            {
                edges.Add(edge);
            }
            else
            {
                throw new KeyNotFoundException("Edge uses a vertex which does not exist");
            }
        }

        public void AddEventVertex(EventVertex targetVertex)
        {
            vertice.Add(targetVertex);
        }
    }
}
