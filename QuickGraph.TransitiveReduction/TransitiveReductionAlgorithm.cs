using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickGraph.Algorithms
{
    using QuickGraph.Algorithms.Observers;
    using QuickGraph.Algorithms.Search;
    using QuickGraph.Algorithms.TopologicalSort;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class TransitiveReductionAlgorithm<TVertex, Edge> : AlgorithmBase<BidirectionalGraph<TVertex, Edge>> where Edge : IEdge<TVertex>
    {
        private BidirectionalGraph<TVertex, Edge> transitiveReduction;

        public TransitiveReductionAlgorithm(
            BidirectionalGraph<TVertex, Edge> visitedGraph
            )
            : base(visitedGraph)
        {
            transitiveReduction = new BidirectionalGraph<TVertex, Edge>();
        }

        public BidirectionalGraph<TVertex, Edge> TransitiveReduction
        {
            get
            {
                return transitiveReduction;
            }
        }

        protected override void InternalCompute()
        {
            // Clone the visited graph
            transitiveReduction.AddVerticesAndEdgeRange(this.VisitedGraph.Edges);

            // Get the topological sorted graph
            var topoSort = this.VisitedGraph.TopologicalSort();

            // Iterate in topo order, track indirect ancestors and remove edges from them to the visited vertex
            var ancestorsOfVertices = new Dictionary<TVertex, HashSet<TVertex>>();
            foreach (var vertexId in this.VisitedGraph.TopologicalSort())
            {
                var thisVertexPredecessors = new List<TVertex>();
                var thisVertexAncestors = new HashSet<TVertex>();
                ancestorsOfVertices[vertexId] = thisVertexAncestors;

                // Get indirect ancestors
                foreach (var inEdge in this.VisitedGraph.InEdges(vertexId))
                {
                    var predecessor = inEdge.Source;
                    thisVertexPredecessors.Add(predecessor);

                    // Add all the ancestors of the predeccessors
                    foreach (var ancestorId in ancestorsOfVertices[predecessor])
                    {
                        thisVertexAncestors.Add(ancestorId);
                    }
                }

                // Remove indirect edges
                foreach (var indirectAncestor in thisVertexAncestors)
                {
                    Edge foundIndirectEdge;
                    if (transitiveReduction.TryGetEdge(indirectAncestor, vertexId, out foundIndirectEdge))
                    {
                        transitiveReduction.RemoveEdge(foundIndirectEdge);
                    }
                }

                // Add predecessors to ancestors list
                foreach (var pred in thisVertexPredecessors)
                {
                    thisVertexAncestors.Add(pred);
                }
            }
        }
    }
}
