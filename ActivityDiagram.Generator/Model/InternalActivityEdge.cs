using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Generator.Model
{
    [DebuggerDisplay("{Source}->{Target}")]
    internal class InternalActivityEdge : IEdge<InternalEventVertex>
    {
        private readonly InternalEventVertex source;
        private readonly InternalEventVertex target;
        private readonly int? activityId;
        private readonly bool isCritical;

        public InternalActivityEdge(InternalEventVertex source, InternalEventVertex target, bool isCritical, int? activityId = null)
        {
            this.source = source;
            this.target = target;
            this.activityId = activityId;
            this.isCritical = isCritical;
        }

        public InternalEventVertex Source { get { return this.source; } }
        public InternalEventVertex Target { get { return this.target; } }
        public int? ActivityId { get { return this.activityId; } }
        public bool IsCritical { get { return this.isCritical; } }

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
