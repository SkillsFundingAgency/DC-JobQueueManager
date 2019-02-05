using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using JobStatusType = ESFA.DC.JobStatus.Interface.JobStatusType;
using JobType = ESFA.DC.Jobs.Model.Enums.JobType;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class FileUploadJobManagerTests
    {
        [Fact]
        public async Task AddJob_Null()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AddJob(null));
            }
        }

        [Fact]
        public async Task AddJob_Success()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                var result = await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                });
                result.Should().Be(1);
            }
        }

        [Fact]
        public async Task AddJobMetaData_Success_Values()
        {
            IContainer container = Registrations();
            FileUploadJob savedJob;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                FileUploadJob job = new FileUploadJob()
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

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                await manager.AddJob(job);

                savedJob = await manager.GetJobById(1);
            }

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
        public async Task GetJobMetaDataById_Success()
        {
            IContainer container = Registrations();
            FileUploadJob result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                var jobId = await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                });

                result = await manager.GetJobById(1);
            }

            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
        }

        [Fact]
        public async Task GetJobMetDataById_Fail_zeroId()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.GetJobById(0));
            }
        }

        [Fact]
        public async Task GetJobById_Fail_IdNotFound()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.GetJobById(100));
            }
        }

        [Fact]
        public async Task UpdateJobMetaData_Fail_Zero()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateJobStage(0, true));
            }
        }

        [Fact]
        public async Task UpdateJob_Fail_InvalidJobId()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateJobStage(100, false));
            }
        }

        [Fact]
        public async Task GetJobsByUkprn_Success()
        {
            IContainer container = Registrations();
            List<FileUploadJob> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();

                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                    Ukprn = 100,
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 100,
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 999900,
                });

                result = (await manager.GetJobsByUkprn(100)).ToList();
            }

            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetJobsByUkprnForDateRange_Success()
        {
            IContainer container = Registrations();
            List<FileUploadJob> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();

                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                    Ukprn = 100
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 100
                });

                result = (await manager.GetJobsByUkprnForDateRange(100, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow)).ToList();
            }

            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetJobsByUkprnForPeriod_Success()
        {
            IContainer container = Registrations();
            List<FileUploadJob> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();

                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                    Ukprn = 100,
                    PeriodNumber = 1
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 999900,
                    PeriodNumber = 2
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 999900,
                    PeriodNumber = 2
                });

                result = (await manager.GetJobsByUkprnForPeriod(999900, 2)).ToList();
            }

            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetLatestJobByUkprnAndContractReference_Success()
        {
            IContainer container = Registrations();
            FileUploadJob result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();

                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                    Ukprn = 10000116,
                    PeriodNumber = 1,
                    FileName = "10000116/SUPPDATA-10000116-ESF-2270-20181109-090919.csv",
                    CollectionName = "ESF"
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 10000116,
                    PeriodNumber = 2,
                    FileName = "10000116/SUPPDATA-10000116-ESF-99999-20181109-090919.csv",
                    CollectionName = "ESF"
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 3,
                    Ukprn = 10000119,
                    PeriodNumber = 2,
                    FileName = "10000119/SUPPDATA-10000119-ESF-2270-20181109-090919.csv",
                    CollectionName = "ESF"
                });

                result = await manager.GetLatestJobByUkprnAndContractReference(10000116, "ESF-2270", "ESF");
            }

            result.Should().NotBeNull();
            result.FileName.Should().Be("10000116/SUPPDATA-10000116-ESF-2270-20181109-090919.csv");
            result.Ukprn.Should().Be(10000116);
            result.JobId.Should().Be(1);
        }

        [Fact]
        public async Task GetLatestJobByUkprn_Success()
        {
            IContainer container = Registrations();
            IEnumerable<FileUploadJob> items;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();

                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                    Ukprn = 10000116,
                    PeriodNumber = 1,
                    FileName = "ilr.xml",
                    CollectionName = "ILR1819"
                });

                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 10000116,
                    PeriodNumber = 2,
                    FileName = "10000116/SUPPDATA-10000116-ESF-99999-20181109-090919.csv",
                    CollectionName = "ESF"
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 3,
                    Ukprn = 10000116,
                    PeriodNumber = 2,
                    FileName = "eas.csv",
                    CollectionName = "EAS"
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 4,
                    Ukprn = 10000119,
                    PeriodNumber = 2,
                    FileName = "eas11.csv",
                    CollectionName = "EAS"
                });

                items = (await manager.GetLatestJobByUkprn(new long[] { 10000116, 10000119 })).ToList();
            }

            items.Should().NotBeEmpty();

            items.Single(x => x.FileName.Equals("ilr.xml") && x.Ukprn == 10000116 && x.JobId == 1).Should().NotBeNull();
            items.Single(x => x.FileName.Equals("eas11.csv") && x.Ukprn == 10000119 && x.JobId == 4).Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllJobs_Success()
        {
            IContainer container = Registrations();
            List<FileUploadJob> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                    Ukprn = 100,
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 100,
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    Ukprn = 999900,
                });

                result = (await manager.GetAllJobs()).ToList();
            }

            result.Should().NotBeNull();
            result.Count.Should().Be(3);
        }

        [Fact]
        public async Task GetLatestJobsPerPeriodByUkprn_Success()
        {
            IContainer container = Registrations();
            List<FileUploadJob> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                IFileUploadJobManager manager = scope.Resolve<IFileUploadJobManager>();

                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 1,
                    JobType = JobType.EsfSubmission,
                    Ukprn = 10000116,
                    FileName = "esf.csv",
                    CollectionName = "ESF",
                    CollectionYear = 1819,
                    Status = JobStatusType.Completed,
                    PeriodNumber = 1,
                    DateTimeSubmittedUtc = DateTime.Now.AddMinutes(-10),
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 2,
                    JobType = JobType.EasSubmission,
                    Ukprn = 10000116,
                    FileName = "eas.csv",
                    CollectionName = "EAS",
                    CollectionYear = 1819,
                    PeriodNumber = 1,
                    Status = JobStatusType.Completed,
                    DateTimeSubmittedUtc = DateTime.Now.AddMinutes(-10),
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 3,
                    Ukprn = 10000116,
                    FileName = "ilr1819.xml",
                    CollectionName = "ILR1819",
                    CollectionYear = 1819,
                    PeriodNumber = 1,
                    JobType = JobType.IlrSubmission,
                    Status = JobStatusType.Completed,
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 4,
                    Ukprn = 10000116,
                    FileName = "ilr1718.xml",
                    CollectionName = "ILR1718",
                    CollectionYear = 1718,
                    PeriodNumber = 1,
                    JobType = JobType.IlrSubmission,
                    Status = JobStatusType.Completed,
                });
                await manager.AddJob(new FileUploadJob()
                {
                    JobId = 5,
                    Ukprn = 10000116,
                    FileName = "ilr_latest_not_completed.xml",
                    CollectionName = "ILR1819",
                    CollectionYear = 1819,
                    PeriodNumber = 1,
                    JobType = JobType.IlrSubmission,
                    Status = JobStatusType.Failed,
                    DateTimeSubmittedUtc = DateTime.Now.AddMinutes(-50),
                });

                result =
                    (await manager.GetLatestJobsPerPeriodByUkprn(10000116, DateTime.Now.AddDays(-1), DateTime.Now))
                    .ToList();
            }

            result.Should().NotBeNull();
            result.Count.Should().Be(4);
            result.Single(x => x.JobType == JobType.EsfSubmission && x.JobId == 1 && x.FileName == "esf.csv").Should().NotBeNull();
            result.Single(x => x.JobType == JobType.EasSubmission && x.JobId == 2 && x.FileName == "eas.csv").Should().NotBeNull();
            result.Single(x => x.JobType == JobType.IlrSubmission && x.JobId == 3 && x.FileName == "ilr1819.xml" && x.CollectionYear == 1819).Should().NotBeNull();
            result.Single(x => x.JobType == JobType.IlrSubmission && x.JobId == 4 && x.FileName == "ilr1718.xml" && x.CollectionYear == 1718).Should().NotBeNull();
        }

        private IContainer Registrations()
        {
            ContainerBuilder builder = new ContainerBuilder();

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(DateTime.UtcNow);

            builder.RegisterInstance(dateTimeProviderMock.Object).As<IDateTimeProvider>().SingleInstance();
            builder.RegisterInstance(new Mock<IEmailNotifier>().Object).As<IEmailNotifier>().SingleInstance();
            builder.RegisterInstance(new Mock<IFileUploadJobManager>().Object).As<IFileUploadJobManager>().SingleInstance();
            builder.RegisterInstance(new Mock<IEmailTemplateManager>().Object).As<IEmailTemplateManager>().SingleInstance();
            builder.RegisterInstance(new Mock<ILogger>().Object).As<ILogger>().SingleInstance();
            builder.RegisterInstance(new Mock<IReturnCalendarService>().Object).As<IReturnCalendarService>().SingleInstance();
            builder.RegisterType<FileUploadJobManager>().As<IFileUploadJobManager>().InstancePerLifetimeScope();
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
