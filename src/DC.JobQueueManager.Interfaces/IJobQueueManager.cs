using System.Collections.Generic;
using ESFA.DC.Jobs.Model;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobQueueManager
    {
        Job GetJobByPriority();

        void RemoveJobFromQueue(long jobId);

        bool UpdateJobStatus(long jobId, JobStatusType status);

        Job GetJobById(long jobId);

        long AddJob(Job job);

        bool UpdateJob(Job job);

        IEnumerable<Job> GetAllJobs();

        IEnumerable<Job> GetJobsByUkprn(long ukrpn);

        bool UpdateJobMetaData(IlrJobMetaData jobMetaData);
    }
}