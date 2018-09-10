using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.Job.Models;
using ESFA.DC.Job.Models.Enums;
using ESFA.DC.JobNotifications.Interfaces;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
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

        public long AddJob(Job.Models.Job job)
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
                context.Jobs.Add(entity);
                context.SaveChanges();
                return entity.JobId;
            }
        }

       public IEnumerable<Job.Models.Job> GetAllJobs()
        {
            var jobs = new List<Job.Models.Job>();
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntities = context.Jobs.ToList();
                jobEntities.ForEach(x =>
                    jobs.Add(JobConverter.Convert(x)));
            }

            return jobs;
        }

        //public IEnumerable<Job.Models.Job> GetJobsByUkprn(long ukprn)
        //{
        //    if (ukprn == 0)
        //    {
        //        throw new ArgumentException("ukprn can not be 0");
        //    }

        //    var jobs = new List<FileUploadJob>();
        //    using (var context = new JobQueueDataContext(_contextOptions))
        //    {
        //        var jobEntities = context.Jobs.Where(x => x.Ukprn == ukprn).ToList();
        //        jobEntities.ForEach(x =>
        //            jobs.Add(JobConverter.Convert(x)));
        //        LoadIlrJobMetaData(jobs, context);
        //    }

        //    return jobs;
        //}

        public Job.Models.Job GetJobById(long jobId)
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

                var job = new Job.Models.Job();
                JobConverter.Convert(entity, job);
                return job;
            }
        }

        public Job.Models.Job GetJobByPriority()
        {
            using (var context = new JobQueueDataContext(_contextOptions))
            {
                var jobEntity = context.Jobs.FromSql("GetJobByPriority").FirstOrDefault();
                if (jobEntity == null)
                {
                    return null;
                }

                var job = JobConverter.Convert(jobEntity);
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

                context.Jobs.Remove(jobEntity);
                context.SaveChanges();
            }
        }

        public bool UpdateJob(Job.Models.Job job)
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

        //TODO: when we get the proper template this needs revisting
        public void SendEmailNotification(long jobId, JobStatusType status, JobType jobType)
        {
            //using (var context = new JobQueueDataContext(_contextOptions))
            //{
            //    var emailTemplate =
            //        context.JobEmailTemplates.SingleOrDefault(
            //            x => x.JobType == (short)jobType && x.JobStatus == (short)status && x.Active);

            //    if (!string.IsNullOrEmpty(emailTemplate?.TemplateId))
            //    {
            //       _emailNotifier.SendEmail()
            //    }
            //}
        }

        public IEnumerable<Job.Models.Job> GetJobsByUkprn(long ukrpn)
        {
            throw new NotImplementedException();
        }
    }
}