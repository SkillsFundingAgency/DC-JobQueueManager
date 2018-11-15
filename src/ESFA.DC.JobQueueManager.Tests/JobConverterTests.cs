using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class JobConverterTests
    {
        [Fact]
        public void JobToJobEntity_Test_Null()
        {
           Job job = null;
            var convertedJob = JobConverter.Convert(job);
            convertedJob.Should().BeNull();
        }

        [Fact]
        public void JobEntityToJob_Test_Null()
        {
            JobEntity job = null;
            var convertedJob = JobConverter.Convert(job);
            convertedJob.Should().BeNull();
        }

        [Fact]
        public void JobToJobEntity_Test()
        {
            var currentTime = DateTime.UtcNow;
            var job = new Job()
            {
                DateTimeSubmittedUtc = currentTime,
                DateTimeUpdatedUtc = currentTime,
                JobId = 1,
                Priority = 1,
                RowVersion = "test",
                Status = JobStatusType.Ready,
                SubmittedBy = "test",
                NotifyEmail = "test@test.com",
                JobType = JobType.IlrSubmission,
                CrossLoadingStatus = JobStatusType.Ready
            };

            var convertedJob = JobConverter.Convert(job);

            convertedJob.JobId.Should().Be(1);
            convertedJob.DateTimeSubmittedUtc.Should().Be(currentTime);
            convertedJob.DateTimeUpdatedUtc.Should().Be(currentTime);
            convertedJob.JobType.Should().Be(1);
            convertedJob.Priority.Should().Be(1);
            convertedJob.Status.Should().Be(1);
            convertedJob.NotifyEmail.Should().Be("test@test.com");
            convertedJob.SubmittedBy.Should().Be("test");
            convertedJob.CrossLoadingStatus.Should().Be((short)JobStatusType.Ready);
        }

        [Fact]
        public void JobEntityToJob_Test()
        {
            var currentTime = System.DateTime.UtcNow;
            var job = new JobEntity()
            {
                DateTimeSubmittedUtc = currentTime,
                DateTimeUpdatedUtc = currentTime,
                JobId = 1,
                JobType = 1,
                Priority = 1,
                RowVersion = null,
                Status = 1,
                NotifyEmail = "email@email.com",
                SubmittedBy = "test",
                CrossLoadingStatus = (short)JobStatusType.Ready
            };

            var convertedJob = JobConverter.Convert(job);

            convertedJob.JobId.Should().Be(1);
            convertedJob.DateTimeSubmittedUtc.Should().Be(currentTime);
            convertedJob.DateTimeUpdatedUtc.Should().Be(currentTime);
            convertedJob.JobType.Should().Be(1);
            convertedJob.Priority.Should().Be(1);
            convertedJob.Status.Should().Be(1);
            convertedJob.NotifyEmail.Should().Be("email@email.com");
            convertedJob.SubmittedBy.Should().Be("test");
            convertedJob.CrossLoadingStatus.Should().Be(JobStatusType.Ready);
        }

        [Fact]
        public void JobMetaDataToEntity_Test()
        {
            var job = new FileUploadJob
            {
                FileName = "test.xml",
                JobId = 1,
                StorageReference = "test-ref",
                Ukprn = 1000,
                CollectionName = "ILR1819",
                PeriodNumber = 10,
                FileSize = 1000,
                CollectionYear = 1819
            };

            var entity = new FileUploadJobMetaDataEntity();
            JobConverter.Convert(job, entity);

            entity.JobId.Should().Be(1);
            entity.FileName.Should().Be("test.xml");
            entity.StorageReference.Should().Be("test-ref");
            entity.Ukprn.Should().Be(1000);
            entity.CollectionName.Should().Be("ILR1819");
            entity.PeriodNumber.Should().Be(10);
            entity.FileSize.Should().Be(1000);
            entity.CollectionYear.Should().Be(1819);
        }

        [Fact]
        public void JobMetaDataEntityToJobMetaData_Test()
        {
            var entity = new FileUploadJobMetaDataEntity
            {
                FileName = "test.xml",
                JobId = 1,
                StorageReference = "test-ref",
                Ukprn = 1000,
                CollectionName = "ILR1819",
                PeriodNumber = 10,
                FileSize = 1000,
                Job = new JobEntity() { JobId = 1 },
                CollectionYear = 1819
            };

            var job = new FileUploadJob();
            JobConverter.Convert(entity, job);

            job.JobId.Should().Be(1);
            job.FileName.Should().Be("test.xml");
            job.StorageReference.Should().Be("test-ref");
            job.Ukprn.Should().Be(1000);
            job.CollectionName.Should().Be("ILR1819");
            job.PeriodNumber.Should().Be(10);
            job.FileSize.Should().Be(1000);
            job.CollectionYear.Should().Be(1819);
        }
    }
}
