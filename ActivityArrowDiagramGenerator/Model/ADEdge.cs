using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model
{
    [DebuggerDisplay("{Source}->{Target}")]
    internal struct ADEdge : IEdge<ADVertex>
    {
        private readonly ADVertex source;
        private readonly ADVertex target;
        private readonly int? activityId;
        private readonly bool isCritical;

        public ADEdge(ADVertex source, ADVertex target, int? activityId = null, bool forceCritical = false)
        {
            this.source = source;
            this.target = target;
            this.activityId = activityId;
            this.isCritical = (source.IsCritical && target.IsCritical) || forceCritical;
        }

        public ADVertex Source { get { return this.source; } }
        public ADVertex Target { get { return this.target; } }
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
