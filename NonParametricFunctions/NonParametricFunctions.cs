using FunctEngine;
using System;
using System.Linq;

namespace NonParametricFunctions
{
    [FunctEngineExport("Non-Parametric Tests", "Biblioteca de pruebas estadísticas no paramétricas")]
    public static class NonParametricLibrary
    {
        // ─── Rank-Based Two-Sample Tests ────────────────────────────────────────

        [FunctEngineExport("MannWhitneyUTest", "Prueba U de Mann-Whitney (equivalente a Wilcoxon rank-sum) para dos muestras independientes")]
        public static (double u, double zStatistic, double pValue) MannWhitneyUTest(double[] a, double[] b)
        {
            int n1 = a.Length, n2 = b.Length;
            if (n1 == 0 || n2 == 0)
                throw new ArgumentException("Ambos grupos deben tener al menos una observación");

            var combined = a.Concat(b).ToArray();
            var ranks = Rank(combined);

            double r1 = ranks.Take(n1).Sum();
            double u1 = r1 - n1 * (n1 + 1) / 2.0;
            double u2 = (double)n1 * n2 - u1;
            double u = Math.Min(u1, u2);

            int nTotal = n1 + n2;
            double meanU = n1 * n2 / 2.0;
            double tieSum = TieCorrectionSum(combined);
            double varU = (double)n1 * n2 / 12.0 * ((nTotal + 1) - tieSum / (nTotal * (nTotal - 1)));

            double z = varU > 0 ? (u1 - meanU) / Math.Sqrt(varU) : 0;
            double pValue = 2.0 * (1.0 - NormalCDF(Math.Abs(z)));

            return (u, z, pValue);
        }

        [FunctEngineExport("TwoSampleKsTest", "Prueba de Kolmogorov-Smirnov de dos muestras: compara las distribuciones de dos conjuntos de datos")]
        public static (double dStatistic, double pValue) TwoSampleKsTest(double[] a, double[] b)
        {
            int n1 = a.Length, n2 = b.Length;
            if (n1 == 0 || n2 == 0)
                throw new ArgumentException("Ambas muestras deben tener al menos una observación");

            var sortedA = a.OrderBy(x => x).ToArray();
            var sortedB = b.OrderBy(x => x).ToArray();
            var allValues = sortedA.Concat(sortedB).Distinct().OrderBy(x => x).ToArray();

            double d = 0;
            foreach (var v in allValues)
            {
                double cdfA = sortedA.Count(x => x <= v) / (double)n1;
                double cdfB = sortedB.Count(x => x <= v) / (double)n2;
                d = Math.Max(d, Math.Abs(cdfA - cdfB));
            }

            double nEff = (double)n1 * n2 / (n1 + n2);
            double pValue = KolmogorovAsymptoticPValue(nEff, d);

            return (d, pValue);
        }

        [FunctEngineExport("WilcoxonSignedRankTest", "Prueba de rangos con signo de Wilcoxon para datos pareados (x - y)")]
        public static (double wStatistic, double zStatistic, double pValue) WilcoxonSignedRankTest(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Las muestras pareadas deben tener la misma longitud");

            var diffs = x.Zip(y, (xi, yi) => xi - yi).Where(d => d != 0).ToArray();
            int n = diffs.Length;
            if (n == 0)
                throw new ArgumentException("No hay diferencias distintas de cero");

            var absDiffs = diffs.Select(d => Math.Abs(d)).ToArray();
            var ranks = Rank(absDiffs);

            double wPlus = 0, wMinus = 0;
            for (int i = 0; i < n; i++)
            {
                if (diffs[i] > 0) wPlus += ranks[i];
                else wMinus += ranks[i];
            }
            double w = Math.Min(wPlus, wMinus);

            double meanW = n * (n + 1) / 4.0;
            double tieSum = TieCorrectionSum(absDiffs);
            double varW = n * (n + 1) * (2 * n + 1) / 24.0 - tieSum / 48.0;

            double z = varW > 0 ? (w - meanW) / Math.Sqrt(varW) : 0;
            double pValue = 2.0 * (1.0 - NormalCDF(Math.Abs(z)));

            return (w, z, pValue);
        }

        [FunctEngineExport("McNemarTest", "Prueba de McNemar (con corrección de continuidad) para una tabla 2x2 de datos pareados")]
        public static (double chiSquared, double pValue) McNemarTest(double[][] table)
        {
            if (table.Length != 2 || table[0].Length != 2 || table[1].Length != 2)
                throw new ArgumentException("Se requiere una tabla 2x2");

            double b = table[0][1];
            double c = table[1][0];
            if (b + c == 0)
                throw new ArgumentException("No hay pares discordantes (b + c = 0)");

            double chiSquared = Math.Pow(Math.Abs(b - c) - 1, 2) / (b + c);
            double pValue = ChiSquarePValue(chiSquared, 1);

            return (chiSquared, pValue);
        }

        // ─── Rank-Based Multi-Sample Tests ──────────────────────────────────────

        [FunctEngineExport("KruskalWallisTest", "Prueba H de Kruskal-Wallis: ANOVA no paramétrico de una vía para k grupos independientes")]
        public static (double hStatistic, int degreesOfFreedom, double pValue) KruskalWallisTest(double[][] groups)
        {
            int k = groups.Length;
            if (k < 2)
                throw new ArgumentException("Se requieren al menos 2 grupos");

            var combined = groups.SelectMany(g => g).ToArray();
            int nTotal = combined.Length;
            var ranks = Rank(combined);

            double h = 0;
            int offset = 0;
            foreach (var group in groups)
            {
                int ni = group.Length;
                if (ni == 0)
                    throw new ArgumentException("Cada grupo debe tener al menos una observación");

                double rankSum = 0;
                for (int i = 0; i < ni; i++) rankSum += ranks[offset + i];
                offset += ni;
                h += (rankSum * rankSum) / ni;
            }
            h = 12.0 / (nTotal * (nTotal + 1)) * h - 3 * (nTotal + 1);

            double tieSum = TieCorrectionSum(combined);
            double correction = 1 - tieSum / (Math.Pow(nTotal, 3) - nTotal);
            if (correction > 0) h /= correction;

            int df = k - 1;
            double pValue = ChiSquarePValue(h, df);

            return (h, df, pValue);
        }

        [FunctEngineExport("FriedmanTest", "Prueba de Friedman: ANOVA no paramétrico de dos vías por rangos para medidas repetidas (filas=bloques, columnas=tratamientos)")]
        public static (double chiSquared, int degreesOfFreedom, double pValue) FriedmanTest(double[][] data)
        {
            int n = data.Length;
            if (n < 2)
                throw new ArgumentException("Se requieren al menos 2 bloques (filas)");
            int k = data[0].Length;
            if (k < 2)
                throw new ArgumentException("Se requieren al menos 2 tratamientos (columnas)");

            var rankSums = new double[k];
            foreach (var row in data)
            {
                if (row.Length != k)
                    throw new ArgumentException("Todas las filas deben tener el mismo número de columnas");

                var ranks = Rank(row);
                for (int j = 0; j < k; j++) rankSums[j] += ranks[j];
            }

            double sumSq = rankSums.Sum(r => r * r);
            double chiSquared = (12.0 / (n * k * (k + 1))) * sumSq - 3.0 * n * (k + 1);

            int df = k - 1;
            double pValue = ChiSquarePValue(chiSquared, df);

            return (chiSquared, df, pValue);
        }

        // ─── Correlation ────────────────────────────────────────────────────────

        [FunctEngineExport("SpearmanRankCorrelation", "Coeficiente de correlación de rangos de Spearman (rho) y su valor p")]
        public static (double rho, double pValue) SpearmanRankCorrelation(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Las muestras deben tener la misma longitud");
            int n = x.Length;
            if (n < 3)
                throw new ArgumentException("Se requieren al menos 3 observaciones");

            var rankX = Rank(x);
            var rankY = Rank(y);

            double meanRX = rankX.Average();
            double meanRY = rankY.Average();

            double cov = 0, varX = 0, varY = 0;
            for (int i = 0; i < n; i++)
            {
                double dx = rankX[i] - meanRX;
                double dy = rankY[i] - meanRY;
                cov += dx * dy;
                varX += dx * dx;
                varY += dy * dy;
            }

            double denom = Math.Sqrt(varX * varY);
            double rho = denom > 0 ? cov / denom : 0;

            double denomT = 1 - rho * rho;
            double pValue;
            if (denomT <= 0)
            {
                pValue = 0;
            }
            else
            {
                double t = rho * Math.Sqrt((n - 2) / denomT);
                pValue = TDistPValue(Math.Abs(t), n - 2);
            }

            return (rho, pValue);
        }

        [FunctEngineExport("KendallTau", "Tau-b de Kendall (ajustada por empates) con valor p por aproximación normal")]
        public static (double tau, double zStatistic, double pValue) KendallTau(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Las muestras deben tener la misma longitud");
            int n = x.Length;
            if (n < 3)
                throw new ArgumentException("Se requieren al menos 3 observaciones");

            long concordant = 0, discordant = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double signX = Math.Sign(x[i] - x[j]);
                    double signY = Math.Sign(y[i] - y[j]);
                    double product = signX * signY;
                    if (product > 0) concordant++;
                    else if (product < 0) discordant++;
                }
            }

            double n0 = n * (n - 1) / 2.0;
            double tiesX = TieBreakdown(x);
            double tiesY = TieBreakdown(y);

            double denom = Math.Sqrt((n0 - tiesX) * (n0 - tiesY));
            double tau = denom > 0 ? (concordant - discordant) / denom : 0;

            double varTau = 2.0 * (2 * n + 5) / (9.0 * n * (n - 1));
            double z = varTau > 0 ? tau / Math.Sqrt(varTau) : 0;
            double pValue = 2.0 * (1.0 - NormalCDF(Math.Abs(z)));

            return (tau, z, pValue);
        }

        // ─── Contingency Tables & Goodness-of-Fit ──────────────────────────────

        [FunctEngineExport("ChiSquareIndependenceTest", "Prueba de chi-cuadrado de independencia para una tabla de contingencia r x c")]
        public static (double chiSquared, int degreesOfFreedom, double pValue) ChiSquareIndependenceTest(double[][] contingencyTable)
        {
            int rows = contingencyTable.Length;
            if (rows < 2)
                throw new ArgumentException("Se requieren al menos 2 filas");
            int cols = contingencyTable[0].Length;
            if (cols < 2)
                throw new ArgumentException("Se requieren al menos 2 columnas");

            var rowTotals = new double[rows];
            var colTotals = new double[cols];
            double grandTotal = 0;

            for (int i = 0; i < rows; i++)
            {
                if (contingencyTable[i].Length != cols)
                    throw new ArgumentException("Todas las filas deben tener el mismo número de columnas");

                for (int j = 0; j < cols; j++)
                {
                    rowTotals[i] += contingencyTable[i][j];
                    colTotals[j] += contingencyTable[i][j];
                    grandTotal += contingencyTable[i][j];
                }
            }
            if (grandTotal == 0)
                throw new ArgumentException("El total de la tabla no puede ser cero");

            double chiSquared = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double expected = rowTotals[i] * colTotals[j] / grandTotal;
                    if (expected > 0)
                        chiSquared += Math.Pow(contingencyTable[i][j] - expected, 2) / expected;
                }
            }

            int df = (rows - 1) * (cols - 1);
            double pValue = ChiSquarePValue(chiSquared, df);

            return (chiSquared, df, pValue);
        }

        [FunctEngineExport("OneSampleKsTest", "Prueba de Kolmogorov-Smirnov de una muestra contra una distribución Normal(mean, stdDev)")]
        public static (double dStatistic, double pValue) OneSampleKsTest(double[] data, double mean, double stdDev)
        {
            if (stdDev <= 0)
                throw new ArgumentException("La desviación estándar debe ser positiva");
            int n = data.Length;
            if (n < 2)
                throw new ArgumentException("Se requieren al menos 2 observaciones");

            var sorted = data.OrderBy(v => v).ToArray();
            double d = 0;
            for (int i = 0; i < n; i++)
            {
                double empiricalBefore = (double)i / n;
                double empiricalAfter = (double)(i + 1) / n;
                double theoretical = NormalCDF((sorted[i] - mean) / stdDev);
                d = Math.Max(d, Math.Max(Math.Abs(theoretical - empiricalBefore), Math.Abs(theoretical - empiricalAfter)));
            }

            double pValue = KolmogorovAsymptoticPValue(n, d);

            return (d, pValue);
        }

        // ─── Internal helpers ───────────────────────────────────────────────────

        // Ranks values in ascending order, assigning the average rank to tied groups (1-based).
        private static double[] Rank(double[] values)
        {
            int n = values.Length;
            var indices = Enumerable.Range(0, n).OrderBy(i => values[i]).ToArray();
            var ranks = new double[n];

            int i = 0;
            while (i < n)
            {
                int j = i;
                while (j + 1 < n && values[indices[j + 1]] == values[indices[i]]) j++;
                double avgRank = (i + 1 + j + 1) / 2.0;
                for (int m = i; m <= j; m++) ranks[indices[m]] = avgRank;
                i = j + 1;
            }

            return ranks;
        }

        private static double TieCorrectionSum(double[] values)
        {
            return values.GroupBy(v => v)
                          .Where(g => g.Count() > 1)
                          .Sum(g => { double t = g.Count(); return t * t * t - t; });
        }

        private static double TieBreakdown(double[] values)
        {
            return values.GroupBy(v => v)
                          .Where(g => g.Count() > 1)
                          .Sum(g => { double t = g.Count(); return t * (t - 1) / 2.0; });
        }

        private static double KolmogorovAsymptoticPValue(double effectiveN, double d)
        {
            if (d <= 0) return 1.0;
            double lambda = (Math.Sqrt(effectiveN) + 0.12 + 0.11 / Math.Sqrt(effectiveN)) * d;

            double sum = 0;
            for (int k = 1; k <= 100; k++)
            {
                double term = (k % 2 == 1 ? 1.0 : -1.0) * Math.Exp(-2.0 * k * k * lambda * lambda);
                sum += term;
                if (Math.Abs(term) < 1e-10) break;
            }

            double p = 2.0 * sum;
            return Math.Max(0.0, Math.Min(1.0, p));
        }

        private static double NormalCDF(double z)
        {
            double t = 1.0 / (1.0 + 0.2316419 * Math.Abs(z));
            double d = 0.3989422820 * Math.Exp(-z * z / 2.0);
            double poly = t * (0.3193815 + t * (-0.3565638 + t * (1.7814779 + t * (-1.8212560 + t * 1.3302744))));
            double p = 1.0 - d * poly;
            return z >= 0 ? p : 1.0 - p;
        }

        private static double LogGamma(double x)
        {
            double[] c = { 76.18009172947146, -86.50532032941677, 24.01409824083091, -1.231739572450155, 1.208650973866179e-3, -5.395239384953e-6 };
            double y = x;
            double tmp = x + 5.5 - (x + 0.5) * Math.Log(x + 5.5);
            double ser = 1.000000000190015;
            for (int j = 0; j < 6; j++) ser += c[j] / ++y;
            return -tmp + Math.Log(2.5066282746310005 * ser / x);
        }

        private static double BetaCF(double a, double b, double x)
        {
            const double fpmin = 1e-30;
            double qab = a + b, qap = a + 1.0, qam = a - 1.0;
            double c = 1.0, d = 1.0 - qab * x / qap;
            if (Math.Abs(d) < fpmin) d = fpmin;
            d = 1.0 / d;
            double h = d;
            for (int mm = 1; mm <= 200; mm++)
            {
                int m2 = 2 * mm;
                double aa = mm * (b - mm) * x / ((qam + m2) * (a + m2));
                d = 1.0 + aa * d; if (Math.Abs(d) < fpmin) d = fpmin;
                c = 1.0 + aa / c; if (Math.Abs(c) < fpmin) c = fpmin;
                d = 1.0 / d; h *= d * c;
                aa = -(a + mm) * (qab + mm) * x / ((a + m2) * (qap + m2));
                d = 1.0 + aa * d; if (Math.Abs(d) < fpmin) d = fpmin;
                c = 1.0 + aa / c; if (Math.Abs(c) < fpmin) c = fpmin;
                d = 1.0 / d;
                double del = d * c; h *= del;
                if (Math.Abs(del - 1.0) <= 3e-7) break;
            }
            return h;
        }

        private static double BetaInc(double a, double b, double x)
        {
            if (x <= 0) return 0;
            if (x >= 1) return 1;
            double lbeta = LogGamma(a + b) - LogGamma(a) - LogGamma(b);
            double bt = Math.Exp(lbeta + a * Math.Log(x) + b * Math.Log(1.0 - x));
            return x < (a + 1.0) / (a + b + 2.0)
                ? bt * BetaCF(a, b, x) / a
                : 1.0 - bt * BetaCF(b, a, 1.0 - x) / b;
        }

        private static double GammaIncSeries(double a, double x)
        {
            double sum = 1.0 / a, del = 1.0 / a, ap = a;
            for (int n = 1; n <= 200; n++) { ap++; del *= x / ap; sum += del; if (Math.Abs(del) < Math.Abs(sum) * 3e-7) break; }
            return sum * Math.Exp(-x + a * Math.Log(x) - LogGamma(a));
        }

        private static double GammaIncCF(double a, double x)
        {
            const double fpmin = 1e-30;
            double b = x + 1.0 - a, c = 1.0 / fpmin, d = 1.0 / b, h = d;
            for (int i = 1; i <= 200; i++)
            {
                double an = -i * (i - a); b += 2.0;
                d = an * d + b; if (Math.Abs(d) < fpmin) d = fpmin;
                c = b + an / c; if (Math.Abs(c) < fpmin) c = fpmin;
                d = 1.0 / d; double del = d * c; h *= del;
                if (Math.Abs(del - 1.0) < 3e-7) break;
            }
            return Math.Exp(-x + a * Math.Log(x) - LogGamma(a)) * h;
        }

        private static double GammaInc(double a, double x)
        {
            if (x <= 0) return 0;
            return x < a + 1.0 ? GammaIncSeries(a, x) : 1.0 - GammaIncCF(a, x);
        }

        private static double ChiSquarePValue(double x, double df)
            => x <= 0 ? 1.0 : 1.0 - GammaInc(df / 2.0, x / 2.0);

        private static double TDistPValue(double t, double df)
            => BetaInc(df / 2.0, 0.5, df / (df + t * t));
    }
}
