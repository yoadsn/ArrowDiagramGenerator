namespace QuickGraph.Algorithms
{
    using QuickGraph.Algorithms.Observers;
    using QuickGraph.Algorithms.Search;
    using QuickGraph.Algorithms.TopologicalSort;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class TransitiveClosureAlgorithm : AlgorithmBase<BidirectionalGraph<int, SEdge<int>>>
    {
        private BidirectionalGraph<int, SEdge<int>> transitiveClosure;

        public TransitiveClosureAlgorithm(
            BidirectionalGraph<int, SEdge<int>> visitedGraph
            )
            : base(visitedGraph)
        {
            transitiveClosure = new BidirectionalGraph<int, SEdge<int>>();
        }

        public BidirectionalGraph<int, SEdge<int>> TransitiveClosure
        {
            get
            {
                return transitiveClosure;
            }
        }

        protected override void InternalCompute()
        {
            // Clone the visited graph
            transitiveClosure.AddVerticesAndEdgeRange(this.VisitedGraph.Edges);

            // Get the topological order
            var topoSort = new TopologicalSortAlgorithm<int, SEdge<int>>(this.VisitedGraph);
            topoSort.Compute();
            var sortedVertices = topoSort.SortedVertices;

            // Iterate in topo order, track indirect ancestors and remove edges from them
            var ancestorsOfVertices = new Dictionary<int, HashSet<int>>();
            foreach (var vertexId in sortedVertices)
            {
                var thisVertexPredecessors = new List<int>();
                var thisVertexAncestors = new HashSet<int>();
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
                    SEdge<int> foundIndirectEdge;
                    if (transitiveClosure.TryGetEdge(indirectAncestor, vertexId, out foundIndirectEdge))
                    {
                        transitiveClosure.RemoveEdge(foundIndirectEdge);
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
