using System.Collections.Generic;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IIlrJobQueueManager
    {
        IlrJob GetJobByPriority();

        void RemoveJobFromQueue(long jobId);

        bool UpdateJobStatus(long jobId, JobStatusType status);

        IlrJob GetJobById(long jobId);

        long AddJob(IlrJob job);

        bool UpdateJob(IlrJob job);

        IEnumerable<IlrJob> GetAllJobs();

        IEnumerable<IlrJob> GetJobsByUkprn(long ukrpn);
    }
}