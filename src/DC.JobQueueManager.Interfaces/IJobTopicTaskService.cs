using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model.Enums;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobTopicTaskService
    {
        IEnumerable<ITopicItem> GetTopicItems(JobType jobType, bool isFirstStage = false);
    }
}
