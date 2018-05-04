using System;
using System.Collections.Generic;
using DC.JobQueueManager.Models;
using DC.JobQueueManager.Models.Enums;

namespace DC.JobQueueManager.Interfaces
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
