using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobSchedulerStatusManager
    {
        Task DisableJobQueueProcessingAsync();

        Task EnableJobQueueProcessingAsync();

        Task<bool> IsJobQueueProcessingEnabledAsync();
    }
}
