using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ESFA.DC.DateTime.Provider.Interface;
using ESFA.DC.JobQueueManager.Data;
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
    public class IlrJobQueueManagerTests
    {
        [Fact]
        public void AddJob_Null()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(It.IsAny<DbContextOptions>(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentNullException>(() => manager.AddJob(null));
        }

        [Fact]
        public void AddJob_Success()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            var result = manager.AddJob(new IlrJob());
            result.Should().Be(1);
        }

        [Fact]
        public void AddJob_Success_Values()
        {
            var job = new IlrJob()
            {
                DateTimeSubmittedUtc = System.DateTime.UtcNow,
                DateTimeUpdatedUtc = System.DateTime.UtcNow,
                JobId = 0,
                Priority = 1,
                RowVersion = null,
                Status = JobStatusType.Ready,
                Ukprn = 1000,
                SubmittedBy = "test user",
                FileName = "test.xml",
                StorageReference = "test-ref",
                FileSize = 10.5m,
                IsFirstStage = true,
            };

            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            manager.AddJob(job);

            var result = manager.GetAllJobs();

            var savedJob = result.SingleOrDefault();
            savedJob.Should().NotBeNull();

            savedJob.JobId.Should().Be(1);
            savedJob.DateTimeSubmittedUtc.Should().BeOnOrBefore(System.DateTime.UtcNow);
            savedJob.DateTimeUpdatedUtc.Should().BeNull();
            savedJob.JobType.Should().Be(JobType.IlrSubmission);
            savedJob.Priority.Should().Be(1);
            savedJob.SubmittedBy.Should().Be("test user");
            savedJob.Status.Should().Be(JobStatusType.Ready);
            savedJob.Ukprn.Should().Be(1000);
            savedJob.FileName.Should().Be("test.xml");
            savedJob.FileSize.Should().Be(10.5m);
            savedJob.StorageReference.Should().Be("test-ref");
            savedJob.IsFirstStage.Should().Be(true);
            savedJob.TotalLearners.Should().Be(0);
        }

        [Fact]
        public void GetJobById_Success()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            var jobId = manager.AddJob(new IlrJob());
            var result = manager.GetJobById(1);

            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
        }

        [Fact]
        public void GetJobById_Fail_zeroId()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentException>(() => manager.GetJobById(0));
        }

        [Fact]
        public void GetJobById_Fail_IdNotFound()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentException>(() => manager.GetJobById(100));
        }

        [Fact]
        public void GetJobsByUkprn_Success()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            var jobId = manager.AddJob(new IlrJob() { Ukprn = 1 });
            var result = manager.GetJobsByUkprn(1);

            result.Should().NotBeNull();
            result.Count().Should().Be(1);
        }

        [Fact]
        public void GetJobsByUkprn_Fail_zeroId()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentException>(() => manager.GetJobsByUkprn(0));
        }

        [Fact]
        public void GetJobsByUkprn_Success_UkprnNotFound()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            var jobId = manager.AddJob(new IlrJob() { Ukprn = 1 });
            var result = manager.GetJobsByUkprn(999);

            result.Should().NotBeNull();
            result.Count().Should().Be(0);
        }

        [Fact]
        public void GetAllJobs_Success()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            manager.AddJob(new IlrJob());
            manager.AddJob(new IlrJob());
            manager.AddJob(new IlrJob());

            var result = manager.GetAllJobs();
            result.Count().Should().Be(3);
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

                var manager = new JobQueueManager.IlrJobQueueManager(options, new Mock<IDateTimeProvider>().Object);
                manager.AddJob(new IlrJob()
                {
                    Priority = 1,
                    Status = JobStatusType.Ready,
                    FileName = "file1",
                    Ukprn = 1000,
                });
                manager.AddJob(new IlrJob()
                {
                    Priority = 2,
                    Status = JobStatusType.Ready,
                    FileName = "file2",
                    Ukprn = 1002,
                });
                var result = manager.GetJobByPriority();
                result.JobId.Should().Be(2);
                result.FileName.Should().Be("file2");
                result.Ukprn.Should().Be(1002);
                result.JobType.Should().Be(JobType.IlrSubmission);
            }
        }

        [Fact]
        public void RemoveJobFromQueue_Fail_ZeroId()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentException>(() => manager.RemoveJobFromQueue(0));
        }

        [Fact]
        public void RemoveJobFromQueue_Fail_IdDontExist()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
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
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            manager.AddJob(new IlrJob
            {
                Status = status,
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => manager.RemoveJobFromQueue(1));
        }

        [Fact]
        public void RemoveJobFromQueue_Success()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            manager.AddJob(new IlrJob
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
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentNullException>(() => manager.UpdateJob(null));
        }

        [Fact]
        public void UpdateJob_Fail_InvalidJobId()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentException>(() => manager.UpdateJob(
                new IlrJob() { JobId = 1000 }));
        }

        [Fact]
        public void UpdateJob_Success()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            manager.AddJob(new IlrJob
            {
                Status = JobStatusType.Ready,
                FileName = "test",
                StorageReference = "st-ref"
            });
            var job = manager.GetJobById(1);
            job.Status = JobStatusType.Completed;
            job.Priority = 2;
            job.Ukprn = 100;
            job.RowVersion = "AAAAAAAAGJw=";

            manager.UpdateJob(job);

            var updatedJob = manager.GetJobById(1);
            updatedJob.JobType.Should().Be(JobType.IlrSubmission);
            updatedJob.DateTimeUpdatedUtc.Should().BeOnOrBefore(System.DateTime.UtcNow);
            updatedJob.Ukprn.Should().Be(100);
            updatedJob.FileName.Should().Be("test");
            updatedJob.StorageReference.Should().Be("st-ref");
            updatedJob.Priority.Should().Be(2);
            updatedJob.Status.Should().Be(JobStatusType.Completed);
        }

        [Fact]
        public void UpdateJobStatus_Fail_ZeroId()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStatus(0, JobStatusType.Completed));
        }

        [Fact]
        public void UpdateJobStatus_Fail_InvalidJobId()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStatus(110, JobStatusType.Completed));
        }

        [Fact]
        public void UpdateJobStatus_Success()
        {
            var manager = new JobQueueManager.IlrJobQueueManager(GetContextOptions(), new Mock<IDateTimeProvider>().Object);
            manager.AddJob(new IlrJob
            {
                Status = JobStatusType.Ready,
            });
            var job = manager.GetJobById(1);
            manager.UpdateJobStatus(1, JobStatusType.Completed);

            var updatedJob = manager.GetJobById(1);
            updatedJob.Status.Should().Be(JobStatusType.Completed);
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
    }
}
