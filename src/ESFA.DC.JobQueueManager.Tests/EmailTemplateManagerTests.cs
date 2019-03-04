using System;
using System.Runtime.CompilerServices;
using Autofac;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using JobStatusType = ESFA.DC.JobStatus.Interface.JobStatusType;
using JobType = ESFA.DC.Jobs.Model.Enums.JobType;
using ReturnPeriod = ESFA.DC.CollectionsManagement.Models.ReturnPeriod;

namespace ESFA.DC.JobQueueManager.Tests
{
    public class EmailTemplateManagerTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetTemplate_Success(bool isClose)
        {
            var returnCalendarMock = new Mock<IReturnCalendarService>();
            returnCalendarMock.Setup(x => x.GetPeriodAsync("ILR1819", It.IsAny<DateTime>())).ReturnsAsync(() => isClose ? null : new ReturnPeriod());

            IContainer container = Registrations(returnCalendarMock.Object);

            using (var scope = container.BeginLifetimeScope())
            {
                var templateManager = scope.Resolve<IEmailTemplateManager>();
                var options = scope.Resolve<DbContextOptions<JobQueueDataContext>>();

                // Create the schema in the database
                using (var context = new JobQueueDataContext(options))
                {
                    context.Database.EnsureCreated();
                    context.JobEmailTemplate.Add(new JobEmailTemplate()
                    {
                        JobType = (short)JobType.IlrSubmission,
                        JobStatus = (short)JobStatusType.Completed,
                        Active = true,
                        TemplateOpenPeriod = "template_open",
                        TemplateClosePeriod = "template_close"
                    });
                    context.FileUploadJobMetaData.Add(new FileUploadJobMetaData()
                    {
                        Job = new Job()
                        {
                            JobId = 1,
                            JobType = (short)JobType.IlrSubmission
                        },
                        CollectionName = "ILR1819",
                        PeriodNumber = 1,
                        FileName = "test",
                        FileSize = 100,
                        IsFirstStage = false,
                        StorageReference = "test"
                    });
                    context.SaveChanges();
                }

                var template =
                    templateManager.GetTemplate(1, JobStatusType.Completed, JobType.IlrSubmission, DateTime.Now).Result;
                template.Should().NotBeNull();
                template.Should().Be(isClose ? "template_close" : "template_open");
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

        private IContainer Registrations(IReturnCalendarService returnCalendarService = null)
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterInstance(new Mock<IDateTimeProvider>().Object).As<IDateTimeProvider>().SingleInstance();
            builder.RegisterInstance(returnCalendarService ?? new Mock<IReturnCalendarService>().Object).As<IReturnCalendarService>().SingleInstance();
            builder.RegisterType<EmailTemplateManager>().As<IEmailTemplateManager>().InstancePerLifetimeScope();

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
