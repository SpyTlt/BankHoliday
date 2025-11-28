// (c) Vitas Ramanchauskas www.SpyTlt.com, MIT license
// Bank holidays for U.S. markets.
// Key methods:
// nowEST - current DateTime in EST timezone
// isWorkingDay - checks if the market is open on a given date
// isMarketOpenNow, isMarketOpenAt - check if the market is open right now or at a specified DateTime

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BankHolidayNS
{
    /// <summary>
    /// Support for US bank holidays.
    /// </summary>
    public static class BankHoliday
    {
        public enum MarketTime { beforeOpen, open, afterClose }


        public static TimeZoneInfo estTimeZone { get; private set; } = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");


        /// <summary>
        /// Convert local DateTime to EST timezone
        /// </summary>
        /// <param name="dt">DateTime to convert</param>
        /// <returns>Given DateTime in EST timezone</returns>
        static public DateTime toEst(DateTime dt) =>
            TimeZoneInfo.ConvertTime(dt, estTimeZone);

        static public DateTime fromEst(DateTime dt) =>
            TimeZoneInfo.ConvertTime(dt, estTimeZone, TimeZoneInfo.Local);

        /// <summary>
        /// Current DateTime in EST timezone
        /// </summary>
        static public DateTime nowEST => toEst(DateTime.Now);


        /// <summary>
        /// List of extra days when market was closed (9/11 for example)
        /// </summary>
        public static readonly HashSet<DateTime> extraCloseDates = new HashSet<DateTime> { new DateTime(2001, 9, 11), new DateTime(2001, 9, 12),
             new DateTime(2001, 9, 13), new DateTime(2001, 9, 14),  new DateTime(2004, 6, 11), new DateTime(2007, 1, 2),
             new DateTime(2012, 10, 29), new DateTime(2012, 10, 30), new DateTime(2018, 12, 5) };

        /// <summary>
        /// When is Good Friday
        /// </summary>
        /// <param name="year">Given year</param>
        /// <returns>Exact date</returns>
        public static DateTime getGoodFriday(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day).AddDays(-2);
        }

        private static DateTime thirdMonday(int year, int month)
        {
            var d = new DateTime(year, month, 1);
            return new DateTime(year, month, 21 - (((int)d.DayOfWeek + 5) % 7));
        }

        private static DateTime fourthThursday(int year, int month)
        {
            var d = new DateTime(year, month, 1);
            return new DateTime(year, month, 28 - (((int)d.DayOfWeek + 2) % 7));
        }

        public static readonly TimeSpan normalCloseTime = new TimeSpan(16, 0, 0);
        public static readonly TimeSpan earlyCloseTime = new TimeSpan(13, 0, 0);

        /// <summary>
        /// When does the market close on a specified date
        /// </summary>
        /// <param name="_date">Date to check</param>
        /// <returns>Closing time as TimeSpan</returns>
        public static TimeSpan getCloseTimeForDate(DateTime _date)
        {
            var date = _date.Date;
            switch (date.Month)
            {
                case 7:
                    if (date.Day == 3 && !isWeekend(date.AddDays(1)))
                        return earlyCloseTime;
                    else
                        return normalCloseTime;
                case 11:
                    var dayAfterThanksgiving = fourthThursday(date.Year, 11).AddDays(1);
                    return (date == dayAfterThanksgiving) ? earlyCloseTime : normalCloseTime;

                case 12:
                    if (date.Day == 24 && !isWeekend(date.AddDays(1)))
                        return earlyCloseTime;
                    else
                        return normalCloseTime;
                default:
                    return normalCloseTime;
            }
        }

        public static bool isEarlyCloseDay(DateTime date) =>
            getCloseTimeForDate(date.Date).TotalHours < normalCloseTime.TotalHours;

        /// <summary>
        /// Check if the given date is a holiday. Returns false for weekends.
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if the date is a holiday</returns>
        public static bool isHoliday(DateTime date) => getHolidays(date.Year).Contains(date.Date);

        public static bool isHolidayToday => isHoliday(nowEST);

        public static bool isExtraClosedDate(DateTime date) => extraCloseDates.Contains(date.Date);

        public static bool isWeekend(DateTime date) => 
            date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;


        /// <summary>
        /// The main method to check if the market is open on a given day.
        /// Checks for weekends, holidays, and special closed days.
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if the market is open on the given day</returns>
        public static bool isWorkingDay(DateTime date)
        {
            if (isWeekend(date))
                return false;
            return !isHoliday(date) && !isExtraClosedDate(date);
        }

        /// <summary>
        /// Get a list of holidays for several years.
        /// </summary>
        /// <param name="yearStart">First year to start</param>
        /// <param name="yearCount">Number of years</param>
        /// <returns>List of holiday dates</returns>
        public static List<DateTime> getHolidays(int yearStart, int yearCount)
        {
            var list = new List<DateTime>();
            for (int i = 0; i < yearCount; i++)
                list.AddRange(getHolidays(yearStart + i));
            return list;
        }


        /// <summary>
        /// New Year holiday date or null if none.
        /// </summary>
        /// <param name="year">Year to check</param>
        /// <returns>New Year holiday date or null if it falls on Saturday</returns>
        public static DateTime? getNewYear(int year)
        {
            var newYearsDate = new DateTime(year, 1, 1);
            switch (newYearsDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return newYearsDate.AddDays(1);

                case DayOfWeek.Saturday:
                    return null;

                default:
                    return newYearsDate;
            }

        }

        public static DateTime getMartinLutherDay(int year) => thirdMonday(year, 1);
        public static DateTime getPresidentDay(int year) => thirdMonday(year, 2);
        public static DateTime getThanksGiving(int year) => fourthThursday(year, 11);

        public static DateTime getIndependenceDay(int year) => adjustForWeekendHoliday(new DateTime(year, 7, 4));
        public static DateTime getJuneteenthDay(int year) => adjustForWeekendHoliday(new DateTime(year, 6, 19));
        public static DateTime getChristmasDay(int year) => adjustForWeekendHoliday(new DateTime(year, 12, 25));

        public static DateTime getMemorialDay(int year)
        {
            var memorialDay = new DateTime(year, 5, 31);
            var dayOfWeek = memorialDay.DayOfWeek;
            while (dayOfWeek != DayOfWeek.Monday)
            {
                memorialDay = memorialDay.AddDays(-1);
                dayOfWeek = memorialDay.DayOfWeek;
            }
            return memorialDay;
        }


        public static DateTime getLaborDay(int year)
        {
            var laborDay = new DateTime(year, 9, 1);
            var dayOfWeek = laborDay.DayOfWeek;
            while (dayOfWeek != DayOfWeek.Monday)
            {
                laborDay = laborDay.AddDays(1);
                dayOfWeek = laborDay.DayOfWeek;
            }
            return laborDay;
        }


        private static readonly ConcurrentDictionary<int, HashSet<DateTime>> dicHolidaysByYear = new ConcurrentDictionary<int, HashSet<DateTime>>();

        /// <summary>
        /// Get all the holidays for a given year.
        /// </summary>
        /// <param name="year">Year to get holidays for</param>
        /// <returns>Set of holiday dates</returns>
        public static HashSet<DateTime> getHolidays(int year)
        {
            return dicHolidaysByYear.GetOrAdd(year, y =>
            {
                var holidays = new HashSet<DateTime>();

                var newYearsDate = getNewYear(y);
                if (newYearsDate.HasValue)
                    holidays.Add(newYearsDate.Value);

                holidays.Add(getMartinLutherDay(y));
                holidays.Add(getPresidentDay(y));
                holidays.Add(getGoodFriday(y));
                holidays.Add(getMemorialDay(y));
                holidays.Add(getJuneteenthDay(y));
                holidays.Add(getIndependenceDay(y));
                holidays.Add(getLaborDay(y));
                holidays.Add(getThanksGiving(y));
                holidays.Add(getChristmasDay(y));

                return holidays;
            });
        }

        private static DateTime adjustForWeekendHoliday(DateTime holiday)
        {
            if (holiday.DayOfWeek == DayOfWeek.Saturday)
                return holiday.AddDays(-1);
            return holiday.DayOfWeek == DayOfWeek.Sunday ? holiday.AddDays(1) : holiday;
        }

        /// <summary>
        /// Get the next trading date after the specified date.
        /// </summary>
        /// <param name="afterDt">Given date</param>
        /// <returns>Next trading day</returns>
        public static DateTime nextTradingDayAfter(DateTime afterDt)
        {
            var dt = afterDt.Date;
            do
                dt = dt.AddDays(1);
            while (!isWorkingDay(dt));
            return dt;
        }

        /// <summary>
        /// Get the last trading day before a given date
        /// </summary>
        /// <param name="beforeDt">Date to check</param> 
        /// <returns>Last trading day before a given date</returns>
        public static DateTime prevTradingDayBefore(DateTime beforeDt)
        {
            var dt = beforeDt.Date;
            do
                dt = dt.AddDays(-1);
            while (!isWorkingDay(dt));
            return dt;
        }

        public static DateTime nextTradingDay => nextTradingDayAfter(nowEST);
        public static DateTime prevTradingDay => prevTradingDayBefore(nowEST);

        public static bool isEarlyCloseToday => isEarlyCloseDay(nowEST);

        public static bool isWorkingDayToday => isWorkingDay(nowEST);

        public static TimeSpan closeTimeForToday => getCloseTimeForDate(nowEST);

        public static MarketTime checkTimeNow => checkTime(nowEST);

        public static MarketTime checkTime(DateTime estTime)
        {
            var m = estTime.TimeOfDay.TotalMinutes;
            if (m < 9 * 60 + 30)
                return MarketTime.beforeOpen;
            if (m >= getCloseTimeForDate(estTime).TotalMinutes)
                return MarketTime.afterClose;
            return MarketTime.open;
        }

        static public TimeSpan timeLeftToOpen => TimeSpan.FromMinutes(9 * 60 + 30) - nowEST.TimeOfDay;
        static public TimeSpan timeLeftToClose => closeTimeForToday - nowEST.TimeOfDay;


        /// <summary>
        /// Checks if the market is open at a given date and time.
        /// Weekends, holidays, and early hours are all considered.
        /// </summary>
        /// <param name="estTime">Date and time to check (in EST)</param>
        /// <returns>True if the market is open at the specified time</returns>
        public static bool isMarketOpenAt(DateTime estTime)
        {
            if (!isWorkingDay(estTime))
                return false;
            return checkTime(estTime) == MarketTime.open;
        }

        public static bool isMarketOpenNow => isMarketOpenAt(nowEST);


        public static bool isDstChangeInBetween(DateTime dt1, DateTime dt2) =>
            estTimeZone.IsDaylightSavingTime(dt1) != estTimeZone.IsDaylightSavingTime(dt2);

    }
}
