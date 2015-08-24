using ActivityDiagram.Contracts.Model.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Contracts.Model.Graph
{
    public class ActivityEdge
    {
        public readonly EventVertex Source;
        public readonly EventVertex Target;
        public readonly Activity Activity;

        public ActivityEdge(EventVertex source, EventVertex target) : this (source, target, null) {}

        public ActivityEdge(EventVertex source, EventVertex target, Activity activity)
        {
            this.Source = source;
            this.Target = target;
            this.Activity = activity;
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
