using ActivityDiagram.Contracts.Model.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Contracts
{
    public interface IArrowGraphWriter
    {
        void Write(ActivityArrowGraph graph);
    }
}
