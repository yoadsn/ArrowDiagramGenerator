using ActivityDiagram.Contracts;
using ActivityDiagram.Contracts.Model.Activities;
using net.sf.mpxj;
using net.sf.mpxj.reader;
using net.sf.mpxj.MpxjUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Readers.Mpp
{
    public class MppActivitiesReader : IActivitiesReader
    {
        private readonly string filename;

        public MppActivitiesReader(string filename)
        {
            this.filename = filename;
        }
        public IEnumerable<ActivityDependency> Read()
        {
            ProjectReader reader = ProjectReaderUtility.getProjectReader(filename);
            ProjectFile mpx = reader.read(filename);


            var actDependnecies = new List<ActivityDependency>();
            foreach (net.sf.mpxj.Task task in mpx.AllTasks.ToIEnumerable())
            {
                var id = task.ID.intValue();
                var duration = task.Duration.Duration;
                var totalSlack = task.TotalSlack.Duration;

                var predecessors = new List<int>();
                var preds = task.Predecessors;
                if (preds != null && !preds.isEmpty())
                {
                    foreach (Relation pred in preds.ToIEnumerable())
                    {
                        predecessors.Add(pred.TargetTask.ID.intValue());
                    }
                }

                actDependnecies.Add(
                    new ActivityDependency(
                        new Activity(id, Convert.ToInt32(duration), Convert.ToInt32(totalSlack)), predecessors
                        )
                    );
            }

            return actDependnecies;
        }
    }
}
