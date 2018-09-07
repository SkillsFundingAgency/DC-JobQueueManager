using System;
using System.Linq;
using ESFA.DC.Job.Models;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public sealed class FileUploadMetaDataManager : IFileUploadMetaDataManager
    {
        private readonly DbContextOptions _contextOptions;

        public FileUploadMetaDataManager(DbContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public FileUploadJobMetaData GetJobMetaData(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.FileUploadJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var meta = new FileUploadJobMetaData();
                JobConverter.Convert(entity, meta);
                return meta;
            }
        }

        public long AddJobMetData(FileUploadJobMetaData job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var metaEntity = new FileUploadJobMetaDataEntity()
                {
                    FileName = job.FileName,
                    FileSize = job.FileSize,
                    IsFirstStage = true,
                    StorageReference = job.StorageReference,
                    JobId = job.JobId,
                    CollectionName = job.CollectionName,
                    PeriodNumber = job.PeriodNumber
                };
                context.FileUploadJobMetaDataEntities.Add(metaEntity);
                context.SaveChanges();
                return job.JobId;
            }
        }

        public bool UpdateJobStage(long jobId, bool isFirstStage)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.FileUploadJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                entity.IsFirstStage = isFirstStage;
                context.Entry(entity).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    throw new Exception("Save failed. Job details have been changed. Reload the job object and try save again");
                }
            }
        }
    }
}