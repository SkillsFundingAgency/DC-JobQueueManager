using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IFileUploadJobManager : IBaseJobManager<FileUploadJob>
    {
        Task<bool> UpdateJobStage(long jobId, bool isFirstStage);

        Task<IEnumerable<FileUploadJob>> GetJobsByUkprn(long ukprn);

        Task<IEnumerable<FileUploadJob>> GetJobsByUkprnForPeriod(long ukprn, int period);

        Task<IEnumerable<FileUploadJob>> GetJobsByUkprnForDateRange(long ukprn, DateTime startDateTimeUtc,
            DateTime endDateTimeUtc);

        FileUploadJob GetLatestJobByUkprn(long ukprn, string collectionName);

        Task<IEnumerable<FileUploadJob>> GetLatestJobByUkprn(long[] ukprns);

        Task<FileUploadJob> GetLatestJobByUkprnAndContractReference(long ukprn, string contractReference,
            string collectionName);

        Task<IEnumerable<FileUploadJob>> GetLatestJobsPerPeriodByUkprn(long ukprn, DateTime startDateTimeUtc,
            DateTime endDateTimeUtc);
    }
}