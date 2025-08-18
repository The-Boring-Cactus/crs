using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class MathFunctions
    {
        private readonly Random random = new Random();

        public object Subtract(object[] args)
        {
            return Convert.ToDouble(args[0]) - Convert.ToDouble(args[1]);
        }

        public object Divide(object[] args)
        {
            return Convert.ToDouble(args[0]) / Convert.ToDouble(args[1]);
        }

        public object Power(object[] args)
        {
            return Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]));
        }

        public object Sqrt(object[] args)
        {
            return Math.Sqrt(Convert.ToDouble(args[0]));
        }

        public object Abs(object[] args)
        {
            return Math.Abs(Convert.ToDouble(args[0]));
        }

        public object Floor(object[] args)
        {
            return Math.Floor(Convert.ToDouble(args[0]));
        }

        public object Ceil(object[] args)
        {
            return Math.Ceiling(Convert.ToDouble(args[0]));
        }

        public object Round(object[] args)
        {
            double value = Convert.ToDouble(args[0]);
            int decimals = args.Length > 1 ? Convert.ToInt32(args[1]) : 0;
            return Math.Round(value, decimals);
        }

        public object Sin(object[] args)
        {
            return Math.Sin(Convert.ToDouble(args[0]));
        }

        public object Cos(object[] args)
        {
            return Math.Cos(Convert.ToDouble(args[0]));
        }

        public object Tan(object[] args)
        {
            return Math.Tan(Convert.ToDouble(args[0]));
        }

        public object Log(object[] args)
        {
            return Math.Log(Convert.ToDouble(args[0]));
        }

        public object Log10(object[] args)
        {
            return Math.Log10(Convert.ToDouble(args[0]));
        }

        public object Exp(object[] args)
        {
            return Math.Exp(Convert.ToDouble(args[0]));
        }

        public object Min(object[] args)
        {
            return args.Select(Convert.ToDouble).Min();
        }

        public object Max(object[] args)
        {
            return args.Select(Convert.ToDouble).Max();
        }

        public object Random(object[] args)
        {
            if (args.Length == 0) return random.NextDouble();
            if (args.Length == 1) return random.Next(Convert.ToInt32(args[0]));
            return random.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
        }

        public object PI(object[] args)
        {
            return Math.PI;
        }

        public object E(object[] args)
        {
            return Math.E;
        }
    }
}
