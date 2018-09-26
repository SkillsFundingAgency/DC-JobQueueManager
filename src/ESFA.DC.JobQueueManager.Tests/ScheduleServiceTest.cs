using System;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.ExternalData;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.JobQueueManager.Tests
{
    public sealed class ScheduleServiceTest
    {
        [Fact]
        public void CanExecuteScheduleLastExecutionTime()
        {
            var dateTimeUtcNow = new DateTime(2018, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Month = 10,
                DayOfTheMonth = 21,
                Hour = 16,
                Minute = 0,
                LastExecuteDateTime = dateTimeUtcNow
            };

            var scheduleService = new ScheduleService(logger.Object);
            var canExecuteSchedule = scheduleService.CanExecuteSchedule(schedule, dateTimeUtcNow, true);

            canExecuteSchedule.Should().BeFalse();
        }

        [Fact]
        public void CanExecuteScheduleRemoveOldDateFalse()
        {
            var dateTimeUtcNow = new DateTime(2018, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Month = 10,
                DayOfTheMonth = 21,
                Hour = 16,
                Minute = 0
            };

            var scheduleService = new ScheduleService(logger.Object);
            var canExecuteSchedule = scheduleService.CanExecuteSchedule(schedule, dateTimeUtcNow, false);

            canExecuteSchedule.Should().BeTrue();
        }

        [Fact]
        public void CanExecuteScheduleRemoveOldDateTrue()
        {
            var dateTimeUtcNow = new DateTime(2018, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Month = 10,
                DayOfTheMonth = 21,
                Hour = 16,
                Minute = 0
            };

            var scheduleService = new ScheduleService(logger.Object);
            var canExecuteSchedule = scheduleService.CanExecuteSchedule(schedule, dateTimeUtcNow, true);

            canExecuteSchedule.Should().BeTrue();
        }

        [Fact]
        public void TestDayOfMonth()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheMonth = 21
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Day.Should().Be(21);
        }

        [Fact]
        public void TestDayOfMonthAndDayOfWeekA()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc); // Wednesday
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheMonth = 23,
                DayOfTheWeek = (byte)DayOfWeek.Thursday
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Day.Should().Be(22);
        }

        [Fact]
        public void TestDayOfMonthAndDayOfWeekB()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc); // Wednesday
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheMonth = 23,
                DayOfTheWeek = (byte)DayOfWeek.Saturday
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Day.Should().Be(23);
        }

        [Fact]
        public void TestDayOfMonthNot()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheMonth = 1
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Day.Should().Be(1);
        }

        [Fact]
        public void TestDayOfMonthNotRemove()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheMonth = 1
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, true);

            nextRun.Should().BeNull();
        }

        [Fact]
        public void TestDayOfWeek()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc); // Wednesday
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheWeek = (byte)DayOfWeek.Thursday
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Day.Should().Be(22);
        }

        [Fact]
        public void TestDayOfWeekNot()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 31, 16, 0, 0, DateTimeKind.Utc); // Saturday
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheWeek = (byte)DayOfWeek.Thursday
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Day.Should().Be(5);
            nextRun?.Month.Should().Be(11);
        }

        [Fact]
        public void TestDayOfWeekNotRemove()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 31, 16, 0, 0, DateTimeKind.Utc); // Saturday
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                DayOfTheWeek = (byte)DayOfWeek.Thursday
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, true);

            nextRun.Should().BeNull();
        }

        [Fact]
        public void TestDefault()
        {
            var dateTimeUtcNow = DateTime.UtcNow;
            var logger = new Mock<ILogger>();

            var schedule = new Schedule();

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Month.Should().Be(dateTimeUtcNow.Month);
            nextRun?.Day.Should().Be(dateTimeUtcNow.Day);
            nextRun?.Hour.Should().Be(dateTimeUtcNow.Hour);
            nextRun?.Minute.Should().Be(dateTimeUtcNow.Minute);
        }

        [Fact]
        public void TestHour()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Hour = 15
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Hour.Should().Be(15);
        }

        [Fact]
        public void TestHourNot()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Hour = 17
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Hour.Should().Be(17);
        }

        [Fact]
        public void TestHourNotRemove()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Hour = 15
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, true);

            nextRun.Should().BeNull();
        }

        [Fact]
        public void TestMinute()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 15, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Minute = 14
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Minute.Should().Be(14);
        }

        [Fact]
        public void TestMinuteCadence()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 15, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                MinuteIsCadence = true,
                Minute = 5
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Minute.Should().Be(15);
        }

        [Fact]
        public void TestMinuteCadenceEnd()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 57, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                MinuteIsCadence = true,
                Minute = 5
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Minute.Should().Be(0);
        }

        [Fact]
        public void TestMinuteCadenceStart()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                MinuteIsCadence = true,
                Minute = 5
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Minute.Should().Be(0);
        }

        [Fact]
        public void TestMinuteNot()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 15, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Minute = 16
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Minute.Should().Be(16);
        }

        [Fact]
        public void TestMinuteNotRemove()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 15, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Minute = 14
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, true);

            nextRun.Should().BeNull();
        }

        [Fact]
        public void TestMonth()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Month = 10
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Month.Should().Be(10);
        }

        [Fact]
        public void TestMonthNot()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Month = 9
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, false);

            nextRun.Should().NotBeNull();
            nextRun?.Month.Should().Be(9);
        }

        [Fact]
        public void TestMonthNotRemove()
        {
            var dateTimeUtcNow = new DateTime(2015, 10, 21, 16, 0, 0, DateTimeKind.Utc);
            var logger = new Mock<ILogger>();

            var schedule = new Schedule
            {
                Month = 9
            };

            var scheduleService = new ScheduleService(logger.Object);
            var nextRun = scheduleService.GetNextRun(dateTimeUtcNow, schedule, true);

            nextRun.Should().BeNull();
        }
    }
}