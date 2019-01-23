using System.Collections.Generic;
using System.Linq;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
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
                    context.JobTopicSubscription.Add(GetJobTopic(1, JobType.IlrSubmission, "TopicA", "Validation", "GenerateReport"));
                    context.SaveChanges();

                    var service = new JobTopicTaskService(options);
                    var result = service.GetTopicItems(JobType.IlrSubmission, false).ToList();
                    result.Should().NotBeNull();
                    result.Count.Should().Be(1);

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
                    context.JobTopicSubscription.Add(GetJobTopic(1, jobType, "TopicA", "Validation", "GenerateReport"));
                    context.JobTopicSubscription.Add(GetJobTopic(2, jobType, "TopicA", "Validation", "GenerateReport", false, false));
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
                    context.JobTopicSubscription.Add(GetJobTopic(1, jobType, "TopicA", "Validation", "GenerateReport"));
                    context.JobTopicSubscription.Add(GetJobTopic(2, jobType, "TopicA", "Funding", "NotEnabledTask", false, true, false));
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

        private JobTopicSubscription GetJobTopic(int id, JobType jobType, string topicName, string subscriptionName, string taskName, bool isFirstStage = false, bool topicEnabled = true, bool taskEnabled = true)
        {
            return new JobTopicSubscription
            {
                JobTopicId = id,
                IsFirstStage = isFirstStage,
                Enabled = topicEnabled,
                JobTypeId = (short)jobType,
                TopicName = topicName,
                SubscriptionName = subscriptionName,
                TopicOrder = 1,
                JobSubscriptionTask = new List<JobSubscriptionTask>()
                {
                    new JobSubscriptionTask
                    {
                        JobTopicTaskId = id,
                        Enabled = taskEnabled,
                        JobTopicId = 1,
                        TaskName = taskName
                    }
                }
            };
        }
    }
}
