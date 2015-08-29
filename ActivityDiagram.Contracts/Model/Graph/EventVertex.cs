using ActivityDiagram.Contracts.Model.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Contracts.Model.Graph
{
    public class EventVertex
    {
        public readonly int Id;
        public readonly EventVertexType Type = EventVertexType.Normal;
        public readonly Activity MilestoneActivity = null;

        private EventVertex(int id, EventVertexType vertexType)
        {
            Id = id;
        }

        private EventVertex(int id, Activity milestoneActivity) : this(id, EventVertexType.Milestone)
        {
            this.MilestoneActivity = milestoneActivity;
        }

        public bool IsMilestone
        {
            get
            {
                return MilestoneActivity != null;
            }
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
        public static EventVertex CreateMilestone(int id, Activity milestoneActivity)
        {
            return new EventVertex(id, milestoneActivity);
        }

        public static EventVertex CreateGraphStart(int id)
        {
            return new EventVertex(id, EventVertexType.GraphStart);
        }

        public static EventVertex CreateGraphEnd(int id)
        {
            return new EventVertex(id, EventVertexType.GraphEnd);
        }

        public static EventVertex Create(int id)
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
