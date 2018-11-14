using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobManager : IBaseJobManager<Job>
    {
        Task<IEnumerable<Job>> GetJobsByPriorityAsync();

        void RemoveJobFromQueue(long jobId);

        bool UpdateJobStatus(long jobId, JobStatusType status);

        bool UpdateJob(Job job);

        bool IsCrossLoadingEnabled(JobType jobType);

        bool UpdateCrossLoadingStatus(long jobId, JobStatusType status);
    }
}