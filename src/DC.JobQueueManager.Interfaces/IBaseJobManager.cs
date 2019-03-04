using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IBaseJobManager<T>
    {
        Task<long> AddJob(T job);

        Task<T> GetJobById(long jobId);

        Task<IEnumerable<T>> GetAllJobs();

        Task SendEmailNotification(long jobId);
    }
}
