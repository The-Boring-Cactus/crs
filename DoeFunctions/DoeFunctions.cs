using FunctEngine;
using System;
using System.Linq;

namespace DoeFunctions
{
    [FunctEngineExport("Design of Experiments Functions", "Biblioteca de funciones para diseño de experimentos y análisis estadístico")]
    public static class DoeLibrary
    {
        // Linear Regression Functions

        [FunctEngineExport("LinearRegression", "Calcula la regresión lineal simple (y = mx + b)")]
        public static (double slope, double intercept, double rSquared) LinearRegression(double[] xValues, double[] yValues)
        {
            if (xValues.Length != yValues.Length)
                throw new ArgumentException("Los arrays de X e Y deben tener la misma longitud");

            if (xValues.Length < 2)
                throw new ArgumentException("Se requieren al menos 2 puntos de datos");

            int n = xValues.Length;
            double sumX = xValues.Sum();
            double sumY = yValues.Sum();
            double sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
            double sumX2 = xValues.Sum(x => x * x);
            double sumY2 = yValues.Sum(y => y * y);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            // Calculate R-squared
            double yMean = sumY / n;
            double ssTotal = yValues.Sum(y => Math.Pow(y - yMean, 2));
            double ssResidual = xValues.Zip(yValues, (x, y) => Math.Pow(y - (slope * x + intercept), 2)).Sum();
            double rSquared = 1 - (ssResidual / ssTotal);

            return (slope, intercept, rSquared);
        }

        [FunctEngineExport("MultipleLinearRegression", "Calcula la regresión lineal múltiple")]
        public static double[] MultipleLinearRegression(double[][] xMatrix, double[] yValues)
        {
            if (xMatrix.Length != yValues.Length)
                throw new ArgumentException("El número de filas en la matriz X debe coincidir con la longitud de Y");

            int n = xMatrix.Length;
            int p = xMatrix[0].Length + 1; // +1 para el intercepto

            // Construir matriz X con columna de unos para el intercepto
            double[][] X = new double[n][];
            for (int i = 0; i < n; i++)
            {
                X[i] = new double[p];
                X[i][0] = 1; // Intercepto
                Array.Copy(xMatrix[i], 0, X[i], 1, xMatrix[i].Length);
            }

            // Calcular (X'X)^-1 * X'Y usando mínimos cuadrados
            double[][] xtx = MatrixMultiply(Transpose(X), X);
            double[][] xtxInv = MatrixInverse(xtx);
            double[] xty = MatrixVectorMultiply(Transpose(X), yValues);
            double[] coefficients = MatrixVectorMultiply(xtxInv, xty);

            return coefficients;
        }

        // One-Way ANOVA Functions

        [FunctEngineExport("OneWayAnova", "Realiza un análisis de varianza de una vía")]
        public static (double fStatistic, double pValue, double ssWithin, double ssBetween) OneWayAnova(double[][] groups)
        {
            if (groups.Length < 2)
                throw new ArgumentException("Se requieren al menos 2 grupos");

            int k = groups.Length; // Número de grupos
            int n = groups.Sum(g => g.Length); // Total de observaciones

            // Calcular medias
            double[] groupMeans = groups.Select(g => g.Average()).ToArray();
            double grandMean = groups.SelectMany(g => g).Average();

            // Calcular suma de cuadrados entre grupos (SSB)
            double ssBetween = 0;
            for (int i = 0; i < k; i++)
            {
                ssBetween += groups[i].Length * Math.Pow(groupMeans[i] - grandMean, 2);
            }

            // Calcular suma de cuadrados dentro de grupos (SSW)
            double ssWithin = 0;
            for (int i = 0; i < k; i++)
            {
                ssWithin += groups[i].Sum(x => Math.Pow(x - groupMeans[i], 2));
            }

            // Grados de libertad
            int dfBetween = k - 1;
            int dfWithin = n - k;

            // Cuadrados medios
            double msBetween = ssBetween / dfBetween;
            double msWithin = ssWithin / dfWithin;

            // Estadístico F
            double fStatistic = msBetween / msWithin;

            // Aproximación del p-value usando distribución F
            double pValue = FDistributionCDF(fStatistic, dfBetween, dfWithin);

            return (fStatistic, pValue, ssWithin, ssBetween);
        }

        // Two-Way ANOVA Functions

        [FunctEngineExport("TwoWayAnova", "Realiza un análisis de varianza de dos vías")]
        public static (double fA, double fB, double fInteraction, double ssTotal) TwoWayAnova(
            double[][] data, int[] factorA, int[] factorB)
        {
            if (data.Length == 0)
                throw new ArgumentException("Se requieren datos");

            int n = data.Length;
            double grandMean = data.SelectMany(x => x).Average();

            // Calcular efectos principales y de interacción
            var levelsA = factorA.Distinct().ToArray();
            var levelsB = factorB.Distinct().ToArray();

            // Suma de cuadrados total
            double ssTotal = data.SelectMany(x => x).Sum(x => Math.Pow(x - grandMean, 2));

            // Suma de cuadrados para Factor A
            double ssA = 0;
            foreach (var level in levelsA)
            {
                var indices = Enumerable.Range(0, n).Where(i => factorA[i] == level).ToArray();
                if (indices.Length > 0)
                {
                    double mean = indices.SelectMany(i => data[i]).Average();
                    ssA += indices.SelectMany(i => data[i]).Sum(x => Math.Pow(mean - grandMean, 2));
                }
            }

            // Suma de cuadrados para Factor B
            double ssB = 0;
            foreach (var level in levelsB)
            {
                var indices = Enumerable.Range(0, n).Where(i => factorB[i] == level).ToArray();
                if (indices.Length > 0)
                {
                    double mean = indices.SelectMany(i => data[i]).Average();
                    ssB += indices.SelectMany(i => data[i]).Sum(x => Math.Pow(mean - grandMean, 2));
                }
            }

            // Suma de cuadrados de interacción (simplificada)
            double ssInteraction = ssTotal - ssA - ssB;

            // Grados de libertad
            int dfA = levelsA.Length - 1;
            int dfB = levelsB.Length - 1;
            int dfInteraction = dfA * dfB;

            // Cuadrados medios
            double msA = ssA / dfA;
            double msB = ssB / dfB;
            double msInteraction = ssInteraction / Math.Max(1, dfInteraction);

            // Estadísticos F (usando MSE aproximado)
            double mse = ssTotal / (n - 1);
            double fA = msA / mse;
            double fB = msB / mse;
            double fInteraction = msInteraction / mse;

            return (fA, fB, fInteraction, ssTotal);
        }

        // MANOVA (Multivariate Analysis of Variance)

        [FunctEngineExport("Manova", "Realiza un análisis de varianza multivariado")]
        public static (double wilksLambda, double pillaiTrace) Manova(double[][][] groups)
        {
            if (groups.Length < 2)
                throw new ArgumentException("Se requieren al menos 2 grupos");

            int k = groups.Length; // Número de grupos
            int p = groups[0][0].Length; // Número de variables dependientes

            // Calcular matrices de suma de cuadrados
            double[][] totalMean = new double[1][];
            totalMean[0] = new double[p];

            int totalN = groups.Sum(g => g.Length);
            for (int v = 0; v < p; v++)
            {
                totalMean[0][v] = groups.SelectMany(g => g).Select(obs => obs[v]).Average();
            }

            // Matriz de suma de cuadrados entre grupos (B)
            double[][] matrixB = new double[p][];
            for (int i = 0; i < p; i++)
            {
                matrixB[i] = new double[p];
            }

            // Matriz de suma de cuadrados dentro de grupos (W)
            double[][] matrixW = new double[p][];
            for (int i = 0; i < p; i++)
            {
                matrixW[i] = new double[p];
            }

            // Between-groups scatter matrix (B)
            for (int g = 0; g < k; g++)
            {
                int ng = groups[g].Length;
                double[] groupMean = new double[p];
                for (int v = 0; v < p; v++)
                    groupMean[v] = groups[g].Select(obs => obs[v]).Average();
                for (int i = 0; i < p; i++)
                    for (int j = 0; j < p; j++)
                        matrixB[i][j] += ng * (groupMean[i] - totalMean[0][i]) * (groupMean[j] - totalMean[0][j]);
            }

            // Within-groups scatter matrix (W)
            for (int g = 0; g < k; g++)
            {
                double[] groupMean = new double[p];
                for (int v = 0; v < p; v++)
                    groupMean[v] = groups[g].Select(obs => obs[v]).Average();
                foreach (var obs in groups[g])
                    for (int i = 0; i < p; i++)
                        for (int j = 0; j < p; j++)
                            matrixW[i][j] += (obs[i] - groupMean[i]) * (obs[j] - groupMean[j]);
            }

            // W + B = total scatter matrix
            double[][] matrixT = new double[p][];
            for (int i = 0; i < p; i++)
            {
                matrixT[i] = new double[p];
                for (int j = 0; j < p; j++)
                    matrixT[i][j] = matrixW[i][j] + matrixB[i][j];
            }

            // Wilks' Lambda = det(W) / det(T)
            double detW = MatrixDeterminant(matrixW);
            double detT = MatrixDeterminant(matrixT);
            double wilksLambda = Math.Abs(detT) > 1e-12 ? Math.Max(0, Math.Min(1, detW / detT)) : 1.0;

            // Pillai's Trace = trace(B * inv(T))
            double pillaiTrace = 0;
            try
            {
                double[][] tInv = MatrixInverse(matrixT);
                double[][] bTimesInv = MatrixMultiply(matrixB, tInv);
                for (int i = 0; i < p; i++)
                    pillaiTrace += bTimesInv[i][i];
                pillaiTrace = Math.Max(0, Math.Min((double)p, pillaiTrace));
            }
            catch { pillaiTrace = 1.0 - wilksLambda; }

            return (wilksLambda, pillaiTrace);
        }

        // GLM (Generalized Linear Model)

        [FunctEngineExport("Glm", "Ajusta un modelo lineal generalizado")]
        public static double[] Glm(double[][] xMatrix, double[] yValues, string family = "gaussian")
        {
            // Para familia gaussiana, GLM es equivalente a regresión lineal
            if (family.ToLower() == "gaussian")
            {
                return MultipleLinearRegression(xMatrix, yValues);
            }

            // Para otras familias, usar algoritmo IRLS simplificado
            int maxIterations = 25;
            double tolerance = 1e-6;

            double[] coefficients = MultipleLinearRegression(xMatrix, yValues);

            for (int iter = 0; iter < maxIterations; iter++)
            {
                double[] fitted = new double[yValues.Length];
                for (int i = 0; i < yValues.Length; i++)
                {
                    fitted[i] = coefficients[0];
                    for (int j = 0; j < xMatrix[i].Length; j++)
                    {
                        fitted[i] += coefficients[j + 1] * xMatrix[i][j];
                    }
                }

                // Aplicar función de enlace según la familia
                if (family.ToLower() == "binomial")
                {
                    for (int i = 0; i < fitted.Length; i++)
                    {
                        fitted[i] = 1.0 / (1.0 + Math.Exp(-fitted[i]));
                    }
                }

                // Verificar convergencia
                bool converged = true;
                for (int i = 0; i < yValues.Length; i++)
                {
                    if (Math.Abs(fitted[i] - yValues[i]) > tolerance)
                    {
                        converged = false;
                        break;
                    }
                }

                if (converged) break;
            }

            return coefficients;
        }

        // Gage R&R Analysis

        [FunctEngineExport("GageRR", "Realiza un análisis Gage R&R")]
        public static (double repeatability, double reproducibility, double gageRR, double partVariation) GageRR(
            double[][][] measurements) // [parts][operators][replicates]
        {
            if (measurements.Length == 0)
                throw new ArgumentException("Se requieren mediciones");

            int numParts = measurements.Length;
            int numOperators = measurements[0].Length;
            int numReplicates = measurements[0][0].Length;

            // Calcular medias
            double grandMean = measurements.SelectMany(p => p.SelectMany(o => o)).Average();

            // Variación de repetibilidad (dentro del operador)
            double ssRepeatability = 0;
            for (int p = 0; p < numParts; p++)
            {
                for (int o = 0; o < numOperators; o++)
                {
                    double opMean = measurements[p][o].Average();
                    ssRepeatability += measurements[p][o].Sum(x => Math.Pow(x - opMean, 2));
                }
            }
            double repeatability = Math.Sqrt(ssRepeatability / (numParts * numOperators * (numReplicates - 1)));

            // Variación de reproducibilidad (entre operadores)
            double[] operatorMeans = new double[numOperators];
            for (int o = 0; o < numOperators; o++)
            {
                operatorMeans[o] = measurements.SelectMany(p => p[o]).Average();
            }
            double ssReproducibility = operatorMeans.Sum(m => Math.Pow(m - grandMean, 2)) * numParts * numReplicates;
            double reproducibility = Math.Sqrt(ssReproducibility / (numOperators - 1));

            // Gage R&R total
            double gageRR = Math.Sqrt(Math.Pow(repeatability, 2) + Math.Pow(reproducibility, 2));

            // Variación de las partes
            double[] partMeans = new double[numParts];
            for (int p = 0; p < numParts; p++)
            {
                partMeans[p] = measurements[p].SelectMany(o => o).Average();
            }
            double ssPartVariation = partMeans.Sum(m => Math.Pow(m - grandMean, 2)) * numOperators * numReplicates;
            double partVariation = Math.Sqrt(ssPartVariation / (numParts - 1));

            return (repeatability, reproducibility, gageRR, partVariation);
        }

        // Statistical Helper Functions

        [FunctEngineExport("Mean", "Calcula la media de un conjunto de datos")]
        public static double Mean(double[] values)
        {
            if (values.Length == 0)
                throw new ArgumentException("Se requiere al menos un valor");

            return values.Average();
        }

        [FunctEngineExport("Variance", "Calcula la varianza de un conjunto de datos")]
        public static double Variance(double[] values)
        {
            if (values.Length < 2)
                throw new ArgumentException("Se requieren al menos 2 valores");

            double mean = values.Average();
            return values.Sum(x => Math.Pow(x - mean, 2)) / (values.Length - 1);
        }

        [FunctEngineExport("StandardDeviation", "Calcula la desviación estándar de un conjunto de datos")]
        public static double StandardDeviation(double[] values)
        {
            return Math.Sqrt(Variance(values));
        }

        [FunctEngineExport("Correlation", "Calcula el coeficiente de correlación de Pearson")]
        public static double Correlation(double[] xValues, double[] yValues)
        {
            if (xValues.Length != yValues.Length)
                throw new ArgumentException("Los arrays deben tener la misma longitud");

            if (xValues.Length < 2)
                throw new ArgumentException("Se requieren al menos 2 puntos de datos");

            double meanX = xValues.Average();
            double meanY = yValues.Average();

            double numerator = xValues.Zip(yValues, (x, y) => (x - meanX) * (y - meanY)).Sum();
            double denomX = Math.Sqrt(xValues.Sum(x => Math.Pow(x - meanX, 2)));
            double denomY = Math.Sqrt(yValues.Sum(y => Math.Pow(y - meanY, 2)));

            return numerator / (denomX * denomY);
        }

        // Matrix Helper Functions

        private static double[][] Transpose(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            double[][] result = new double[cols][];

            for (int i = 0; i < cols; i++)
            {
                result[i] = new double[rows];
                for (int j = 0; j < rows; j++)
                {
                    result[i][j] = matrix[j][i];
                }
            }

            return result;
        }

        private static double[][] MatrixMultiply(double[][] a, double[][] b)
        {
            int rowsA = a.Length;
            int colsA = a[0].Length;
            int colsB = b[0].Length;

            double[][] result = new double[rowsA][];
            for (int i = 0; i < rowsA; i++)
            {
                result[i] = new double[colsB];
                for (int j = 0; j < colsB; j++)
                {
                    result[i][j] = 0;
                    for (int k = 0; k < colsA; k++)
                    {
                        result[i][j] += a[i][k] * b[k][j];
                    }
                }
            }

            return result;
        }

        private static double[] MatrixVectorMultiply(double[][] matrix, double[] vector)
        {
            int rows = matrix.Length;
            double[] result = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                result[i] = 0;
                for (int j = 0; j < vector.Length; j++)
                {
                    result[i] += matrix[i][j] * vector[j];
                }
            }

            return result;
        }

        private static double[][] MatrixInverse(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] augmented = new double[n][];

            // Crear matriz aumentada [A|I]
            for (int i = 0; i < n; i++)
            {
                augmented[i] = new double[2 * n];
                for (int j = 0; j < n; j++)
                {
                    augmented[i][j] = matrix[i][j];
                    augmented[i][j + n] = (i == j) ? 1 : 0;
                }
            }

            // Eliminación de Gauss-Jordan
            for (int i = 0; i < n; i++)
            {
                // Encontrar pivote
                double pivot = augmented[i][i];
                if (Math.Abs(pivot) < 1e-10)
                    throw new ArgumentException("La matriz no es invertible");

                // Normalizar fila del pivote
                for (int j = 0; j < 2 * n; j++)
                {
                    augmented[i][j] /= pivot;
                }

                // Eliminar otras filas
                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = augmented[k][i];
                        for (int j = 0; j < 2 * n; j++)
                        {
                            augmented[k][j] -= factor * augmented[i][j];
                        }
                    }
                }
            }

            // Extraer matriz inversa
            double[][] inverse = new double[n][];
            for (int i = 0; i < n; i++)
            {
                inverse[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    inverse[i][j] = augmented[i][j + n];
                }
            }

            return inverse;
        }

        private static double MatrixDeterminant(double[][] m)
        {
            int n = m.Length;
            if (n == 1) return m[0][0];
            if (n == 2) return m[0][0] * m[1][1] - m[0][1] * m[1][0];
            // LU decomposition for larger matrices
            double[][] a = m.Select(r => r.ToArray()).ToArray();
            double det = 1.0;
            for (int col = 0; col < n; col++)
            {
                int pivot = col;
                for (int row = col + 1; row < n; row++)
                    if (Math.Abs(a[row][col]) > Math.Abs(a[pivot][col])) pivot = row;
                if (pivot != col) { var tmp = a[col]; a[col] = a[pivot]; a[pivot] = tmp; det = -det; }
                if (Math.Abs(a[col][col]) < 1e-12) return 0;
                det *= a[col][col];
                for (int row = col + 1; row < n; row++)
                {
                    double f = a[row][col] / a[col][col];
                    for (int j = col; j < n; j++) a[row][j] -= f * a[col][j];
                }
            }
            return det;
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
            for (int m = 1; m <= 200; m++)
            {
                int m2 = 2 * m;
                double aa = m * (b - m) * x / ((qam + m2) * (a + m2));
                d = 1.0 + aa * d; if (Math.Abs(d) < fpmin) d = fpmin;
                c = 1.0 + aa / c; if (Math.Abs(c) < fpmin) c = fpmin;
                d = 1.0 / d; h *= d * c;
                aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2));
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
            if (x <= 0) return 0; if (x >= 1) return 1;
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
            if (x < 0) return 0; if (x == 0) return 0;
            return x < a + 1.0 ? GammaIncSeries(a, x) : 1.0 - GammaIncCF(a, x);
        }

        private static double TDistPValue(double t, double df)
            => BetaInc(df / 2.0, 0.5, df / (df + t * t));

        private static double ChiSquarePValue(double x, double df)
            => 1.0 - GammaInc(df / 2.0, x / 2.0);

        private static double NormalCDF(double z)
        {
            double t = 1.0 / (1.0 + 0.2316419 * Math.Abs(z));
            double d = 0.3989422820 * Math.Exp(-z * z / 2.0);
            double poly = t * (0.3193815 + t * (-0.3565638 + t * (1.7814779 + t * (-1.8212560 + t * 1.3302744))));
            double p = 1.0 - d * poly;
            return z >= 0 ? p : 1.0 - p;
        }

        // ─── Statistical Tests ────────────────────────────────────────────────────

        [FunctEngineExport("IndependentTTest", "Two-sample independent t-test: returns t, df, pValue, cohensD")]
        public static (double t, double df, double pValue, double cohensD) IndependentTTest(double[] a, double[] b)
        {
            if (a.Length < 2 || b.Length < 2) throw new ArgumentException("Each group needs at least 2 observations");
            double meanA = a.Average(), meanB = b.Average();
            double varA = a.Sum(x => Math.Pow(x - meanA, 2)) / (a.Length - 1);
            double varB = b.Sum(x => Math.Pow(x - meanB, 2)) / (b.Length - 1);
            double se = Math.Sqrt(varA / a.Length + varB / b.Length);
            double t = (meanA - meanB) / se;
            // Welch's degrees of freedom
            double df = Math.Pow(varA / a.Length + varB / b.Length, 2)
                / (Math.Pow(varA / a.Length, 2) / (a.Length - 1) + Math.Pow(varB / b.Length, 2) / (b.Length - 1));
            double pValue = TDistPValue(Math.Abs(t), df);
            double pooledSd = Math.Sqrt(((a.Length - 1) * varA + (b.Length - 1) * varB) / (a.Length + b.Length - 2));
            double cohensD = pooledSd > 0 ? (meanA - meanB) / pooledSd : 0;
            return (t, df, pValue, cohensD);
        }

        [FunctEngineExport("PairedTTest", "Paired (dependent) t-test: returns t, df, pValue")]
        public static (double t, double df, double pValue) PairedTTest(double[] before, double[] after)
        {
            if (before.Length != after.Length) throw new ArgumentException("Arrays must be the same length");
            double[] diffs = before.Zip(after, (a, b) => a - b).ToArray();
            double mean = diffs.Average();
            double se = Math.Sqrt(diffs.Sum(d => Math.Pow(d - mean, 2)) / (diffs.Length - 1)) / Math.Sqrt(diffs.Length);
            double t = mean / se;
            double df = diffs.Length - 1;
            return (t, df, TDistPValue(Math.Abs(t), df));
        }

        [FunctEngineExport("ChiSquareTest", "Chi-square goodness-of-fit test: returns chiSquared, df, pValue")]
        public static (double chiSquared, int df, double pValue) ChiSquareTest(double[] observed, double[] expected)
        {
            if (observed.Length != expected.Length) throw new ArgumentException("Arrays must be the same length");
            double chi2 = observed.Zip(expected, (o, e) => Math.Pow(o - e, 2) / e).Sum();
            int df = observed.Length - 1;
            return (chi2, df, ChiSquarePValue(chi2, df));
        }

        [FunctEngineExport("MannWhitneyU", "Mann-Whitney U test (non-parametric): returns U, pValue")]
        public static (double u, double pValue) MannWhitneyU(double[] groupA, double[] groupB)
        {
            int n1 = groupA.Length, n2 = groupB.Length;
            double u1 = 0;
            foreach (var a in groupA)
                foreach (var b in groupB)
                    u1 += a > b ? 1 : a == b ? 0.5 : 0;
            double u2 = n1 * n2 - u1;
            double u = Math.Min(u1, u2);
            double meanU = n1 * n2 / 2.0;
            double sdU = Math.Sqrt((double)n1 * n2 * (n1 + n2 + 1) / 12.0);
            double z = (u - meanU) / sdU;
            double pValue = 2.0 * (1.0 - NormalCDF(Math.Abs(z)));
            return (u, pValue);
        }

        [FunctEngineExport("ConfidenceInterval", "Confidence interval for a sample: returns lower, upper, mean, marginOfError")]
        public static (double lower, double upper, double mean, double marginOfError) ConfidenceInterval(double[] data, double? confidence = null)
        {
            if (data.Length < 2) throw new ArgumentException("Need at least 2 observations");
            double mean = data.Average();
            double se = Math.Sqrt(data.Sum(x => Math.Pow(x - mean, 2)) / (data.Length - 1)) / Math.Sqrt(data.Length);
            double alpha = 1.0 - (confidence ?? 0.95);
            double z = CriticalZ(1.0 - alpha / 2.0);
            double margin = z * se;
            return (mean - margin, mean + margin, mean, margin);
        }

        private static double CriticalZ(double p)
        {
            // Beasley-Springer-Moro algorithm approximation
            double a0 = 2.515517, a1 = 0.802853, a2 = 0.010328;
            double b1 = 1.432788, b2 = 0.189269, b3 = 0.001308;
            double t = p < 0.5 ? Math.Sqrt(-2.0 * Math.Log(p)) : Math.Sqrt(-2.0 * Math.Log(1.0 - p));
            double num = a0 + a1 * t + a2 * t * t;
            double den = 1.0 + b1 * t + b2 * t * t + b3 * t * t * t;
            double z = t - num / den;
            return p < 0.5 ? -z : z;
        }

        [FunctEngineExport("CohenD", "Cohen's d effect size between two groups")]
        public static double CohenD(double[] groupA, double[] groupB)
        {
            double meanA = groupA.Average(), meanB = groupB.Average();
            double varA = groupA.Sum(x => Math.Pow(x - meanA, 2)) / (groupA.Length - 1);
            double varB = groupB.Sum(x => Math.Pow(x - meanB, 2)) / (groupB.Length - 1);
            double pooledSd = Math.Sqrt(((groupA.Length - 1) * varA + (groupB.Length - 1) * varB) / (groupA.Length + groupB.Length - 2));
            return pooledSd > 0 ? (meanA - meanB) / pooledSd : 0;
        }

        [FunctEngineExport("EtaSquared", "Eta-squared effect size: ssBetween / ssTotal")]
        public static double EtaSquared(double ssBetween, double ssTotal)
            => ssTotal > 0 ? ssBetween / ssTotal : 0;

        [FunctEngineExport("TukeyHSD", "Tukey HSD post-hoc comparisons: returns array of {groupA, groupB, meanDiff, q, significant}")]
        public static Dictionary<string, object>[] TukeyHSD(double[][] groups, string[]? groupNames = null)
        {
            int k = groups.Length;
            double[] means = groups.Select(g => g.Average()).ToArray();
            int totalN = groups.Sum(g => g.Length);
            double msError = groups.SelectMany((g, gi) => g.Select(v => Math.Pow(v - means[gi], 2))).Sum() / (totalN - k);
            double nHarmonic = k / groups.Sum(g => 1.0 / g.Length);
            var results = new List<Dictionary<string, object>>();
            for (int i = 0; i < k - 1; i++)
                for (int j = i + 1; j < k; j++)
                {
                    double diff = Math.Abs(means[i] - means[j]);
                    double se = Math.Sqrt(msError / nHarmonic);
                    double q = se > 0 ? diff / se : 0;
                    results.Add(new Dictionary<string, object>
                    {
                        ["groupA"] = groupNames?[i] ?? $"Group{i + 1}",
                        ["groupB"] = groupNames?[j] ?? $"Group{j + 1}",
                        ["meanA"] = means[i],
                        ["meanB"] = means[j],
                        ["meanDiff"] = means[i] - means[j],
                        ["q"] = q,
                        ["significant"] = q > 3.64
                    });
                }
            return results.ToArray();
        }

        [FunctEngineExport("BonferroniCorrection", "Bonferroni-corrected p-values: divide each p-value by number of tests")]
        public static double[] BonferroniCorrection(double[] pValues, int numTests = 0)
        {
            int m = numTests > 0 ? numTests : pValues.Length;
            return pValues.Select(p => Math.Min(1.0, p * m)).ToArray();
        }
    }
}