using System.Reflection;
using Antlr4.Runtime;


namespace FunctEngine;

public class RuntimeInterpreter
{
        private Dictionary<string, object> variables;
        private Dictionary<string, ExternalFunction> externalFunctions;
        private List<Assembly> loadedAssemblies;
        private static RuntimeInterpreter currentInstance;

        public RuntimeInterpreter()
        {
            variables = new Dictionary<string, object>();
            externalFunctions = new Dictionary<string, ExternalFunction>();
            loadedAssemblies = new List<Assembly>();
            currentInstance = this;
            
            // Cargar funciones built-in
            LoadBuiltInFunctions();
        }

        // Método estático para acceder a la instancia actual desde DLLs externos
        public static RuntimeInterpreter GetCurrentInstance()
        {
            return currentInstance;
        }

        public event StatusUpdateHandler StatusUpdate;
        
        public void LoadExternalFunctions(string dllPath)
        {
            try
            {
                if (!File.Exists(dllPath))
                {
                    Console.WriteLine($"⚠️  DLL no encontrado: {dllPath}");
                    return;
                }

                var assembly = Assembly.LoadFrom(dllPath);
                loadedAssemblies.Add(assembly);

                // Buscar tipos con el atributo [FunctEngineExport]
                IEnumerable<Type> exportedTypes;
                exportedTypes = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<FunctEngineExportAttribute>() != null);

                foreach (var type in exportedTypes)
                {
                    LoadFunctionsFromType(type);
                }

                Console.WriteLine($"✅ DLL cargado exitosamente: {dllPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando DLL {dllPath}: {ex.Message}");
            }
        }

        private void LoadFunctionsFromType(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<FunctEngineExportAttribute>() != null);

            foreach (var method in methods)
            {
                var exportAttr = method.GetCustomAttribute<FunctEngineExportAttribute>();
                string functionName = exportAttr?.Name ?? method.Name;

                var externalFunc = new ExternalFunction
                {
                    Method = method,
                    Name = functionName,
                    Description = exportAttr?.Description ?? "",
                    ParameterCount = method.GetParameters().Length
                };

                externalFunctions[functionName] = externalFunc;
                Console.WriteLine($"📦 Función cargada: {functionName}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
            }
        }

        private void LoadBuiltInFunctions()
        {
            // Funciones built-in del intérprete
            externalFunctions["Print"] = new ExternalFunction
            {
                Name = "Print",
                Description = "Imprime un valor en la consola",
                ParameterCount = 1,
                BuiltInAction = args => 
                {
                    if (StatusUpdate != null)
                    {
                        this.StatusUpdate(this, new StatusString(args[0]?.ToString() ?? "null"));
                    }
                    return null;
                }
            };

            externalFunctions["ToString"] = new ExternalFunction
            {
                Name = "ToString",
                Description = "Convierte un valor a string",
                ParameterCount = 1,
                BuiltInAction = args => args[0]?.ToString() ?? "null"
            };

            externalFunctions["ToNumber"] = new ExternalFunction
            {
                Name = "ToNumber",
                Description = "Convierte un string a número",
                ParameterCount = 1,
                BuiltInAction = args =>
                {
                    if (double.TryParse(args[0]?.ToString(), out double result))
                        return result;
                    return 0.0;
                }
            };

            // Nuevas funciones para manejo de variables globales
            externalFunctions["GetVar"] = new ExternalFunction
            {
                Name = "GetVar",
                Description = "Obtiene el valor de una variable global",
                ParameterCount = 1,
                BuiltInAction = args =>
                {
                    string varName = args[0]?.ToString();
                    return GetVariable(varName);
                }
            };

            externalFunctions["SetVar"] = new ExternalFunction
            {
                Name = "SetVar",
                Description = "Establece el valor de una variable global",
                ParameterCount = 2,
                BuiltInAction = args =>
                {
                    string varName = args[0]?.ToString();
                    object value = args[1];
                    SetVariable(varName, value);
                    return value;
                }
            };

            externalFunctions["HasVar"] = new ExternalFunction
            {
                Name = "HasVar",
                Description = "Verifica si existe una variable global",
                ParameterCount = 1,
                BuiltInAction = args =>
                {
                    string varName = args[0]?.ToString();
                    return HasVariable(varName);
                }
            };

            externalFunctions["RemoveVar"] = new ExternalFunction
            {
                Name = "RemoveVar",
                Description = "Elimina una variable global",
                ParameterCount = 1,
                BuiltInAction = args =>
                {
                    string varName = args[0]?.ToString();
                    RemoveVariable(varName);
                    return null;
                }
            };

            externalFunctions["ListVars"] = new ExternalFunction
            {
                Name = "ListVars",
                Description = "Lista todas las variables globales",
                ParameterCount = 0,
                BuiltInAction = args =>
                {
                    Console.WriteLine("=== Variables Globales ===");
                    foreach (var kvp in variables)
                    {
                        Console.WriteLine($"  {kvp.Key} = {kvp.Value ?? "null"} ({kvp.Value?.GetType().Name ?? "null"})");
                    }
                    return null;
                }
            };
        }

        public void Execute(string input)
        {
            try
            {
                Console.WriteLine($"🔍 Ejecutando código:\n{input}\n");
                
                var inputStream = new AntlrInputStream(input);
                var lexer = new FunctEngineLexer(inputStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new FunctEngineParser(tokenStream);

                parser.RemoveErrorListeners();
                parser.AddErrorListener(new CustomErrorListener());

                var tree = parser.program();

                if (parser.NumberOfSyntaxErrors == 0)
                {
                    Console.WriteLine("✅ Análisis sintáctico exitoso");
                    var interpreter = new FunctEngineExecutionVisitor(this);
                    interpreter.Visit(tree);
                }
                else
                {
                    Console.WriteLine($"❌ Errores de sintaxis encontrados: {parser.NumberOfSyntaxErrors}");
                    // Mostrar tokens para debugging
                    Console.WriteLine("🔍 Tokens generados:");
                    tokenStream.Fill();
                    foreach (IToken token in tokenStream.GetTokens().Take(20)) // Primeros 20 tokens
                    {
                        if (token.Type != TokenConstants.EOF)
                        {
                            string tokenName = lexer.Vocabulary.GetSymbolicName(token.Type) ?? $"Unknown({token.Type})";
                            Console.WriteLine($"  {tokenName}: '{token.Text}' (línea {token.Line}, col {token.Column})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error de ejecución: {ex.Message}");
                Console.WriteLine($"📍 Stack trace: {ex.StackTrace}");
            }
        }

        public object CallExternalFunction(string functionName, object[] arguments)
        {
            if (!externalFunctions.ContainsKey(functionName))
            {
                throw new InvalidOperationException($"Función '{functionName}' no encontrada");
            }

            var func = externalFunctions[functionName];

            if (func.ParameterCount >= 0 && arguments.Length != func.ParameterCount)
            {
                throw new ArgumentException($"Función '{functionName}' espera {func.ParameterCount} argumentos, pero recibió {arguments.Length}");
            }

            try
            {
                if (func.BuiltInAction != null)
                {
                    return func.BuiltInAction(arguments);
                }
                else if (func.Method != null)
                {
                    // Convertir argumentos a tipos esperados
                    var parameters = func.Method.GetParameters();
                    var convertedArgs = new object[arguments.Length];

                    for (int i = 0; i < arguments.Length; i++)
                    {
                        convertedArgs[i] = ConvertArgument(arguments[i], parameters[i].ParameterType);
                    }

                    return func.Method.Invoke(null, convertedArgs);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error ejecutando función '{functionName}': {ex.Message}");
            }

            return null;
        }

        private object ConvertArgument(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsAssignableFrom(value.GetType())) return value;

            try
            {
                if (targetType == typeof(int)) return Convert.ToInt32(value);
                if (targetType == typeof(double)) return Convert.ToDouble(value);
                if (targetType == typeof(float)) return Convert.ToSingle(value);
                if (targetType == typeof(string)) return value.ToString();
                if (targetType == typeof(bool)) return Convert.ToBoolean(value);
                
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                throw new ArgumentException($"No se puede convertir '{value}' de tipo {value.GetType()} a {targetType}");
            }
        }

        public void SetVariable(string name, object value)
        {
            variables[name] = value;
        }

        public object GetVariable(string name)
        {
            return variables.ContainsKey(name) ? variables[name] : null;
        }

        public bool HasVariable(string name)
        {
            return variables.ContainsKey(name);
        }

        public void RemoveVariable(string name)
        {
            variables.Remove(name);
        }

        public Dictionary<string, object> GetAllVariables()
        {
            return new Dictionary<string, object>(variables);
        }

        public void ClearVariables()
        {
            variables.Clear();
        }

        public void ListAvailableFunctions()
        {
            Console.WriteLine("Funciones disponibles:");
            foreach (var func in externalFunctions.Values)
            {
                string paramInfo = func.ParameterCount >= 0 ? $"({func.ParameterCount} parámetros)" : "(parámetros variables)";
                Console.WriteLine($"  • {func.Name}{paramInfo} - {func.Description}");
            }
        }
}