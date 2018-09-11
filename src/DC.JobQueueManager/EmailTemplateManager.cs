using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public class EmailTemplateManager : IEmailTemplateManager
    {
        private readonly DbContextOptions _contextOptions;

        public EmailTemplateManager(DbContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public string GetTemplate(long jobId, JobStatusType status, JobType jobType)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var emailTemplate =
                    context.JobEmailTemplates.SingleOrDefault(
                        x => x.JobType == (short)jobType && x.JobStatus == (short)status && x.Active);
                return emailTemplate?.TemplateId;
            }
        }
    }
}
