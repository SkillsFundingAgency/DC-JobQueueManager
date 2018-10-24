using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public abstract class AbstractJobManager
    {
        private readonly DbContextOptions _contextOptions;
        private readonly IReturnCalendarService _returnCalendarService;
        private readonly IEmailTemplateManager _emailTemplateManager;

        protected AbstractJobManager(
            DbContextOptions contextOptions,
            IReturnCalendarService returnCalendarService,
            IEmailTemplateManager emailTemplateManager)
        {
            _contextOptions = contextOptions;
            _returnCalendarService = returnCalendarService;
            _emailTemplateManager = emailTemplateManager;
        }

        public bool IsCrossLoadingEnabled(JobType jobType)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.JobTypes.SingleOrDefault(x => x.JobTypeId == (short)jobType);
                return entity != null && entity.IsCrossLoadingEnabled;
            }
        }

        public ReturnPeriod GetNextReturnPeriod(string collectionName)
        {
            return _returnCalendarService.GetNextPeriodAsync(collectionName).Result;
        }

        public string GetTemplate(long jobId, JobStatusType status, JobType jobType, DateTime dateTimeJobSubmittedUtc)
        {
            return _emailTemplateManager.GetTemplate(jobId, status, jobType, dateTimeJobSubmittedUtc);
        }
    }
}
