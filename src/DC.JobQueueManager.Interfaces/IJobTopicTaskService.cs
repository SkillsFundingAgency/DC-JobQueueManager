using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model.Enums;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobTopicTaskService
    {
        Task<IEnumerable<ITopicItem>> GetTopicItems(JobType jobType, bool isFirstStage = false, CancellationToken cancellationToken = default(CancellationToken));
    }
}
