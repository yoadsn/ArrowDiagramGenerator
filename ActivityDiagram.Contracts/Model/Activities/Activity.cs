using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Contracts.Model.Activities
{
    public class Activity
    {
        public readonly int Id;
        public readonly int? Duration;
        public readonly int? TotalSlack;
        
        public Activity(int id):
            this(id, null, null)

        {

        }

        public Activity(int id, int? duration, int? totalSlack)
        {
            this.Id = id;
            this.Duration = duration;
            this.TotalSlack = totalSlack;
        }

        public bool IsCritical
        {
            get
            {
                return TotalSlack == 0;
            }
        }

        public override bool Equals(Object obj)
        {
            return obj is Activity && this == (Activity)obj;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(Activity x, Activity y)
        {
            if (System.Object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            return x.Id == y.Id;
        }
        public static bool operator !=(Activity x, Activity y)
        {
            return !(x == y);
        }
    }
}
