using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class FunctionManager
    {
        private readonly Dictionary<string, Func<object[], object>> builtInFunctions = new Dictionary<string, Func<object[], object>>();
        private readonly Dictionary<string, Func<object[], object>> externalFunctions = new Dictionary<string, Func<object[], object>>();
        private readonly List<Assembly> loadedAssemblies = new List<Assembly>();
        private readonly Dictionary<string, object> variables;
        private readonly Dictionary<string, int> counters;
        private readonly CodeEngine engine;
        private DatabaseManager databaseManager;

        public FunctionManager(Dictionary<string, object> variables, Dictionary<string, int> counters, CodeEngine engine)
        {
            this.variables = variables;
            this.counters = counters;
            this.engine = engine;
        }

        public List<string> GetFunctionNames()
        {
            List<string> functionNames = new List<string>();
            
            functionNames.AddRange(builtInFunctions.Keys);
            functionNames.AddRange(externalFunctions.Keys);
            return functionNames;
        }
        public void SetDatabaseManager(DatabaseManager dbManager)
        {
            this.databaseManager = dbManager;
        }

        public void InitializeBuiltInFunctions()
        {
            var arrayFunctions = new ArrayFunctions();
            var mathFunctions = new MathFunctions();
            var stringFunctions = new StringFunctions();
            var statisticsFunctions = new StatisticsFunctions(engine);
            var basicFunctions = new BasicFunctions(engine);
            var databaseFunctions = new DatabaseFunctions(databaseManager);
            var outputFunctions = new OutputFunctions(engine);

            // Registrar funciones básicas
            RegisterBasicFunctions(basicFunctions);

            // Registrar funciones de arrays
            RegisterArrayFunctions(arrayFunctions);

            // Registrar funciones matemáticas
            RegisterMathFunctions(mathFunctions);

            // Registrar funciones de strings
            RegisterStringFunctions(stringFunctions);

            // Registrar funciones estadísticas
            RegisterStatisticsFunctions(statisticsFunctions);

            // Registrar funciones de base de datos
            RegisterDatabaseFunctions(databaseFunctions);

            // Registrar funciones de salida (Table, Chart)
            RegisterOutputFunctions(outputFunctions);

            // Registrar funciones de contadores y memoria
            RegisterCounterAndMemoryFunctions();
        }

        private void RegisterBasicFunctions(BasicFunctions basicFunctions)
        {
            builtInFunctions["Print"] = basicFunctions.Print;
            builtInFunctions["Concat"] = basicFunctions.Concat;
            builtInFunctions["ToString"] = basicFunctions.ToString;
            builtInFunctions["Add"] = basicFunctions.Add;
            builtInFunctions["Multiply"] = basicFunctions.Multiply;
            builtInFunctions["CountWords"] = basicFunctions.CountWords;
            builtInFunctions["GetTextStats"] = basicFunctions.GetTextStats;
        }

        private void RegisterArrayFunctions(ArrayFunctions arrayFunctions)
        {
            builtInFunctions["Array"] = arrayFunctions.CreateArray;
            builtInFunctions["ArrayLength"] = arrayFunctions.GetLength;
            builtInFunctions["ArrayGet"] = arrayFunctions.GetElement;
            builtInFunctions["ArraySet"] = arrayFunctions.SetElement;
            builtInFunctions["ArrayPush"] = arrayFunctions.Push;
            builtInFunctions["ArrayPop"] = arrayFunctions.Pop;
            builtInFunctions["ArraySlice"] = arrayFunctions.Slice;
            builtInFunctions["ArrayJoin"] = arrayFunctions.Join;
            builtInFunctions["ArraySort"] = arrayFunctions.Sort;
            builtInFunctions["ArrayReverse"] = arrayFunctions.Reverse;
        }

        private void RegisterMathFunctions(MathFunctions mathFunctions)
        {
            builtInFunctions["Subtract"] = mathFunctions.Subtract;
            builtInFunctions["Divide"] = mathFunctions.Divide;
            builtInFunctions["Power"] = mathFunctions.Power;
            builtInFunctions["Sqrt"] = mathFunctions.Sqrt;
            builtInFunctions["Abs"] = mathFunctions.Abs;
            builtInFunctions["Floor"] = mathFunctions.Floor;
            builtInFunctions["Ceil"] = mathFunctions.Ceil;
            builtInFunctions["Round"] = mathFunctions.Round;
            builtInFunctions["Sin"] = mathFunctions.Sin;
            builtInFunctions["Cos"] = mathFunctions.Cos;
            builtInFunctions["Tan"] = mathFunctions.Tan;
            builtInFunctions["Log"] = mathFunctions.Log;
            builtInFunctions["Log10"] = mathFunctions.Log10;
            builtInFunctions["Exp"] = mathFunctions.Exp;
            builtInFunctions["Min"] = mathFunctions.Min;
            builtInFunctions["Max"] = mathFunctions.Max;
            builtInFunctions["Random"] = mathFunctions.Random;
            builtInFunctions["PI"] = mathFunctions.PI;
            builtInFunctions["E"] = mathFunctions.E;
        }

        private void RegisterStringFunctions(StringFunctions stringFunctions)
        {
            builtInFunctions["StringLength"] = stringFunctions.GetLength;
            builtInFunctions["Substring"] = stringFunctions.Substring;
            builtInFunctions["IndexOf"] = stringFunctions.IndexOf;
            builtInFunctions["ToUpper"] = stringFunctions.ToUpper;
            builtInFunctions["ToLower"] = stringFunctions.ToLower;
            builtInFunctions["Trim"] = stringFunctions.Trim;
            builtInFunctions["Replace"] = stringFunctions.Replace;
            builtInFunctions["Split"] = stringFunctions.Split;
            builtInFunctions["StartsWith"] = stringFunctions.StartsWith;
            builtInFunctions["EndsWith"] = stringFunctions.EndsWith;
            builtInFunctions["Contains"] = stringFunctions.Contains;
            builtInFunctions["PadLeft"] = stringFunctions.PadLeft;
            builtInFunctions["PadRight"] = stringFunctions.PadRight;
        }

        private void RegisterStatisticsFunctions(StatisticsFunctions statisticsFunctions)
        {
            builtInFunctions["Mean"] = statisticsFunctions.Mean;
            builtInFunctions["Median"] = statisticsFunctions.Median;
            builtInFunctions["Mode"] = statisticsFunctions.Mode;
            builtInFunctions["Range"] = statisticsFunctions.Range;
            builtInFunctions["Variance"] = statisticsFunctions.Variance;
            builtInFunctions["StandardDeviation"] = statisticsFunctions.StandardDeviation;
            builtInFunctions["Sum"] = statisticsFunctions.Sum;
            builtInFunctions["Count"] = statisticsFunctions.Count;
            builtInFunctions["Percentile"] = statisticsFunctions.Percentile;
            builtInFunctions["Quartile"] = statisticsFunctions.Quartile;
            builtInFunctions["Correlation"] = statisticsFunctions.Correlation;
            builtInFunctions["ZScore"] = statisticsFunctions.ZScore;
            builtInFunctions["PrintHistogram"] = statisticsFunctions.PrintHistogram;
            builtInFunctions["CreateHistogram"] = statisticsFunctions.CreateHistogram;
        }

        private void RegisterOutputFunctions(OutputFunctions outputFunctions)
        {
            builtInFunctions["Table"] = outputFunctions.EmitTable;
            builtInFunctions["Chart"] = outputFunctions.EmitChart;
            builtInFunctions["StatReport"] = outputFunctions.EmitStatReport;
            builtInFunctions["Value"] = outputFunctions.EmitValue;
        }

        private void RegisterDatabaseFunctions(DatabaseFunctions databaseFunctions)
        {
            builtInFunctions["ConnectPostgres"] = databaseFunctions.ConnectPostgres;
            builtInFunctions["ConnectSqlServer"] = databaseFunctions.ConnectSqlServer;
            builtInFunctions["DisconnectDB"] = databaseFunctions.DisconnectDB;
            builtInFunctions["ExecuteQuery"] = databaseFunctions.ExecuteQuery;
            builtInFunctions["ExecuteNonQuery"] = databaseFunctions.ExecuteNonQuery;
            builtInFunctions["ExecuteScalar"] = databaseFunctions.ExecuteScalar;
            builtInFunctions["GetRowValue"] = databaseFunctions.GetRowValue;
            builtInFunctions["GetRowKeys"] = databaseFunctions.GetRowKeys;
            builtInFunctions["BeginTransaction"] = databaseFunctions.BeginTransaction;
            builtInFunctions["CommitTransaction"] = databaseFunctions.CommitTransaction;
            builtInFunctions["RollbackTransaction"] = databaseFunctions.RollbackTransaction;
        }

        private void RegisterCounterAndMemoryFunctions()
        {
            // Funciones de contadores
            builtInFunctions["Counter"] = args => {
                string counterName = args.Length > 0 ? args[0]?.ToString() ?? "default" : "default";
                if (!counters.ContainsKey(counterName))
                    counters[counterName] = 0;
                return ++counters[counterName];
            };

            // Funciones de memoria/variables
            builtInFunctions["SaveToVar"] = args => {
                if (args.Length < 2) return null;
                string varName = args[0]?.ToString() ?? "";
                object value = args[1];
                variables[varName] = value;
                return value;
            };

            builtInFunctions["LoadFromVar"] = args => {
                if (args.Length == 0) return null;
                string varName = args[0]?.ToString() ?? "";
                return variables.ContainsKey(varName) ? variables[varName] : 0;
            };

            builtInFunctions["MaxVar"] = args => {
                if (args.Length < 2) return null;
                string varName = args[0]?.ToString() ?? "";
                double newValue = Convert.ToDouble(args[1]);

                if (!variables.ContainsKey(varName))
                    variables[varName] = newValue;
                else
                {
                    double currentValue = Convert.ToDouble(variables[varName]);
                    variables[varName] = Math.Max(currentValue, newValue);
                }
                return variables[varName];
            };

            builtInFunctions["IncrementVar"] = args => {
                if (args.Length == 0) return null;
                string varName = args[0]?.ToString() ?? "";

                if (!variables.ContainsKey(varName))
                    variables[varName] = 1;
                else
                {
                    double currentValue = Convert.ToDouble(variables[varName]);
                    variables[varName] = currentValue + 1;
                }
                return variables[varName];
            };
        }

        public void LoadExternalDll(string dllPath)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllPath);
                loadedAssemblies.Add(assembly);

                // Buscar métodos públicos estáticos que pueden ser llamados como funciones
                var exportedTypes = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<FunctEngineExportAttribute>() != null);
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        string functionName = $"{type.Name}.{method.Name}";
                        externalFunctions[functionName] = args => {
                            try
                            {
                                // Convertir argumentos al tipo esperado — soporta arrays
                                // (List<object> del script -> T[]/T[][]/...), tipos anulables,
                                // y parámetros opcionales omitidos por el script.
                                ParameterInfo[] parameters = method.GetParameters();
                                object[] convertedArgs = new object[parameters.Length];

                                for (int i = 0; i < parameters.Length; i++)
                                {
                                    if (i < args.Length && args[i] != null)
                                    {
                                        convertedArgs[i] = ConvertExternalArgument(args[i], parameters[i].ParameterType);
                                    }
                                    else if (parameters[i].HasDefaultValue)
                                    {
                                        convertedArgs[i] = parameters[i].DefaultValue;
                                    }
                                    else
                                    {
                                        convertedArgs[i] = parameters[i].ParameterType.IsValueType
                                            ? Activator.CreateInstance(parameters[i].ParameterType)
                                            : null;
                                    }
                                }

                                var result = method.Invoke(null, convertedArgs);
                                return ConvertExternalReturnValue(result);
                            }
                            catch (TargetInvocationException tie) when (tie.InnerException != null)
                            {
                                throw new Exception($"Error calling external function {functionName}: {tie.InnerException.Message}");
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Error calling external function {functionName}: {ex.Message}");
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading DLL {dllPath}: {ex.Message}");
            }
        }

        // Converts a pseudocode value (double/string/bool/List<object>) into the
        // .NET type an external DLL function parameter expects. List<object> is
        // converted element-by-element into a properly-typed (possibly jagged)
        // array; Nullable<T> targets unwrap to T; everything else falls back to
        // Convert.ChangeType (handles double/int/string/DateTime as before).
        private static object ConvertExternalArgument(object value, Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying != null)
            {
                return ConvertExternalArgument(value, underlying);
            }

            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }

            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType()!;
                if (value is List<object> list)
                {
                    var typedArray = Array.CreateInstance(elementType, list.Count);
                    for (int i = 0; i < list.Count; i++)
                        typedArray.SetValue(ConvertExternalArgument(list[i], elementType), i);
                    return typedArray;
                }
                throw new ArgumentException($"Expected an array value for a {targetType.Name} parameter");
            }

            return Convert.ChangeType(value, targetType);
        }

        // Converts an external function's .NET return value back into something
        // the pseudocode can use: tuples and arrays (including jagged arrays)
        // become List<object> so ArrayLength()/ArrayGet() work on them; scalars
        // and dictionaries (e.g. from TukeyHSD, usable via GetRowValue) pass
        // through unchanged.
        private static object ConvertExternalReturnValue(object result)
        {
            if (result == null) return null;

            if (result is System.Runtime.CompilerServices.ITuple tuple)
            {
                var list = new List<object>();
                for (int i = 0; i < tuple.Length; i++)
                    list.Add(ConvertExternalReturnValue(tuple[i]));
                return list;
            }

            if (result is Array arr)
            {
                var list = new List<object>();
                foreach (var item in arr)
                    list.Add(ConvertExternalReturnValue(item));
                return list;
            }

            return result;
        }

        public void RegisterExternalFunction(string name, Func<object[], object> function)
        {
            externalFunctions[name] = function;
        }

        public object CallFunction(string functionName, object[] args)
        {
            // Buscar en funciones built-in primero
            if (builtInFunctions.ContainsKey(functionName))
            {
                return builtInFunctions[functionName](args);
            }

            // Buscar en funciones externas
            if (externalFunctions.ContainsKey(functionName))
            {
                return externalFunctions[functionName](args);
            }

            throw new Exception($"Función desconocida: {functionName}");
        }
    }
}
