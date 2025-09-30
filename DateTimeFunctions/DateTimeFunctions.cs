using FunctEngine;
using System;
using System.Globalization;

namespace DateTimeFunctions
{
    [FunctEngineExport("Date & Time Functions", "Biblioteca de funciones de fecha y hora")]
    public static class DateTimeLibrary
    {
        [FunctEngineExport("Date", "Retorna la fecha actual o formatea una fecha con el formato especificado")]
        public static string Date(string format = "yyyy-MM-dd")
        {
            return DateTime.Now.ToString(format);
        }

        [FunctEngineExport("Datediff", "Calcula la diferencia en días entre dos fechas")]
        public static int Datediff(DateTime startDate, DateTime endDate)
        {
            return (endDate - startDate).Days;
        }

        [FunctEngineExport("Datevalue", "Convierte una fecha en formato de texto a un valor de fecha")]
        public static DateTime Datevalue(string dateText)
        {
            if (DateTime.TryParse(dateText, out DateTime result))
            {
                return result;
            }
            throw new ArgumentException("Formato de fecha inválido");
        }

        [FunctEngineExport("Day", "Retorna el día del mes (1-31) de una fecha")]
        public static int Day(DateTime date)
        {
            return date.Day;
        }

        [FunctEngineExport("Hour", "Retorna la hora (0-23) de una fecha/hora")]
        public static int Hour(DateTime dateTime)
        {
            return dateTime.Hour;
        }

        [FunctEngineExport("Minute", "Retorna los minutos (0-59) de una fecha/hora")]
        public static int Minute(DateTime dateTime)
        {
            return dateTime.Minute;
        }

        [FunctEngineExport("Month", "Retorna el mes (1-12) de una fecha")]
        public static int Month(DateTime date)
        {
            return date.Month;
        }

        [FunctEngineExport("Now", "Retorna la fecha y hora actual en formato específico")]
        public static string Now(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.Now.ToString(format);
        }

        [FunctEngineExport("Quarter", "Retorna el trimestre del año (1-4) de una fecha")]
        public static int Quarter(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        [FunctEngineExport("Second", "Retorna los segundos (0-59) de una fecha/hora")]
        public static int Second(DateTime dateTime)
        {
            return dateTime.Second;
        }

        [FunctEngineExport("Time", "Convierte horas, minutos y segundos en un valor de tiempo")]
        public static DateTime Time(int hours, int minutes, int seconds)
        {
            if (hours < 0 || hours > 23)
                throw new ArgumentException("Las horas deben estar entre 0 y 23");
            if (minutes < 0 || minutes > 59)
                throw new ArgumentException("Los minutos deben estar entre 0 y 59");
            if (seconds < 0 || seconds > 59)
                throw new ArgumentException("Los segundos deben estar entre 0 y 59");

            return new DateTime(1900, 1, 1, hours, minutes, seconds);
        }

        [FunctEngineExport("Timevalue", "Convierte una hora en formato de texto a un valor de tiempo")]
        public static DateTime Timevalue(string timeText)
        {
            if (DateTime.TryParse(timeText, out DateTime result))
            {
                return result;
            }
            throw new ArgumentException("Formato de hora inválido");
        }

        [FunctEngineExport("Today", "Retorna la fecha actual sin hora")]
        public static DateTime Today()
        {
            return DateTime.Today;
        }

        [FunctEngineExport("UtcNow", "Retorna la fecha y hora actual en UTC")]
        public static string UtcNow(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.UtcNow.ToString(format);
        }

        [FunctEngineExport("WeekDay", "Retorna un número que identifica el día de la semana (0=Domingo, 6=Sábado)")]
        public static int WeekDay(DateTime date)
        {
            return (int)date.DayOfWeek;
        }

        [FunctEngineExport("WeekNum", "Retorna el número de semana del año")]
        public static int WeekNum(DateTime date)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            Calendar calendar = culture.Calendar;
            CalendarWeekRule weekRule = culture.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;

            return calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }

        [FunctEngineExport("Year", "Retorna el año de una fecha como un entero de cuatro dígitos")]
        public static int Year(DateTime date)
        {
            return date.Year;
        }

        [FunctEngineExport("YearFrac", "Retorna la fracción del año entre dos fechas")]
        public static double YearFrac(DateTime startDate, DateTime endDate)
        {
            TimeSpan difference = endDate - startDate;
            return difference.TotalDays / 365.25;
        }
    }
}