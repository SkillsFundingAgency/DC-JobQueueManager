using System;
using System.Collections.Generic;
using ESFA.DC.Job.Models;
using ESFA.DC.Jobs.Model;

namespace ESFA.DC.Job.Service.Interfaces
{
    public interface IJobService
    {
        long AddFileUploadJob(Models.Job job, FileUploadJobMetaData metaData);

        IEnumerable<FileUploadJobDto> GetFileUploadJobs();

        IEnumerable<FileUploadJobDto> GetFileUploadJobs(long ukprn);

        FileUploadJobDto GetFileUploadJob(long ukprn, long jobId);
    }
}
