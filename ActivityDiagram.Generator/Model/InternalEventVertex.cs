using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Generator.Model
{
    internal class InternalEventVertex
    {
        private readonly string id;
        private readonly int activityId;
        private readonly ActivityVertexType type;
        private readonly bool critical;

        private InternalEventVertex(string id, int activityId, ActivityVertexType type, bool critical)
        {
            this.activityId = activityId;
            this.type = type;
            this.critical = critical;
            this.id = id;
        }

        public string Id
        {
            get
            {
                return this.ToString();
            }
        }

        public int ActivityId
        {
            get
            {
                return this.activityId;
            }
        }

        public ActivityVertexType Type
        {
            get
            {
                return this.type;
            }
        }

        public bool IsCritical
        {
            get
            {
                return this.critical;
            }
        }

        public override bool Equals(Object obj)
        {
            return obj is InternalEventVertex && this == (InternalEventVertex)obj;
        }
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        public static bool operator ==(InternalEventVertex x, InternalEventVertex y)
        {
            return x.id == y.id;
        }
        public static bool operator !=(InternalEventVertex x, InternalEventVertex y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return this.id;
        }

        public static InternalEventVertex Create(int activityId, ActivityVertexType type, bool critical)
        {
            return new InternalEventVertex(FormatId(activityId, type), activityId, type, critical);
        }

        private static string FormatId(int activityId, ActivityVertexType type)
        {
            if (type == ActivityVertexType.ActivityStart)
            {
                return "S" + activityId;
            }
            else
            {
                return "E" + activityId;
            }
        }
    }

    public enum ActivityVertexType
    {
        ActivityStart,
        ActivityEnd
    }
}
