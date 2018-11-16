using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.ExternalData;
using ESFA.DC.JobQueueManager.Interfaces.ExternalData;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ESFA.DC.JobQueueManager.Tests
{
    public sealed class ExternalDataScheduleServiceTest
    {
        [Fact]
        public async Task TestRemove()
        {
            DateTime utcNow = DateTime.UtcNow;
            Mock<IDateTimeProvider> dateTimeProvider = new Mock<IDateTimeProvider>();
            Mock<IScheduleService> scheduleService = new Mock<IScheduleService>();
            Mock<ILogger> logger = new Mock<ILogger>();

            dateTimeProvider.Setup(x => x.GetNowUtc()).Returns(utcNow);
            scheduleService
                .Setup(x => x.CanExecuteSchedule(It.IsAny<Schedule>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
                .Returns(true);

            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<JobQueueDataContext>()
                    .UseSqlite(connection)
                    .Options;

                // Create the schema in the database
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobTypeGroup.Add(new JobTypeGroup
                    {
                        JobTypeGroupId = 3,
                        Description = "Reference Data",
                        ConcurrentExecutionCount = 25
                    });
                    context.JobType.Add(new JobType
                    {
                        JobTypeId = 40,
                        JobTypeGroupId = 3,
                        Description = "ReferenceData EPA",
                        Title = "ReferenceData EPA",
                        IsCrossLoadingEnabled = false
                    });
                    context.Schedule.Add(new Schedule
                    {
                        Enabled = true,
                        ExecuteOnceOnly = true,
                        JobTypeId = 40
                    });
                    await context.SaveChangesAsync();

                    ExternalDataScheduleService externalDataScheduleService = new ExternalDataScheduleService(
                        options,
                        scheduleService.Object,
                        dateTimeProvider.Object,
                        logger.Object);
                    IEnumerable<Jobs.Model.Enums.JobType> results = await externalDataScheduleService.GetJobs(true, CancellationToken.None);
                    results.Should().HaveCount(1);

                    var schedules = await context.Schedule.ToListAsync();
                    schedules.Should().BeEmpty();
                }
            }
        }

        [Fact]
        public async Task TestUpdate()
        {
            DateTime utcNow = DateTime.UtcNow;
            Mock<IDateTimeProvider> dateTimeProvider = new Mock<IDateTimeProvider>();
            Mock<IScheduleService> scheduleService = new Mock<IScheduleService>();
            Mock<ILogger> logger = new Mock<ILogger>();

            dateTimeProvider.Setup(x => x.GetNowUtc()).Returns(utcNow);
            scheduleService
                .Setup(x => x.CanExecuteSchedule(It.IsAny<Schedule>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
                .Returns(true);

            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<JobQueueDataContext>()
                    .UseSqlite(connection)
                    .Options;

                // Create the schema in the database
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobTypeGroup.Add(new JobTypeGroup
                    {
                        JobTypeGroupId = 3,
                        Description = "Reference Data",
                        ConcurrentExecutionCount = 25
                    });
                    context.JobType.Add(new JobType
                    {
                        JobTypeId = 40,
                        JobTypeGroupId = 3,
                        Description = "ReferenceData EPA",
                        Title = "ReferenceData EPA",
                        IsCrossLoadingEnabled = false
                    });
                    context.Schedule.Add(new Schedule
                    {
                        Enabled = true,
                        JobTypeId = 40
                    });
                    await context.SaveChangesAsync();
                }

                ExternalDataScheduleService externalDataScheduleService = new ExternalDataScheduleService(
                    options,
                    scheduleService.Object,
                    dateTimeProvider.Object,
                    logger.Object);
                IEnumerable<Jobs.Model.Enums.JobType> results = await externalDataScheduleService.GetJobs(true, CancellationToken.None);
                results.Should().HaveCount(1);

                using (var context = new JobQueueDataContext(options))
                {
                    var schedules = await context.Schedule.ToListAsync();
                    schedules.Should().HaveCount(1);
                    schedules[0].LastExecuteDateTime.Should().Be(utcNow.TrimSeconds());
                }
            }
        }
    }
}
