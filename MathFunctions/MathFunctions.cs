using FunctEngine;
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

        
    }

    
}

