using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Writers.Graphml.Model
{
    internal enum GraphmlEdgeType
    {
        Activity,
        CriticalActivity,
        Dummy,
        CriticalDummy,
    }
}
