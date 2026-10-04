using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankHolidayNS;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CheckDstDetect()
        {
            Assert.IsTrue(BankHoliday.isDstChangeInBetween(new DateTime(2016, 3, 12), new DateTime(2016, 3, 14)));
            Assert.IsTrue(BankHoliday.isDstChangeInBetween(new DateTime(2016, 11, 6), new DateTime(2016, 11, 7)));
            Assert.IsFalse(BankHoliday.isDstChangeInBetween(new DateTime(2016, 11, 5), new DateTime(2016, 11, 6)));
            Assert.IsFalse(BankHoliday.isDstChangeInBetween(new DateTime(2016, 3, 12), new DateTime(2016, 3, 13)));
        }

        [TestMethod]
        public void TestCloseTime()
        {
            // Day after Thanksgiving - early close
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2015, 11, 27)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2014, 11, 28)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2013, 11, 29)).TotalHours);

            // Christmas Eve - early close when Christmas not on weekend
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2015, 12, 24)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2014, 12, 24)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2013, 12, 24)).TotalHours);
            // Christmas on Sunday 2016 - Dec 24 is Saturday, normal close on Friday
            Assert.AreEqual(16, BankHoliday.getCloseTimeForDate(new DateTime(2016, 12, 24)).TotalHours);

            // July 3rd - early close when July 4th not on weekend
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2014, 7, 3)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2013, 7, 3)).TotalHours);
            // July 4th on Saturday 2015 - July 3rd is Friday holiday, July 2nd normal close
            Assert.AreEqual(16, BankHoliday.getCloseTimeForDate(new DateTime(2016, 7, 2)).TotalHours);

            // Regular day - normal close
            Assert.AreEqual(16, BankHoliday.getCloseTimeForDate(new DateTime(2024, 3, 15)).TotalHours);
        }

        [TestMethod]
        public void TestHolidays2019()
        {
            // 2019 holidays
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 1, 1)));   // New Year's Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 1, 21)));  // MLK Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 2, 18)));  // Presidents Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 4, 19)));  // Good Friday
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 5, 27)));  // Memorial Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 7, 4)));   // Independence Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 9, 2)));   // Labor Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 11, 28))); // Thanksgiving
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 12, 25))); // Christmas
        }

        [TestMethod]
        public void TestWeekendHolidayAdjustment()
        {
            // July 4th on Saturday 2020 -> observed Friday July 3rd
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2020, 7, 3)));
            Assert.IsTrue(BankHoliday.isWorkingDay(new DateTime(2019, 7, 3)));

            // July 4th on Sunday 2021 -> observed Monday July 5th
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2021, 7, 5)));

            // Christmas on Saturday 2021 -> observed Friday Dec 24th
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2021, 12, 24)));

            // Juneteenth 2022 on Sunday -> observed Monday June 20th
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2022, 6, 20)));
        }

        [TestMethod]
        public void TestWeekends()
        {
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 6, 22))); // Saturday
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 6, 23))); // Sunday
            Assert.IsTrue(BankHoliday.isWorkingDay(new DateTime(2019, 6, 21)));  // Friday

            Assert.IsTrue(BankHoliday.isWeekend(new DateTime(2024, 11, 30)));  // Saturday
            Assert.IsTrue(BankHoliday.isWeekend(new DateTime(2024, 12, 1)));   // Sunday
            Assert.IsFalse(BankHoliday.isWeekend(new DateTime(2024, 11, 29))); // Friday
        }

        [TestMethod]
        public void TestExtraCloseDates()
        {
            // 9/11 closure
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2001, 9, 11)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2001, 9, 12)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2001, 9, 13)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2001, 9, 14)));

            // Hurricane Sandy
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2012, 10, 29)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2012, 10, 30)));

            // President George H.W. Bush National Day of Mourning
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2018, 12, 5)));

            Assert.IsTrue(BankHoliday.isExtraClosedDate(new DateTime(2001, 9, 11)));
            Assert.IsFalse(BankHoliday.isExtraClosedDate(new DateTime(2024, 11, 28)));
        }

        [TestMethod]
        public void TestGoodFriday()
        {
            Assert.AreEqual(new DateTime(2019, 4, 19), BankHoliday.getGoodFriday(2019));
            Assert.AreEqual(new DateTime(2020, 4, 10), BankHoliday.getGoodFriday(2020));
            Assert.AreEqual(new DateTime(2021, 4, 2), BankHoliday.getGoodFriday(2021));
            Assert.AreEqual(new DateTime(2024, 3, 29), BankHoliday.getGoodFriday(2024));
            Assert.AreEqual(new DateTime(2025, 4, 18), BankHoliday.getGoodFriday(2025));
        }

        [TestMethod]
        public void TestMemorialDay()
        {
            // Last Monday of May
            Assert.AreEqual(new DateTime(2019, 5, 27), BankHoliday.getMemorialDay(2019));
            Assert.AreEqual(new DateTime(2024, 5, 27), BankHoliday.getMemorialDay(2024));
            Assert.AreEqual(new DateTime(2025, 5, 26), BankHoliday.getMemorialDay(2025));
        }

        [TestMethod]
        public void TestLaborDay()
        {
            // First Monday of September
            Assert.AreEqual(new DateTime(2019, 9, 2), BankHoliday.getLaborDay(2019));
            Assert.AreEqual(new DateTime(2024, 9, 2), BankHoliday.getLaborDay(2024));
            Assert.AreEqual(new DateTime(2025, 9, 1), BankHoliday.getLaborDay(2025));
        }

        [TestMethod]
        public void TestThanksgiving()
        {
            // Fourth Thursday of November
            Assert.AreEqual(new DateTime(2019, 11, 28), BankHoliday.getThanksGiving(2019));
            Assert.AreEqual(new DateTime(2024, 11, 28), BankHoliday.getThanksGiving(2024));
            Assert.AreEqual(new DateTime(2025, 11, 27), BankHoliday.getThanksGiving(2025));
        }

        [TestMethod]
        public void TestNewYear()
        {
            // Regular weekday
            Assert.AreEqual(new DateTime(2019, 1, 1), BankHoliday.getNewYear(2019));

            // Sunday -> observed Monday
            Assert.AreEqual(new DateTime(2023, 1, 2), BankHoliday.getNewYear(2023));

            // Saturday -> null (observed previous year Dec 31)
            Assert.IsNull(BankHoliday.getNewYear(2022));
        }

        [TestMethod]
        public void TestMarketTime()
        {
            var date = new DateTime(2024, 11, 25); // Regular Monday

            // Before open
            Assert.AreEqual(BankHoliday.MarketTime.beforeOpen, BankHoliday.checkTime(date.AddHours(9)));
            Assert.AreEqual(BankHoliday.MarketTime.beforeOpen, BankHoliday.checkTime(date.AddHours(9).AddMinutes(29)));

            // Market open
            Assert.AreEqual(BankHoliday.MarketTime.open, BankHoliday.checkTime(date.AddHours(9).AddMinutes(30)));
            Assert.AreEqual(BankHoliday.MarketTime.open, BankHoliday.checkTime(date.AddHours(12)));
            Assert.AreEqual(BankHoliday.MarketTime.open, BankHoliday.checkTime(date.AddHours(15).AddMinutes(59)));

            // After close
            Assert.AreEqual(BankHoliday.MarketTime.afterClose, BankHoliday.checkTime(date.AddHours(16)));
            Assert.AreEqual(BankHoliday.MarketTime.afterClose, BankHoliday.checkTime(date.AddHours(17)));
        }

        [TestMethod]
        public void TestMarketTimeEarlyClose()
        {
            var date = new DateTime(2024, 11, 29); // Day after Thanksgiving - early close

            Assert.AreEqual(BankHoliday.MarketTime.open, BankHoliday.checkTime(date.AddHours(12)));
            Assert.AreEqual(BankHoliday.MarketTime.afterClose, BankHoliday.checkTime(date.AddHours(13)));
        }

        [TestMethod]
        public void TestIsMarketOpenAt()
        {
            // Regular trading day during market hours
            Assert.IsTrue(BankHoliday.isMarketOpenAt(new DateTime(2024, 11, 25, 10, 0, 0)));

            // Regular trading day before open
            Assert.IsFalse(BankHoliday.isMarketOpenAt(new DateTime(2024, 11, 25, 9, 0, 0)));

            // Weekend
            Assert.IsFalse(BankHoliday.isMarketOpenAt(new DateTime(2024, 11, 30, 12, 0, 0)));

            // Holiday
            Assert.IsFalse(BankHoliday.isMarketOpenAt(new DateTime(2024, 11, 28, 12, 0, 0)));
        }

        [TestMethod]
        public void TestNextTradingDay()
        {
            // Thursday -> Friday
            Assert.AreEqual(new DateTime(2024, 11, 22), BankHoliday.nextTradingDayAfter(new DateTime(2024, 11, 21)));

            // Friday -> Monday (skip weekend)
            Assert.AreEqual(new DateTime(2024, 11, 25), BankHoliday.nextTradingDayAfter(new DateTime(2024, 11, 22)));

            // Wednesday before Thanksgiving -> Friday (skip Thursday holiday)
            Assert.AreEqual(new DateTime(2024, 11, 29), BankHoliday.nextTradingDayAfter(new DateTime(2024, 11, 27)));
        }

        [TestMethod]
        public void TestPrevTradingDay()
        {
            // Friday -> Thursday
            Assert.AreEqual(new DateTime(2024, 11, 21), BankHoliday.prevTradingDayBefore(new DateTime(2024, 11, 22)));

            // Monday -> Friday (skip weekend)
            Assert.AreEqual(new DateTime(2024, 11, 22), BankHoliday.prevTradingDayBefore(new DateTime(2024, 11, 25)));

            // Friday after Thanksgiving -> Wednesday (skip Thursday holiday)
            Assert.AreEqual(new DateTime(2024, 11, 27), BankHoliday.prevTradingDayBefore(new DateTime(2024, 11, 29)));
        }

        [TestMethod]
        public void TestGetHolidaysForYear()
        {
            var holidays2024 = BankHoliday.getHolidays(2024);

            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 1, 1)));   // New Year's Day
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 1, 15)));  // MLK Day
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 2, 19)));  // Presidents Day
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 3, 29)));  // Good Friday
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 5, 27)));  // Memorial Day
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 6, 19)));  // Juneteenth
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 7, 4)));   // Independence Day
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 9, 2)));   // Labor Day
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 11, 28))); // Thanksgiving
            Assert.IsTrue(holidays2024.Contains(new DateTime(2024, 12, 25))); // Christmas

            Assert.AreEqual(10, holidays2024.Count);
        }

        [TestMethod]
        public void TestGetHolidaysMultipleYears()
        {
            var holidays = BankHoliday.getHolidays(2023, 3); // 2023, 2024, 2025

            Assert.IsTrue(holidays.Count >= 28); // At least 9-10 holidays per year
            Assert.IsTrue(holidays.Contains(new DateTime(2023, 12, 25)));
            Assert.IsTrue(holidays.Contains(new DateTime(2024, 12, 25)));
            Assert.IsTrue(holidays.Contains(new DateTime(2025, 12, 25)));
        }

        [TestMethod]
        public void TestIsEarlyCloseDay()
        {
            Assert.IsTrue(BankHoliday.isEarlyCloseDay(new DateTime(2024, 11, 29)));  // Day after Thanksgiving
            Assert.IsTrue(BankHoliday.isEarlyCloseDay(new DateTime(2024, 12, 24)));  // Christmas Eve
            Assert.IsTrue(BankHoliday.isEarlyCloseDay(new DateTime(2024, 7, 3)));    // Day before July 4th

            Assert.IsFalse(BankHoliday.isEarlyCloseDay(new DateTime(2024, 11, 25))); // Regular Monday
        }

        [TestMethod]
        public void TestIsHoliday()
        {
            Assert.IsTrue(BankHoliday.isHoliday(new DateTime(2024, 12, 25)));
            Assert.IsFalse(BankHoliday.isHoliday(new DateTime(2024, 12, 24)));
            Assert.IsFalse(BankHoliday.isHoliday(new DateTime(2024, 11, 30))); // Weekend - not a holiday
        }

        [TestMethod]
        public void TestTimeConversion()
        {
            var localTime = DateTime.Now;
            var estTime = BankHoliday.toEst(localTime);
            var backToLocal = BankHoliday.fromEst(estTime);

            // Round-trip should be close (within a second due to processing time)
            Assert.IsTrue(Math.Abs((localTime - backToLocal).TotalSeconds) < 1);
        }

        [TestMethod]
        public void TestHolidays2025()
        {
            // 2025 holidays
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 1, 1)));   // New Year's Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 1, 20)));  // MLK Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 2, 17)));  // Presidents Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 4, 18)));  // Good Friday
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 5, 26)));  // Memorial Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 6, 19)));  // Juneteenth
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 7, 4)));   // Independence Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 9, 1)));   // Labor Day
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 11, 27))); // Thanksgiving
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 12, 25))); // Christmas
        }

        [TestMethod]
        public void TestJuneteenthBefore2022NotHoliday()
        {
            // NYSE began observing Juneteenth as a market holiday in 2022.
            // For earlier years it should NOT be in the holiday set.
            Assert.IsFalse(BankHoliday.getHolidays(2019).Contains(new DateTime(2019, 6, 19)));
            Assert.IsFalse(BankHoliday.getHolidays(2020).Contains(new DateTime(2020, 6, 19)));
            Assert.IsFalse(BankHoliday.getHolidays(2021).Contains(new DateTime(2021, 6, 18))); // observed Friday for Sat 6/19
            Assert.IsFalse(BankHoliday.isHoliday(new DateTime(2019, 6, 19)));
            Assert.IsFalse(BankHoliday.isHoliday(new DateTime(2020, 6, 19)));
            Assert.IsFalse(BankHoliday.isHoliday(new DateTime(2021, 6, 18)));

            // June 19, 2019 was a Wednesday — without the holiday, it's a working day
            Assert.IsTrue(BankHoliday.isWorkingDay(new DateTime(2019, 6, 19)));

            // From 2022 onward Juneteenth is a holiday
            Assert.IsTrue(BankHoliday.getHolidays(2022).Contains(new DateTime(2022, 6, 20))); // observed Mon for Sun 6/19
            Assert.IsTrue(BankHoliday.getHolidays(2023).Contains(new DateTime(2023, 6, 19)));
        }

        [TestMethod]
        public void TestCarterDayOfMourning()
        {
            // President Jimmy Carter National Day of Mourning - Jan 9, 2025
            Assert.IsTrue(BankHoliday.isExtraClosedDate(new DateTime(2025, 1, 9)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2025, 1, 9)));
            Assert.IsFalse(BankHoliday.isMarketOpenAt(new DateTime(2025, 1, 9, 12, 0, 0)));
        }

        [TestMethod]
        public void TestDateOnlyOverloads()
        {
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateOnly(2024, 12, 25))); // Christmas
            Assert.IsTrue(BankHoliday.isWorkingDay(new DateOnly(2024, 11, 25)));  // Regular Monday
            Assert.IsTrue(BankHoliday.isHoliday(new DateOnly(2024, 12, 25)));
            Assert.IsTrue(BankHoliday.isEarlyCloseDay(new DateOnly(2024, 11, 29)));
        }

        [TestMethod]
        public void TestOpenTime()
        {
            Assert.AreEqual(new TimeSpan(9, 30, 0), BankHoliday.openTime);
            Assert.AreEqual(new TimeSpan(16, 0, 0), BankHoliday.normalCloseTime);
            Assert.AreEqual(new TimeSpan(13, 0, 0), BankHoliday.earlyCloseTime);
        }

        [TestMethod]
        public void TestCountWorkingDaysBetweenDates()
        {
            // Mon Nov 25 - Fri Nov 29 (Thu 11/28 is Thanksgiving)
            // Trading days strictly between: Tue 26, Wed 27 = 2
            Assert.AreEqual(2, BankHoliday.countWorkingDaysBetweenDates(new DateTime(2024, 11, 25), new DateTime(2024, 11, 29)));

            // Same week, Mon to Fri: Tue, Wed, Thu = 3 trading days
            Assert.AreEqual(3, BankHoliday.countWorkingDaysBetweenDates(new DateTime(2024, 11, 18), new DateTime(2024, 11, 22)));

            // Friday to next Monday: nothing strictly between except weekend
            Assert.AreEqual(0, BankHoliday.countWorkingDaysBetweenDates(new DateTime(2024, 11, 22), new DateTime(2024, 11, 25)));
        }

        [TestMethod]
        public void TestThirdFriday()
        {
            Assert.AreEqual(new DateTime(2024, 1, 19), BankHoliday.thirdFriday(2024, 1));
            Assert.AreEqual(new DateTime(2024, 6, 21), BankHoliday.thirdFriday(2024, 6));
            Assert.AreEqual(new DateTime(2024, 12, 20), BankHoliday.thirdFriday(2024, 12));
            Assert.AreEqual(new DateTime(2025, 4, 18), BankHoliday.thirdFriday(2025, 4)); // Good Friday
        }

        [TestMethod]
        public void TestThirdFridaySmart()
        {
            // April 2025: 3rd Friday is April 18 - that's Good Friday, so smart should give Thursday April 17
            Assert.AreEqual(new DateTime(2025, 4, 17), BankHoliday.thirdFridaySmart(2025, 4));

            // Regular month (no holiday) - smart equals plain third Friday
            Assert.AreEqual(new DateTime(2024, 1, 19), BankHoliday.thirdFridaySmart(2024, 1));
        }

        [TestMethod]
        public void TestThirdFridayAfter()
        {
            // Before the third Friday of June -> third Friday of June
            Assert.AreEqual(new DateTime(2024, 6, 21), BankHoliday.thirdFridayAfter(new DateTime(2024, 6, 1)));

            // After the third Friday of June -> third Friday of July
            Assert.AreEqual(new DateTime(2024, 7, 19), BankHoliday.thirdFridayAfter(new DateTime(2024, 6, 22)));

            // December rolls to next year January
            Assert.AreEqual(new DateTime(2025, 1, 17), BankHoliday.thirdFridayAfter(new DateTime(2024, 12, 22)));
        }

        [TestMethod]
        public void TestNextFriday()
        {
            // From Monday -> upcoming Friday
            Assert.AreEqual(new DateTime(2024, 11, 29), BankHoliday.nextFriday(new DateTime(2024, 11, 25)));

            // From the same Friday -> same day (>= today behavior)
            Assert.AreEqual(new DateTime(2024, 11, 29), BankHoliday.nextFriday(new DateTime(2024, 11, 29)));

            // From Saturday -> next Friday
            Assert.AreEqual(new DateTime(2024, 12, 6), BankHoliday.nextFriday(new DateTime(2024, 11, 30)));
        }

        [TestMethod]
        public void TestNextWeeklyOptionsDate()
        {
            // Mon Nov 25, 2024 -> Fri Nov 29 (regular working day)
            Assert.AreEqual(new DateTime(2024, 11, 29), BankHoliday.nextWeeklyOptionsDate(new DateTime(2024, 11, 25), false));

            // From Fri Nov 29 with bWeekAheadIfFriday=true -> Fri Dec 6
            Assert.AreEqual(new DateTime(2024, 12, 6), BankHoliday.nextWeeklyOptionsDate(new DateTime(2024, 11, 29), true));

            // From Fri Nov 29 with bWeekAheadIfFriday=false -> Fri Nov 29
            Assert.AreEqual(new DateTime(2024, 11, 29), BankHoliday.nextWeeklyOptionsDate(new DateTime(2024, 11, 29), false));

            // Around Good Friday 2025: Fri April 18 is closed, so options expire Thu April 17
            Assert.AreEqual(new DateTime(2025, 4, 17), BankHoliday.nextWeeklyOptionsDate(new DateTime(2025, 4, 14), false));
        }

        [TestMethod]
        public void TestGetHolidaysReturnsImmutableSet()
        {
            // FrozenSet is immutable - should still support standard read operations
            var holidays = BankHoliday.getHolidays(2024);
            Assert.IsTrue(holidays.Contains(new DateTime(2024, 12, 25)));
            Assert.IsFalse(holidays.Contains(new DateTime(2024, 12, 24)));
        }
    }
}
