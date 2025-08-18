using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class BasicFunctions
    {
        private readonly CodeEngine engine;

        public BasicFunctions(CodeEngine engine)
        {
            this.engine = engine;
        }

        public object Print(object[] args)
        {
            Console.WriteLine(args.Length > 0 ? args[0]?.ToString() ?? "" : "");
            return null;
        }

        public object Concat(object[] args)
        {
            return string.Join("", args.Select(a => a?.ToString() ?? ""));
        }

        public object ToString(object[] args)
        {
            return args.Length > 0 ? args[0]?.ToString() ?? "" : "";
        }

        public object Add(object[] args)
        {
            return Convert.ToDouble(args[0]) + Convert.ToDouble(args[1]);
        }

        public object Multiply(object[] args)
        {
            return Convert.ToDouble(args[0]) * Convert.ToDouble(args[1]);
        }

        public object CountWords(object[] args)
        {
            if (args.Length == 0) return 0;
            string text = args[0]?.ToString() ?? "";
            var words = text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int wordCount = words.Length;
            engine.IncrementTextStats(wordCount);
            return wordCount;
        }

        public object GetTextStats(object[] args)
        {
            Console.WriteLine($"=== ESTADÍSTICAS DE TEXTO ===");
            Console.WriteLine($"Textos analizados: {engine.TotalTextsAnalyzed}");
            Console.WriteLine($"Total de palabras procesadas: {engine.TotalWordsProcessed}");
            Console.WriteLine($"Promedio de palabras por texto: {(engine.TotalTextsAnalyzed > 0 ? (double)engine.TotalWordsProcessed / engine.TotalTextsAnalyzed : 0):F2}");
            return null;
        }
    }
}
