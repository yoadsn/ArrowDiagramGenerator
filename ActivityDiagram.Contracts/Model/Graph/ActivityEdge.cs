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
        public readonly int Id;
        public readonly EventVertex Source;
        public readonly EventVertex Target;
        public readonly Activity Activity;

        public ActivityEdge(int id, EventVertex source, EventVertex target) : this (id, source, target, null) {}

        public ActivityEdge(int id, EventVertex source, EventVertex target, Activity activity)
        {
            this.Id = id;
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
            return Id.GetHashCode();
        }
        public static bool operator ==(ActivityEdge x, ActivityEdge y)
        {
            return x.Id == y.Id;
        }
        public static bool operator !=(ActivityEdge x, ActivityEdge y)
        {
            return !(x == y);
        }
    }
}
