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
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class FileUploadJobManagerTests
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
            var result = manager.AddJob(new FileUploadJob()
            {
                JobId = 1,
            });
            result.Should().Be(1);
        }

        [Fact]
        public void AddJobMetaData_Success_Values()
        {
            var job = new FileUploadJob()
            {
                JobId = 1,
                Ukprn = 1000,
                FileName = "test.xml",
                StorageReference = "test-ref",
                FileSize = 10.5m,
                IsFirstStage = true,
                CollectionName = "ILR1718",
                PeriodNumber = 10,
                TermsAccepted = true,
                CollectionYear = 1819
            };

            var manager = GetJobManager();
            manager.AddJob(job);

            var savedJob = manager.GetJobById(1);

            savedJob.Should().NotBeNull();
            savedJob.JobId.Should().Be(1);
            savedJob.Ukprn.Should().Be(1000);
            savedJob.FileName.Should().Be("test.xml");
            savedJob.FileSize.Should().Be(10.5m);
            savedJob.StorageReference.Should().Be("test-ref");
            savedJob.IsFirstStage.Should().Be(true);
            savedJob.CollectionName.Should().Be("ILR1718");
            savedJob.PeriodNumber.Should().Be(10);
            savedJob.TermsAccepted.Should().Be(true);
            savedJob.CollectionYear.Should().Be(1819);
        }

        [Fact]
        public void GetJobMetaDataById_Success()
        {
            var manager = GetJobManager();
            var jobId = manager.AddJob(new FileUploadJob()
            {
                JobId = 1,
            });
            var result = manager.GetJobById(1);

            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
        }

        [Fact]
        public void GetJobMetDataById_Fail_zeroId()
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
        public void UpdateJobMetaData_Fail_Zero()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStage(0, true));
        }

        [Fact]
        public void UpdateJob_Fail_InvalidJobId()
        {
            var manager = GetJobManager();
            Assert.Throws<ArgumentException>(() => manager.UpdateJobStage(100, false));
        }

        [Fact]
        public void GetJobsByUkprn_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new FileUploadJob()
            {
                JobId = 1,
                Ukprn = 100,
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 100,
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 999900,
            });
            var result = manager.GetJobsByUkprn(100).ToList();

            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }

        [Fact]
        public void GetJobsByUkprnForDateRange_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new FileUploadJob()
            {
                JobId = 1,
                Ukprn = 100
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 100
            });

            var result = manager.GetJobsByUkprnForDateRange(100, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow).ToList();

            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }

        [Fact]
        public void GetJobsByUkprnForPeriod_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new FileUploadJob()
            {
                JobId = 1,
                Ukprn = 100,
                PeriodNumber = 1
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 999900,
                PeriodNumber = 2
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 999900,
                PeriodNumber = 2
            });
            var result = manager.GetJobsByUkprnForPeriod(999900, 2).ToList();

            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }

        [Fact]
        public void GetLatestJobByUkprnAndContractReference_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new FileUploadJob()
            {
                JobId = 1,
                Ukprn = 10000116,
                PeriodNumber = 1,
                FileName = "10000116/SUPPDATA-10000116-ESF-2270-20181109-090919.csv",
                CollectionName = "ESF"
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 10000116,
                PeriodNumber = 2,
                FileName = "10000116/SUPPDATA-10000116-ESF-99999-20181109-090919.csv",
                CollectionName = "ESF"
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 3,
                Ukprn = 10000119,
                PeriodNumber = 2,
                FileName = "10000119/SUPPDATA-10000119-ESF-2270-20181109-090919.csv",
                CollectionName = "ESF"
            });
            var result = manager.GetLatestJobByUkprnAndContractReference(10000116, "ESF-2270", "ESF");

            result.Should().NotBeNull();
            result.FileName.Should().Be("10000116/SUPPDATA-10000116-ESF-2270-20181109-090919.csv");
            result.Ukprn.Should().Be(10000116);
            result.JobId.Should().Be(1);
        }

        [Fact]
        public void GetAllJobs_Success()
        {
            var manager = GetJobManager();
            manager.AddJob(new FileUploadJob()
            {
                JobId = 1,
                Ukprn = 100,
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 100,
            });
            manager.AddJob(new FileUploadJob()
            {
                JobId = 2,
                Ukprn = 999900,
            });
            var result = manager.GetAllJobs().ToList();

            result.Should().NotBeNull();
            result.Count().Should().Be(3);
        }

        private DbContextOptions<JobQueueDataContext> GetContextOptions([CallerMemberName]string functionName = "")
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

        private FileUploadJobManager GetJobManager()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(DateTime.UtcNow);
            return new FileUploadJobManager(
                GetContextOptions(),
                dateTimeProviderMock.Object,
                new Mock<IReturnCalendarService>().Object,
                new Mock<IEmailTemplateManager>().Object,
                new Mock<IEmailNotifier>().Object,
                new Mock<ILogger>().Object);
        }
    }
}
