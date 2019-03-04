using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.JobQueueManager.Interfaces.ExternalData
{
    public interface IExternalDataScheduleService
    {
        Task<IEnumerable<Jobs.Model.Enums.JobType>> GetJobs(bool removeOldDates, CancellationToken cancellationToken);
    }
}
