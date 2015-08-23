using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model.Graph
{
    public class ActivityArrowGraph
    {
        private readonly Dictionary<ActivityEdge, ActivityEdge> edges;

        public int EdgeCount
        {
            get
            {
                return this.edges.Count;
            }
        }

        public IEnumerable<ActivityEdge> Edges
        {
            get
            {
                return this.edges.Keys;
            }
        }

        public bool ContainsEdge(ActivityEdge edge)
        {
            return this.edges.ContainsKey(edge);
        }

        public bool AddEdge(ActivityEdge edge)
        {
            if (this.ContainsEdge(edge))
                return false;
            this.edges.Add(edge, edge);
            return true;
        }

        public int AddEdgeRange(IEnumerable<ActivityEdge> edges)
        {
            int count = 0;
            foreach (var edge in edges)
                if (this.AddEdge(edge))
                    count++;
            return count;
        }

        public bool RemoveEdge(ActivityEdge edge)
        {
            if (this.edges.Remove(edge))
            {
                return true;
            }
            else
                return false;
        }

        public void Clear()
        {
            this.edges.Clear();   
        }
    }
}
