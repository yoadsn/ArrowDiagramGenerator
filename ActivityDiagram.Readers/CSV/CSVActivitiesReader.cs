using ActivityDiagram.Contracts;
using ActivityDiagram.Contracts.Model.Activities;
using ActivityDiagram.Readers.CSV.Model;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ActivityDiagram.Readers.CSV
{
    public class CSVActivitiesReader : IActivitiesReader, IDisposable
    {
        private readonly CsvReader csvReader;
        private readonly string filename;

        public CSVActivitiesReader(string filename)
        {
            this.filename = filename;
            csvReader = new CsvReader(new StreamReader(filename));
            csvReader.Configuration.RegisterClassMap<ActivityRowMap>();
        }

        public IEnumerable<ActivityDependency> Read()
        {
            var rows = csvReader.GetRecords<ActivityRow>();

            return rows.Select(actrow =>
                new ActivityDependency(
                    new Activity(actrow.ActivityId),
                    actrow.Predecessors));
        }

        public void Dispose()
        {
            if (csvReader != null)
            {
                csvReader.Dispose();
            }
        }
    }

    internal sealed class ActivityRowMap : CsvClassMap<ActivityRow>
    {
        public ActivityRowMap()
        {
            Map(m => m.Predecessors).ConvertUsing(ParseIntList);
            Map(m => m.ActivityId).Name("ID");
        }

        List<int> ParseIntList(ICsvReaderRow row)
        {
            var stringList = row.GetField<string>("Predecessors").Trim();
            if (String.IsNullOrEmpty(stringList)) return new List<int>();

            return stringList.Split(',')
                .Select(sId => Int32.Parse(sId))
                .ToList();
        }
    }
}
