using System.Collections.Generic;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IFileUploadJobManager : IBaseJobManager<FileUploadJob>
    {
        bool UpdateJobStage(long jobId, bool isFirstStage);

        IEnumerable<FileUploadJob> GetJobsByUkprn(long ukprn);
    }
}