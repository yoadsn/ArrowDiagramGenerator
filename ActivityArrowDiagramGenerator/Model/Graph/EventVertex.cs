using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model.Graph
{
    public class EventVertex
    {
        public readonly string Id;
        public readonly EventVertexType Type = EventVertexType.Normal;
        public readonly int? MilestoneActivityId = null;

        private EventVertex(string id, EventVertexType vertexType)
        {
            Id = id;
        }

        private EventVertex(string id, int milestoneActivityId) : this(id, EventVertexType.Milestone)
        {
            this.MilestoneActivityId = milestoneActivityId;
        }

        public override bool Equals(Object obj)
        {
            return obj is EventVertex && this == (EventVertex)obj;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Type.GetHashCode();
        }
        public static bool operator ==(EventVertex x, EventVertex y)
        {
            return x.Id == y.Id;
        }
        public static bool operator !=(EventVertex x, EventVertex y)
        {
            return !(x == y);
        }

        #region Factory Methods
        public static EventVertex CreateMilestone(string id, int milestoneActivityId)
        {
            return new EventVertex(id, milestoneActivityId);
        }

        public static EventVertex CreateGraphStart(string id)
        {
            return new EventVertex(id, EventVertexType.GraphStart);
        }

        public static EventVertex CreateGraphEnd(string id)
        {
            return new EventVertex(id, EventVertexType.GraphEnd);
        }

        public static EventVertex Create(string id)
        {
            return new EventVertex(id, EventVertexType.Normal);
        }
        #endregion
    }

    public enum EventVertexType
    {
        Normal,
        GraphStart,
        GraphEnd,
        Milestone
    }
}
