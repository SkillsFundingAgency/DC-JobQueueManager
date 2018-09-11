using System.Collections.Generic;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IFileUploadJobManager
    {
        FileUploadJob GetJob(long jobId);

        long AddJob(FileUploadJob job);

        bool UpdateJobStage(long jobId, bool isFirstStage);

        IEnumerable<FileUploadJob> GetJobsByUkprn(long ukprn);

        IEnumerable<FileUploadJob> GetAllJobs();
    }
}