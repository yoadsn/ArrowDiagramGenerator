using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model.Project
{
    public class ActivityDependency
    {
        public Activity Activity { get; set; }
        public Activity DependsOnActivity { get; set; }
    }
}
