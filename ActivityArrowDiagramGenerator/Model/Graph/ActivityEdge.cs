using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model.Graph
{
    public class ActivityEdge
    {
        public EventVertex Source { get; private set; }
        public EventVertex Target { get; private set; }

        public ActivityEdge(EventVertex source, EventVertex target, int? activityId, Dictionary<string, string> attributes)
        {

        }

        public override bool Equals(Object obj)
        {
            return obj is ActivityEdge && this == (ActivityEdge)obj;
        }
        public override int GetHashCode()
        {
            return Source.GetHashCode() ^ Target.GetHashCode();
        }
        public static bool operator ==(ActivityEdge x, ActivityEdge y)
        {
            return x.Source == y.Source && x.Target == y.Target;
        }
        public static bool operator !=(ActivityEdge x, ActivityEdge y)
        {
            return !(x == y);
        }
    }
}
