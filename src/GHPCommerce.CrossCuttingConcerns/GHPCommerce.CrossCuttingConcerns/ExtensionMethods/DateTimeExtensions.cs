using System;

namespace GHPCommerce.CrossCuttingConcerns.ExtensionMethods
{
    public static class DateTimeExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        public static DateTime ConvertFromUtcToLocalZone(this DateTime date, string tzName = "W. Central Africa Standard Time")
        {
            return TimeZoneInfo.ConvertTimeFromUtc( date,TimeZoneInfo.FindSystemTimeZoneById(tzName));
        }
        public static  DateTime GetFirstDayOfQuarter(this DateTime currentDate)
        {
            int currentQuarter = (currentDate.Month - 1) / 3 + 1;
            return  new DateTime(currentDate.Year, 3 * currentQuarter - 2, 1);
        }

        public static DateTime GetLastDayOfQuarter(this DateTime currentDate)
        {
            /*int currentQuarter = (currentDate.Month - 1) / 3 + 1;
            var month = 3 * currentQuarter + 1;
            return new DateTime(currentDate.Year,month>12? month : 12 , 1).AddDays(-1);*/
            var result =
                currentDate.Date
                    .AddMonths(3 - (currentDate.Month - 1) % 3);
           return result.AddDays(-result.Day);
        }
    }
}
