using System.Collections.Generic;
using ESFA.DC.Job.Models;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobManager
    {
        Job.Models.Job GetJobByPriority();

        void RemoveJobFromQueue(long jobId);

        bool UpdateJobStatus(long jobId, JobStatusType status);

        Job.Models.Job GetJobById(long jobId);

        long AddJob(Job.Models.Job job);

        bool UpdateJob(Job.Models.Job job);

        IEnumerable<Job.Models.Job> GetAllJobs();

        IEnumerable<Job.Models.Job> GetJobsByUkprn(long ukrpn);
    }
}