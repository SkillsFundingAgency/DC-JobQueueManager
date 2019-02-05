using System;
using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Data;
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
        private readonly Func<IJobQueueDataContext> _contextFactory;
        private readonly IReturnCalendarService _returnCalendarService;
        private readonly IEmailTemplateManager _emailTemplateManager;

        protected AbstractJobManager(
            Func<IJobQueueDataContext> contextFactory,
            IReturnCalendarService returnCalendarService,
            IEmailTemplateManager emailTemplateManager)
        {
            _contextFactory = contextFactory;
            _returnCalendarService = returnCalendarService;
            _emailTemplateManager = emailTemplateManager;
        }

        public async Task<bool> IsCrossLoadingEnabled(JobType jobType)
        {
            using (var context = _contextFactory())
            {
                var entity = await context.JobType.SingleOrDefaultAsync(x => x.JobTypeId == (short)jobType);
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
