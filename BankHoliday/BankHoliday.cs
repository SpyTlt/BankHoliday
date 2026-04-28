// (c) Vitas Ramanchauskas www.SpyTlt.com, MIT license
// Bank holidays and trading hours for U.S. equity markets.
// Key methods:
//   nowEST                       - current DateTime in EST timezone
//   isWorkingDay                 - checks if the market is open on a given date
//   isMarketOpenNow / isMarketOpenAt - check if the market is open right now or at a specified DateTime
//   nextWeeklyOptionsDate        - next weekly options expiration (Friday, adjusted for holidays)
//   thirdFriday / thirdFridaySmart - monthly options expiration helpers

using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace BankHolidayNS;

/// <summary>
/// Support for US bank holidays and equity market trading hours.
/// </summary>
public static class BankHoliday
{
    /// <summary>
    /// Current market trading period.
    /// </summary>
    public enum MarketTime { beforeOpen, open, afterClose }

    /// <summary>
    /// Eastern Standard Time zone (handles DST automatically).
    /// </summary>
    public static TimeZoneInfo estTimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

    /// <summary>
    /// Convert local DateTime to EST timezone.
    /// </summary>
    /// <param name="dt">DateTime to convert</param>
    /// <returns>Given DateTime in EST timezone</returns>
    public static DateTime toEst(DateTime dt) => TimeZoneInfo.ConvertTime(dt, estTimeZone);

    /// <summary>
    /// Convert EST DateTime back to local timezone.
    /// </summary>
    /// <param name="dt">DateTime in EST</param>
    /// <returns>Same instant in local timezone</returns>
    public static DateTime fromEst(DateTime dt) => TimeZoneInfo.ConvertTime(dt, estTimeZone, TimeZoneInfo.Local);

    /// <summary>
    /// Current DateTime in EST timezone.
    /// </summary>
    public static DateTime nowEST => toEst(DateTime.Now);

    /// <summary>
    /// Extra days when the market was closed for exceptional reasons (9/11, Hurricane Sandy, presidential funerals, etc.).
    /// </summary>
    public static readonly HashSet<DateTime> extraCloseDates =
    [
        new(2001, 9, 11), new(2001, 9, 12), new(2001, 9, 13), new(2001, 9, 14),
        new(2004, 6, 11),                                 // Reagan funeral
        new(2007, 1, 2),                                  // Ford funeral
        new(2012, 10, 29), new(2012, 10, 30),             // Hurricane Sandy
        new(2018, 12, 5),                                 // GHW Bush funeral
        new(2025, 1, 9)                                   // Carter National Day of Mourning
    ];

    /// <summary>
    /// Compute Good Friday for a given year (Western Easter algorithm).
    /// </summary>
    /// <param name="year">Given year</param>
    /// <returns>Exact date of Good Friday</returns>
    public static DateTime getGoodFriday(int year)
    {
        var g = year % 19;
        var c = year / 100;
        var h = (c - c / 4 - (8 * c + 13) / 25 + 19 * g + 15) % 30;
        var i = h - h / 28 * (1 - h / 28 * (29 / (h + 1)) * ((21 - g) / 11));

        var day = i - (year + year / 4 + i + 2 - c + c / 4) % 7 + 28;
        var month = 3;

        if (day > 31)
        {
            month++;
            day -= 31;
        }

        return new DateTime(year, month, day).AddDays(-2);
    }

    private static DateTime thirdMonday(int year, int month) =>
        new(year, month, 21 - ((int)new DateTime(year, month, 1).DayOfWeek + 5) % 7);

    private static DateTime fourthThursday(int year, int month) =>
        new(year, month, 28 - ((int)new DateTime(year, month, 1).DayOfWeek + 2) % 7);

    /// <summary>
    /// Standard market opening time (9:30 AM EST).
    /// </summary>
    public static TimeSpan openTime { get; } = new(9, 30, 0);

    /// <summary>
    /// Normal market closing time (4:00 PM EST).
    /// </summary>
    public static TimeSpan normalCloseTime { get; } = new(16, 0, 0);

    /// <summary>
    /// Early market closing time (1:00 PM EST).
    /// </summary>
    public static TimeSpan earlyCloseTime { get; } = new(13, 0, 0);

    /// <summary>
    /// When does the market close on a specified date.
    /// </summary>
    /// <param name="_date">Date to check</param>
    /// <returns>Closing time as TimeSpan</returns>
    public static TimeSpan getCloseTimeForDate(DateTime _date)
    {
        var date = _date.Date;
        switch (date.Month)
        {
            case 7:
                return date.Day == 3 && !isWeekend(date.AddDays(1)) ? earlyCloseTime : normalCloseTime;

            case 11:
                var dayAfterThanksgiving = fourthThursday(date.Year, 11).AddDays(1);
                return date == dayAfterThanksgiving ? earlyCloseTime : normalCloseTime;

            case 12:
                return date.Day == 24 && !isWeekend(date.AddDays(1)) ? earlyCloseTime : normalCloseTime;

            default:
                return normalCloseTime;
        }
    }

    /// <summary>
    /// Check if the market closes early on the given date.
    /// </summary>
    public static bool isEarlyCloseDay(DateOnly date) => isEarlyCloseDay(date.ToDateTime(TimeOnly.MinValue));

    /// <summary>
    /// Check if the market closes early on the given date.
    /// </summary>
    public static bool isEarlyCloseDay(DateTime date) =>
        getCloseTimeForDate(date.Date).TotalHours < normalCloseTime.TotalHours;

    /// <summary>
    /// Check if the given date is a market holiday. Returns false for weekends.
    /// </summary>
    public static bool isHoliday(DateOnly date) => isHoliday(date.ToDateTime(TimeOnly.MinValue));

    /// <summary>
    /// Check if the given date is a market holiday. Returns false for weekends.
    /// </summary>
    public static bool isHoliday(DateTime date) => getHolidays(date.Year).Contains(date.Date);

    /// <summary>
    /// True if today is a market holiday.
    /// </summary>
    public static bool isHolidayToday => isHoliday(nowEST);

    /// <summary>
    /// True if the date is in the extra closed-dates list (9/11, Sandy, etc.).
    /// </summary>
    public static bool isExtraClosedDate(DateTime date) => extraCloseDates.Contains(date.Date);

    /// <summary>
    /// True if the date is Saturday or Sunday.
    /// </summary>
    public static bool isWeekend(DateTime date) => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// The main method to check if the market is open on a given day.
    /// Considers weekends, holidays, and special closed days.
    /// </summary>
    public static bool isWorkingDay(DateTime date) =>
        !isWeekend(date) && !isHoliday(date) && !isExtraClosedDate(date);

    /// <summary>
    /// The main method to check if the market is open on a given day.
    /// Considers weekends, holidays, and special closed days.
    /// </summary>
    public static bool isWorkingDay(DateOnly date) => isWorkingDay(date.ToDateTime(TimeOnly.MinValue));

    /// <summary>
    /// Get a list of holidays for several years.
    /// </summary>
    /// <param name="yearStart">First year to start</param>
    /// <param name="yearCount">Number of years</param>
    /// <returns>List of holiday dates</returns>
    public static List<DateTime> getHolidays(int yearStart, int yearCount)
    {
        var list = new List<DateTime>();
        for (var i = 0; i < yearCount; i++)
            list.AddRange(getHolidays(yearStart + i));
        return list;
    }

    /// <summary>
    /// New Year holiday date or null if it falls on Saturday.
    /// </summary>
    public static DateTime? getNewYear(int year)
    {
        var newYearsDate = new DateTime(year, 1, 1);
        return newYearsDate.DayOfWeek switch
        {
            DayOfWeek.Sunday => (DateTime?)newYearsDate.AddDays(1),
            DayOfWeek.Saturday => null,
            _ => (DateTime?)newYearsDate
        };
    }

    /// <summary>Martin Luther King Jr. Day (third Monday of January).</summary>
    public static DateTime getMartinLutherDay(int year) => thirdMonday(year, 1);

    /// <summary>Presidents' Day (third Monday of February).</summary>
    public static DateTime getPresidentDay(int year) => thirdMonday(year, 2);

    /// <summary>Thanksgiving Day (fourth Thursday of November).</summary>
    public static DateTime getThanksGiving(int year) => fourthThursday(year, 11);

    /// <summary>Independence Day (July 4, adjusted for weekends).</summary>
    public static DateTime getIndependenceDay(int year) => adjustForWeekendHoliday(new DateTime(year, 7, 4));

    /// <summary>Juneteenth (June 19, adjusted for weekends; observed by NYSE starting 2022).</summary>
    public static DateTime getJuneteenthDay(int year) => adjustForWeekendHoliday(new DateTime(year, 6, 19));

    /// <summary>Christmas Day (December 25, adjusted for weekends).</summary>
    public static DateTime getChristmasDay(int year) => adjustForWeekendHoliday(new DateTime(year, 12, 25));

    /// <summary>Memorial Day (last Monday of May).</summary>
    public static DateTime getMemorialDay(int year)
    {
        var memorialDay = new DateTime(year, 5, 31);
        while (memorialDay.DayOfWeek != DayOfWeek.Monday)
            memorialDay = memorialDay.AddDays(-1);
        return memorialDay;
    }

    /// <summary>Labor Day (first Monday of September).</summary>
    public static DateTime getLaborDay(int year)
    {
        var laborDay = new DateTime(year, 9, 1);
        while (laborDay.DayOfWeek != DayOfWeek.Monday)
            laborDay = laborDay.AddDays(1);
        return laborDay;
    }

    private static readonly ConcurrentDictionary<int, FrozenSet<DateTime>> dicHolidaysByYear = new();

    /// <summary>
    /// Get all market holidays for a given year (cached, immutable).
    /// </summary>
    public static FrozenSet<DateTime> getHolidays(int year) =>
        dicHolidaysByYear.GetOrAdd(year, buildHolidays);

    private static FrozenSet<DateTime> buildHolidays(int year)
    {
        var holidays = new HashSet<DateTime>();

        var newYearsDate = getNewYear(year);
        if (newYearsDate.HasValue)
            holidays.Add(newYearsDate.Value);

        holidays.Add(getMartinLutherDay(year));
        holidays.Add(getPresidentDay(year));
        holidays.Add(getGoodFriday(year));
        holidays.Add(getMemorialDay(year));
        // NYSE began observing Juneteenth as a market holiday in 2022.
        if (year >= 2022)
            holidays.Add(getJuneteenthDay(year));
        holidays.Add(getIndependenceDay(year));
        holidays.Add(getLaborDay(year));
        holidays.Add(getThanksGiving(year));
        holidays.Add(getChristmasDay(year));

        return holidays.ToFrozenSet();
    }

    private static DateTime adjustForWeekendHoliday(DateTime holiday) =>
        holiday.DayOfWeek == DayOfWeek.Saturday ? holiday.AddDays(-1)
        : holiday.DayOfWeek == DayOfWeek.Sunday ? holiday.AddDays(1)
        : holiday;

    /// <summary>
    /// Get the next trading day strictly after the specified date.
    /// </summary>
    public static DateTime nextTradingDayAfter(DateTime afterDt)
    {
        var dt = afterDt.Date;
        do
            dt = dt.AddDays(1);
        while (!isWorkingDay(dt));
        return dt;
    }

    /// <summary>
    /// Get the last trading day strictly before the specified date.
    /// </summary>
    public static DateTime prevTradingDayBefore(DateTime beforeDt)
    {
        var dt = beforeDt.Date;
        do
            dt = dt.AddDays(-1);
        while (!isWorkingDay(dt));
        return dt;
    }

    /// <summary>Next trading day after now.</summary>
    public static DateTime nextTradingDay => nextTradingDayAfter(nowEST);

    /// <summary>Previous trading day before now.</summary>
    public static DateTime prevTradingDay => prevTradingDayBefore(nowEST);

    /// <summary>True if today is an early-close day.</summary>
    public static bool isEarlyCloseToday => isEarlyCloseDay(nowEST);

    /// <summary>True if today is a trading day.</summary>
    public static bool isWorkingDayToday => isWorkingDay(nowEST);

    /// <summary>Market closing time for today.</summary>
    public static TimeSpan closeTimeForToday => getCloseTimeForDate(nowEST);

    /// <summary>Current market period (before open, open, after close).</summary>
    public static MarketTime checkTimeNow => checkTime(nowEST);

    /// <summary>
    /// Determine market period for a given EST time.
    /// </summary>
    public static MarketTime checkTime(DateTime estTime) =>
        estTime.TimeOfDay < openTime ? MarketTime.beforeOpen
        : estTime.TimeOfDay >= getCloseTimeForDate(estTime) ? MarketTime.afterClose
        : MarketTime.open;

    /// <summary>Time remaining until market opens today (negative if already open).</summary>
    public static TimeSpan timeLeftToOpen => openTime - nowEST.TimeOfDay;

    /// <summary>Time remaining until market closes today.</summary>
    public static TimeSpan timeLeftToClose => closeTimeForToday - nowEST.TimeOfDay;

    /// <summary>
    /// Check if the market is open at a given date and time (EST).
    /// Weekends, holidays, and early-close hours are all considered.
    /// </summary>
    public static bool isMarketOpenAt(DateTime estTime) =>
        isWorkingDay(estTime) && checkTime(estTime) == MarketTime.open;

    /// <summary>True if the market is currently open.</summary>
    public static bool isMarketOpenNow => isMarketOpenAt(nowEST);

    /// <summary>
    /// Check whether DST status changes between two dates.
    /// </summary>
    public static bool isDstChangeInBetween(DateTime dt1, DateTime dt2) =>
        estTimeZone.IsDaylightSavingTime(dt1) != estTimeZone.IsDaylightSavingTime(dt2);

    /// <summary>
    /// Get the most recent date that should have complete EOD data.
    /// </summary>
    /// <param name="bTodayAlwaysExclude">If true, always exclude today even after the close</param>
    public static DateTime getLastGoodDateForHistory(bool bTodayAlwaysExclude = false)
    {
        var baseDate = nowEST;
        if (baseDate.Hour >= 16 && !bTodayAlwaysExclude)
            baseDate = baseDate.AddDays(1);
        return prevTradingDayBefore(baseDate);
    }

    /// <summary>
    /// Count trading days strictly between two dates (excludes both endpoints).
    /// </summary>
    public static int countWorkingDaysBetweenDates(DateTime dt1, DateTime dt2)
    {
        dt2 = dt2.Date;
        var cnt = 0;
        for (var dt = nextTradingDayAfter(dt1); dt < dt2; dt = nextTradingDayAfter(dt))
            cnt++;
        return cnt;
    }

    /// <summary>
    /// Third Friday of the given month (used for monthly options expiration).
    /// </summary>
    public static DateTime thirdFriday(int year, int month) =>
        new(year, month, new DateTime(year, month, 1).DayOfWeek switch
        {
            DayOfWeek.Sunday => 20,
            DayOfWeek.Monday => 19,
            DayOfWeek.Tuesday => 18,
            DayOfWeek.Wednesday => 17,
            DayOfWeek.Thursday => 16,
            DayOfWeek.Friday => 15,
            DayOfWeek.Saturday => 21,
            _ => throw new ArgumentException()
        });

    /// <summary>
    /// Monthly options expiration. If the third Friday is a holiday, returns the previous Thursday.
    /// </summary>
    public static DateTime thirdFridaySmart(int year, int month)
    {
        var dt = thirdFriday(year, month);
        return isWorkingDay(dt) ? dt : dt.AddDays(-1);
    }

    /// <summary>Third Friday of the current month.</summary>
    public static DateTime thirdFridayCurMonth() => thirdFriday(DateTime.Now.Year, DateTime.Now.Month);

    /// <summary>Monthly options expiration for the current month (holiday-adjusted).</summary>
    public static DateTime thirdFridayCurMonthSmart() => thirdFridaySmart(DateTime.Now.Year, DateTime.Now.Month);

    /// <summary>
    /// Next third Friday on or after the given date.
    /// </summary>
    public static DateTime thirdFridayAfter(DateTime xdt)
    {
        var f1 = thirdFriday(xdt.Year, xdt.Month);
        return f1.Date >= xdt.Date ? f1
            : xdt.Month < 12 ? thirdFriday(xdt.Year, xdt.Month + 1)
            : thirdFriday(xdt.Year + 1, 1);
    }

    /// <summary>
    /// Next Friday on or after the given date.
    /// </summary>
    public static DateTime nextFriday(DateTime date) =>
        date.AddDays((DayOfWeek.Friday - date.DayOfWeek + 7) % 7);

    /// <summary>Next Friday on or after today.</summary>
    public static DateTime nextFriday() => nextFriday(DateTime.Today);

    /// <summary>
    /// Next weekly options expiration (Friday, adjusted backward for holidays).
    /// </summary>
    /// <param name="date">Reference date</param>
    /// <param name="bWeekAheadIfFriday">If today is the expiration day, jump to next week</param>
    public static DateTime nextWeeklyOptionsDate(DateTime date, bool bWeekAheadIfFriday)
    {
        var friday = nextFriday(date);
        var dt = friday;
        while (!isWorkingDay(dt))
            dt = dt.AddDays(-1);
        if (date.Date == dt.Date && bWeekAheadIfFriday)
        {
            dt = friday.AddDays(7);
            while (!isWorkingDay(dt))
                dt = dt.AddDays(-1);
        }
        return dt;
    }

    /// <summary>
    /// Next weekly options expiration from today.
    /// </summary>
    public static DateTime nextWeeklyOptionsDate(bool bWeekAheadIfFriday) =>
        nextWeeklyOptionsDate(DateTime.Today, bWeekAheadIfFriday);
}
