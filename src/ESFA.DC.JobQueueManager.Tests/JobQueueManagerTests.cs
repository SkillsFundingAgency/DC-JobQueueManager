using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Models;
using ESFA.DC.JobQueueManager.Models.Enums;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class JobQueueManagerTests
    {
        [Fact]
        public void AddJob_Null()
        {
            var manager = new JobQueueManager(It.IsAny<DbContextOptions>());
            Assert.Throws<ArgumentNullException>(() => manager.AddJob(null));
        }

        [Fact]
        public void AddJob_Success()
        {
            var manager = new JobQueueManager(GetContextOptions());
            var result = manager.AddJob(new Job());
            result.Should().Be(1);
        }

        [Fact]
        public void AddJob_Success_Values()
        {
            var job = new Job()
            {
                DateTimeSubmittedUtc = DateTime.UtcNow,
                DateTimeUpdatedUtc = DateTime.UtcNow,
                FileName = "test.xml",
                JobId = 0,
                JobType = JobType.IlrSubmission,
                Priority = 1,
                RowVersion = null,
                Status = JobStatus.Ready,
                StorageReference = "test-ref",
                Ukprn = 1000,
            };

            var manager = new JobQueueManager(GetContextOptions());
            manager.AddJob(job);

            var result = manager.GetAllJobs();

            var savedJob = result.SingleOrDefault();
            savedJob.Should().NotBeNull();

            savedJob.JobId.Should().Be(1);
            savedJob.DateTimeSubmittedUtc.Should().BeOnOrBefore(DateTime.UtcNow);
            savedJob.DateTimeUpdatedUtc.Should().BeNull();
            savedJob.FileName.Should().Be("test.xml");
            savedJob.JobType.Should().Be(JobType.IlrSubmission);
            savedJob.Priority.Should().Be(1);
            savedJob.StorageReference.Should().Be("test-ref");
            savedJob.Status.Should().Be(JobStatus.Ready);
            savedJob.Ukprn.Should().Be(1000);
        }

        [Fact]
        public void GetJobById_Success()
        {
            var manager = new JobQueueManager(GetContextOptions());
            var jobId = manager.AddJob(new Job());
            var result = manager.GetJobById(1);

            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
        }

        [Fact]
        public void GetJobById_Fail_zeroId()
        {
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentException>(() => manager.GetJobById(0));
        }

        [Fact]
        public void GetJobById_Fail_IdNotFound()
        {
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentException>(() => manager.GetJobById(100));
        }

        [Fact]
        public void GetAllJobs_Success()
        {
            var manager = new JobQueueManager(GetContextOptions());
            manager.AddJob(new Job());
            manager.AddJob(new Job());
            manager.AddJob(new Job());

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

                var manager = new JobQueueManager(options);
                manager.AddJob(new Job()
                {
                    JobType = JobType.IlrSubmission,
                    Priority = 1,
                    Status = JobStatus.Ready,
                    FileName = "file1",
                    Ukprn = 1000,
                });
                manager.AddJob(new Job()
                {
                    JobType = JobType.IlrSubmission,
                    Priority = 2,
                    Status = JobStatus.Ready,
                    FileName = "file2",
                    Ukprn = 1002,
                });
                var result = manager.GetJobByPriority();
                result.JobId.Should().Be(2);
                result.FileName.Should().Be("file2");
                result.Ukprn.Should().Be(1002);
            }
        }

        [Fact]
        public void RemoveJobFromQueue_Fail_ZeroId()
        {
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentException>(() => manager.RemoveJobFromQueue(0));
        }

        [Fact]
        public void RemoveJobFromQueue_Fail_IdDontExist()
        {
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentException>(() => manager.RemoveJobFromQueue(200));
        }

        [Theory]
        [InlineData(JobStatus.MovedForProcessing)]
        [InlineData(JobStatus.Processing)]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.Failed)]
        [InlineData(JobStatus.FailedRetry)]
        [InlineData(JobStatus.Paused)]
        public void RemoveJobFromQueue_Fail_InvalidJobStatus(JobStatus status)
        {
            var manager = new JobQueueManager(GetContextOptions());
            manager.AddJob(new Job
            {
                JobType = JobType.IlrSubmission,
                Status = status,
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => manager.RemoveJobFromQueue(1));
        }

        [Fact]
        public void RemoveJobFromQueue_Success()
        {
            var manager = new JobQueueManager(GetContextOptions());
            manager.AddJob(new Job
            {
                JobType = JobType.IlrSubmission,
                Status = JobStatus.Ready,
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
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentNullException>(() => manager.UpdateJob(null));
        }

        [Fact]
        public void UpdateJob_Fail_InvalidJobId()
        {
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentException>(() => manager.UpdateJob(
                new Job() { JobId = 1000 }));
        }

        [Fact]
        public void UpdateJob_Success()
        {
            var manager = new JobQueueManager(GetContextOptions());
            manager.AddJob(new Job
            {
                JobType = JobType.IlrSubmission,
                Status = JobStatus.Ready,
            });
            var job = manager.GetJobById(1);
            job.FileName = "test";
            job.Status = JobStatus.Completed;
            job.Priority = 2;
            job.StorageReference = "st-ref";
            job.Ukprn = 100;

            manager.UpdateJob(job);

            var updatedJob = manager.GetJobById(1);
            updatedJob.JobType.Should().Be(JobType.IlrSubmission);
            updatedJob.DateTimeUpdatedUtc.Should().BeOnOrBefore(DateTime.UtcNow);
            updatedJob.Ukprn.Should().Be(100);
            updatedJob.FileName.Should().Be("test");
            updatedJob.StorageReference.Should().Be("st-ref");
            updatedJob.Priority.Should().Be(2);
            updatedJob.Status.Should().Be(JobStatus.Completed);
        }

        [Fact]
        public void UpdateJobStatus_Fail_ZeroId()
        {
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStatus(0, JobStatus.Completed));
        }

        [Fact]
        public void UpdateJobStatus_Fail_InvalidJobId()
        {
            var manager = new JobQueueManager(GetContextOptions());
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStatus(110, JobStatus.Completed));
        }

        [Fact]
        public void UpdateJobStatus_Success()
        {
            var manager = new JobQueueManager(GetContextOptions());
            manager.AddJob(new Job
            {
                JobType = JobType.IlrSubmission,
                Status = JobStatus.Ready,
            });
            var job = manager.GetJobById(1);
            manager.UpdateJobStatus(1, JobStatus.Completed);

            var updatedJob = manager.GetJobById(1);
            updatedJob.Status.Should().Be(JobStatus.Completed);
        }

        //[Fact]
        //public void SaveChanges_Success()
        //{
        //    var contextOptions = GetContextOptions();
        //    var manager = new JobQueueManager(contextOptions);

        //    manager.AddJob(new Job
        //    {
        //        JobType = JobType.IlrSubmission,
        //        Status = JobStatus.Ready,
        //        FileName = "test1.xml",
        //    });

        //    using (var context = new JobQueueDataContext(contextOptions))
        //    {
        //        //reload the entity
        //        var job = context.Jobs.First();
        //        job.FileName = "test2.xml";

        //        manager.SaveChanges(job, context);
        //    }

        //    var updatedJob = manager.GetJobById(1);
        //    updatedJob.FileName.Should().Be("test2.xml");
        //}

        [Fact]
        public void UpdateJob_Fail_Concurrency()
        {
            var contextOptions = GetContextOptions();
            var manager = new JobQueueManager(contextOptions);

            manager.AddJob(new Job
            {
                JobType = JobType.IlrSubmission,
                Status = JobStatus.Ready,
                FileName = "test1.xml",
            });

            //var job = manager.GetJobById(1);
            //job.FileName = "test2.xml";
            //job.RowVersion = "test";
            //manager.UpdateJob(job);

            var job = manager.GetJobById(1);

            var context = new JobQueueDataContext(contextOptions);
            var j = context.Jobs.First();
            j.RowVersion = Convert.FromBase64String("testZXXX");
            context.Entry(j).State = EntityState.Modified;
            context.SaveChanges();

            //Row version is updated now, so lets try to save with old row version
            job.FileName = "test3.xml";
            job.RowVersion = "TESTABCD";
            Assert.Throws<DbUpdateConcurrencyException>(() => manager.UpdateJob(job));
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
