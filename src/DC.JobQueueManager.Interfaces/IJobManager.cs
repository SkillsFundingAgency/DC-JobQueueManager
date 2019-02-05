using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobManager : IBaseJobManager<Job>
    {
        Task<IEnumerable<Job>> GetJobsByPriorityAsync(int resultCount);

        Task RemoveJobFromQueue(long jobId);

        Task<bool> UpdateJobStatus(long jobId, JobStatusType status);

        Task<bool> UpdateJob(Job job);

        Task<bool> IsCrossLoadingEnabled(JobType jobType);

        Task<bool> UpdateCrossLoadingStatus(long jobId, JobStatusType status);

        void SendEmailNotification(Jobs.Model.Job job);
    }
}