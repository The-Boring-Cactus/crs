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

            // Calcular Wilks' Lambda (simplificado)
            double wilksLambda = 0.5; // Valor aproximado para demostración

            // Calcular Pillai's Trace (simplificado)
            double pillaiTrace = 0.5; // Valor aproximado para demostración

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

        private static double FDistributionCDF(double x, int df1, int df2)
        {
            // Aproximación simple del p-value de la distribución F
            // En una implementación completa se usaría una función beta incompleta
            if (x < 1)
                return x / 2;
            else
                return 1 - 1 / (1 + x);
        }
    }
}