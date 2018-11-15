using System;
using System.Runtime.CompilerServices;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using FluentAssertions;
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
            var contextOptions = GetContextOptions();
            var returnCalendarMock = new Mock<IReturnCalendarService>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

            returnCalendarMock.Setup(x => x.GetPeriodAsync("ILR1819", It.IsAny<DateTime>())).ReturnsAsync(() => isClose ? null : new ReturnPeriod());

            var templateManager = new EmailTemplateManager(contextOptions, returnCalendarMock.Object, dateTimeProviderMock.Object);

            // Create the schema in the database
            using (var context = new JobQueueDataContext(contextOptions))
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

            var template = templateManager.GetTemplate(1, JobStatusType.Completed, JobType.IlrSubmission, DateTime.Now);
            template.Should().NotBeNull();
            template.Should().Be(isClose ? "template_close" : "template_open");
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
    }
}
