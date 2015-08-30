using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model
{
    internal struct ADVertex
    {
        private readonly int? activityId;
        private readonly ActivityVertexType type;
        private readonly bool critical;

        public ADVertex(int? activityId, ActivityVertexType type, bool critical)
        {
            this.activityId = activityId;
            this.type = type;
            this.critical = critical;
        }

        public string Id
        {
            get
            {
                return this.ToString();
            }
        }

        public int? ActivityId
        {
            get
            {
                return this.ActivityId;
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
            return obj is ADVertex && this == (ADVertex)obj;
        }
        public override int GetHashCode()
        {
            return activityId.GetHashCode() ^ type.GetHashCode();
        }
        public static bool operator ==(ADVertex x, ADVertex y)
        {
            return x.activityId == y.activityId && x.type == y.type;
        }
        public static bool operator !=(ADVertex x, ADVertex y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return this.FormatId();
        }

        private string FormatId()
        {
            Debug.Assert(this.activityId.HasValue);

            if (this.type == ActivityVertexType.ActivityStart)
            {
                return "S" + this.activityId.Value;
            }
            else if (this.type == ActivityVertexType.ActivityEnd)
            {
                return "E" + this.activityId.Value;
            }
            else
            {
                throw new FormatException("Verex with an activity ID must be the start or end of an activity.");
            }
        }

        public static ADVertex New(int? activityId, ActivityVertexType type, bool critical)
        {
            return new ADVertex(activityId, type, critical);
        }
    }

    public enum ActivityVertexType
    {
        ActivityStart,
        ActivityEnd,
        Milestone
    }
}
