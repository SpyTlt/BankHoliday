# BankHoliday

A lightweight .NET library for U.S. stock market holidays and trading hours.

## Features

- Determine if a given date is a trading day
- Check if the market is currently open
- Get market open/close times (including early close days)
- Calculate next/previous trading days
- Handle all U.S. market holidays (New Year's, MLK Day, Presidents Day, Good Friday, Memorial Day, Juneteenth, Independence Day, Labor Day, Thanksgiving, Christmas)
- Account for special closure dates (e.g., 9/11, Hurricane Sandy)
- DST change detection

## Installation

Add a reference to `BankHoliday.dll` or include the project in your solution.

## Usage

```csharp
using BankHolidayNS;

// Check if market is open right now
bool isOpen = BankHoliday.isMarketOpenNow;

// Check if a specific date is a trading day
bool isWorkDay = BankHoliday.isWorkingDay(new DateTime(2025, 12, 25)); // false (Christmas)

// Get today's close time (handles early close days)
TimeSpan closeTime = BankHoliday.closeTimeForToday;

// Get next/previous trading days
DateTime nextDay = BankHoliday.nextTradingDay;
DateTime prevDay = BankHoliday.prevTradingDay;

// Check market status at a specific time (in EST)
MarketTime status = BankHoliday.checkTime(someDateTime);
// Returns: MarketTime.beforeOpen, MarketTime.open, or MarketTime.afterClose

// Get current time in EST
DateTime nowEst = BankHoliday.nowEST;

// Get all holidays for a year
HashSet<DateTime> holidays = BankHoliday.getHolidays(2025);
```

## Early Close Days

The library handles early close days (1:00 PM ET):
- Day after Thanksgiving
- Christmas Eve (if Christmas is not on weekend)
- July 3rd (if July 4th is not on weekend)

## Requirements

- .NET 10.0+

## License

MIT License - (c) Vitas Ramanchauskas, [SpyTlt.com](https://www.SpyTlt.com)
