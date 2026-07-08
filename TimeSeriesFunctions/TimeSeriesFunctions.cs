using FunctEngine;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Linq;

namespace TimeSeriesFunctions
{
    [FunctEngineExport("Time Series Functions", "Biblioteca de funciones para análisis de series temporales")]
    public static class TimeSeriesLibrary
    {
        // ─── Autocorrelation ────────────────────────────────────────────────────

        [FunctEngineExport("Acf", "Calcula la función de autocorrelación (ACF) de una serie temporal hasta el rezago máximo indicado")]
        public static double[] Acf(double[] series, int maxLag)
        {
            int n = series.Length;
            if (n < 2)
                throw new ArgumentException("Se requieren al menos 2 observaciones");
            if (maxLag < 0 || maxLag >= n)
                throw new ArgumentException("maxLag debe estar entre 0 y n-1");

            double mean = series.Average();
            double c0 = series.Sum(x => (x - mean) * (x - mean)) / n;
            if (c0 == 0)
                throw new ArgumentException("La serie tiene varianza cero");

            var result = new double[maxLag + 1];
            for (int k = 0; k <= maxLag; k++)
            {
                double ck = 0;
                for (int t = 0; t < n - k; t++)
                    ck += (series[t] - mean) * (series[t + k] - mean);
                ck /= n;
                result[k] = ck / c0;
            }
            return result;
        }

        [FunctEngineExport("Pacf", "Calcula la función de autocorrelación parcial (PACF) mediante la recursión de Durbin-Levinson")]
        public static double[] Pacf(double[] series, int maxLag)
        {
            if (maxLag < 1)
                throw new ArgumentException("maxLag debe ser al menos 1");

            double[] rho = Acf(series, maxLag);

            var pacf = new double[maxLag + 1];
            pacf[0] = 1.0;

            var phiPrev = new double[maxLag + 1];
            var phiCurr = new double[maxLag + 1];

            phiPrev[1] = rho[1];
            pacf[1] = rho[1];

            for (int k = 2; k <= maxLag; k++)
            {
                double numerator = rho[k];
                for (int j = 1; j <= k - 1; j++)
                    numerator -= phiPrev[j] * rho[k - j];

                double denominator = 1.0;
                for (int j = 1; j <= k - 1; j++)
                    denominator -= phiPrev[j] * rho[j];

                double phiKK = denominator != 0 ? numerator / denominator : 0.0;

                for (int j = 1; j <= k - 1; j++)
                    phiCurr[j] = phiPrev[j] - phiKK * phiPrev[k - j];
                phiCurr[k] = phiKK;
                pacf[k] = phiKK;

                (phiPrev, phiCurr) = (phiCurr, phiPrev);
            }

            return pacf;
        }

        // ─── Smoothing & Transformations ────────────────────────────────────────

        [FunctEngineExport("MovingAverage", "Calcula la media móvil (rolling mean) de una serie temporal con la ventana indicada")]
        public static double[] MovingAverage(double[] series, int windowSize)
        {
            if (windowSize < 1 || windowSize > series.Length)
                throw new ArgumentException("El tamaño de ventana debe estar entre 1 y la longitud de la serie");

            int outLen = series.Length - windowSize + 1;
            var result = new double[outLen];

            double sum = 0;
            for (int i = 0; i < windowSize; i++)
                sum += series[i];
            result[0] = sum / windowSize;

            for (int i = windowSize; i < series.Length; i++)
            {
                sum += series[i] - series[i - windowSize];
                result[i - windowSize + 1] = sum / windowSize;
            }

            return result;
        }

        [FunctEngineExport("ExponentialSmoothing", "Aplica suavizado exponencial simple (SES) a una serie temporal con el factor alpha indicado")]
        public static double[] ExponentialSmoothing(double[] series, double alpha)
        {
            if (series.Length == 0)
                throw new ArgumentException("La serie no puede estar vacía");
            if (alpha <= 0 || alpha > 1)
                throw new ArgumentException("alpha debe estar en el rango (0, 1]");

            var result = new double[series.Length];
            result[0] = series[0];
            for (int t = 1; t < series.Length; t++)
                result[t] = alpha * series[t] + (1 - alpha) * result[t - 1];

            return result;
        }

        [FunctEngineExport("BoxCoxTransform", "Aplica la transformación Box-Cox a una serie; lambda=0 equivale al logaritmo natural")]
        public static double[] BoxCoxTransform(double[] series, double lambda)
        {
            if (series.Any(x => x <= 0))
                throw new ArgumentException("Box-Cox requiere valores estrictamente positivos");

            return series
                .Select(x => Math.Abs(lambda) < 1e-10 ? Math.Log(x) : (Math.Pow(x, lambda) - 1.0) / lambda)
                .ToArray();
        }

        [FunctEngineExport("LogTransform", "Aplica la transformación logarítmica natural a una serie temporal")]
        public static double[] LogTransform(double[] series)
        {
            if (series.Any(x => x <= 0))
                throw new ArgumentException("El logaritmo requiere valores estrictamente positivos");

            return series.Select(x => Math.Log(x)).ToArray();
        }

        // ─── Diagnostic Tests ───────────────────────────────────────────────────

        [FunctEngineExport("LjungBoxTest", "Prueba de Ljung-Box: evalúa si existe autocorrelación conjunta hasta el rezago indicado")]
        public static (double qStatistic, int degreesOfFreedom, double pValue) LjungBoxTest(double[] series, int maxLag)
        {
            int n = series.Length;
            if (maxLag < 1 || maxLag >= n)
                throw new ArgumentException("maxLag debe estar entre 1 y n-1");

            double[] rho = Acf(series, maxLag);

            double q = 0;
            for (int k = 1; k <= maxLag; k++)
                q += (rho[k] * rho[k]) / (n - k);
            q *= n * (n + 2);

            double pValue = ChiSquarePValue(q, maxLag);
            return (q, maxLag, pValue);
        }

        [FunctEngineExport("DurbinWatsonTest", "Calcula el estadístico Durbin-Watson para detectar autocorrelación de primer orden en residuos")]
        public static double DurbinWatsonTest(double[] residuals)
        {
            int n = residuals.Length;
            if (n < 2)
                throw new ArgumentException("Se requieren al menos 2 residuos");

            double numerator = 0;
            for (int t = 1; t < n; t++)
                numerator += Math.Pow(residuals[t] - residuals[t - 1], 2);

            double denominator = residuals.Sum(e => e * e);
            if (denominator == 0)
                throw new ArgumentException("La suma de cuadrados de los residuos no puede ser cero");

            return numerator / denominator;
        }

        [FunctEngineExport("GrangerCausalityTest", "Prueba de causalidad de Granger: evalúa si 'cause' ayuda a predecir 'effect' usando p rezagos")]
        public static (double fStatistic, int df1, int df2, double pValue) GrangerCausalityTest(double[] cause, double[] effect, int maxLag)
        {
            if (cause.Length != effect.Length)
                throw new ArgumentException("Las series deben tener la misma longitud");
            if (maxLag < 1)
                throw new ArgumentException("maxLag debe ser al menos 1");

            int n = cause.Length;
            int usable = n - maxLag;
            int df1 = maxLag;
            int df2 = usable - (2 * maxLag + 1);
            if (df2 <= 0)
                throw new ArgumentException("Se requieren más observaciones en relación al número de rezagos");

            var yDep = new double[usable];
            var xRestricted = new double[usable][];
            var xUnrestricted = new double[usable][];

            for (int i = 0; i < usable; i++)
            {
                int t = i + maxLag;
                yDep[i] = effect[t];

                var restricted = new double[maxLag];
                var unrestricted = new double[maxLag * 2];
                for (int lag = 1; lag <= maxLag; lag++)
                {
                    restricted[lag - 1] = effect[t - lag];
                    unrestricted[lag - 1] = effect[t - lag];
                    unrestricted[maxLag + lag - 1] = cause[t - lag];
                }
                xRestricted[i] = restricted;
                xUnrestricted[i] = unrestricted;
            }

            var (_, rssRestricted) = OlsFit(xRestricted, yDep);
            var (_, rssUnrestricted) = OlsFit(xUnrestricted, yDep);

            double fStatistic = Math.Max(0.0, ((rssRestricted - rssUnrestricted) / df1) / (rssUnrestricted / df2));
            double pValue = 1.0 - FDistributionCDF(fStatistic, df1, df2);

            return (fStatistic, df1, df2, pValue);
        }

        // ─── Cointegration ──────────────────────────────────────────────────────

        [FunctEngineExport("EngleGrangerTest", "Prueba de cointegración de Engle-Granger (dos pasos): regresión y luego Dickey-Fuller sobre los residuos")]
        public static (double alpha, double beta, double adfStatistic, double criticalValue5pct, bool cointegrated) EngleGrangerTest(double[] y, double[] x)
        {
            if (y.Length != x.Length)
                throw new ArgumentException("Las series deben tener la misma longitud");

            int n = y.Length;
            if (n < 10)
                throw new ArgumentException("Se requieren al menos 10 observaciones");

            var xRows = new double[n][];
            for (int i = 0; i < n; i++)
                xRows[i] = new[] { x[i] };

            var (coeffs, _) = OlsFit(xRows, y);
            double alpha = coeffs[0];
            double beta = coeffs[1];

            var resid = new double[n];
            for (int i = 0; i < n; i++)
                resid[i] = y[i] - (alpha + beta * x[i]);

            // Dickey-Fuller sin intercepto ni tendencia sobre los residuos:
            // Δe_t = rho * e_{t-1} + u_t
            int m = n - 1;
            double sumLagSq = 0, sumCross = 0;
            for (int t = 1; t < n; t++)
            {
                double lag = resid[t - 1];
                double diff = resid[t] - resid[t - 1];
                sumCross += lag * diff;
                sumLagSq += lag * lag;
            }
            if (sumLagSq == 0)
                throw new ArgumentException("Los residuos no tienen variación suficiente");

            double rho = sumCross / sumLagSq;

            double ssResid = 0;
            for (int t = 1; t < n; t++)
            {
                double lag = resid[t - 1];
                double diff = resid[t] - resid[t - 1];
                double u = diff - rho * lag;
                ssResid += u * u;
            }
            double se = Math.Sqrt((ssResid / (m - 1)) / sumLagSq);
            double adfStatistic = se != 0 ? rho / se : 0;

            // Valor crítico asintótico aproximado de Engle-Granger (2 variables, con
            // constante, sin tendencia) al 5%, según MacKinnon (2010).
            const double criticalValue5pct = -3.34;
            bool cointegrated = adfStatistic < criticalValue5pct;

            return (alpha, beta, adfStatistic, criticalValue5pct, cointegrated);
        }

        [FunctEngineExport("JohansenTest", "Prueba de cointegración de Johansen (procedimiento simplificado, un rezago, sin término determinístico) para múltiples series")]
        public static (double[] eigenvalues, double[] traceStatistics) JohansenTest(double[][] data)
        {
            // data: cada fila es una observación en el tiempo, cada columna es una variable
            int n = data.Length;
            if (n < 4)
                throw new ArgumentException("Se requieren al menos 4 observaciones");
            int k = data[0].Length;
            if (k < 2)
                throw new ArgumentException("Se requieren al menos 2 variables (columnas)");

            int t = n - 1;

            var z0 = Matrix<double>.Build.Dense(t, k, (i, j) => data[i + 1][j] - data[i][j]);
            var z1 = Matrix<double>.Build.Dense(t, k, (i, j) => data[i][j]);

            var s00 = (z0.Transpose() * z0) / t;
            var s01 = (z0.Transpose() * z1) / t;
            var s11 = (z1.Transpose() * z1) / t;
            var s10 = s01.Transpose();

            var m = s11.Inverse() * s10 * s00.Inverse() * s01;

            var eigenvalues = m.Evd().EigenValues
                .Select(c => Math.Max(0.0, Math.Min(0.999999, c.Real)))
                .OrderByDescending(e => e)
                .ToArray();

            var traceStatistics = new double[k];
            for (int r = 0; r < k; r++)
            {
                double sum = 0;
                for (int i = r; i < k; i++)
                    sum += Math.Log(1 - eigenvalues[i]);
                traceStatistics[r] = -t * sum;
            }

            return (eigenvalues, traceStatistics);
        }

        // ─── Internal helpers ───────────────────────────────────────────────────

        private static (double[] coefficients, double rss) OlsFit(double[][] xRows, double[] y)
        {
            int n = xRows.Length;
            int p = xRows[0].Length + 1; // +1 para el intercepto

            var x = Matrix<double>.Build.Dense(n, p, (i, j) => j == 0 ? 1.0 : xRows[i][j - 1]);
            var yVec = Vector<double>.Build.DenseOfArray(y);

            var beta = (x.Transpose() * x).Inverse() * x.Transpose() * yVec;
            var resid = yVec - x * beta;
            double rss = resid.DotProduct(resid);

            return (beta.ToArray(), rss);
        }

        private static double FDistributionCDF(double f, int df1, int df2)
        {
            if (f <= 0) return 0;
            double x = (double)df1 * f / ((double)df1 * f + df2);
            return BetaInc(df1 / 2.0, df2 / 2.0, x);
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
            => 1.0 - GammaInc(df / 2.0, x / 2.0);
    }
}
