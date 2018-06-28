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
            IlrJob job = null;
            var convertedJob = IlrJobConverter.Convert(job);
            convertedJob.Should().BeNull();
        }

        [Fact]
        public void JobEntityToJob_Test_Null()
        {
            JobEntity job = null;
            var convertedJob = IlrJobConverter.Convert(job);
            convertedJob.Should().BeNull();
        }

        [Fact]
        public void JobToJobEntity_Test()
        {
            var currentTime = System.DateTime.UtcNow;
            var job = new IlrJob()
            {
                DateTimeSubmittedUtc = currentTime,
                DateTimeUpdatedUtc = currentTime,
                FileName = "test.xml",
                JobId = 1,
                Priority = 1,
                RowVersion = null,
                Status = JobStatusType.Ready,
                StorageReference = "test-ref",
                Ukprn = 1000,
            };

            var convertedJob = IlrJobConverter.Convert(job);

            convertedJob.JobId.Should().Be(1);
            convertedJob.DateTimeSubmittedUtc.Should().Be(currentTime);
            convertedJob.DateTimeUpdatedUtc.Should().Be(currentTime);
            //convertedJob.FileName.Should().Be("test.xml");
            convertedJob.JobType.Should().Be(1);
            convertedJob.Priority.Should().Be(1);
            //convertedJob.StorageReference.Should().Be("test-ref");
            convertedJob.Status.Should().Be(1);
            convertedJob.Ukprn.Should().Be(1000);
        }

        [Fact]
        public void JobEntityToJob_Test()
        {
            var currentTime = System.DateTime.UtcNow;
            var job = new JobEntity()
            {
                DateTimeSubmittedUtc = currentTime,
                DateTimeUpdatedUtc = currentTime,
                //FileName "test.xml",
                JobId = 1,
                JobType = 1,
                Priority = 1,
                RowVersion = null,
                Status = 1,
                //StorageReference = "test-ref",
                Ukprn = 1000,
            };

            var convertedJob = IlrJobConverter.Convert(job);

            convertedJob.JobId.Should().Be(1);
            convertedJob.DateTimeSubmittedUtc.Should().Be(currentTime);
            convertedJob.DateTimeUpdatedUtc.Should().Be(currentTime);
            //convertedJob.FileName.Should().Be("test.xml");
            convertedJob.JobType.Should().Be(1);
            convertedJob.Priority.Should().Be(1);
            //convertedJob.StorageReference.Should().Be("test-ref");
            convertedJob.Status.Should().Be(1);
            convertedJob.Ukprn.Should().Be(1000);
        }
    }
}
