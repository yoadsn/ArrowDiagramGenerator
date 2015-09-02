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
                    new Activity(actrow.ActivityId, actrow.ActivityDuration, actrow.ActivityTotalSlack),
                    actrow.Predecessors)).ToList();
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
            Map(m => m.ActivityId).Name("ID");
            Map(m => m.Predecessors).ConvertUsing(ParsePredecessorsIntList);
            Map(m => m.ActivityDuration).ConvertUsing(ParseDuration);
            Map(m => m.ActivityTotalSlack).ConvertUsing(ParseTotalSlack);
        }

        List<int> ParsePredecessorsIntList(ICsvReaderRow row)
        {
            return ParseIntList(row, "Predecessors");
        }

        int? ParseDuration(ICsvReaderRow row)
        {
            return ParseSafeIntegerValuesWithSuffix(row, "Duration", "days");
        }

        int? ParseTotalSlack(ICsvReaderRow row)
        {
            return ParseSafeIntegerValuesWithSuffix(row, "Total Slack", "days");
        }

        List<int> ParseIntList(ICsvReaderRow row, string fieldName)
        {
            var stringList = row.GetField<string>(fieldName).Trim();
            if (String.IsNullOrEmpty(stringList)) return new List<int>();

            return stringList.Split(',')
                .Select(sId => Int32.Parse(sId))
                .ToList();
        }

        int? ParseSafeIntegerValuesWithSuffix(ICsvReaderRow row, string fieldName, string suffix)
        {
            var stringValue = row.GetField<string>(fieldName).Trim();
            if (String.IsNullOrEmpty(stringValue)) return null;

            try
            {
                var startIndexOfSuffix = stringValue.ToLowerInvariant().IndexOf(suffix);
                if (startIndexOfSuffix > 0)
                {
                    stringValue = stringValue.Substring(0, startIndexOfSuffix).Trim();
                }
            }
            catch { }

            int integerValue;
            if (Int32.TryParse(stringValue, out integerValue))
            {
                return integerValue;
            }
            else
            {
                return null;
            }
        }
    }
}
