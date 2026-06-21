using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctEngine
{
    public class OutputFunctions
    {
        private readonly CodeEngine engine;

        public OutputFunctions(CodeEngine engine)
        {
            this.engine = engine;
        }

        // Table(data, title?)
        // data: result of ExecuteQuery() -> List<Dictionary<string,object>>
        //       or a List<List<object>> for manual rows
        public object EmitTable(object[] args)
        {
            if (args.Length == 0) return null;

            var data = args[0];
            var title = args.Length > 1 ? args[1]?.ToString() ?? "" : "";

            var (columns, rows) = ExtractTableData(data);

            engine.EmitOutput("Table", new { title, columns, rows });
            return null;
        }

        // Chart(chartType, labels, values, title?)
        // chartType: "bar", "line", "pie", "doughnut", "area", "radar", "scatter"
        // labels: array of category labels
        // values: single array (single series) or array of arrays (multi-series)
        // title: optional string
        public object EmitChart(object[] args)
        {
            if (args.Length < 3) return null;

            var chartType = args[0]?.ToString() ?? "bar";
            var labels = ExtractList(args[1]);
            var title = args.Length > 3 ? args[3]?.ToString() ?? "" : "";

            object datasetsPayload;

            // Multi-series: args[2] is a list of lists
            if (args[2] is List<object> outerList && outerList.Count > 0 && outerList[0] is List<object>)
            {
                var datasets = outerList.Select((ds, i) => (object)new
                {
                    label = $"Series {i + 1}",
                    data = ExtractList(ds)
                }).ToList();
                datasetsPayload = datasets;
            }
            else
            {
                datasetsPayload = new List<object>
                {
                    new { label = title.Length > 0 ? title : "Values", data = ExtractList(args[2]) }
                };
            }

            engine.EmitOutput("Chart", new { chartType, title, labels, datasets = datasetsPayload });
            return null;
        }

        private (List<string> columns, List<object> rows) ExtractTableData(object data)
        {
            var columns = new List<string>();
            var rows = new List<object>();

            var items = data is List<object> l ? l : new List<object> { data };

            foreach (var item in items)
            {
                if (item is Dictionary<string, object> dict)
                {
                    if (columns.Count == 0)
                        columns.AddRange(dict.Keys);

                    // Serialize values to primitives for JSON transport
                    var row = dict.ToDictionary(k => k.Key, k => k.Value?.ToString() as object);
                    rows.Add(row);
                }
                else if (item is List<object> rowArr)
                {
                    if (columns.Count == 0)
                    {
                        for (int i = 0; i < rowArr.Count; i++)
                            columns.Add($"Column{i + 1}");
                    }

                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < Math.Min(rowArr.Count, columns.Count); i++)
                        row[columns[i]] = rowArr[i];
                    rows.Add(row);
                }
            }

            return (columns, rows);
        }

        private List<object> ExtractList(object data)
        {
            if (data is List<object> list) return list;
            if (data is object[] arr) return arr.ToList();
            return new List<object> { data };
        }
    }
}
