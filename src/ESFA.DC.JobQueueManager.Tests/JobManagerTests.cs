using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.DateTimeProvider.Interface;
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
using JobStatusType = ESFA.DC.JobStatus.Interface.JobStatusType;
using JobType = ESFA.DC.Jobs.Model.Enums.JobType;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class JobManagerTests
    {
        [Fact]
        public async Task AddJob_Null()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                IJobManager manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AddJob(null));
            }
        }

        [Fact]
        public async Task AddJob_Success()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                IJobManager manager = scope.Resolve<IJobManager>();
                var result = await manager.AddJob(new Job());
                result.Should().Be(1);
            }
        }

        [Theory]
        [InlineData(JobType.IlrSubmission)]
        [InlineData(JobType.EsfSubmission)]
        [InlineData(JobType.EasSubmission)]
        [InlineData(JobType.ReferenceDataEPA)]
        [InlineData(JobType.ReferenceDataFCS)]
        public async Task AddJob_Success_Values(JobType jobType)
        {
            var job = new Job
            {
                DateTimeSubmittedUtc = DateTime.UtcNow,
                DateTimeUpdatedUtc = DateTime.UtcNow,
                JobId = 0,
                Priority = 1,
                RowVersion = null,
                Status = JobStatusType.Ready,
                SubmittedBy = "test user",
                JobType = jobType,
                NotifyEmail = "test@email.com"
            };

            IContainer container = Registrations();
            Job savedJob;

            using (var scope = container.BeginLifetimeScope())
            {
                IJobManager manager = scope.Resolve<IJobManager>();
                await manager.AddJob(job);

                var result = await manager.GetAllJobs();

                savedJob = result.SingleOrDefault();
            }

            savedJob.Should().NotBeNull();

            savedJob.JobId.Should().Be(1);
            savedJob.DateTimeSubmittedUtc.Should().BeOnOrBefore(DateTime.UtcNow);
            savedJob.DateTimeUpdatedUtc.Should().BeNull();
            savedJob.JobType.Should().Be(jobType);
            savedJob.Priority.Should().Be(1);
            savedJob.SubmittedBy.Should().Be("test user");
            savedJob.Status.Should().Be(JobStatusType.Ready);
            savedJob.NotifyEmail.Should().Be("test@email.com");
            savedJob.CrossLoadingStatus.Should().Be(null);
        }

        [Fact]
        public async Task GetJobById_Success()
        {
            IContainer container = Registrations();
            Job result;

            using (var scope = container.BeginLifetimeScope())
            {
                IJobManager manager = scope.Resolve<IJobManager>();
                var jobId = await manager.AddJob(new Job());
                result = await manager.GetJobById(1);
            }

            result.Should().NotBeNull();
            result.JobId.Should().Be(1);
        }

        [Fact]
        public async Task GetJobById_Fail_zeroId()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                IJobManager manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.GetJobById(0));
            }
        }

        [Fact]
        public async Task GetJobById_Fail_IdNotFound()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                IJobManager manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.GetJobById(100));
            }
        }

        [Fact]
        public async Task GetAllJobs_Success()
        {
            IContainer container = Registrations();
            IEnumerable<Job> result;

            using (var scope = container.BeginLifetimeScope())
            {
                IJobManager manager = scope.Resolve<IJobManager>();
                await manager.AddJob(new Job());
                await manager.AddJob(new Job());
                await manager.AddJob(new Job());
                result = await manager.GetAllJobs();
            }

            result.Count().Should().Be(3);
        }

        public async Task GetJobByPriority_Ilr_NoJobs()
        {
            IContainer container = Registrations();
            IEnumerable<Job> result;

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                }

                var manager = container.Resolve<IJobManager>();
                result = await manager.GetJobsByPriorityAsync(100);
            }

            result.Should().BeEmpty();
        }

        public async Task GetJobByPriority_Ilr_submission()
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

                var manager = container.Resolve<IJobManager>();
                await manager.AddJob(new Job
                {
                    Priority = 1,
                    Status = JobStatusType.Ready
                });
                await manager.AddJob(new Job
                {
                    Priority = 2,
                    Status = JobStatusType.Ready
                });

                IEnumerable<Job> result = (await manager.GetJobsByPriorityAsync(100)).ToList();
                result.Should().NotBeEmpty();
                Job job = result.First();
                job.JobId.Should().Be(2);
                job.JobType.Should().Be(JobType.IlrSubmission);
            }
        }

        [Fact]
        public async Task RemoveJobFromQueue_Fail_ZeroId()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveJobFromQueue(0));
            }
        }

        [Fact]
        public async Task RemoveJobFromQueue_Fail_IdDontExist()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveJobFromQueue(200));
            }
        }

        [Theory]
        [InlineData(JobStatusType.MovedForProcessing)]
        [InlineData(JobStatusType.Processing)]
        [InlineData(JobStatusType.Completed)]
        [InlineData(JobStatusType.Failed)]
        [InlineData(JobStatusType.FailedRetry)]
        [InlineData(JobStatusType.Paused)]
        public async Task RemoveJobFromQueue_Fail_InvalidJobStatus(JobStatusType status)
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await manager.AddJob(new Job
                {
                    Status = status
                });

                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => manager.RemoveJobFromQueue(1));
            }
        }

        [Fact]
        public async Task RemoveJobFromQueue_Success()
        {
            IContainer container = Registrations();
            IEnumerable<Job> jobsAfterRemoval;

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await manager.AddJob(new Job
                {
                    Status = JobStatusType.Ready
                });
                var jobs = await manager.GetAllJobs();
                jobs.Count().Should().Be(1);

                await manager.RemoveJobFromQueue(1);

                jobsAfterRemoval = await manager.GetAllJobs();
            }

            jobsAfterRemoval.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateJob_Fail_Null()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentNullException>(() => manager.UpdateJob(null));
            }
        }

        [Fact]
        public async Task UpdateJob_Fail_InvalidJobId()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateJob(new Job { JobId = 1000 }));
            }
        }

        [Fact]
        public async Task UpdateJob_Success()
        {
            IContainer container = Registrations();
            Job updatedJob;

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await manager.AddJob(new Job
                {
                    Status = JobStatusType.Ready,
                    JobType = JobType.IlrSubmission
                });
                var job = await manager.GetJobById(1);
                job.Status = JobStatusType.Completed;
                job.Priority = 2;
                job.NotifyEmail = "test@test.com";
                job.SubmittedBy = "test";
                job.CrossLoadingStatus = JobStatusType.MovedForProcessing;

                await manager.UpdateJob(job);

                updatedJob = await manager.GetJobById(1);
            }

            updatedJob.JobType.Should().Be(JobType.IlrSubmission);
            updatedJob.DateTimeUpdatedUtc.Should().BeOnOrBefore(DateTime.UtcNow);
            updatedJob.Priority.Should().Be(2);
            updatedJob.Status.Should().Be(JobStatusType.Completed);
            updatedJob.SubmittedBy.Should().Be("test");
            updatedJob.NotifyEmail.Should().Be("test@test.com");
            updatedJob.CrossLoadingStatus.Should().Be(JobStatusType.MovedForProcessing);
        }

        [Fact]
        public async Task UpdateJobStatus_Fail_ZeroId()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateJobStatus(0, JobStatusType.Completed));
            }
        }

        [Fact]
        public async Task UpdateJobStatus_Fail_InvalidJobId()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateJobStatus(110, JobStatusType.Completed));
            }
        }

        [Fact]
        public async Task UpdateJobStatus_Success()
        {
            IContainer container = Registrations();
            Job updatedJob;

            using (var scope = container.BeginLifetimeScope())
            {
                var manager = scope.Resolve<IJobManager>();
                await manager.AddJob(new Job
                {
                    Status = JobStatusType.Ready
                });
                await manager.UpdateJobStatus(1, JobStatusType.Completed);

                updatedJob = await manager.GetJobById(1);
            }

            updatedJob.Status.Should().Be(JobStatusType.Completed);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(JobStatusType.MovedForProcessing)]
        public async Task UpdateJobStatus_Success_EmailSent(JobStatusType? crossLoadingStatus)
        {
            var emailTemplateManager = new Mock<IEmailTemplateManager>();
            emailTemplateManager.Setup(x => x.GetTemplate(It.IsAny<long>(), It.IsAny<JobStatusType>(), It.IsAny<JobType>(), It.IsAny<DateTime>())).Returns("template");
            var emailNotifier = new Mock<IEmailNotifier>();
            emailNotifier.Setup(x => x.SendEmail(It.IsAny<string>(), "test", It.IsAny<Dictionary<string, dynamic>>()));

            IContainer container = Registrations(emailTemplateManager.Object, emailNotifier.Object);

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    if (crossLoadingStatus.HasValue)
                    {
                        context.JobTypeGroup.Add(new JobTypeGroup
                        {
                            JobTypeGroupId = 1,
                            Description = "Collection Submission",
                            ConcurrentExecutionCount = 25
                        });
                        context.JobType.Add(new Data.Entities.JobType
                        {
                            IsCrossLoadingEnabled = true,
                            Title = "Title",
                            Description = "Description",
                            JobTypeId = 1,
                            JobTypeGroupId = 1
                        });
                        context.SaveChanges();
                    }
                }

                var manager = scope.Resolve<IJobManager>();
                await manager.AddJob(new Job
                {
                    Status = JobStatusType.Ready,
                    JobType = JobType.IlrSubmission,
                    CrossLoadingStatus = crossLoadingStatus
                });

                await manager.UpdateJobStatus(1, JobStatusType.Completed);

                var updatedJob = await manager.GetJobById(1);
                updatedJob.Status.Should().Be(JobStatusType.Completed);

                emailNotifier.Verify(
                    x => x.SendEmail(It.IsAny<string>(), "template", It.IsAny<Dictionary<string, dynamic>>()), Times.Once());
            }
        }

        [Fact]
        public async Task UpdateCrossLoadingStatus_Success_EmailSent()
        {
            var emailTemplateManager = new Mock<IEmailTemplateManager>();
            emailTemplateManager.Setup(x => x.GetTemplate(It.IsAny<long>(), It.IsAny<JobStatusType>(), It.IsAny<JobType>(), It.IsAny<DateTime>())).Returns("template");
            var emailNotifier = new Mock<IEmailNotifier>();
            emailNotifier.Setup(x => x.SendEmail(It.IsAny<string>(), "test", It.IsAny<Dictionary<string, dynamic>>()));

            IContainer container = Registrations(emailTemplateManager.Object, emailNotifier.Object);

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobTypeGroup.Add(new JobTypeGroup
                    {
                        JobTypeGroupId = 1,
                        Description = "Collection Submission",
                        ConcurrentExecutionCount = 25
                    });
                    context.JobType.Add(new Data.Entities.JobType
                    {
                        IsCrossLoadingEnabled = true,
                        Title = "Title",
                        Description = "Description",
                        JobTypeId = 1,
                        JobTypeGroupId = 1
                    });
                    context.SaveChanges();
                }

                var manager = scope.Resolve<IJobManager>();
                await manager.AddJob(new Job
                {
                    Status = JobStatusType.Ready,
                    JobType = JobType.IlrSubmission,
                    CrossLoadingStatus = JobStatusType.MovedForProcessing
                });

                await manager.UpdateCrossLoadingStatus(1, JobStatusType.Completed);

                var updatedJob = await manager.GetJobById(1);
                updatedJob.CrossLoadingStatus.Should().Be(JobStatusType.Completed);
                emailNotifier.Verify(x => x.SendEmail(It.IsAny<string>(), "template", It.IsAny<Dictionary<string, dynamic>>()), Times.Never);
            }
        }

        [Fact]
        public async Task IsCrossLoadingEnabled_Success()
        {
            IContainer container = Registrations();

            using (var scope = container.BeginLifetimeScope())
            {
                // Create the schema in the database
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobTypeGroup.Add(new JobTypeGroup
                    {
                        JobTypeGroupId = 1,
                        Description = "Collection Submission",
                        ConcurrentExecutionCount = 25
                    });
                    context.JobType.Add(new Data.Entities.JobType
                    {
                        IsCrossLoadingEnabled = true,
                        Title = "Title",
                        Description = "Description",
                        JobTypeId = 1,
                        JobTypeGroupId = 1
                    });
                    context.SaveChanges();
                }

                var manager = scope.Resolve<IJobManager>();
                (await manager.IsCrossLoadingEnabled(JobType.IlrSubmission)).Should().BeTrue();
            }
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

        private IContainer Registrations(IEmailTemplateManager emailTemplateManager = null, IEmailNotifier emailNotifier = null)
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterInstance(new Mock<IDateTimeProvider>().Object).As<IDateTimeProvider>().SingleInstance();
            builder.RegisterInstance(emailTemplateManager ?? new Mock<IEmailTemplateManager>().Object).As<IEmailTemplateManager>().SingleInstance();
            builder.RegisterInstance(emailNotifier ?? new Mock<IEmailNotifier>().Object).As<IEmailNotifier>().SingleInstance();
            builder.RegisterInstance(new Mock<IFileUploadJobManager>().Object).As<IFileUploadJobManager>().SingleInstance();
            builder.RegisterInstance(new Mock<ILogger>().Object).As<ILogger>().SingleInstance();
            builder.RegisterInstance(new Mock<IReturnCalendarService>().Object).As<IReturnCalendarService>().SingleInstance();

            builder.RegisterType<JobManager>().As<IJobManager>().InstancePerLifetimeScope();
            builder.RegisterType<JobQueueDataContext>().As<IJobQueueDataContext>().InstancePerDependency();
            builder.Register(context =>
                {
                    SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    return GetContextOptions();
                })
                .As<DbContextOptions<JobQueueDataContext>>()
                .SingleInstance();

            IContainer container = builder.Build();
            return container;
        }
    }
}
