using System;

namespace ZenDeskTicketProcessJob.Utilities
{
    public static class DateUtils
    {
        public static string DateFormat(string date)
        {
            if (!string.IsNullOrEmpty(date))
            {
                if (DateTime.TryParse(date, out DateTime newDate))
                {
                    return newDate.Year < 1900 ? "N/A" : GetDateString(newDate);
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
