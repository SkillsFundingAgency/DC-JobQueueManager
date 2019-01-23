using System;
using System.Linq;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using IReturnCalendarService = ESFA.DC.JobQueueManager.Interfaces.IReturnCalendarService;
using JobStatusType = ESFA.DC.JobStatus.Interface.JobStatusType;
using JobType = ESFA.DC.Jobs.Model.Enums.JobType;
using ReturnPeriod = ESFA.DC.CollectionsManagement.Models.ReturnPeriod;

namespace ESFA.DC.JobQueueManager
{
    public abstract class AbstractJobManager
    {
        private readonly DbContextOptions<JobQueueDataContext> _contextOptions;
        private readonly IReturnCalendarService _returnCalendarService;
        private readonly IEmailTemplateManager _emailTemplateManager;

        protected AbstractJobManager(
            DbContextOptions<JobQueueDataContext> contextOptions,
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
                var entity = context.JobType.SingleOrDefault(x => x.JobTypeId == (short)jobType);
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
