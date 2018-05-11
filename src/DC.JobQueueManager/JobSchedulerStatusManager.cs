using System;
using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Interfaces;

namespace ESFA.DC.JobQueueManager
{
    public class JobSchedulerStatusManager : IJobSchedulerStatusManager
    {
        public async Task DisableJobQueueProcessingAsync()
        {
            throw new NotImplementedException();
        }

        public async Task EnableJobQueueProcessingAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsJobQueueProcessingEnabledAsync()
        {
            return true;
        }
    }
}