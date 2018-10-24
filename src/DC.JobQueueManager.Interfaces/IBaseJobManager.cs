using System.Collections.Generic;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IBaseJobManager<T>
    {
        long AddJob(T job);

        T GetJobById(long jobId);

        IEnumerable<T> GetAllJobs();

        void SendEmailNotification(long jobId);
    }
}
