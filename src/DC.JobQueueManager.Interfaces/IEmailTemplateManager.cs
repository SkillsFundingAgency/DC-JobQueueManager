using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IEmailTemplateManager
    {
        Task<string> GetTemplate(long jobId, JobStatusType status, JobType jobType, DateTime dateTimeJobSubmittedUtc);
    }
}
