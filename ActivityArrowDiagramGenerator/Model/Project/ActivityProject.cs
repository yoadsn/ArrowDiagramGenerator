using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityArrowDiagramGenerator.Model.Project
{
    public class ActivityProject
    {
        public IEnumerable<ActivityDependency> ActivityDependencies { get; set; }

        public void AddActivityDependency(ActivityDependency dependency)
        {

        }
    }
}
