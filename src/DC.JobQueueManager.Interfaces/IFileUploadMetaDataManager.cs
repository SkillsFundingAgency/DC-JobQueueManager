using ESFA.DC.Job.Models;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IFileUploadMetaDataManager
    {
        FileUploadJobMetaData GetJobMetaData(long jobId);

        long AddJobMetData(FileUploadJobMetaData job);

        bool UpdateJobStage(long jobId, bool isFirstStage);
    }
}