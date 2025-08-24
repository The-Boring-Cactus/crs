using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class CodeEngine : IDisposable
    {
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Dictionary<string, int> counters = new Dictionary<string, int>();
        private readonly FunctionManager functionManager;
        private readonly DatabaseManager databaseManager;

        // Variables para estadísticas de texto
        private int totalWordsProcessed = 0;
        private int totalTextsAnalyzed = 0;

        public List<string> GetFunctions()
        {
            return functionManager.GetFunctionNames();
        }
        public CodeEngine()
        {
            functionManager = new FunctionManager(variables, counters, this);
            databaseManager = new DatabaseManager(this);
            functionManager.SetDatabaseManager(databaseManager);
            functionManager.InitializeBuiltInFunctions();
        }

        // Propiedades públicas para acceso a estadísticas
        public int TotalWordsProcessed => totalWordsProcessed;
        public int TotalTextsAnalyzed => totalTextsAnalyzed;

        public event StatusUpdateHandler StatusUpdate;
        
        public void PrintCore(string msg)
        {
            if(StatusUpdate != null)
            {
                var e = new StatusString(msg);
                StatusUpdate(this, e);
            }
        }
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
            variables = new Dictionary<string, object>();
            counters = new Dictionary<string, int>();
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code);

            var parser = new Parser(tokens);
            var ast = parser.Parse(this);

            var executor = new StatementExecutor(variables, functionManager);
            executor.ExecuteStatements(ast.Statements);
        }

        public void Dispose()
        {
            databaseManager?.Dispose();
        }
    }
}
