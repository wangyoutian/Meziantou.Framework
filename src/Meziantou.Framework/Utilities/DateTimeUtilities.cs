﻿using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Meziantou.Framework.Utilities
{
    public static class DateTimeUtilities
    {
        [Pure]
        public static DateTime FirstDateOfWeekIso8601(int year, int weekOfYear, DayOfWeek weekStart = DayOfWeek.Monday)
        {
            var jan1 = new DateTime(year, 1, 1);
            var fourthDay = (DayOfWeek)(((int)weekStart + 3) % 7);
            var daysOffset = fourthDay - jan1.DayOfWeek;

            var firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            var firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, weekStart);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

        [Pure]
        public static DateTime StartOfWeek(this DateTime dt)
        {
            return StartOfWeek(dt, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
        }

        [Pure]
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff);
        }

        [Pure]
        public static DateTime StartOfMonth(this DateTime dt)
        {
            return StartOfMonth(dt, false);
        }

        [Pure]
        public static DateTime StartOfMonth(this DateTime dt, bool keepTime)
        {
            if (keepTime)
            {
                return dt.AddDays(-dt.Day + 1);
            }

            return new DateTime(dt.Year, dt.Month, 1);
        }

        [Pure]
        public static DateTime EndOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
        }

        [Pure]
        public static DateTime StartOfYear(this DateTime dt)
        {
            return StartOfYear(dt, false);
        }

        [Pure]
        public static DateTime StartOfYear(this DateTime dt, bool keepTime)
        {
            if (keepTime)
            {
                return dt.AddDays(-dt.DayOfYear + 1);
            }
            else
            {
                return new DateTime(dt.Year, 1, 1);
            }
        }
    }
}
