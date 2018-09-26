using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.JobQueueManager.Interfaces.ExternalData
{
    public interface IExternalDataScheduleService
    {
        Task<IEnumerable<string>> GetJobs(bool removeOldDates, CancellationToken cancellationToken);
    }
}
