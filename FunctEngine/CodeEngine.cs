using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class CodeEngine : IDisposable
    {
        private readonly Dictionary<string, object> variables = new Dictionary<string, object>();
        private readonly Dictionary<string, int> counters = new Dictionary<string, int>();
        private readonly FunctionManager functionManager;
        private readonly DatabaseManager databaseManager;

        // Variables para estadísticas de texto
        private int totalWordsProcessed = 0;
        private int totalTextsAnalyzed = 0;

        public CodeEngine()
        {
            functionManager = new FunctionManager(variables, counters, this);
            databaseManager = new DatabaseManager();
            functionManager.SetDatabaseManager(databaseManager);
            functionManager.InitializeBuiltInFunctions();
        }

        // Propiedades públicas para acceso a estadísticas
        public int TotalWordsProcessed => totalWordsProcessed;
        public int TotalTextsAnalyzed => totalTextsAnalyzed;

        public Action<object, StatusString> StatusUpdate { get; set; }

        public void IncrementTextStats(int words)
        {
            totalWordsProcessed += words;
            totalTextsAnalyzed++;
        }

        // Cargar funciones desde DLL externa
        public void LoadExternalDll(string dllPath)
        {
            functionManager.LoadExternalDll(dllPath);
        }

        // Registrar una función externa manualmente
        public void RegisterExternalFunction(string name, Func<object[], object> function)
        {
            functionManager.RegisterExternalFunction(name, function);
        }

        // Parsear y ejecutar el código
        public void Execute(string code)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code);

            var parser = new Parser(tokens);
            var ast = parser.Parse();

            var executor = new StatementExecutor(variables, functionManager);
            executor.ExecuteStatements(ast);
        }

        public void Dispose()
        {
            databaseManager?.Dispose();
        }
    }
}
