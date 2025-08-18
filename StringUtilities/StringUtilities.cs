using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StringUtilities
{
    [FunctEngineExport("String Utilities", "Biblioteca de utilidades para strings con acceso a variables globales")]
    public static class StringLibrary
    {
        [FunctEngineExport("Concat", "Concatena dos strings")]
        public static string Concat(string str1, string str2)
        {
            return (str1 ?? "") + (str2 ?? "");
        }

        [FunctEngineExport("Length", "Devuelve la longitud de un string")]
        public static int Length(string str)
        {
            return str?.Length ?? 0;
        }

        [FunctEngineExport("ToUpper", "Convierte string a mayúsculas")]
        public static string ToUpper(string str)
        {
            return str?.ToUpper() ?? "";
        }

        [FunctEngineExport("ToLower", "Convierte string a minúsculas")]
        public static string ToLower(string str)
        {
            return str?.ToLower() ?? "";
        }

        [FunctEngineExport("Substring", "Extrae una subcadena")]
        public static string Substring(string str, int startIndex, int length)
        {
            if (str == null) return "";
            
            if (startIndex < 0 || startIndex >= str.Length)
                return "";
            
            int actualLength = Math.Min(length, str.Length - startIndex);
            return str.Substring(startIndex, actualLength);
        }

        [FunctEngineExport("IndexOf", "Busca la posición de una subcadena")]
        public static int IndexOf(string str, string searchStr)
        {
            if (str == null || searchStr == null)
                return -1;
            
            return str.IndexOf(searchStr);
        }

        [FunctEngineExport("Replace", "Reemplaza todas las ocurrencias de una subcadena")]
        public static string Replace(string str, string oldValue, string newValue)
        {
            if (str == null) return "";
            if (oldValue == null) return str;
            
            return str.Replace(oldValue, newValue ?? "");
        }

        [FunctEngineExport("Trim", "Elimina espacios al inicio y final")]
        public static string Trim(string str)
        {
            return str?.Trim() ?? "";
        }

        [FunctEngineExport("StartsWith", "Verifica si string comienza con un prefijo")]
        public static bool StartsWith(string str, string prefix)
        {
            if (str == null || prefix == null)
                return false;
            
            return str.StartsWith(prefix);
        }

        [FunctEngineExport("EndsWith", "Verifica si string termina con un sufijo")]
        public static bool EndsWith(string str, string suffix)
        {
            if (str == null || suffix == null)
                return false;
            
            return str.EndsWith(suffix);
        }

        [FunctEngineExport("Contains", "Verifica si string contiene una subcadena")]
        public static bool Contains(string str, string searchStr)
        {
            if (str == null || searchStr == null)
                return false;
            
            return str.Contains(searchStr);
        }

        [FunctEngineExport("Split", "Divide un string usando un separador")]
        public static string[] Split(string str, string separator)
        {
            if (str == null) return new string[0];
            if (separator == null) separator = " ";
            
            return str.Split(new[] { separator }, StringSplitOptions.None);
        }

        [FunctEngineExport("Join", "Une elementos de un array con un separador")]
        public static string Join(string separator, string[] values)
        {
            if (values == null) return "";
            return string.Join(separator ?? "", values);
        }

        [FunctEngineExport("Reverse", "Invierte un string")]
        public static string Reverse(string str)
        {
            if (str == null) return "";
            return new string(str.Reverse().ToArray());
        }

        [FunctEngineExport("PadLeft", "Rellena string a la izquierda hasta una longitud")]
        public static string PadLeft(string str, int totalLength, char paddingChar = ' ')
        {
            if (str == null) str = "";
            return str.PadLeft(totalLength, paddingChar);
        }

        [FunctEngineExport("PadRight", "Rellena string a la derecha hasta una longitud")]
        public static string PadRight(string str, int totalLength, char paddingChar = ' ')
        {
            if (str == null) str = "";
            return str.PadRight(totalLength, paddingChar);
        }

        [FunctEngineExport("IsEmpty", "Verifica si un string está vacío o es null")]
        public static bool IsEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        [FunctEngineExport("IsWhitespace", "Verifica si un string solo contiene espacios")]
        public static bool IsWhitespace(string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        [FunctEngineExport("Repeat", "Repite un string n veces")]
        public static string Repeat(string str, int count)
        {
            if (str == null || count <= 0) return "";
            
            return string.Concat(Enumerable.Repeat(str, count));
        }

        [FunctEngineExport("RemoveWhitespace", "Elimina todos los espacios de un string")]
        public static string RemoveWhitespace(string str)
        {
            if (str == null) return "";
            return Regex.Replace(str, @"\s+", "");
        }

        [FunctEngineExport("CountOccurrences", "Cuenta las ocurrencias de una subcadena")]
        public static int CountOccurrences(string str, string searchStr)
        {
            if (str == null || searchStr == null || searchStr.Length == 0)
                return 0;
            
            int count = 0;
            int index = 0;
            
            while ((index = str.IndexOf(searchStr, index)) != -1)
            {
                count++;
                index += searchStr.Length;
            }
            
            return count;
        }

        [FunctEngineExport("IsNumeric", "Verifica si un string representa un número")]
        public static bool IsNumeric(string str)
        {
            if (str == null) return false;
            return double.TryParse(str, out _);
        }

        [FunctEngineExport("ToTitleCase", "Convierte a formato Title Case")]
        public static string ToTitleCase(string str)
        {
            if (str == null) return "";
            
            var words = str.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + 
                              (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
                }
            }
            
            return string.Join(" ", words);
        }

        [FunctEngineExport("RemoveSpecialChars", "Elimina caracteres especiales, mantiene solo letras y números")]
        public static string RemoveSpecialChars(string str)
        {
            if (str == null) return "";
            return Regex.Replace(str, @"[^a-zA-Z0-9\s]", "");
        }

        [FunctEngineExport("GetRandomString", "Genera un string aleatorio de longitud especificada")]
        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [FunctEngineExport("WordCount", "Cuenta las palabras en un string")]
        public static int WordCount(string str)
        {
            if (str == null) return 0;
            
            return str.Split(new char[] { ' ', '\t', '\n', '\r' }, 
                           StringSplitOptions.RemoveEmptyEntries).Length;
        }

        // === NUEVAS FUNCIONES CON ACCESO A VARIABLES GLOBALES ===

        [FunctEngineExport("AppendToBuffer", "Añade texto a una variable 'buffer' (la crea si no existe)")]
        public static string AppendToBuffer(string text)
        {
            string current = FunctEngineGlobalContext.GetVariable<string>("buffer", "");
            string result = current + text;
            FunctEngineGlobalContext.SetVariable("buffer", result);
            return result;
        }

        [FunctEngineExport("ClearBuffer", "Limpia la variable 'buffer'")]
        public static string ClearBuffer()
        {
            FunctEngineGlobalContext.SetVariable("buffer", "");
            return "";
        }

        [FunctEngineExport("GetBuffer", "Obtiene el contenido actual del 'buffer'")]
        public static string GetBuffer()
        {
            return FunctEngineGlobalContext.GetVariable<string>("buffer", "");
        }

        [FunctEngineExport("SaveStringToVar", "Guarda un string en una variable global")]
        public static string SaveStringToVar(string varName, string value)
        {
            FunctEngineGlobalContext.SetVariable(varName, value);
            FunctEngineGlobalContext.Print($"💾 String guardado: {varName} = \"{value}\"");
            return value;
        }

        [FunctEngineExport("LoadStringFromVar", "Carga un string desde una variable global")]
        public static string LoadStringFromVar(string varName, string defaultValue = "")
        {
            string value = FunctEngineGlobalContext.GetVariable<string>(varName, defaultValue);
            FunctEngineGlobalContext.Print($"📂 String cargado: {varName} = \"{value}\"");
            return value;
        }

        [FunctEngineExport("BuildMessage", "Construye un mensaje usando plantillas desde variables")]
        public static string BuildMessage(string template)
        {
            if (template == null) return "";
            
            string result = template;
            var variables = FunctEngineGlobalContext.GetAllVariables();
            
            foreach (var kvp in variables)
            {
                string placeholder = "{" + kvp.Key + "}";
                if (result.Contains(placeholder))
                {
                    result = result.Replace(placeholder, kvp.Value?.ToString() ?? "null");
                }
            }
            
            return result;
        }

        [FunctEngineExport("AddToLog", "Añade una línea al log global con timestamp")]
        public static string AddToLog(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}";
            
            string currentLog = FunctEngineGlobalContext.GetVariable<string>("log", "");
            string newLog = string.IsNullOrEmpty(currentLog) ? logEntry : currentLog + "\n" + logEntry;
            
            FunctEngineGlobalContext.SetVariable("log", newLog);
            return logEntry;
        }

        [FunctEngineExport("GetLog", "Obtiene el contenido completo del log")]
        public static string GetLog()
        {
            return FunctEngineGlobalContext.GetVariable<string>("log", "");
        }

        [FunctEngineExport("ClearLog", "Limpia el log global")]
        public static string ClearLog()
        {
            FunctEngineGlobalContext.SetVariable("log", "");
            return "Log limpiado";
        }

        [FunctEngineExport("PrintLog", "Imprime el log completo en consola")]
        public static void PrintLog()
        {
            string log = FunctEngineGlobalContext.GetVariable<string>("log", "");
            if (string.IsNullOrEmpty(log))
            {
                FunctEngineGlobalContext.Print("📄 Log vacío");
            }
            else
            {
                FunctEngineGlobalContext.Print("📄 === LOG COMPLETO ===");
                FunctEngineGlobalContext.Print(log);
                FunctEngineGlobalContext.Print("📄 === FIN DEL LOG ===");
            }
        }

        [FunctEngineExport("CountWords", "Cuenta palabras y guarda estadísticas en variables globales")]
        public static int CountWords(string text)
        {
            if (text == null) return 0;
            
            int wordCount = WordCount(text);
            int charCount = text.Length;
            int lineCount = text.Split('\n').Length;
            
            // Guardar estadísticas
            FunctEngineGlobalContext.SetVariable("last_word_count", wordCount);
            FunctEngineGlobalContext.SetVariable("last_char_count", charCount);
            FunctEngineGlobalContext.SetVariable("last_line_count", lineCount);
            
            // Actualizar totales acumulados
            int totalWords = FunctEngineGlobalContext.GetVariable<int>("total_words_processed", 0);
            int totalChars = FunctEngineGlobalContext.GetVariable<int>("total_chars_processed", 0);
            
            FunctEngineGlobalContext.SetVariable("total_words_processed", totalWords + wordCount);
            FunctEngineGlobalContext.SetVariable("total_chars_processed", totalChars + charCount);
            
            return wordCount;
        }

        [FunctEngineExport("GetTextStats", "Muestra estadísticas de texto procesado")]
        public static void GetTextStats()
        {
            int lastWords = FunctEngineGlobalContext.GetVariable<int>("last_word_count", 0);
            int lastChars = FunctEngineGlobalContext.GetVariable<int>("last_char_count", 0);
            int lastLines = FunctEngineGlobalContext.GetVariable<int>("last_line_count", 0);
            int totalWords = FunctEngineGlobalContext.GetVariable<int>("total_words_processed", 0);
            int totalChars = FunctEngineGlobalContext.GetVariable<int>("total_chars_processed", 0);
            
            FunctEngineGlobalContext.Print("📊 === ESTADÍSTICAS DE TEXTO ===");
            FunctEngineGlobalContext.Print($"Último texto analizado:");
            FunctEngineGlobalContext.Print($"  - Palabras: {lastWords}");
            FunctEngineGlobalContext.Print($"  - Caracteres: {lastChars}");
            FunctEngineGlobalContext.Print($"  - Líneas: {lastLines}");
            FunctEngineGlobalContext.Print($"Total procesado:");
            FunctEngineGlobalContext.Print($"  - Palabras totales: {totalWords}");
            FunctEngineGlobalContext.Print($"  - Caracteres totales: {totalChars}");
        }

        [FunctEngineExport("FilterAndStore", "Filtra texto por criterios y almacena resultado")]
        public static string FilterAndStore(string text, string filterType, string varName)
        {
            if (text == null) return "";
            
            string result = filterType.ToLower() switch
            {
                "numbers" => Regex.Replace(text, @"[^0-9]", ""),
                "letters" => Regex.Replace(text, @"[^a-zA-Z]", ""),
                "alphanumeric" => Regex.Replace(text, @"[^a-zA-Z0-9]", ""),
                "uppercase" => Regex.Replace(text, @"[^A-Z]", ""),
                "lowercase" => Regex.Replace(text, @"[^a-z]", ""),
                "vowels" => Regex.Replace(text, @"[^aeiouAEIOU]", ""),
                "consonants" => Regex.Replace(text, @"[aeiouAEIOU0-9\W]", "", RegexOptions.IgnoreCase),
                _ => text
            };
            
            FunctEngineGlobalContext.SetVariable(varName, result);
            FunctEngineGlobalContext.Print($"🔍 Filtrado '{filterType}' guardado en '{varName}': \"{result}\"");
            
            return result;
        }
    }

    // Clase de acceso a contexto global (debe coincidir con el del intérprete)
    public static class FunctEngineGlobalContext
    {
        public static object GetVariable(string name)
        {
            // En un DLL externo, necesitarías usar reflexión o un mecanismo de callback
            // Por simplicidad, aquí asumimos que hay una referencia al intérprete
            throw new NotImplementedException("Debe ser implementado por el host");
        }

        public static void SetVariable(string name, object value)
        {
            throw new NotImplementedException("Debe ser implementado por el host");
        }

        public static bool HasVariable(string name)
        {
            throw new NotImplementedException("Debe ser implementado por el host");
        }

        public static void RemoveVariable(string name)
        {
            throw new NotImplementedException("Debe ser implementado por el host");
        }

        public static Dictionary<string, object> GetAllVariables()
        {
            throw new NotImplementedException("Debe ser implementado por el host");
        }

        public static void Print(object value)
        {
            Console.WriteLine(value?.ToString() ?? "null");
        }

        public static T GetVariable<T>(string name, T defaultValue = default(T))
        {
            try
            {
                var value = GetVariable(name);
                if (value == null) return defaultValue;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)value.ToString();
                
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}

// Atributo para compatibilidad (debe coincidir con el del intérprete)
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FunctEngineExportAttribute : Attribute
{
    public string Name { get; set; }
    public string Description { get; set; }

    public FunctEngineExportAttribute(string name = null, string description = "")
    {
        Name = name;
        Description = description;
    }
}

