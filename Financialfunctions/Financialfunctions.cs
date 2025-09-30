using FunctEngine;
using System;
using System.Linq;

namespace Financialfunctions
{
    [FunctEngineExport("Financial Functions", "Biblioteca de funciones financieras")]
    public static class FinancialLibrary
    {
        [FunctEngineExport("Fv", "Calcula el valor futuro de una inversión basado en una tasa de interés constante")]
        public static double Fv(double rate, int nper, double pmt, double pv = 0, int type = 0)
        {
            if (rate == 0)
                return -pv - pmt * nper;

            double pvif = Math.Pow(1 + rate, nper);
            return -pv * pvif - pmt * (1 + rate * type) * (pvif - 1) / rate;
        }

        [FunctEngineExport("Pv", "Calcula el valor presente de un préstamo o inversión")]
        public static double Pv(double rate, int nper, double pmt, double fv = 0, int type = 0)
        {
            if (rate == 0)
                return -fv - pmt * nper;

            double pvif = Math.Pow(1 + rate, nper);
            return (-pmt * (1 + rate * type) * (pvif - 1) / rate - fv) / pvif;
        }

        [FunctEngineExport("Pmt", "Calcula el pago de un préstamo basado en pagos constantes y una tasa de interés constante")]
        public static double Pmt(double rate, int nper, double pv, double fv = 0, int type = 0)
        {
            if (rate == 0)
                return -(pv + fv) / nper;

            double pvif = Math.Pow(1 + rate, nper);
            return -(rate * (fv + pv * pvif)) / ((pvif - 1) * (1 + rate * type));
        }

        [FunctEngineExport("Nper", "Retorna el número de períodos para una inversión")]
        public static double Nper(double rate, double pmt, double pv, double fv = 0, int type = 0)
        {
            if (rate == 0)
                return -(pv + fv) / pmt;

            double num = pmt * (1 + rate * type) - fv * rate;
            double den = pv * rate + pmt * (1 + rate * type);
            return Math.Log(num / den) / Math.Log(1 + rate);
        }

        [FunctEngineExport("Rate", "Retorna la tasa de interés por período de una anualidad")]
        public static double Rate(int nper, double pmt, double pv, double fv = 0, int type = 0, double guess = 0.1)
        {
            double rate = guess;
            int maxIterations = 20;
            double tolerance = 0.0000001;

            for (int i = 0; i < maxIterations; i++)
            {
                double f = Pv(rate, nper, pmt, fv, type) - pv;
                if (Math.Abs(f) < tolerance)
                    return rate;

                double df = (Pv(rate + tolerance, nper, pmt, fv, type) - Pv(rate - tolerance, nper, pmt, fv, type)) / (2 * tolerance);
                rate = rate - f / df;
            }

            throw new ArgumentException("No se pudo converger a una solución");
        }

        [FunctEngineExport("Ipmt", "Retorna el pago de interés para un período dado")]
        public static double Ipmt(double rate, int period, int nper, double pv, double fv = 0, int type = 0)
        {
            double pmt = Pmt(rate, nper, pv, fv, type);
            double ipmt;

            if (period == 1 && type == 1)
                ipmt = 0;
            else
                ipmt = Fv(rate, period - 1, pmt, pv, type) * rate;

            if (type == 1)
                ipmt /= (1 + rate);

            return ipmt;
        }

        [FunctEngineExport("Ppmt", "Retorna el pago del principal para un período dado")]
        public static double Ppmt(double rate, int period, int nper, double pv, double fv = 0, int type = 0)
        {
            double pmt = Pmt(rate, nper, pv, fv, type);
            double ipmt = Ipmt(rate, period, nper, pv, fv, type);
            return pmt - ipmt;
        }

        [FunctEngineExport("Cumipmt", "Retorna el interés acumulado pagado entre dos períodos")]
        public static double Cumipmt(double rate, int nper, double pv, int startPeriod, int endPeriod, int type)
        {
            if (startPeriod < 1 || endPeriod < startPeriod || rate <= 0 || nper <= 0)
                throw new ArgumentException("Parámetros inválidos");

            double cumInterest = 0;
            for (int period = startPeriod; period <= endPeriod; period++)
            {
                cumInterest += Ipmt(rate, period, nper, pv, 0, type);
            }
            return cumInterest;
        }

        [FunctEngineExport("Cumprinc", "Retorna el capital acumulado pagado entre dos períodos")]
        public static double Cumprinc(double rate, int nper, double pv, int startPeriod, int endPeriod, int type)
        {
            if (startPeriod < 1 || endPeriod < startPeriod || rate <= 0 || nper <= 0)
                throw new ArgumentException("Parámetros inválidos");

            double cumPrincipal = 0;
            for (int period = startPeriod; period <= endPeriod; period++)
            {
                cumPrincipal += Ppmt(rate, period, nper, pv, 0, type);
            }
            return cumPrincipal;
        }

        [FunctEngineExport("Sln", "Retorna la depreciación lineal de un activo por un período")]
        public static double Sln(double cost, double salvage, int life)
        {
            if (life == 0)
                throw new DivideByZeroException("La vida útil no puede ser cero");

            return (cost - salvage) / life;
        }

        [FunctEngineExport("Syd", "Retorna la depreciación por suma de dígitos de años")]
        public static double Syd(double cost, double salvage, int life, int period)
        {
            if (period > life || period < 1)
                throw new ArgumentException("El período debe estar entre 1 y la vida útil");

            double syd = (life * (life + 1)) / 2.0;
            return ((cost - salvage) * (life - period + 1)) / syd;
        }

        [FunctEngineExport("Db", "Retorna la depreciación de un activo usando el método de saldo decreciente fijo")]
        public static double Db(double cost, double salvage, int life, int period, int month = 12)
        {
            if (period > life || period < 1)
                throw new ArgumentException("El período debe estar entre 1 y la vida útil");

            double rate = 1 - Math.Pow(salvage / cost, 1.0 / life);
            rate = Math.Round(rate, 3);

            double depreciation = cost * rate * month / 12;
            if (period == 1)
                return depreciation;

            double totalDepreciation = depreciation;
            for (int i = 2; i <= period; i++)
            {
                depreciation = (cost - totalDepreciation) * rate;
                totalDepreciation += depreciation;
            }

            return depreciation;
        }

        [FunctEngineExport("Ddb", "Retorna la depreciación usando el método de saldo decreciente doble")]
        public static double Ddb(double cost, double salvage, int life, int period, double factor = 2)
        {
            if (period > life || period < 1)
                throw new ArgumentException("El período debe estar entre 1 y la vida útil");

            double rate = factor / life;
            double depreciation = 0;
            double totalDepreciation = 0;

            for (int i = 1; i <= period; i++)
            {
                depreciation = (cost - totalDepreciation) * rate;
                if (cost - totalDepreciation - depreciation < salvage)
                    depreciation = cost - totalDepreciation - salvage;

                totalDepreciation += depreciation;
            }

            return depreciation;
        }

        [FunctEngineExport("Effect", "Retorna la tasa de interés anual efectiva")]
        public static double Effect(double nominalRate, int npery)
        {
            if (nominalRate <= 0 || npery < 1)
                throw new ArgumentException("Parámetros inválidos");

            return Math.Pow(1 + nominalRate / npery, npery) - 1;
        }

        [FunctEngineExport("Nominal", "Retorna la tasa de interés anual nominal")]
        public static double Nominal(double effectRate, int npery)
        {
            if (effectRate <= 0 || npery < 1)
                throw new ArgumentException("Parámetros inválidos");

            return (Math.Pow(effectRate + 1, 1.0 / npery) - 1) * npery;
        }

        [FunctEngineExport("Xnpv", "Retorna el valor presente neto para un flujo de efectivo no periódico")]
        public static double Xnpv(double rate, double[] values, DateTime[] dates)
        {
            if (values.Length != dates.Length)
                throw new ArgumentException("Los arrays de valores y fechas deben tener la misma longitud");

            if (values.Length == 0)
                throw new ArgumentException("Se requieren al menos un valor y una fecha");

            double xnpv = 0;
            DateTime baseDate = dates[0];

            for (int i = 0; i < values.Length; i++)
            {
                double days = (dates[i] - baseDate).TotalDays;
                xnpv += values[i] / Math.Pow(1 + rate, days / 365.0);
            }

            return xnpv;
        }

        [FunctEngineExport("Xirr", "Retorna la tasa interna de retorno para un flujo de efectivo no periódico")]
        public static double Xirr(double[] values, DateTime[] dates, double guess = 0.1)
        {
            if (values.Length != dates.Length)
                throw new ArgumentException("Los arrays de valores y fechas deben tener la misma longitud");

            double rate = guess;
            int maxIterations = 100;
            double tolerance = 0.000001;

            for (int i = 0; i < maxIterations; i++)
            {
                double f = Xnpv(rate, values, dates);
                if (Math.Abs(f) < tolerance)
                    return rate;

                double df = (Xnpv(rate + tolerance, values, dates) - Xnpv(rate - tolerance, values, dates)) / (2 * tolerance);
                rate = rate - f / df;
            }

            throw new ArgumentException("No se pudo converger a una solución");
        }

        [FunctEngineExport("Rri", "Retorna una tasa de interés equivalente para el crecimiento de una inversión")]
        public static double Rri(int nper, double pv, double fv)
        {
            if (nper <= 0 || pv <= 0)
                throw new ArgumentException("Parámetros inválidos");

            return Math.Pow(fv / pv, 1.0 / nper) - 1;
        }

        [FunctEngineExport("Pduration", "Retorna el número de períodos requeridos para alcanzar un valor específico")]
        public static double Pduration(double rate, double pv, double fv)
        {
            if (rate <= 0 || pv <= 0 || fv <= 0)
                throw new ArgumentException("Parámetros inválidos");

            return Math.Log(fv / pv) / Math.Log(1 + rate);
        }

        [FunctEngineExport("Ispmt", "Calcula el interés pagado durante un período específico")]
        public static double Ispmt(double rate, int period, int nper, double pv)
        {
            return pv * rate * (period / (double)nper - 1);
        }

        [FunctEngineExport("Dollarde", "Convierte un precio en dólares expresado como fracción a decimal")]
        public static double Dollarde(double fractionalDollar, int fraction)
        {
            if (fraction <= 0)
                throw new ArgumentException("La fracción debe ser positiva");

            int integerPart = (int)fractionalDollar;
            double fractionalPart = fractionalDollar - integerPart;

            return integerPart + (fractionalPart * Math.Pow(10, Math.Floor(Math.Log10(fraction) + 1))) / fraction;
        }

        [FunctEngineExport("Dollarfr", "Convierte un precio en dólares expresado como decimal a fracción")]
        public static double Dollarfr(double decimalDollar, int fraction)
        {
            if (fraction <= 0)
                throw new ArgumentException("La fracción debe ser positiva");

            int integerPart = (int)decimalDollar;
            double fractionalPart = decimalDollar - integerPart;

            return integerPart + (fractionalPart * fraction) / Math.Pow(10, Math.Floor(Math.Log10(fraction) + 1));
        }

        [FunctEngineExport("Disc", "Retorna la tasa de descuento de un valor")]
        public static double Disc(DateTime settlement, DateTime maturity, double pr, double redemption, int basis = 0)
        {
            double dsm = DaysBetween(settlement, maturity, basis);
            double b = DaysInYear(settlement, basis);

            return (redemption - pr) / redemption * (b / dsm);
        }

        [FunctEngineExport("Intrate", "Retorna la tasa de interés de un valor totalmente invertido")]
        public static double Intrate(DateTime settlement, DateTime maturity, double investment, double redemption, int basis = 0)
        {
            double dsm = DaysBetween(settlement, maturity, basis);
            double b = DaysInYear(settlement, basis);

            return (redemption - investment) / investment * (b / dsm);
        }

        [FunctEngineExport("Received", "Retorna la cantidad recibida al vencimiento de un valor totalmente invertido")]
        public static double Received(DateTime settlement, DateTime maturity, double investment, double discount, int basis = 0)
        {
            double dsm = DaysBetween(settlement, maturity, basis);
            double b = DaysInYear(settlement, basis);

            return investment / (1 - discount * dsm / b);
        }

        [FunctEngineExport("Pricedisc", "Retorna el precio por $100 de valor nominal de un valor con descuento")]
        public static double Pricedisc(DateTime settlement, DateTime maturity, double discount, double redemption, int basis = 0)
        {
            double dsm = DaysBetween(settlement, maturity, basis);
            double b = DaysInYear(settlement, basis);

            return redemption - discount * redemption * dsm / b;
        }

        [FunctEngineExport("Tbillprice", "Retorna el precio por $100 de valor nominal de una letra del tesoro")]
        public static double Tbillprice(DateTime settlement, DateTime maturity, double discount)
        {
            double dsm = (maturity - settlement).TotalDays;

            if (dsm > 360)
                throw new ArgumentException("El vencimiento debe ser dentro de un año");

            return 100 * (1 - discount * dsm / 360);
        }

        [FunctEngineExport("TbillYield", "Retorna el rendimiento de una letra del tesoro")]
        public static double TbillYield(DateTime settlement, DateTime maturity, double pr)
        {
            double dsm = (maturity - settlement).TotalDays;

            if (dsm > 360)
                throw new ArgumentException("El vencimiento debe ser dentro de un año");

            return (100 - pr) / pr * 360 / dsm;
        }

        [FunctEngineExport("Tbilleq", "Retorna el rendimiento equivalente de bono para una letra del tesoro")]
        public static double Tbilleq(DateTime settlement, DateTime maturity, double discount)
        {
            double dsm = (maturity - settlement).TotalDays;

            if (dsm > 360)
                throw new ArgumentException("El vencimiento debe ser dentro de un año");

            return (365 * discount) / (360 - discount * dsm);
        }

        private static double DaysBetween(DateTime start, DateTime end, int basis)
        {
            return basis switch
            {
                0 or 4 => (end - start).TotalDays,
                1 => (end - start).TotalDays,
                2 => (end - start).TotalDays,
                3 => (end - start).TotalDays,
                _ => (end - start).TotalDays
            };
        }

        private static double DaysInYear(DateTime date, int basis)
        {
            return basis switch
            {
                0 or 2 or 4 => 360,
                1 => DateTime.IsLeapYear(date.Year) ? 366 : 365,
                3 => 365,
                _ => 365
            };
        }
    }
}