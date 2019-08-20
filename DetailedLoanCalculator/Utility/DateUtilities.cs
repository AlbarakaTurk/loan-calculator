using System;
using System.Globalization;

namespace Albaraka.Utils.Calculator.LoanCalculator.Utility
{
    /// <summary>
    /// Date utilities.
    /// </summary>
    public static class DateUtilities
    {
        /// <summary>
        /// Adds months to date.
        /// </summary>
        /// <returns>The month.</returns>
        /// <param name="date">Date.</param>
        /// <param name="months">Months.</param>
        public static DateTime AddMonth(DateTime date, int months)
        {
            if (date.Day != DateTime.DaysInMonth(date.Year, date.Month))
            {
                return date.AddMonths(months);
            }
            else
            {
                return date.AddDays(1).AddMonths(months).AddDays(-1);
            }
        }

        /// <summary>
        /// Gets the business day.
        /// Gönderilen tarih tatil değilse gönderilen tarihi döner,
        /// Eğer tatil ise bir sonraki iş gününü döner
        /// </summary>
        /// <returns>The business day.</returns>
        /// <param name="date">Date.</param>
        /// <param name="culture">Culture.</param>
        public static DateTime GetBusinessDay(DateTime date, CultureInfo culture)
        {
            switch (culture.TwoLetterISOLanguageName)
            {
                case "tr":
                   //TODO Consider Public Holidays for cultures
                default:
                    while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(1);
                    }
                    break;
            }

            return date.Date;
        }
    }
}
