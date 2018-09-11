using System.Collections.Generic;
using ESFA.DC.Jobs.Model;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobManager : IBaseJobManager<Job>
    {
        Job GetJobByPriority();

        void RemoveJobFromQueue(long jobId);

        bool UpdateJobStatus(long jobId, JobStatusType status);

        bool UpdateJob(Job job);
    }
}