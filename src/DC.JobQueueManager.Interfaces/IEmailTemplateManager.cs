using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IEmailTemplateManager
    {
        string GetTemplate(long jobId, JobStatusType status, JobType jobType);
    }
}
