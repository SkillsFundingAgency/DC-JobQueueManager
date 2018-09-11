using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public sealed class FileUploadJobManager : IFileUploadJobManager
    {
        private readonly DbContextOptions _contextOptions;
        private readonly IDateTimeProvider _dateTimeProvider;

        public FileUploadJobManager(DbContextOptions contextOptions, IDateTimeProvider dateTimeProvider)
        {
            _contextOptions = contextOptions;
            _dateTimeProvider = dateTimeProvider;
        }

        public FileUploadJob GetJobById(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.FileUploadJobMetaDataEntities.Include(x => x.Job).SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var job = new FileUploadJob();
                JobConverter.Convert(entity, job);
                return job;
            }
        }

        public long AddJob(FileUploadJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = new JobEntity
                {
                    DateTimeSubmittedUtc = _dateTimeProvider.GetNowUtc(),
                    JobType = (short)job.JobType,
                    Priority = job.Priority,
                    Status = (short)job.Status,
                    SubmittedBy = job.SubmittedBy,
                    NotifyEmail = job.NotifyEmail
                };

                var metaEntity = new FileUploadJobMetaDataEntity()
                {
                    FileName = job.FileName,
                    FileSize = job.FileSize,
                    IsFirstStage = true,
                    StorageReference = job.StorageReference,
                    Job = entity,
                    CollectionName = job.CollectionName,
                    PeriodNumber = job.PeriodNumber,
                    Ukprn = job.Ukprn
                };
                context.Jobs.Add(entity);
                context.FileUploadJobMetaDataEntities.Add(metaEntity);
                context.SaveChanges();
                return entity.JobId;
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

        public IEnumerable<FileUploadJob> GetJobsByUkprn(long ukprn)
        {
            var items = new List<FileUploadJob>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entities = context.FileUploadJobMetaDataEntities.Include(x => x.Job).Where(x => x.Ukprn == ukprn)
                    .ToList();
                foreach (var entity in entities)
                {
                    var model = new FileUploadJob();
                    JobConverter.Convert(entity, model);
                    items.Add(model);
                }

                return items;
            }
        }

        public IEnumerable<FileUploadJob> GetAllJobs()
        {
            var items = new List<FileUploadJob>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entities = context.FileUploadJobMetaDataEntities.Include(x => x.Job).ToList();
                foreach (var entity in entities)
                {
                    var model = new FileUploadJob();
                    JobConverter.Convert(entity, model);
                    items.Add(model);
                }

                return items;
            }
        }

        public void PopulatePersonalisation(long jobId, Dictionary<string, dynamic> personalisation)
        {
            var job = GetJobById(jobId);
            if (job != null)
            {
                personalisation.Add("FileName", job.FileName);
                personalisation.Add("CollectionName", job.CollectionName);
                personalisation.Add(
                    "PeriodName",
                    $"R{job.PeriodNumber.ToString("00", NumberFormatInfo.InvariantInfo)}");
                personalisation.Add("Ukprn", job.Ukprn);
            }
        }
    }
}