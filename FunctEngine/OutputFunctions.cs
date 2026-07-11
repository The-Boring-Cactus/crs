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

        // StatReport(title, data)
        // Emits a formatted statistical report. 'data' can be:
        //   - A Dictionary<string,object> from DoeFunctions (ANOVA, regression, etc.)
        //   - A List<object> of section Dictionaries: { heading, type, content/columns/rows }
        public object EmitStatReport(object[] args)
        {
            if (args.Length == 0) return null;

            var title = args[0]?.ToString() ?? "Statistical Report";
            var sections = new List<object>();

            if (args.Length > 1)
            {
                var data = args[1];

                if (data is List<object> sectionList)
                {
                    // Pre-structured sections list
                    foreach (var s in sectionList)
                    {
                        if (s is Dictionary<string, object> sd)
                            sections.Add(sd);
                    }
                }
                else if (data is Dictionary<string, object> dict)
                {
                    // Auto-format a DoeFunctions result dictionary
                    sections.AddRange(FormatStatDict(dict));
                }
            }

            engine.EmitOutput("StatReport", new { title, sections });
            return null;
        }

        // Value(value, label?, unit?)
        // Emits a single scalar value for Variable dashboard widgets
        public object EmitValue(object[] args)
        {
            if (args.Length == 0) return null;
            var value = args[0]?.ToString() ?? "";
            var label = args.Length > 1 ? args[1]?.ToString() ?? "" : "";
            var unit  = args.Length > 2 ? args[2]?.ToString() ?? "" : "";
            engine.EmitOutput("Value", new { value, label, unit });
            return null;
        }

        // Markdown(content, title?)
        // Renders formatted prose for building a report from a script's output.
        // Content is standard Markdown; inline ($...$) and block ($$...$$) LaTeX
        // are typeset with MathJax on the frontend, so formulas can be embedded
        // directly in the prose without a separate Formula() call.
        public object EmitMarkdown(object[] args)
        {
            if (args.Length == 0) return null;
            var content = args[0]?.ToString() ?? "";
            var title = args.Length > 1 ? args[1]?.ToString() ?? "" : "";
            engine.EmitOutput("Markdown", new { title, content });
            return null;
        }

        // Formula(latex, label?)
        // Renders a single LaTeX expression as a centered, prominent display
        // formula (MathJax), for calling out one equation on its own rather than
        // embedding it inline within Markdown() prose.
        public object EmitFormula(object[] args)
        {
            if (args.Length == 0) return null;
            var latex = args[0]?.ToString() ?? "";
            var label = args.Length > 1 ? args[1]?.ToString() ?? "" : "";
            engine.EmitOutput("Formula", new { label, latex });
            return null;
        }

        private List<object> FormatStatDict(Dictionary<string, object> dict)
        {
            var sections = new List<object>();
            foreach (var kv in dict)
            {
                if (kv.Value is Dictionary<string, object> nested)
                {
                    var cols = nested.Keys.ToList();
                    sections.Add(new
                    {
                        heading = kv.Key,
                        type = "table",
                        columns = cols,
                        rows = new List<object> { nested.ToDictionary(k => k.Key, k => k.Value?.ToString() as object) }
                    });
                }
                else if (kv.Value is List<object> list && list.Count > 0 && list[0] is Dictionary<string, object>)
                {
                    var firstRow = (Dictionary<string, object>)list[0];
                    var cols = firstRow.Keys.ToList();
                    var rows = list.OfType<Dictionary<string, object>>()
                                   .Select(r => r.ToDictionary(k => k.Key, k => k.Value?.ToString() as object))
                                   .ToList<object>();
                    sections.Add(new { heading = kv.Key, type = "table", columns = cols, rows });
                }
                else
                {
                    sections.Add(new { heading = kv.Key, type = "text", content = kv.Value?.ToString() ?? "" });
                }
            }
            return sections;
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
