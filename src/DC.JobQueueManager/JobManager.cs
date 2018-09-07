using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ESFA.DC.JobQueueManager
{
    public sealed class JobManager : IJobManager
    {
        private readonly DbContextOptions _contextOptions;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailNotifier _emailNotifier;

        public JobManager(DbContextOptions contextOptions, IDateTimeProvider dateTimeProvider, IEmailNotifier emailNotifier)
        {
            _contextOptions = contextOptions;
            _dateTimeProvider = dateTimeProvider;
            _emailNotifier = emailNotifier;
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
                    Ukprn = job.Ukprn,
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
                    PeriodNumber = job.PeriodNumber
                };
                context.IlrJobMetaDataEntities.Add(metaEntity);

                context.Jobs.Add(entity);
                context.SaveChanges();
                return entity.JobId;
            }
        }

       public IEnumerable<FileUploadJob> GetAllJobs()
        {
            var jobs = new List<FileUploadJob>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntities = context.Jobs.ToList();
                jobEntities.ForEach(x =>
                    jobs.Add(JobConverter.Convert(x)));
                LoadIlrJobMetaData(jobs, context);
            }

            return jobs;
        }

        public IEnumerable<FileUploadJob> GetJobsByUkprn(long ukprn)
        {
            if (ukprn == 0)
            {
                throw new ArgumentException("ukprn can not be 0");
            }

            var jobs = new List<FileUploadJob>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntities = context.Jobs.Where(x => x.Ukprn == ukprn).ToList();
                jobEntities.ForEach(x =>
                    jobs.Add(JobConverter.Convert(x)));
                LoadIlrJobMetaData(jobs, context);
            }

            return jobs;
        }

        public FileUploadJob GetJobById(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var job = new FileUploadJob();
                JobConverter.Convert(entity, job);
                LoadIlrJobMetaData(job, context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId));
                return job;
            }
        }

        public FileUploadJob GetJobByPriority()
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntity = context.Jobs.FromSql("GetJobByPriority").FirstOrDefault();
                if (jobEntity == null)
                {
                    return null;
                }

                var job = JobConverter.Convert(jobEntity);
                LoadIlrJobMetaData(job, context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == job.JobId));
                return job;
            }
        }

        public void RemoveJobFromQueue(long jobId)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (jobEntity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                if (jobEntity.Status != 1) // if already moved, then dont delete
                {
                    throw new ArgumentOutOfRangeException("Job is already moved from ready status, unable to delete");
                }

                var metaDataEntity = context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId);
                if (metaDataEntity != null)
                {
                    context.IlrJobMetaDataEntities.Remove(metaDataEntity);
                }

                context.Jobs.Remove(jobEntity);
                context.SaveChanges();
            }
        }

        public bool UpdateJob(FileUploadJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == job.JobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {job.JobId} does not exist");
                }

                var statusChanged = entity.Status != (short)job.Status;

                JobConverter.Convert(job, entity);
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).Property("RowVersion").OriginalValue = Convert.FromBase64String(job.RowVersion);
                context.Entry(entity).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();
                    UpdateJobMetaData(job);

                    if (statusChanged)
                    {
                        SendEmailNotification(job.JobId, job.Status, job.JobType);
                    }

                    return true;
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    throw new Exception("Save failed. Job details have been changed. Reload the job object and try save again");
                }
            }
        }

        public bool UpdateJobMetaData(FileUploadJob jobMetaData)
        {
            if (jobMetaData == null)
            {
                throw new ArgumentNullException();
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobMetaData.JobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobMetaData.JobId} does not exist");
                }

                JobConverter.Convert(jobMetaData, entity);
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

        public bool UpdateJobStatus(long jobId, JobStatusType status)
        {
            if (jobId == 0)
            {
                throw new ArgumentException("Job id can not be 0");
            }

            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var entity = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                if (entity == null)
                {
                    throw new ArgumentException($"Job id {jobId} does not exist");
                }

                var statusChanged = entity.Status != (short)status;
                entity.Status = (short)status;
                entity.DateTimeUpdatedUtc = _dateTimeProvider.GetNowUtc();
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();

                if (statusChanged)
                {
                    SendEmailNotification(jobId, status, (JobType)entity.JobType);
                }

                return true;
            }
        }

        public void LoadIlrJobMetaData(IEnumerable<FileUploadJob> jobs, JobQueueDataContext context)
        {
            if (!jobs.Any())
            {
                return;
            }

            foreach (var job in jobs.Where(x => x.JobType == JobType.IlrSubmission))
            {
                LoadIlrJobMetaData(job, context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == job.JobId));
            }
        }

        public void LoadIlrJobMetaData(FileUploadJob job, FileUploadJobMetaDataEntity metaDataEntity)
        {
            if (job == null)
            {
                return;
            }

            JobConverter.Convert(metaDataEntity, job);
        }

        public void SendEmailNotification(long jobId, JobStatusType status, JobType jobType)
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var emailTemplate =
                    context.JobEmailTemplates.SingleOrDefault(
                        x => x.JobType == (short)jobType && x.JobStatus == (short)status && x.Active);

                if (!string.IsNullOrEmpty(emailTemplate?.TemplateId))
                {
                    var job = context.Jobs.SingleOrDefault(x => x.JobId == jobId);
                    var jobMetaData = context.IlrJobMetaDataEntities.SingleOrDefault(x => x.JobId == jobId);
                    var personalisation = new Dictionary<string, dynamic>
                    {
                        { "JobId", job.JobId },
                        { "FileName", jobMetaData.FileName },
                        { "CollectionName", jobMetaData.CollectionName },
                        { "Name", job.SubmittedBy }
                    };
                    _emailNotifier.SendEmail(job.NotifyEmail, emailTemplate?.TemplateId, personalisation);
                }
            }
        }
    }
}