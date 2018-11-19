using System;
using System.Collections.Generic;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IFileUploadJobManager : IBaseJobManager<FileUploadJob>
    {
        bool UpdateJobStage(long jobId, bool isFirstStage);

        IEnumerable<FileUploadJob> GetJobsByUkprn(long ukprn);

        IEnumerable<FileUploadJob> GetJobsByUkprnForPeriod(long ukprn, int period);

        IEnumerable<FileUploadJob> GetJobsByUkprnForDateRange(long ukprn, DateTime startDateTimeUtc, DateTime endDateTimeUtc);

        FileUploadJob GetLatestJobByUkprn(long ukprn, string collectionName);
    }
}