using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
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
                context.JobEmailTemplates.Add(new JobEmailTemplateEntity()
                {
                    JobType = (short)JobType.IlrSubmission,
                    JobStatus = (short)JobStatusType.Completed,
                    Active = true,
                    TemplateOpenPeriod = "template_open",
                    TemplateClosePeriod = "template_close"
                });
                context.FileUploadJobMetaDataEntities.Add(new FileUploadJobMetaDataEntity()
                {
                    Job = new JobEntity()
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
