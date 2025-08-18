using System;

namespace MathFunctions
{
    [FunctEngineExport("Math Functions", "Biblioteca de funciones matemáticas con acceso a variables globales")]
    public static class MathLibrary
    {
        [FunctEngineExport("Add", "Suma dos números")]
        public static double Add(double a, double b)
        {
            return a + b;
        }

        [FunctEngineExport("Subtract", "Resta dos números")]
        public static double Subtract(double a, double b)
        {
            return a - b;
        }

        [FunctEngineExport("Multiply", "Multiplica dos números")]
        public static double Multiply(double a, double b)
        {
            return a * b;
        }

        [FunctEngineExport("Divide", "Divide dos números")]
        public static double Divide(double a, double b)
        {
            if (b == 0)
                throw new DivideByZeroException("División por cero no permitida");
            return a / b;
        }

        [FunctEngineExport("Sqrt", "Calcula la raíz cuadrada")]
        public static double Sqrt(double value)
        {
            if (value < 0)
                throw new ArgumentException("No se puede calcular la raíz cuadrada de un número negativo");
            return Math.Sqrt(value);
        }

        [FunctEngineExport("Power", "Eleva un número a una potencia")]
        public static double Power(double baseValue, double exponent)
        {
            return Math.Pow(baseValue, exponent);
        }

        [FunctEngineExport("Abs", "Valor absoluto de un número")]
        public static double Abs(double value)
        {
            return Math.Abs(value);
        }

        [FunctEngineExport("Sin", "Seno de un ángulo en radianes")]
        public static double Sin(double angle)
        {
            return Math.Sin(angle);
        }

        [FunctEngineExport("Cos", "Coseno de un ángulo en radianes")]
        public static double Cos(double angle)
        {
            return Math.Cos(angle);
        }

        [FunctEngineExport("Tan", "Tangente de un ángulo en radianes")]
        public static double Tan(double angle)
        {
            return Math.Tan(angle);
        }

        [FunctEngineExport("Log", "Logaritmo natural")]
        public static double Log(double value)
        {
            if (value <= 0)
                throw new ArgumentException("El logaritmo solo está definido para números positivos");
            return Math.Log(value);
        }

        [FunctEngineExport("Log10", "Logaritmo base 10")]
        public static double Log10(double value)
        {
            if (value <= 0)
                throw new ArgumentException("El logaritmo solo está definido para números positivos");
            return Math.Log10(value);
        }

        [FunctEngineExport("Factorial", "Calcula el factorial de un número")]
        public static long Factorial(int n)
        {
            if (n < 0)
                throw new ArgumentException("El factorial no está definido para números negativos");
            
            if (n == 0 || n == 1)
                return 1;
            
            long result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        [FunctEngineExport("Min", "Devuelve el menor de dos números")]
        public static double Min(double a, double b)
        {
            return Math.Min(a, b);
        }

        [FunctEngineExport("Max", "Devuelve el mayor de dos números")]
        public static double Max(double a, double b)
        {
            return Math.Max(a, b);
        }

        [FunctEngineExport("Round", "Redondea un número al entero más cercano")]
        public static double Round(double value)
        {
            return Math.Round(value);
        }

        [FunctEngineExport("Floor", "Devuelve el mayor entero menor o igual al número")]
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        [FunctEngineExport("Ceiling", "Devuelve el menor entero mayor o igual al número")]
        public static double Ceiling(double value)
        {
            return Math.Ceiling(value);
        }

        [FunctEngineExport("Random", "Genera un número aleatorio entre 0 y 1")]
        public static double Random()
        {
            return new Random().NextDouble();
        }

        [FunctEngineExport("RandomRange", "Genera un número aleatorio en un rango")]
        public static int RandomRange(int min, int max)
        {
            return new Random().Next(min, max + 1);
        }

        // === NUEVAS FUNCIONES CON ACCESO A VARIABLES GLOBALES ===

        [FunctEngineExport("Accumulate", "Suma un valor a la variable 'accumulator' (la crea si no existe)")]
        public static double Accumulate(double value)
        {
            double current = FunctEngineGlobalContext.GetVariable<double>("accumulator", 0);
            double result = current + value;
            FunctEngineGlobalContext.SetVariable("accumulator", result);
            return result;
        }

        [FunctEngineExport("Counter", "Incrementa un contador global y devuelve el nuevo valor")]
        public static int Counter(string counterName = "counter")
        {
            int current = FunctEngineGlobalContext.GetVariable<int>(counterName, 0);
            int result = current + 1;
            FunctEngineGlobalContext.SetVariable(counterName, result);
            return result;
        }

        [FunctEngineExport("Average", "Calcula el promedio usando variables globales 'sum' y 'count'")]
        public static double Average()
        {
            double sum = FunctEngineGlobalContext.GetVariable<double>("sum", 0);
            int count = FunctEngineGlobalContext.GetVariable<int>("count", 1); // Evitar división por 0
            return sum / count;
        }

        [FunctEngineExport("AddToSum", "Añade un valor a la variable 'sum' y incrementa 'count'")]
        public static double AddToSum(double value)
        {
            double currentSum = FunctEngineGlobalContext.GetVariable<double>("sum", 0);
            int currentCount = FunctEngineGlobalContext.GetVariable<int>("count", 0);
            
            FunctEngineGlobalContext.SetVariable("sum", currentSum + value);
            FunctEngineGlobalContext.SetVariable("count", currentCount + 1);
            
            return currentSum + value;
        }

        [FunctEngineExport("SaveToVar", "Guarda un resultado matemático en una variable global")]
        public static double SaveToVar(string varName, double value)
        {
            FunctEngineGlobalContext.SetVariable(varName, value);
            FunctEngineGlobalContext.Print($"💾 Guardado: {varName} = {value}");
            return value;
        }

        [FunctEngineExport("LoadFromVar", "Carga un valor numérico desde una variable global")]
        public static double LoadFromVar(string varName, double defaultValue = 0)
        {
            double value = FunctEngineGlobalContext.GetVariable<double>(varName, defaultValue);
            FunctEngineGlobalContext.Print($"📂 Cargado: {varName} = {value}");
            return value;
        }

        [FunctEngineExport("IncrementVar", "Incrementa una variable numérica global")]
        public static double IncrementVar(string varName, double increment = 1)
        {
            double current = FunctEngineGlobalContext.GetVariable<double>(varName, 0);
            double newValue = current + increment;
            FunctEngineGlobalContext.SetVariable(varName, newValue);
            return newValue;
        }

        [FunctEngineExport("MaxVar", "Guarda el máximo entre el valor actual de una variable y un nuevo valor")]
        public static double MaxVar(string varName, double value)
        {
            double current = FunctEngineGlobalContext.GetVariable<double>(varName, double.MinValue);
            double result = Math.Max(current, value);
            FunctEngineGlobalContext.SetVariable(varName, result);
            return result;
        }

        [FunctEngineExport("MinVar", "Guarda el mínimo entre el valor actual de una variable y un nuevo valor")]
        public static double MinVar(string varName, double value)
        {
            double current = FunctEngineGlobalContext.GetVariable<double>(varName, double.MaxValue);
            double result = Math.Min(current, value);
            FunctEngineGlobalContext.SetVariable(varName, result);
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