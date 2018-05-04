using System.Collections.Generic;
using ESFA.DC.JobQueueManager.Models;
using ESFA.DC.JobQueueManager.Models.Enums;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobQueueManager
    {
        Job GetJobByPriority();

        void RemoveJobFromQueue(long jobId);

        bool UpdateJobStatus(long jobId, JobStatus status);

        Job GetJobById(long jobId);

        bool AnyInProgressReferenceJob();

        long AddJob(Job job);

        bool UpdateJob(Job job);

        IEnumerable<Job> GetAllJobs();
    }
}