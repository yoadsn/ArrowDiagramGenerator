using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Readers.CSV.Model
{
    internal class ActivityRow
    {
        public int ActivityId { get; set; }
        public List<int> Predecessors { get; set; }
        public int? ActivityDuration { get; set; }
        public int? ActivityTotalSlack { get; set; }

        public ActivityRow()
        {

        }
    }
}
