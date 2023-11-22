using System;
using System.Globalization;

namespace ZenDeskTicketProcessJob.Utilities
{
    public static class DateUtils
    {
        public static string DateFormat(string date)
        {
            if (!string.IsNullOrEmpty(date))
            {
                DateTime newDate;
                if (DateTime.TryParse(date, out newDate))
                {
                    if (newDate.Year < 1900)
                    {
                        return "N/A";
                    }
                    return GetDateString(newDate);
                }
            }
            return string.Empty;
        }

        public static string GetDateString(DateTime? newDate)
        {
            if (newDate.HasValue)
            {
                string dateString = $"{newDate?.ToString("MMM")} {newDate?.Day.ToString("D2")}, {newDate?.Year}";
                return dateString;
            }

            return string.Empty; // or 'N/A' if you prefer
        }
    }

}
