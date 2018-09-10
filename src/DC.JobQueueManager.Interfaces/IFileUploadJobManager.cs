using System.Collections.Generic;
using ESFA.DC.Job.Models;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IFileUploadJobManager
    {
        FileUploadJobDto GetJob(long jobId);

        long AddJob(FileUploadJobDto job);

        bool UpdateJobStage(long jobId, bool isFirstStage);

        IEnumerable<FileUploadJobDto> GetJobsByUkprn(long ukprn);
    }
}