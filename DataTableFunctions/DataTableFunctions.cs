using FunctEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataTableFunctions
{
    [FunctEngineExport("DataTable Functions", "Biblioteca de funciones para manipular DataTables")]
    public static class DataTableLibrary
    {
        [FunctEngineExport("Inner_Join", "Realiza un INNER JOIN entre dos DataTables")]
        public static DataTable Inner_Join(DataTable tableA, DataTable tableB, string conditions)
        {
            return PerformJoin(tableA, tableB, conditions, JoinType.Inner);
        }

        [FunctEngineExport("Left_Join", "Realiza un LEFT JOIN entre dos DataTables")]
        public static DataTable Left_Join(DataTable tableA, DataTable tableB, string conditions)
        {
            return PerformJoin(tableA, tableB, conditions, JoinType.Left);
        }

        [FunctEngineExport("Right_Join", "Realiza un RIGHT JOIN entre dos DataTables")]
        public static DataTable Right_Join(DataTable tableA, DataTable tableB, string conditions)
        {
            return PerformJoin(tableA, tableB, conditions, JoinType.Right);
        }

        [FunctEngineExport("Sum", "Calcula la suma de una columna con condiciones opcionales")]
        public static double Sum(DataTable table, string columnName, string conditions = "")
        {
            var filteredRows = FilterRows(table, conditions);
            if (!table.Columns.Contains(columnName))
                throw new ArgumentException($"La columna '{columnName}' no existe en la tabla");

            return filteredRows
                .Where(row => row[columnName] != DBNull.Value)
                .Sum(row => Convert.ToDouble(row[columnName]));
        }

        [FunctEngineExport("Avg", "Calcula el promedio de una columna con condiciones opcionales")]
        public static double Avg(DataTable table, string columnName, string conditions = "")
        {
            var filteredRows = FilterRows(table, conditions);
            if (!table.Columns.Contains(columnName))
                throw new ArgumentException($"La columna '{columnName}' no existe en la tabla");

            var validRows = filteredRows.Where(row => row[columnName] != DBNull.Value).ToList();
            if (validRows.Count == 0)
                return 0;

            return validRows.Average(row => Convert.ToDouble(row[columnName]));
        }

        [FunctEngineExport("Count", "Cuenta las filas con condiciones opcionales")]
        public static int Count(DataTable table, string columnName, string conditions = "")
        {
            var filteredRows = FilterRows(table, conditions);

            if (string.IsNullOrWhiteSpace(columnName))
                return filteredRows.Length;

            if (!table.Columns.Contains(columnName))
                throw new ArgumentException($"La columna '{columnName}' no existe en la tabla");

            return filteredRows.Count(row => row[columnName] != DBNull.Value);
        }

        // ─── Bridges between pseudocode data and DataTable ─────────────────────
        // A DataTable can't be built or unpacked directly from pseudocode (no
        // dictionary/DataTable literal exists in the language), so these bridge
        // functions round-trip through the same "list of column->value
        // dictionaries" row shape already used by ExecuteQuery/ReadDataset/Table().

        [FunctEngineExport("MakeRow", "Construye una fila tipo diccionario (columna->valor) a partir de una lista de nombres de columna y una lista de valores")]
        public static Dictionary<string, object> MakeRow(object columns, object values)
        {
            var columnList = columns as List<object> ?? throw new ArgumentException("Se espera una lista de nombres de columna");
            var valueList = values as List<object> ?? throw new ArgumentException("Se espera una lista de valores");

            var row = new Dictionary<string, object>();
            for (int i = 0; i < columnList.Count; i++)
            {
                var key = columnList[i]?.ToString() ?? $"Column{i + 1}";
                row[key] = i < valueList.Count ? valueList[i] : null;
            }
            return row;
        }

        [FunctEngineExport("FromRows", "Crea un DataTable a partir de filas tipo diccionario (por ejemplo, el resultado de ExecuteQuery o MakeRow)")]
        public static DataTable FromRows(object rows)
        {
            var rowList = rows as List<object> ?? throw new ArgumentException("Se espera una lista de filas (diccionarios)");
            var table = new DataTable();
            if (rowList.Count == 0)
                return table;

            var firstRow = rowList[0] as Dictionary<string, object>
                ?? throw new ArgumentException("Cada fila debe ser un diccionario columna->valor");

            foreach (var key in firstRow.Keys)
                table.Columns.Add(key, firstRow[key]?.GetType() ?? typeof(object));

            foreach (var rowObj in rowList)
            {
                var rowDict = rowObj as Dictionary<string, object>
                    ?? throw new ArgumentException("Cada fila debe ser un diccionario columna->valor");

                var newRow = table.NewRow();
                foreach (var key in firstRow.Keys)
                    newRow[key] = rowDict.TryGetValue(key, out var value) && value != null ? value : DBNull.Value;
                table.Rows.Add(newRow);
            }

            return table;
        }

        [FunctEngineExport("FromArrayRows", "Crea un DataTable a partir de una lista de nombres de columna y filas tipo array (por ejemplo, datos construidos con Array())")]
        public static DataTable FromArrayRows(object columns, object rows)
        {
            var columnList = columns as List<object> ?? throw new ArgumentException("Se espera una lista de nombres de columna");
            var rowList = rows as List<object> ?? throw new ArgumentException("Se espera una lista de filas (arrays)");

            var columnNames = columnList.Select(c => c?.ToString() ?? "").ToArray();
            var firstRow = rowList.Count > 0 ? rowList[0] as List<object> : null;

            var table = new DataTable();
            for (int i = 0; i < columnNames.Length; i++)
            {
                var sampleValue = firstRow != null && i < firstRow.Count ? firstRow[i] : null;
                table.Columns.Add(columnNames[i], sampleValue?.GetType() ?? typeof(object));
            }

            foreach (var rowObj in rowList)
            {
                var rowArray = rowObj as List<object> ?? throw new ArgumentException("Cada fila debe ser un array de valores");
                var newRow = table.NewRow();
                for (int i = 0; i < columnNames.Length && i < rowArray.Count; i++)
                    newRow[i] = rowArray[i] ?? (object)DBNull.Value;
                table.Rows.Add(newRow);
            }

            return table;
        }

        [FunctEngineExport("ToRows", "Convierte un DataTable de vuelta a filas tipo diccionario, utilizables con Table() y GetRowValue()")]
        public static List<object> ToRows(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var rows = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                rows.Add(dict);
            }
            return rows;
        }

        // Métodos privados auxiliares
        private enum JoinType { Inner, Left, Right }

        private static DataTable PerformJoin(DataTable tableA, DataTable tableB, string conditions, JoinType joinType)
        {
            if (tableA == null || tableB == null)
                throw new ArgumentNullException("Las tablas no pueden ser nulas");

            var result = new DataTable();
            var joinConditions = ParseJoinConditions(conditions);

            // Crear columnas del resultado
            foreach (DataColumn col in tableA.Columns)
                result.Columns.Add($"A.{col.ColumnName}", col.DataType);
            foreach (DataColumn col in tableB.Columns)
                result.Columns.Add($"B.{col.ColumnName}", col.DataType);

            // Realizar el join
            foreach (DataRow rowA in tableA.Rows)
            {
                bool matchFound = false;
                foreach (DataRow rowB in tableB.Rows)
                {
                    if (EvaluateJoinConditions(rowA, rowB, joinConditions, tableA, tableB))
                    {
                        matchFound = true;
                        var newRow = result.NewRow();
                        for (int i = 0; i < tableA.Columns.Count; i++)
                            newRow[i] = rowA[i];
                        for (int i = 0; i < tableB.Columns.Count; i++)
                            newRow[tableA.Columns.Count + i] = rowB[i];
                        result.Rows.Add(newRow);
                    }
                }

                // Para LEFT JOIN, agregar fila incluso si no hay match
                if (!matchFound && joinType == JoinType.Left)
                {
                    var newRow = result.NewRow();
                    for (int i = 0; i < tableA.Columns.Count; i++)
                        newRow[i] = rowA[i];
                    for (int i = tableA.Columns.Count; i < result.Columns.Count; i++)
                        newRow[i] = DBNull.Value;
                    result.Rows.Add(newRow);
                }
            }

            // Para RIGHT JOIN
            if (joinType == JoinType.Right)
            {
                foreach (DataRow rowB in tableB.Rows)
                {
                    bool matchFound = false;
                    foreach (DataRow rowA in tableA.Rows)
                    {
                        if (EvaluateJoinConditions(rowA, rowB, joinConditions, tableA, tableB))
                        {
                            matchFound = true;
                            break;
                        }
                    }

                    if (!matchFound)
                    {
                        var newRow = result.NewRow();
                        for (int i = 0; i < tableA.Columns.Count; i++)
                            newRow[i] = DBNull.Value;
                        for (int i = 0; i < tableB.Columns.Count; i++)
                            newRow[tableA.Columns.Count + i] = rowB[i];
                        result.Rows.Add(newRow);
                    }
                }
            }

            return result;
        }

        private static DataRow[] FilterRows(DataTable table, string conditions)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (string.IsNullOrWhiteSpace(conditions))
                return table.Select();

            try
            {
                return table.Select(conditions);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Condición inválida: {conditions}", ex);
            }
        }

        private static (string colA, string colB, string op)[] ParseJoinConditions(string conditions)
        {
            var conditionsList = new System.Collections.Generic.List<(string, string, string)>();
            var parts = Regex.Split(conditions, @"\s+(and|or)\s+", RegexOptions.IgnoreCase);

            foreach (var part in parts)
            {
                if (part.Equals("and", StringComparison.OrdinalIgnoreCase) ||
                    part.Equals("or", StringComparison.OrdinalIgnoreCase))
                {
                    conditionsList.Add((part.ToUpper(), "", "LOGICAL"));
                    continue;
                }

                var match = Regex.Match(part.Trim(), @"A\.(\w+)\s*=\s*B\.(\w+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    conditionsList.Add((match.Groups[1].Value, match.Groups[2].Value, "="));
                }
            }

            return conditionsList.ToArray();
        }

        private static bool EvaluateJoinConditions(DataRow rowA, DataRow rowB,
            (string colA, string colB, string op)[] conditions, DataTable tableA, DataTable tableB)
        {
            bool result = true;
            string logicalOp = "AND";

            foreach (var condition in conditions)
            {
                if (condition.op == "LOGICAL")
                {
                    logicalOp = condition.colA;
                    continue;
                }

                if (!tableA.Columns.Contains(condition.colA))
                    throw new ArgumentException($"La columna '{condition.colA}' no existe en la tabla A");
                if (!tableB.Columns.Contains(condition.colB))
                    throw new ArgumentException($"La columna '{condition.colB}' no existe en la tabla B");

                bool conditionResult = rowA[condition.colA].Equals(rowB[condition.colB]);

                if (logicalOp == "AND")
                    result = result && conditionResult;
                else if (logicalOp == "OR")
                    result = result || conditionResult;
            }

            return result;
        }
    }
}