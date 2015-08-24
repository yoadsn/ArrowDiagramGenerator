using ActivityDiagram.Contracts.Model.Activities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Contracts
{
    public interface IActivitiesReader
    {
        IEnumerable<ActivityDependency> Read();
    }
}
