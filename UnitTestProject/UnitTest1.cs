using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankHolidayNS;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void checkDstDetect()
        {
            Assert.IsTrue(BankHoliday.isDstChangeInBetween(new DateTime(2016, 3, 12), new DateTime(2016, 3, 14)));
            Assert.IsTrue(BankHoliday.isDstChangeInBetween(new DateTime(2016, 11, 6), new DateTime(2016, 11, 7)));
            Assert.IsFalse(BankHoliday.isDstChangeInBetween(new DateTime(2016, 11, 5), new DateTime(2016, 11, 6)));
            Assert.IsFalse(BankHoliday.isDstChangeInBetween(new DateTime(2016, 3, 12), new DateTime(2016, 3, 13)));
        }

        [TestMethod]
        public void testCloseTime()
        {
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2015, 11, 27)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2015, 12, 24)).TotalHours);
            Assert.AreEqual(16, BankHoliday.getCloseTimeForDate(new DateTime(2016, 12, 24)).TotalHours);
            Assert.AreEqual(16, BankHoliday.getCloseTimeForDate(new DateTime(2016, 7, 2)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2014, 7, 3)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2014, 12, 24)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2014, 11, 28)).TotalHours);

            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2013, 7, 3)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2013, 11, 29)).TotalHours);
            Assert.AreEqual(13, BankHoliday.getCloseTimeForDate(new DateTime(2013, 12, 24)).TotalHours);
        }

        [TestMethod]
        public void testHolidays()
        {
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 1, 1)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 1, 21)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 2, 18)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 4, 19)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 5, 27)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 7, 4)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 9, 2)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 11, 28)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 12, 25)));


            Assert.IsTrue(BankHoliday.isWorkingDay(new DateTime(2019, 7, 3)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2020, 7, 3)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2021, 7, 5)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2021, 12, 24)));

            Assert.IsTrue(BankHoliday.isWorkingDay(new DateTime(2019, 6, 21)));
            Assert.IsFalse(BankHoliday.isWorkingDay(new DateTime(2019, 6, 22)));
        }

    }
}
