using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Job = ESFA.DC.Jobs.Model.Job;
using JobType = ESFA.DC.Jobs.Model.Enums.JobType;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class JobTopicServiceTests
    {
        [Fact]
        public void GetTopicTasks_OneTaskTopic_Success()
        {
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
                    context.JobTopic.Add(GetJobTopic(JobType.IlrSubmission, "Validation", "GenerateReport"));
                    context.SaveChanges();

                    var service = new JobTopicTaskService(options);
                    var result = service.GetTopicItems(JobType.IlrSubmission, false).ToList();
                    result.Should().NotBeNull();
                    result.Count().Should().Be(1);

                    var topicItem = result.First();
                    topicItem.SubscriptionName.Should().Be("Validation");
                    topicItem.Tasks.Count.Should().Be(1);
                    topicItem.Tasks.Any(x => x.Tasks.Contains("GenerateReport")).Should().BeTrue();
                }
            }
        }

        [Theory]
        [InlineData(JobType.IlrSubmission)]
        [InlineData(JobType.EsfSubmission)]
        [InlineData(JobType.EasSubmission)]
        public void GetTopicTasks_Topic_NotEnabled_Success(JobType jobType)
        {
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
                    context.JobTopic.Add(GetJobTopic(jobType, "Validation", "GenerateReport"));
                    context.JobTopic.Add(GetJobTopic(jobType, "Validation", "GenerateReport", false, false));
                    context.SaveChanges();

                    var service = new JobTopicTaskService(options);
                    var result = service.GetTopicItems(jobType, false).ToList();
                    result.Should().NotBeNull();
                    result.Count().Should().Be(1);

                    var topicItem = result.First();
                    topicItem.SubscriptionName.Should().Be("Validation");
                    topicItem.Tasks.Count.Should().Be(1);
                }
            }
        }

        [Theory]
        [InlineData(JobType.IlrSubmission)]
        [InlineData(JobType.EsfSubmission)]
        [InlineData(JobType.EasSubmission)]
        public void GetTopicTasks_Task_NotEnabled_Success(JobType jobType)
        {
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
                    context.JobTopic.Add(GetJobTopic(jobType, "Validation", "GenerateReport"));
                    context.JobTopic.Add(GetJobTopic(jobType, "Funding", "NotEnabledTask", false, true, false));
                    context.SaveChanges();

                    var service = new JobTopicTaskService(options);
                    var result = service.GetTopicItems(jobType, false).ToList();
                    result.Should().NotBeNull();
                    result.Count().Should().Be(2);
                    result.Single(x => x.SubscriptionName == "Validation").Tasks.Count.Should().Be(1);
                    result.Single(x => x.SubscriptionName == "Funding").Tasks.Any(x => x.Tasks.Contains("NotEnabledTask")).Should().BeFalse();
                }
            }
        }

        private JobTopic GetJobTopic(JobType jobType, string topicName, string taskName, bool isFirstStage = false, bool topicEnabled = true, bool taskEnabled = true)
        {
            return new JobTopic()
            {
                IsFirstStage = isFirstStage,
                Enabled = topicEnabled,
                JobTypeId = (short)jobType,
                TopicName = topicName,
                TopicOrder = 1,
                JobTopicTask = new List<JobTopicTask>()
                {
                    new JobTopicTask()
                    {
                        Enabled = taskEnabled,
                        JobTopicId = 1,
                        TaskName = taskName
                    }
                }
            };
        }
    }
}
