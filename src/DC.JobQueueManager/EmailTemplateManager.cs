using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public class EmailTemplateManager : IEmailTemplateManager
    {
        private readonly Func<IJobQueueDataContext> _contextFactory;
        private readonly IReturnCalendarService _returnCalendarService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public EmailTemplateManager(
            Func<IJobQueueDataContext> contextFactory,
            IReturnCalendarService returnCalendarService,
            IDateTimeProvider dateTimeProvider)
        {
            _contextFactory = contextFactory;
            _returnCalendarService = returnCalendarService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<string> GetTemplate(long jobId, JobStatusType status, JobType jobType, DateTime dateTimeJobSubmittedUtc)
        {
            using (IJobQueueDataContext context = _contextFactory())
            {
                var job = await context.FileUploadJobMetaData.SingleOrDefaultAsync(x => x.JobId == jobId);

                ReturnPeriod period = null;
                if (job != null)
                {
                    period = await GetReturnPeriod(job.CollectionName, dateTimeJobSubmittedUtc);
                }

                var emailTemplate = await 
                    context.JobEmailTemplate.SingleOrDefaultAsync(
                        x => x.JobType == (short)jobType && x.JobStatus == (short)status && x.Active.Value);

                if (emailTemplate == null)
                {
                    return string.Empty;
                }

                if (period != null)
                {
                    return emailTemplate.TemplateOpenPeriod;
                }

                return emailTemplate.TemplateClosePeriod ?? emailTemplate.TemplateOpenPeriod;
            }
        }

        public async Task<ReturnPeriod> GetReturnPeriod(string collectionName, DateTime dateTimeUtc)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                return null;
            }

            return await _returnCalendarService.GetPeriodAsync(collectionName, _dateTimeProvider.ConvertUtcToUk(dateTimeUtc));
        }
    }
}
