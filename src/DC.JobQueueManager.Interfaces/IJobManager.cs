using System.Collections.Generic;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IJobManager
    {
        FileUploadJob GetJobByPriority();

        void RemoveJobFromQueue(long jobId);

        bool UpdateJobStatus(long jobId, JobStatusType status);

        FileUploadJob GetJobById(long jobId);

        long AddJob(FileUploadJob job);

        bool UpdateJob(FileUploadJob job);

        IEnumerable<FileUploadJob> GetAllJobs();

        IEnumerable<FileUploadJob> GetJobsByUkprn(long ukrpn);
    }
}