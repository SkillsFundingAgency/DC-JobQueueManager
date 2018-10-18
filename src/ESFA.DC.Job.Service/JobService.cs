//using ESFA.DC.Job.Models;
//using ESFA.DC.Job.Service.Interfaces;
//using ESFA.DC.JobQueueManager.Interfaces;
//using ESFA.DC.Jobs.Model;
//using System;
//using System.Collections.Generic;

//namespace ESFA.DC.Job.Service
//{
//    public class JobService : IJobService
//    {
//        private readonly IJobManager _jobManager;
//        private readonly IFileUploadMetaDataManager _fileUploadMetaDataManager;

//        public JobService(IJobManager jobManager, IFileUploadMetaDataManager fileUploadMetaDataManager)
//        {
//            _fileUploadMetaDataManager = fileUploadMetaDataManager;
//            _jobManager = jobManager;
//        }

//        public long AddFileUploadJob(Models.Job job, FileUploadJobMetaData metaData)
//        {
//            throw new NotImplementedException();
//        }

//        public FileUploadJobDto GetFileUploadJob(long ukprn, long jobId)
//        {

//            var metaData = _fileUploadMetaDataManager.GetJobMetaData(jobId);
//            if (metaData?.Ukprn != ukprn)
//            {
//                return null;
//            }

//            var job = _jobManager.GetJobById(jobId);

//            var jobDto = new FileUploadJobDto()
//            {
//                JobId = job.JobId,
//                DateTimeSubmittedUtc = job.DateTimeSubmittedUtc,
//                DateTimeUpdatedUtc = job.DateTimeUpdatedUtc,
//                NotifyEmail = job.NotifyEmail,
//                Priority = job.Priority,
//                SubmittedBy = job.SubmittedBy,
//                Status = (short)job.Status,
//                JobType = (short)job.JobType,
//                Ukprn = metaData.Ukprn,
//                CollectionName = metaData.CollectionName,
//                FileName = metaData.FileName,
//                FileSize = metaData.FileSize,
//                IsFirstStage = metaData.IsFirstStage,
//                PeriodNumber = metaData.PeriodNumber,
//                RowVersion = job.RowVersion,
//                StorageReference = metaData.StorageReference,
//            };

//            return jobDto;
//        }

//        public IEnumerable<FileUploadJobDto> GetFileUploadJobs()
//        {
//            throw new NotImplementedException();
//        }

//        public IEnumerable<FileUploadJobDto> GetFileUploadJobs(long ukprn)
//        {
//            var data = _fileUploadMetaDataManager.ge
//        }
//    }
//}
