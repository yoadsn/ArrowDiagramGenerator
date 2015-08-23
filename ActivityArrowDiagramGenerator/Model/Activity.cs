using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model
{
    public class Activity
    {
        public readonly int Id;
        
        public Activity(int id)
        {
            this.Id = id;
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
            return x.Id == y.Id;
        }
        public static bool operator !=(Activity x, Activity y)
        {
            return !(x == y);
        }
    }
}
