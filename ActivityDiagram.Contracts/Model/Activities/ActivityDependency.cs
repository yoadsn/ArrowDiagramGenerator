using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Contracts.Model.Activities
{
    public class ActivityDependency
    {
        public readonly Activity Activity;
        public readonly IReadOnlyList<int> Predecessors;

        public ActivityDependency(Activity activity, List<int> predecessors)
        {
            this.Activity = activity;
            this.Predecessors = predecessors.AsReadOnly();
        }
    }
}
