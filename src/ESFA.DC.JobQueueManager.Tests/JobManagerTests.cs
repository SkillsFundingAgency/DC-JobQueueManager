﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class JobManagerTests
    {
        [Fact]
        public void AddJob_Null()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentNullException>(() => manager.AddJob(null));
        }

        [Fact]
        public void AddJob_Success()
        {
            var manager = GetJobManager();
            var result = manager.AddJob(new Job());
            result.Should().Be(1);
        }

        [Theory]
        [InlineData(JobType.IlrSubmission)]
        [InlineData(JobType.EsfSubmission)]
        [InlineData(JobType.EasSubmission)]
        [InlineData(JobType.ReferenceData)]
        public void AddJob_Success_Values(JobType jobType)
        {
            var job = new Job
            {
                DateTimeSubmittedUtc = System.DateTime.UtcNow,
                DateTimeUpdatedUtc = System.DateTime.UtcNow,
                JobId = 0,
                Priority = 1,
                RowVersion = null,
                Status = JobStatusType.Ready,
                SubmittedBy = "test user",
                JobType = jobType,
                NotifyEmail = "test@email.com",
                IsCrossLoaded = true
            };

            var manager = GetJobManager();
            manager.AddJob(job);

            var result = manager.GetAllJobs();

            var savedJob = result.SingleOrDefault();
            savedJob.Should().NotBeNull();

            savedJob.JobId.Should().Be(1);
            savedJob.DateTimeSubmittedUtc.Should().BeOnOrBefore(System.DateTime.UtcNow);
            savedJob.DateTimeUpdatedUtc.Should().BeNull();
            savedJob.JobType.Should().Be(jobType);
            savedJob.Priority.Should().Be(1);
            savedJob.SubmittedBy.Should().Be("test user");
            savedJob.Status.Should().Be(JobStatusType.Ready);
            savedJob.NotifyEmail.Should().Be("test@email.com");
            savedJob.IsCrossLoaded.Should().Be(true);
        }

        [Fact]
        public void GetJobById_Success()
        {
            var manager = GetJobManager();
            var jobId = manager.AddJob(new Job());
            var result = manager.GetJobById(1);

            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
        }

        [Fact]
        public void GetJobById_Fail_zeroId()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.GetJobById(0));
        }

        [Fact]
        public void GetJobById_Fail_IdNotFound()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.GetJobById(100));
        }

        [Fact]
        public void GetAllJobs_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new Job());
            manager.AddJob(new Job());
            manager.AddJob(new Job());

            var result = manager.GetAllJobs();
            result.Count().Should().Be(3);
        }

        public void GetJobByPriority_Ilr_NoJobs()
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
                }

                var manager = new JobManager(options, new Mock<IDateTimeProvider>().Object, new Mock<IEmailNotifier>().Object, new Mock<IFileUploadJobManager>().Object, new Mock<IEmailTemplateManager>().Object);
                var result = manager.GetJobByPriority();
                result.Should().BeNull();
            }
        }

        public void GetJobByPriority_Ilr_submission()
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
                }

                var manager = new JobManager(options, new Mock<IDateTimeProvider>().Object, new Mock<IEmailNotifier>().Object, new Mock<IFileUploadJobManager>().Object, new Mock<IEmailTemplateManager>().Object);
                manager.AddJob(new Job()
                {
                    Priority = 1,
                    Status = JobStatusType.Ready,
                });
                manager.AddJob(new Job()
                {
                    Priority = 2,
                    Status = JobStatusType.Ready,
                });
                var result = manager.GetJobByPriority();
                result.JobId.Should().Be(2);
                result.JobType.Should().Be(JobType.IlrSubmission);
            }
        }

        [Fact]
        public void RemoveJobFromQueue_Fail_ZeroId()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.RemoveJobFromQueue(0));
        }

        [Fact]
        public void RemoveJobFromQueue_Fail_IdDontExist()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.RemoveJobFromQueue(200));
        }

        [Theory]
        [InlineData(JobStatusType.MovedForProcessing)]
        [InlineData(JobStatusType.Processing)]
        [InlineData(JobStatusType.Completed)]
        [InlineData(JobStatusType.Failed)]
        [InlineData(JobStatusType.FailedRetry)]
        [InlineData(JobStatusType.Paused)]
        public void RemoveJobFromQueue_Fail_InvalidJobStatus(JobStatusType status)
        {
            var manager = GetJobManager();
            manager.AddJob(new Job
            {
                Status = status,
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => manager.RemoveJobFromQueue(1));
        }

        [Fact]
        public void RemoveJobFromQueue_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new Job()
            {
                Status = JobStatusType.Ready,
            });
            var jobs = manager.GetAllJobs();
            jobs.Count().Should().Be(1);

            manager.RemoveJobFromQueue(1);

            var jobsAfterRemoval = manager.GetAllJobs();
            jobsAfterRemoval.Count().Should().Be(0);
        }

        [Fact]
        public void UpdateJob_Fail_Null()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentNullException>(() => manager.UpdateJob(null));
        }

        [Fact]
        public void UpdateJob_Fail_InvalidJobId()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.UpdateJob(
                new Job() { JobId = 1000 }));
        }

        [Fact]
        public void UpdateJob_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new Job()
            {
                Status = JobStatusType.Ready,
                JobType = JobType.IlrSubmission
            });
            var job = manager.GetJobById(1);
            job.Status = JobStatusType.Completed;
            job.Priority = 2;
            job.RowVersion = "AAAAAAAAGJw=";
            job.NotifyEmail = "test@test.com";
            job.SubmittedBy = "test";
            job.IsCrossLoaded = true;

            manager.UpdateJob(job);

            var updatedJob = manager.GetJobById(1);
            updatedJob.JobType.Should().Be(JobType.IlrSubmission);
            updatedJob.DateTimeUpdatedUtc.Should().BeOnOrBefore(System.DateTime.UtcNow);
            updatedJob.Priority.Should().Be(2);
            updatedJob.Status.Should().Be(JobStatusType.Completed);
            updatedJob.SubmittedBy.Should().Be("test");
            updatedJob.NotifyEmail.Should().Be("test@test.com");
            updatedJob.IsCrossLoaded.Should().Be(true);
        }

        [Fact]
        public void UpdateJobStatus_Fail_ZeroId()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStatus(0, JobStatusType.Completed));
        }

        [Fact]
        public void UpdateJobStatus_Fail_InvalidJobId()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStatus(110, JobStatusType.Completed));
        }

        [Fact]
        public void UpdateJobStatus_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new Job()
            {
                Status = JobStatusType.Ready,
            });
            manager.UpdateJobStatus(1, JobStatusType.Completed);

            var updatedJob = manager.GetJobById(1);
            updatedJob.Status.Should().Be(JobStatusType.Completed);
        }

        [Fact]
        public void UpdateJobStatus_Success_EmailSent()
        {
            var emailNotifier = new Mock<IEmailNotifier>();
            emailNotifier.Setup(x => x.SendEmail(It.IsAny<string>(), "test", It.IsAny<Dictionary<string, dynamic>>()));

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
                }

                var emailTemplateManager = new Mock<IEmailTemplateManager>();
                emailTemplateManager
                    .Setup(x => x.GetTemplate(It.IsAny<long>(), It.IsAny<JobStatusType>(), It.IsAny<JobType>()))
                    .Returns("template");

                var manager = new JobManager(options, new Mock<IDateTimeProvider>().Object, emailNotifier.Object, new Mock<IFileUploadJobManager>().Object, emailTemplateManager.Object);
                manager.AddJob(new Job()
                {
                    Status = JobStatusType.Ready,
                    JobType = JobType.IlrSubmission
                });

                manager.UpdateJobStatus(1, JobStatusType.Completed);

                var updatedJob = manager.GetJobById(1);
                updatedJob.Status.Should().Be(JobStatusType.Completed);
                emailNotifier.Verify(x => x.SendEmail(It.IsAny<string>(), "template", It.IsAny<Dictionary<string, dynamic>>()), () => Times.Once());
            }
        }

        [Fact]
        public void IsCrossLoadingEnabled_Success()
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
                    context.JobTypes.Add(new JobTypeEntity()
                    {
                        IsCrossLoadingEnabled = true,
                        JobTypeId = 1
                    });
                    context.SaveChanges();
                }

                var manager = new JobManager(
                    options,
                    new Mock<IDateTimeProvider>().Object,
                    new Mock<IEmailNotifier>().Object,
                    new Mock<IFileUploadJobManager>().Object,
                    new Mock<IEmailTemplateManager>().Object);

                manager.IsCrossLoadingEnabled(JobType.IlrSubmission).Should().BeTrue();
            }
        }

        private DbContextOptions GetContextOptions([CallerMemberName]string functionName = "")
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<JobQueueDataContext>()
                .UseInMemoryDatabase(functionName)
                .UseInternalServiceProvider(serviceProvider)
                .Options;
            return options;
        }

        private JobManager GetJobManager(IDateTimeProvider dateTimeProvider = null, IEmailNotifier emailNotifier = null, IEmailTemplateManager emailTemplateManager = null)
        {
            return new JobManager(
                GetContextOptions(),
                dateTimeProvider ?? new Mock<IDateTimeProvider>().Object,
                emailNotifier ?? new Mock<IEmailNotifier>().Object,
                new Mock<IFileUploadJobManager>().Object,
                emailTemplateManager ?? new Mock<IEmailTemplateManager>().Object);
        }
    }
}
