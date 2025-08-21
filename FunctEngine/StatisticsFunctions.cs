using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    
    public class StatisticsFunctions(CodeEngine engine)
    {
        public object Mean(object[] args)
        {
            var numbers = GetNumbersFromArgs(args);
            return numbers.Count > 0 ? numbers.Average() : 0;
        }

        public object Median(object[] args)
        {
            var numbers = GetNumbersFromArgs(args).OrderBy(x => x).ToList();
            if (numbers.Count == 0) return 0;
            int mid = numbers.Count / 2;
            return numbers.Count % 2 == 0 ? (numbers[mid - 1] + numbers[mid]) / 2 : numbers[mid];
        }

        public object Mode(object[] args)
        {
            var numbers = GetNumbersFromArgs(args);
            if (numbers.Count == 0) return 0;
            var groups = numbers.GroupBy(x => x);
            var maxCount = groups.Max(g => g.Count());
            return groups.Where(g => g.Count() == maxCount).Select(g => g.Key).First();
        }

        public object Range(object[] args)
        {
            var numbers = GetNumbersFromArgs(args);
            return numbers.Count > 0 ? numbers.Max() - numbers.Min() : 0;
        }

        public object Variance(object[] args)
        {
            var numbers = GetNumbersFromArgs(args);
            if (numbers.Count <= 1) return 0;
            double mean = numbers.Average();
            return numbers.Sum(x => Math.Pow(x - mean, 2)) / numbers.Count;
        }

        public object StandardDeviation(object[] args)
        {
            var numbers = GetNumbersFromArgs(args);
            if (numbers.Count <= 1) return 0;
            double mean = numbers.Average();
            double variance = numbers.Sum(x => Math.Pow(x - mean, 2)) / numbers.Count;
            return Math.Sqrt(variance);
        }

        public object Sum(object[] args)
        {
            return GetNumbersFromArgs(args).Sum();
        }

        public object Count(object[] args)
        {
            return GetNumbersFromArgs(args).Count;
        }

        public object Percentile(object[] args)
        {
            var numbers = GetNumbersFromArgs(args.Take(args.Length - 1).ToArray()).OrderBy(x => x).ToList();
            double percentile = Convert.ToDouble(args[args.Length - 1]);
            if (numbers.Count == 0) return 0;
            double index = (percentile / 100.0) * (numbers.Count - 1);
            int lowerIndex = (int)Math.Floor(index);
            int upperIndex = (int)Math.Ceiling(index);
            if (lowerIndex == upperIndex) return numbers[lowerIndex];
            double weight = index - lowerIndex;
            return numbers[lowerIndex] * (1 - weight) + numbers[upperIndex] * weight;
        }

        public object Quartile(object[] args)
        {
            int quartile = Convert.ToInt32(args[args.Length - 1]);
            double percentile = quartile * 25.0;
            var newArgs = args.Take(args.Length - 1).Concat(new object[] { percentile }).ToArray();
            return Percentile(newArgs);
        }

        public object Correlation(object[] args)
        {
            var numbers1 = GetNumbersFromList(args[0]);
            var numbers2 = GetNumbersFromList(args[1]);
            if (numbers1.Count != numbers2.Count || numbers1.Count < 2) return 0;

            double mean1 = numbers1.Average();
            double mean2 = numbers2.Average();
            double numerator = numbers1.Zip(numbers2, (x, y) => (x - mean1) * (y - mean2)).Sum();
            double denominator = Math.Sqrt(numbers1.Sum(x => Math.Pow(x - mean1, 2)) * numbers2.Sum(y => Math.Pow(y - mean2, 2)));
            return denominator == 0 ? 0 : numerator / denominator;
        }

        public object ZScore(object[] args)
        {
            double value = Convert.ToDouble(args[0]);
            var numbers = GetNumbersFromArgs(args.Skip(1).ToArray());
            if (numbers.Count <= 1) return 0;
            double mean = numbers.Average();
            double stdDev = Math.Sqrt(numbers.Sum(x => Math.Pow(x - mean, 2)) / numbers.Count);
            return stdDev == 0 ? 0 : (value - mean) / stdDev;
        }

        private List<double> GetNumbersFromArgs(object[] args)
        {
            var numbers = new List<double>();
            foreach (var arg in args)
            {
                if (arg is List<object> list)
                {
                    numbers.AddRange(GetNumbersFromList(list));
                }
                else if (double.TryParse(arg?.ToString(), out double value))
                {
                    numbers.Add(value);
                }
            }
            return numbers;
        }

        private List<double> GetNumbersFromList(object listObj)
        {
            var numbers = new List<double>();
            if (listObj is List<object> list)
            {
                foreach (var item in list)
                {
                    if (double.TryParse(item?.ToString(), out double value))
                    {
                        numbers.Add(value);
                    }
                }
            }
            return numbers;
        }
        /// <summary>
        /// Imprime el histograma en formato de texto simple
        /// </summary>
        public object PrintHistogram(object[] args)
        {
            List<StatsHelper.Bin> histograma = (List<StatsHelper.Bin>)args[0];
            int anchoMaximo = args[1] == null ? 50 : Convert.ToInt32(args[1]);
                
            if (histograma == null || histograma.Count == 0)
                return null;
        
            int maxFrecuencia = histograma.Max(b => b.Frecuencia);
        
            engine.PrintCore("\nHistograma:");
            engine.PrintCore(new string('-', 60));
        
            foreach (var bin in histograma)
            {
                int anchoBarra = maxFrecuencia > 0 
                    ? (int)((double)bin.Frecuencia / maxFrecuencia * anchoMaximo) 
                    : 0;
            
                string barra = new string('█', anchoBarra);
                engine.PrintCore($"{bin.LimiteInferior,8:F2} - {bin.LimiteSuperior,8:F2} | {barra} {bin.Frecuencia}");
            }

            return null;
        }
        // <summary>
        /// Calcula un histograma a partir de una lista de valores doubles
        /// </summary>
        /// <param name="datos">Lista de valores double para crear el histograma</param>
        /// <param name="numeroBins">Número de bins (intervalos) deseados</param>
        /// <returns>Lista de bins con sus frecuencias</returns>
        public object CreateHistogram(object[] args)
        {
            List<double> datos =args[0] as List<double>;
            int numeroBins = args[1] is int ? (int)args[1] : 0;
            if (datos == null || datos.Count == 0)
                throw new ArgumentException("La lista de datos no puede estar vacía");
        
            if (numeroBins <= 0)
                throw new ArgumentException("El número de bins debe ser mayor que cero");
        
            // Encontrar valores mínimo y máximo
            double min = datos.Min();
            double max = datos.Max();
        
            // Si todos los valores son iguales
            if (min == max)
            {
                return new List<StatsHelper.Bin> 
                { 
                    new StatsHelper.Bin 
                    { 
                        LimiteInferior = min - 0.5, 
                        LimiteSuperior = max + 0.5, 
                        Frecuencia = datos.Count 
                    } 
                };
            }
        
            // Calcular el ancho de cada bin
            double anchoBin = (max - min) / numeroBins;
        
            // Inicializar los bins
            List<StatsHelper.Bin> histograma = new List<StatsHelper.Bin>();
            for (int i = 0; i < numeroBins; i++)
            {
                histograma.Add(new StatsHelper.Bin
                {
                    LimiteInferior = min + (i * anchoBin),
                    LimiteSuperior = min + ((i + 1) * anchoBin),
                    Frecuencia = 0
                });
            }
        
            // Ajustar el límite superior del último bin para incluir el valor máximo
            histograma[numeroBins - 1].LimiteSuperior = max + double.Epsilon;
        
            // Contar frecuencias
            foreach (double valor in datos)
            {
                // Encontrar el bin correspondiente
                for (int i = 0; i < numeroBins; i++)
                {
                    if (valor >= histograma[i].LimiteInferior && valor < histograma[i].LimiteSuperior)
                    {
                        histograma[i].Frecuencia++;
                        break;
                    }
                }
            }
        
            return histograma;
        }
        
    }
}
