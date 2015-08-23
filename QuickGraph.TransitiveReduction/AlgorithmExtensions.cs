using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickGraph.Algorithms
{
    public static class AlgorithmExtensions
    {
        public static BidirectionalGraph<TVertex, Edge> ComputeTransitiveReduction<TVertex, Edge>(
            this BidirectionalGraph<TVertex, Edge> visitedGraph
            ) where Edge : IEdge<TVertex>
        {
            var algo = new TransitiveReductionAlgorithm<TVertex, Edge>(visitedGraph);
            algo.Compute();
            return algo.TransitiveReduction;
        }
    }
}
