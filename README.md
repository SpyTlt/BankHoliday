# BankHoliday

A lightweight .NET library for U.S. stock market holidays and trading hours.

## Features

- Determine if a given date is a trading day
- Check if the market is currently open (or at any specified DateTime)
- Market open / close times, including early-close days (1:00 PM ET)
- Compute next / previous trading days
- All U.S. equity market holidays: New Year's, MLK Day (from 1998), Presidents Day, Good Friday,
  Memorial Day, Juneteenth (from 2022), Independence Day, Labor Day, Thanksgiving, Christmas
- Special closure dates: 9/11, Hurricane Sandy, presidential funerals (Nixon 1994, Reagan 2004,
  Ford 2007, G.H.W. Bush 2018), Carter National Day of Mourning (2025-01-09)
- DST change detection between two dates
- Options expiration helpers: monthly third-Friday and weekly expirations (holiday-adjusted)
- `DateOnly` overloads for `isHoliday`, `isWorkingDay`, `isEarlyCloseDay`

## Requirements

- .NET 10.0+

## Installation

Install the [NuGet package](https://www.nuget.org/packages/BankHoliday):

```
dotnet add package BankHoliday
```

Or include the project in your solution:

```xml
<ProjectReference Include="path\to\BankHoliday\BankHoliday.csproj" />
```

## Usage

```csharp
using BankHolidayNS;

// Is the market open right now?
bool isOpen = BankHoliday.isMarketOpenNow;

// Is a specific date a trading day?
bool isWorkDay = BankHoliday.isWorkingDay(new DateTime(2025, 12, 25)); // false (Christmas)

// Today's close time (handles early-close days)
TimeSpan closeTime = BankHoliday.closeTimeForToday;

// Next / previous trading days
DateTime nextDay = BankHoliday.nextTradingDay;
DateTime prevDay = BankHoliday.prevTradingDay;

// Market status at a specific time (in EST)
BankHoliday.MarketTime status = BankHoliday.checkTime(someDateTime);
// Returns: MarketTime.beforeOpen, MarketTime.open, or MarketTime.afterClose

// Current time in EST
DateTime nowEst = BankHoliday.nowEST;

// All holidays for a year (immutable, cached)
var holidays = BankHoliday.getHolidays(2025);

// Count trading days strictly between two dates
int days = BankHoliday.countWorkingDaysBetweenDates(
    new DateTime(2025, 1, 1), new DateTime(2025, 2, 1));

// Options expirations
DateTime monthly = BankHoliday.thirdFridaySmart(2025, 4); // adjusts for Good Friday
DateTime weekly  = BankHoliday.nextWeeklyOptionsDate(bWeekAheadIfFriday: true);
```

Methods without a date argument (`nextFriday()`, `nextWeeklyOptionsDate(bool)`, `thirdFridayCurMonth()`, ...)
use the current date in the Eastern time zone, not the local machine date.

## Early Close Days

The library handles early-close days (1:00 PM ET):
- Day after Thanksgiving
- Christmas Eve (when Christmas is not on a weekend)
- July 3rd (when July 4th is not on a weekend)

`isEarlyCloseDay` is true only on a trading day; `getCloseTimeForDate` applies the calendar rule as is.

## License

MIT — see [LICENSE](LICENSE). (c) Vitas Ramanchauskas, [SpyTlt.com](https://www.SpyTlt.com)
