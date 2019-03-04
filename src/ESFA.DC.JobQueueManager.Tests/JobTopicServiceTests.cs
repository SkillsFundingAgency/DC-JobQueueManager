using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
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
        public async Task GetTopicTasks_OneTaskTopic_Success()
        {
            IContainer container = Registrations();
            List<ITopicItem> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobTopicSubscription.Add(GetJobTopic(1, JobType.IlrSubmission, "TopicA", "Validation", "GenerateReport"));
                    context.SaveChanges();
                }

                var jobTopicTaskService = scope.Resolve<IJobTopicTaskService>();
                result = (await jobTopicTaskService.GetTopicItems(JobType.IlrSubmission, false)).ToList();
            }

            result.Should().NotBeNull();
            result.Count.Should().Be(1);

            var topicItem = result.First();
            topicItem.SubscriptionSqlFilterValue.Should().Be("TopicA");
            topicItem.SubscriptionName.Should().Be("Validation");
            topicItem.Tasks.Count.Should().Be(1);
            topicItem.Tasks.Any(x => x.Tasks.Contains("GenerateReport")).Should().BeTrue();
        }

        [Theory]
        [InlineData(JobType.IlrSubmission)]
        [InlineData(JobType.EsfSubmission)]
        [InlineData(JobType.EasSubmission)]
        public async Task GetTopicTasks_Topic_NotEnabled_Success(JobType jobType)
        {
            IContainer container = Registrations();
            List<ITopicItem> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobTopicSubscription.Add(GetJobTopic(1, jobType, "TopicA", "Validation", "GenerateReport"));
                    context.JobTopicSubscription.Add(GetJobTopic(2, jobType, "TopicA", "Validation", "GenerateReport", false, false));
                    context.SaveChanges();

                    var jobTopicTaskService = scope.Resolve<IJobTopicTaskService>();
                    result = (await jobTopicTaskService.GetTopicItems(jobType, false)).ToList();
                }

                result.Should().NotBeNull();
                result.Count.Should().Be(1);

                var topicItem = result.First();
                topicItem.SubscriptionSqlFilterValue.Should().Be("TopicA");
                topicItem.SubscriptionName.Should().Be("Validation");
                topicItem.Tasks.Count.Should().Be(1);
            }
        }

        [Theory]
        [InlineData(JobType.IlrSubmission)]
        [InlineData(JobType.EsfSubmission)]
        [InlineData(JobType.EasSubmission)]
        public async Task GetTopicTasks_Task_NotEnabled_Success(JobType jobType)
        {
            IContainer container = Registrations();
            List<ITopicItem> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobTopicSubscription.Add(GetJobTopic(1, jobType, "TopicA", "Validation", "GenerateReport"));
                    context.JobTopicSubscription.Add(GetJobTopic(2, jobType, "TopicA", "Funding", "NotEnabledTask", false, true, false));
                    context.SaveChanges();

                    var jobTopicTaskService = scope.Resolve<IJobTopicTaskService>();
                    result = (await jobTopicTaskService.GetTopicItems(jobType, false)).ToList();
                }

                result.Should().NotBeNull();
                result.Count.Should().Be(2);
                result.Single(x => x.SubscriptionName == "Validation").Tasks.Count.Should().Be(1);
                result.Single(x => x.SubscriptionName == "Funding").Tasks.Any(x => x.Tasks.Contains("NotEnabledTask")).Should().BeFalse();
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

        private IContainer Registrations()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterType<JobTopicTaskService>().As<IJobTopicTaskService>().InstancePerLifetimeScope();
            builder.RegisterType<JobQueueDataContext>().As<IJobQueueDataContext>().InstancePerDependency();
            builder.Register(context =>
                {
                    SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    DbContextOptionsBuilder<JobQueueDataContext> optionsBuilder = new DbContextOptionsBuilder<JobQueueDataContext>()
                        .UseSqlite(connection);
                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<JobQueueDataContext>>()
                .SingleInstance();

            IContainer container = builder.Build();
            return container;
        }
    }
}
