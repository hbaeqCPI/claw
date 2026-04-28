using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LawPortal.Core.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime AddBusinessDays(this DateTime value, int days)
        {
            var businessHolidays = (new List<DateTime>(value.GetUSHolidays().Keys).Select(d => d.NearestWeekDay()));
            var newDate = value.AddDays(days < 0 ? 1 : - 1);
            int count = 0;

            while (count <= Math.Abs(days))
            {
                newDate = newDate.AddDays(days < 0 ? -1 : 1);
                if (newDate.DayOfWeek != DayOfWeek.Saturday && newDate.DayOfWeek != DayOfWeek.Sunday && !businessHolidays.Any(d => d.Date == newDate.Date))
                    count += 1;
            }

            return newDate.Date;
        }

        public static Dictionary<DateTime, string> GetUSHolidays(this DateTime value)
        {
            var year = value.Year;
            var usHolidays = new Dictionary<DateTime, string>();

            usHolidays.Add((new DateTime(year, 1, 1)), "New Year's Day");                                   //January 1st
            usHolidays.Add(GetOrdinalDayOfWeek(3, DayOfWeek.Monday, year, 1), "Martin Luther King Day");    //3rd Monday of January
            usHolidays.Add(GetOrdinalDayOfWeek(3, DayOfWeek.Monday, year, 2), "President's day");           //3rd Monday of February
            usHolidays.Add(GetOrdinalDayOfWeek(1, DayOfWeek.Monday, year, 6).AddDays(-7), "Memorial Day");  //Last Monday of May
            usHolidays.Add((new DateTime(year, 7, 4)), "Independence Day");                                 //July 4th
            usHolidays.Add(GetOrdinalDayOfWeek(1, DayOfWeek.Monday, year, 9), "Labor Day");                 //1st Monday of September
            usHolidays.Add(GetOrdinalDayOfWeek(2, DayOfWeek.Monday, year, 10), "Columbus day");             //2nd Monday of October
            usHolidays.Add((new DateTime(year, 11, 11)), "Veteran's Day");                                  //November 11th
            usHolidays.Add(GetOrdinalDayOfWeek(4, DayOfWeek.Thursday, year, 11), "Thanksgiving Day");       //4th Thursday of November
            usHolidays.Add((new DateTime(year, 12, 25)), "Christmas Day");                                  //December 25th

            return usHolidays;
        }

        public static DateTime NearestWeekDay(this DateTime value)
        {
            var weekdayDate = value;

            if (value.DayOfWeek == DayOfWeek.Saturday)
                weekdayDate = value.AddDays(-1);
            else if (value.DayOfWeek == DayOfWeek.Sunday)
                weekdayDate = value.AddDays(1);

            return weekdayDate.Date;
        }

        public static DateTime GetOrdinalDayOfWeek(int ordinalValue, DayOfWeek dayOfWeek, int year, int month)
        {
            var day = 1 + ((int)dayOfWeek - (int)new DateTime(year, month, 1).DayOfWeek + 7) % 7 + 7 * (ordinalValue - 1);

            return (new DateTime(year, month, day)).Date;
        }

        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-diff).Date;
        }

        public static DateTime LastDayOfWeek(this DateTime dt) =>
            dt.FirstDayOfWeek().AddDays(6);

        public static DateTime FirstDayOfMonth(this DateTime dt) =>
            new DateTime(dt.Year, dt.Month, 1);

        public static DateTime LastDayOfMonth(this DateTime dt) =>
            dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);

        public static DateTime FirstDayOfYear(this DateTime dt) =>
            new DateTime(dt.Year, 1, 1);

        public static DateTime LastDayOfYear(this DateTime dt) =>
            dt.FirstDayOfYear().AddYears(1).AddDays(-1);

        public static DateTime LastYear(this DateTime dt) =>
            new DateTime(dt.Year - 1, dt.Month, dt.Day);
    }
}
