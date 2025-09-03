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
                                // Convertir argumentos al tipo esperado
                                ParameterInfo[] parameters = method.GetParameters();
                                object[] convertedArgs = new object[parameters.Length];

                                for (int i = 0; i < Math.Min(args.Length, parameters.Length); i++)
                                {
                                    convertedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                                }

                                return method.Invoke(null, convertedArgs);
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
