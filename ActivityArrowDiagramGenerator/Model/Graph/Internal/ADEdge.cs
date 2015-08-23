using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model.Graph.Internal
{
    [DebuggerDisplay("{Source}->{Target}")]
    internal struct ADEdge<TVertex> : IEdge<TVertex>
    {
        private readonly TVertex source;
        private readonly TVertex target;
        private readonly int? activityId;

        public ADEdge(TVertex source, TVertex target, int? activityId = null)
        {
            this.source = source;
            this.target = target;
            this.activityId = activityId;
        }

        public TVertex Source { get { return this.source; } }
        public TVertex Target { get { return this.target; } }
        public int? ActivityId { get { return this.activityId; } }

        public override string ToString()
        {
            return String.Format(
                "{Source}-{ActivityId}->{Target}",
                this.Source,
                this.Target,
                this.ActivityId.HasValue ? "(" + this.ActivityId.Value.ToString() + ")" : "");
        }
    }
}
